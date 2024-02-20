using System.ComponentModel;

namespace MicroservicesHelpers;

/// <summary>
/// Contains enumerated types used in the Microservices application.
/// </summary>
public class Enumerated
{
    /// <summary>
    /// Represents possible error codes for MicroservicesResponse.
    /// </summary>
    public enum MicroservicesCode
    {
        /// <summary>
        /// 
        /// </summary>
        NoError = 0,

        /// <summary>
        /// Indicates a successful operation.
        /// </summary>
        OK = 1,

        /// <summary>
        /// Indicates an error detected through validations.
        /// </summary>
        Validation = 2,

        /// <summary>
        /// Indicates a fatal error (catch).
        /// </summary>
        FatalError = 3
    }


    /// <summary>
    /// 
    /// </summary>
    public enum UserType
    {
        Admin = 0,
        User = 1
    }


    public enum MicroservicesName
    {
        [Description("EventsAPI")]
        EventsAPI,

        [Description("IdentityServer")]
        IdentityServer,

        [Description("MicroservicesGateway")]
        MicroservicesGateway,

        [Description("UserDetailsAPI")]
        UserDetailsAPI,
    }

    public enum LanguageCode
    {
        [Description("Arabic")]
        AR = 1,
        [Description("Bulgarian")]
        BG,
        [Description("Czech")]
        CS,
        [Description("Danish")]
        DA,
        [Description("German")]
        DE,
        [Description("Greek")]
        EL,
        [Description("English")]
        EN,
        [Description("Spanish")]
        ES,
        [Description("Estonian")]
        ET,
        [Description("Finnish")]
        FI,
        [Description("French")]
        FR,
        [Description("Hungarian")]
        HU,
        [Description("Indonesian")]
        ID,
        [Description("Italian")]
        IT,
        [Description("Japanese")]
        JA,
        [Description("Korean")]
        KO,
        [Description("Lithuanian")]
        LT,
        [Description("Latvian")]
        LV,
        [Description("Norwegian (Bokmål)")]
        NB,
        [Description("Dutch")]
        NL,
        [Description("Polish")]
        PL,
        [Description("Portuguese (all Portuguese varieties mixed)")]
        PT,
        [Description("Romanian")]
        RO,
        [Description("Russian")]
        RU,
        [Description("Slovak")]
        SK,
        [Description("Slovenian")]
        SL,
        [Description("Swedish")]
        SV,
        [Description("Turkish")]
        TR,
        [Description("Ukrainian")]
        UK,
        [Description("Chinese")]
        ZH
    }



}
