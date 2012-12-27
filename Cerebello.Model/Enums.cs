using System.ComponentModel.DataAnnotations;

// todo: this namespace is wrong... should be Cerebello.Model
namespace Cerebello.Model
{
    /// <summary>
    /// Enumeration of all Cerebello usage contract types.
    /// Each contract type allow very little variation.
    /// </summary>
    public enum ContractTypes
    {
        TrialContract = 1,
        MonthlyFeeSubscriptionContract = 2,
        OneFeeOneYearSubscriptionContract = 3,
        MonthlyFeeSubscriptionForNewcomersContract = 4,
    }

    public enum TypeAppointmentStatus : int
    {
        [Display(Name="Agendada")]
        Undefined = 0,

        [Display(Name = "Não realizada")]
        NotAccomplished = 1,

        [Display(Name = "Realizada")]
        Accomplished = 10,

        /// <summary>
        /// This status is only for generic appointments. 
        /// It indicates the user received the notification and then discarded
        /// </summary>
        [Display(Name = "Descartada")]
        Discarded = 50
    }

    public enum TypeMaritalStatus
    {
        [Display(Name = "Solteiro(a)")]
        Solteiro = 0,

        [Display(Name = "Casado(a)")]
        Casado = 1,

        [Display(Name = "Viuvo(a)")]
        Viuvo = 2,

        [Display(Name = "Divorciado(a)")]
        Divorciado = 3
    }

    public enum TypeInstructionLevel
    {
        [Display(Name = "Nenhum")]
        None = 0,

        [Display(Name = "Ensino fundamental completo")]
        FundamentalEducation = 1,

        [Display(Name = "Ensino Médio completo")]
        HighSchool = 2,

        [Display(Name = "Superior incompleto")]
        GraduationNotCompleted = 3,

        [Display(Name = "Superior completo")]
        GraduationCompleted = 4,

        [Display(Name = "Pós-graduação")]
        PostGraduation = 5
    }

    public enum TypeCpfOwner
    {
        [Display(Name = "O próprio paciente")]
        PatientItself,

        [Display(Name = "Pai do paciente")]
        PatientsFather,

        [Display(Name = "Mãe do paciente")]
        PatientsMother,

        [Display(Name = "Outro responsável")]
        Other
    }

    /// <summary>
    /// Appointment type
    /// </summary>
    public enum TypeAppointment
    {
        [Display(Name = "Compromisso")]
        GenericAppointment = 0,

        [Display(Name = "Consulta")]
        MedicalAppointment = 1
    }

    /// <summary>
    /// Gender type
    /// </summary>
    public enum TypeGender
    {
        [Display(Name = "Masculino")]
        Male = 0,

        [Display(Name = "Feminino")]
        Female = 1
    }

    public enum TypeEstadoBrasileiro
    {
        [TimeZoneData(Id = "SA Western Standard Time")]
        [Display(Name = "Acre")]
        AC,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Alagoas")]
        AL,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Amapá")]
        AP,

        [TimeZoneData(Id = "SA Western Standard Time")]
        [Display(Name = "Amazonas")]
        AM,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Bahia")]
        BA,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Ceará")]
        CE,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Distrito Federal")]
        DF,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Espírito Santo")]
        ES,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Goiás")]
        GO,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Maranhão")]
        MA,

        [TimeZoneData(Id = "Central Brazilian Standard Time")]
        [Display(Name = "Mato Grosso")]
        MT,

        [TimeZoneData(Id = "Central Brazilian Standard Time")]
        [Display(Name = "Mato Grosso do Sul")]
        MS,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Minas Gerais")]
        MG,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Pará")]
        PA,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Paraíba")]
        PB,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Paraná")]
        PR,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Pernambuco")]
        PE,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Piauí")]
        PI,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Rio de Janeiro")]
        RJ,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Rio Grande do Norte")]
        RN,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Rio Grande do Sul")]
        RS,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Rondônia")]
        RO,

