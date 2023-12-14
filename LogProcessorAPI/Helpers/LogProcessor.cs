using MicroservicesHelpers.Models;
using LogProcessorAPI.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using Newtonsoft.Json;
using LogProcessorAPI.Services;

namespace LogProcessorAPI.Helpers;

public class LogProcessor
{
    private readonly Logs _logs;
    private readonly LogService _logService;

    public LogProcessor(IOptions<Logs> log, LogService logService)
    {
        _logs = log.Value;
        _logService = logService;
    }

    public async Task ProcessarLogs()
    {
        string[] arquivosDeLog = Directory.GetFiles(_logs.Path, "*.txt");

        foreach (string arquivo in arquivosDeLog)
        {
            await ProcessarArquivoDeLog(arquivo);
        }
    }

    private async Task ProcessarArquivoDeLog(string caminhoArquivo)
    {
        string[] linhas = File.ReadAllLines(caminhoArquivo);
        string nomeFicheiro = ExtrairNomeFicheiro(caminhoArquivo);

        foreach (string linha in linhas)
        {
            LogInformation informacao = ExtrairInformacao(linha, nomeFicheiro);
            await _logService.InsertsLogs(informacao);
            RemoverLinhaDoLog(caminhoArquivo, linha);
        }
    }

    private string ExtrairNomeFicheiro(string caminhoArquivo)
    {
        string nomeFicheiro = Path.GetFileNameWithoutExtension(caminhoArquivo);
        int index = nomeFicheiro.IndexOf("Log_", StringComparison.OrdinalIgnoreCase);

        if (index != -1)
        {
            return nomeFicheiro.Substring(0, index) + "Log";
        }

        return nomeFicheiro;
    }

    private LogInformation ExtrairInformacao(string linha, string nomeArquivo)
    {
        Regex regex = new Regex(@"@mt"":""(.+?)""");
        Match match = regex.Match(linha);

        if (match.Success)
        {
            return new LogInformation
            {
                WebAPI = nomeArquivo,
                Value = BsonDocument.Parse(linha)
            };
        }

        return null;
    }

    private void RemoverLinhaDoLog(string caminhoArquivo, string linha)
    {
        string[] linhas = File.ReadAllLines(caminhoArquivo);
        File.WriteAllLines(caminhoArquivo, linhas.Where(l => l != linha));
    }
}
