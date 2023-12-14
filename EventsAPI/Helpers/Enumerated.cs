using System.ComponentModel;

namespace EventsAPI.Helpers;

public class Enumerated
{
    public enum EventState
    {
        [Description("Cancelado por Circunstâncias Imprevistas")]
        CanceladoCircunstanciasImprevistas = 0,

        [Description("Adiado para Data Futura")]
        AdiadoParaDataFutura = 1,

        [Description("Cancelado por Restrições de Saúde")]
        CanceladoRestricoesSaude = 2,

        [Description("Alteração de Local")]
        AlteracaoLocal = 3,

        [Description("Cancelado por Condições Meteorológicas")]
        CanceladoCondicoesMeteorologicas,

        [Description("Cancelado por Problemas Logísticos")]
        CanceladoProblemasLogisticos,

        [Description("Acontecendo Normalmente")]
        AcontecendoNormalmente,

        [Description("Término do Evento")]
        TerminoDoEvento,

        [Description("Lotado")]
        Lotado,

        [Description("Esgotado")]
        Esgotado,

        [Description("Em Andamento com Restrições")]
        EmAndamentoComRestricoes,

        [Description("Adiado por Razões Técnicas")]
        AdiadoPorRazoesTecnicas,

        [Description("Em Pausa")]
        EmPausa,

        [Description("Cancelado por Baixa Participação")]
        CanceladoPorBaixaParticipacao,

        [Description("Programação Modificada")]
        ProgramacaoModificada,

        [Description("Aguardando Aprovação")]
        AguardandoAprovacao,

        [Description("Planejamento em Andamento")]
        PlanejamentoEmAndamento,

        [Description("Inscrições Abertas")]
        InscricoesAbertas,

        [Description("Em Espera")]
        EmEspera,

        [Description("Cancelado por Força Maior")]
        CanceladoPorForcaMaior,

        [Description("Finalizado com Sucesso")]
        FinalizadoComSucesso,

        [Description("Cancelado devido a Falhas Técnicas")]
        CanceladoDevidoAFalhasTecnicas,

        [Description("Em Execução")]
        EmExecucao,

        [Description("Cancelado por Decisão do Governo")]
        CanceladoPorDecisaoGoverno,

        [Description("Em Negociação de Contrato")]
        EmNegociacaoContrato,

        [Description("Em Revisão Logística")]
        EmRevisaoLogistica,

        [Description("Atraso na Programação")]
        AtrasoNaProgramacao
    }

    public enum ParticipantStatus
    {
        [Description("Não Confirmado")]
        NaoConfirmado = 0,

        [Description("Confirmado")]
        Confirmado = 1,
        
        [Description("Interessado")]
        Interessado = 2,
        
        [Description("Desinteressado")]
        Desinteressado = 3,
        
        [Description("Vai Participar")]
        VaiParticipar = 4,
        
        [Description("Não Vai Participar")]
        NãoVaiParticipar = 5
    }

    public enum ParticipantType
    {
        [Description("Convidado")]
        Convidado,

        [Description("Convidado Por Email")]
        ConvidadoPorEmail,

        [Description("Participante Regular")]
        ParticipanteRegular,
    }


}
