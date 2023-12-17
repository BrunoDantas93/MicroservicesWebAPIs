using MicroservicesHelpers.Models;
using LogProcessorAPI.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using Newtonsoft.Json;
using LogProcessorAPI.Services;
using Newtonsoft.Json.Linq;
using Serilog;

namespace LogProcessorAPI.Helpers;

public class LogProcessor
{
    private Timer _timer;
    private readonly Logs _logs;
    private readonly LogService _logService;

    public LogProcessor(IOptions<Logs> log, LogService logService)
    {
        // Start the scheduled task in the constructor.
        _timer = new Timer(ProcessarLogs, null, TimeSpan.Zero, TimeSpan.FromHours(1));

        _logs = log.Value;
        _logService = logService;
    }

    /// <summary>
    /// Método chamado pelo Timer para processar os logs.
    /// </summary>
    /// <param name="state">O estado do objeto Timer (não usado).</param>
    private void ProcessarLogs(object state)
    {
        // Execute the asynchronous method to process logs using Task.Run
        // This is done so that the method can be executed asynchronously, as TimerCallback does not support async directly
        Task.Run(async () => await ProcessarLogs());
    }



    /// <summary>
    /// Processes log files in the specified directory.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ProcessarLogs()
    {
        // Get a list of log files in the specified directory with a ".txt" extension
        string[] logfiles = Directory.GetFiles(_logs.Path, "*.txt");

        // Process each log file
        foreach (string file in logfiles)
        {
            await ProcessLogFile(file);
        }
    }

    /// <summary>
    /// Processes a single log file.
    /// </summary>
    /// <param name="pathFile">The path of the log file to process.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ProcessLogFile(string pathFile)
    {
        // Read all lines from the log file
        string[] lines = File.ReadAllLines(pathFile);

        // Extract the file name without extension
        string fileName = ExtractFileName(pathFile);

        // Process each line in the log file
        foreach (string line in lines)
        {
            // Extract log information from the line and the file name
            LogInformation log = ExtractInformation(line, fileName);

            if(log != null)
            {
                // Insert log information into the database
                await _logService.InsertsLogs(log);

                // Remove the processed line from the log file
                RemoveLogLine(pathFile, line);
            }
        }
    }

    /// <summary>
    /// Extracts the base name of the log file without the extension.
    /// </summary>
    /// <param name="pathFile">The path of the log file.</param>
    /// <returns>The base name of the log file.</returns>
    private string ExtractFileName(string pathFile)
    {
        // Get the base name of the file without extension
        string fileName = Path.GetFileNameWithoutExtension(pathFile);

        // Find the index of "Log_" in the file name and adjust the result
        int index = fileName.IndexOf("Log_", StringComparison.OrdinalIgnoreCase);

        if (index != -1)
        {
            return fileName.Substring(0, index);
        }

        return fileName;
    }

    /// <summary>
    /// Extracts log information from a log file line.
    /// </summary>
    /// <param name="line">The log file line to process.</param>
    /// <param name="fileName">The base name of the log file.</param>
    /// <returns>Log information extracted from the line.</returns>
    private LogInformation ExtractInformation(string line, string fileName)
    {
        // Use a regular expression to extract log information from the line
        Regex regex = new Regex(@"@t"":""(.+?)""");
        Match match = regex.Match(line);

        // If a match is found, create a LogInformation object
        if (match.Success)
        {
            BsonDocument bsonDocument = BsonDocument.Parse(line);
            return new LogInformation
            {
                WebAPI = fileName,
                TimeStanp = ExtrairDataDoLog(bsonDocument),
                Value = bsonDocument.ToDictionary()
            };
        }

        return null;
    }

    /// <summary>
    /// Removes a specific line from a log file.
    /// </summary>
    /// <param name="pathFile">The path of the log file.</param>
    /// <param name="line">The line to remove from the log file.</param>
    private void RemoveLogLine(string pathFile, string line)
    {
        // Read all lines from the log file
        string[] linhas = File.ReadAllLines(pathFile);

        // Write back all lines except the one to be removed
        File.WriteAllLines(pathFile, linhas.Where(l => l != line));
    }

    /// <summary>
    /// Extracts the date and time information from a log document.
    /// </summary>
    /// <param name="logDocument">The log document containing the date and time information.</param>
    /// <returns>
    /// The extracted DateTime value. Returns DateTime.MinValue if the "@t" field is not present or the value is not a valid DateTime.
    /// </returns>
    public DateTime ExtrairDataDoLog(BsonDocument logDocument)
    {
        // Check if the "@t" field exists in the document
        if (logDocument.TryGetValue("@t", out BsonValue dataValue))
        {
            // Return the value converted to DateTime in UTC
            return DateTime.Parse(dataValue.ToString());
        }

        // Return DateTime.MinValue if the "@t" field is not present or its value is not a valid DateTime
        return DateTime.MinValue;
    }


}