        [TimeZoneData(Id = "SA Western Standard Time")]
        [Display(Name = "Roraima")]
        RR,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Santa Catarina")]
        SC,

        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "São Paulo")]
        SP,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Sergipe")]
        SE,

        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Tocantins")]
        TO,
    }

    public enum TypeTimeZone
    {
        // sources:
        // Brazil Time Zones (have official references): http://wwp.greenwichmeantime.com/time-zone/south-america/brazil/time-brazil/
        // Fusos horários no Brasil (wikipedia): http://pt.wikipedia.org/wiki/Fusos_hor%C3%A1rios_no_Brasil
        // Time Zone Map (interactive map): http://www.timeanddate.com/time/map/

        // note: alternative to Windows Time-Zones: http://zoneinfo.codeplex.com/
        // http://en.wikipedia.org/wiki/Tz_database

        //  Bahia, Brasilia, Espírito Santo, Goiás, Minas Gerais, Paraná, Rio de Janeiro, Rio Grande do Sul, Santa Catarina, São Paulo
        [TimeZoneData(Id = "E. South America Standard Time")]
        [Display(Name = "Brasília (GMT-3 com ajuste de +1h no horário de verão)")]
        Brasilia = 0,

        // Mato Grosso,  Mato Grosso do Sul
        [TimeZoneData(Id = "Central Brazilian Standard Time")]
        [Display(Name = "Cuiabá (GMT-4 com ajuste de +1h no horário de verão)")]
        Cuiaba = 1,

        // Acre State, Amazonas, Roraima,  Roraima
        [TimeZoneData(Id = "SA Western Standard Time")]
        [Display(Name = "Manaus (GMT-4 sem ajuste de horário de verão)")]
        Manaus = 2,

        //  Alagoas, Amapá, Ceará, Maranhão, Pará, Paraíba, Pernambuco, Piauí, Rio Grande do Norte,  Rondônia, Sergipe, Tocantins
        [TimeZoneData(Id = "SA Eastern Standard Time")]
        [Display(Name = "Fortaleza (GMT-3 sem ajuste de horário de verão)")]
        Fortaleza = 3,

        // Fernando de Noronha Archipelago
        [TimeZoneData(Id = "Mid-Atlantic Standard Time")]
        [Display(Name = "Mid-Atlantic (GMT-2 sem ajuste de horário de verão)")]
        MidAtlantic = 4,

        // note: Don't know why Windows defines this TimeZone in the system... Salvador is officially in the same time-zone as Brasília.
        [TimeZoneData(Id = "Bahia Standard Time")]
        [Display(Name = "Salvador (GMT-3 com ajuste de +1h no horário de verão)")]
        Salvador = 5,
    }

    public enum TypeAppointmentDuration
    {
        [Display(Name = "15 minutos")]
        Dur15 = 15,
        [Display(Name = "20 minutos")]
        Dur20 = 20,
        [Display(Name = "30 minutos")]
        Dur30 = 30,
        [Display(Name = "60 minutos")]
        Dur60 = 60
    }

    /// <summary>
    /// Descreve os tipos que um telefone pode ser
    /// </summary>
    public enum TypePhone
    {
        [Display(Name = "Casa")]
        Home,
        [Display(Name = "Celular")]
        Mobile,
        [Display(Name = "Trabalho")]
        Work,
        [Display(Name = "Fax")]
        Fax,
        [Display(Name = "Skype")]
        Skype,
        [Display(Name = "Outro")]
        Other
    }


    public enum TypeLicense
    {
        [Display(Name = "Demonstração")]
        Trial = 0,

        [Display(Name = "Comercial")]
        Commercial = 1
    }

    public enum TypeUsage
    {
        [Display(Name = "Cutâneo")]
        Cutaneo,

        [Display(Name = "Externo")]
        Externo,

        [Display(Name = "Inalatório")]
        Inalatorio,

        [Display(Name = "Interno")]
        Interno,

        [Display(Name = "Intra-muscular")]
        Intramuscular,

        [Display(Name = "Intra-venoso")]
        Intravenoso,

        [Display(Name = "Nasal")]
        Nasal,

        [Display(Name = "Ocular")]
        Ocular,

        [Display(Name = "Oral")]
        Oral,

        [Display(Name = "Ratal")]
        Retal,

        [Display(Name = "Sub-cutâneo")]
        Subcutaneo
    }
}