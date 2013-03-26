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
        /// <summary>
        /// The trial contract.
        /// </summary>
        TrialContract = 1,

        /// <summary>
        /// The professional contract.
        /// </summary>
        ProfessionalContract = 2,
    }

    public enum TypeAppointmentStatus : int
    {
        [Display(Name = "Agendada")]
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
    public enum TypeNotification
    {
        [Display(Name = "Genérica")]
        Generic = 0,

        [Display(Name = "Consulta")]
        PatientAppointment = 1,

        [Display(Name = "Compromisso")]
        GenericAppointment = 1,

        [Display(Name = "Paciente chegou")]
        PatientArrived = 2
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

    public static class TimeZoneName
    {
        public const string GmtL4_GmtL3 = "Central Brazilian Standard Time";
        public const string GmtL4 = "SA Western Standard Time";
        public const string GmtL3_GmtL2 = "E. South America Standard Time";
        public const string GmtL3 = "SA Eastern Standard Time";
        public const string GmtL2 = "Mid-Atlantic Standard Time";
    }

    public enum TypeEstadoBrasileiro
    {
        [TimeZoneData(Id = TimeZoneName.GmtL4)]
        [Display(Name = "Acre")]
        AC,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Alagoas")]
        AL,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Amapá")]
        AP,

        [TimeZoneData(Id = TimeZoneName.GmtL4)]
        [Display(Name = "Amazonas")]
        AM,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Bahia")]
        BA,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Ceará")]
        CE,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Distrito Federal")]
        DF,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Espírito Santo")]
        ES,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Goiás")]
        GO,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Maranhão")]
        MA,

        [TimeZoneData(Id = TimeZoneName.GmtL4_GmtL3)]
        [Display(Name = "Mato Grosso")]
        MT,

        [TimeZoneData(Id = TimeZoneName.GmtL4_GmtL3)]
        [Display(Name = "Mato Grosso do Sul")]
        MS,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Minas Gerais")]
        MG,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Pará")]
        PA,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Paraíba")]
        PB,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Paraná")]
        PR,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Pernambuco")]
        PE,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Piauí")]
        PI,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Rio de Janeiro")]
        RJ,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Rio Grande do Norte")]
        RN,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Rio Grande do Sul")]
        RS,

        [TimeZoneData(Id = TimeZoneName.GmtL4)]
        [Display(Name = "Rondônia")]
        RO,

        [TimeZoneData(Id = TimeZoneName.GmtL4)]
        [Display(Name = "Roraima")]
        RR,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Santa Catarina")]
        SC,

        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "São Paulo")]
        SP,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Sergipe")]
        SE,

        [TimeZoneData(Id = TimeZoneName.GmtL3)]
        [Display(Name = "Tocantins")]
        TO,

        [TimeZoneData(Id = TimeZoneName.GmtL2)]
        [Display(Name = "Arquipélagos Atlânticos")]
        Arquipelagos,
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
        [TimeZoneData(Id = TimeZoneName.GmtL3_GmtL2)]
        [Display(Name = "Brasília (GMT-3 com ajuste de +1h no horário de verão)")]
        Brasilia = 0,

        // Mato Grosso,  Mato Grosso do Sul
        [TimeZoneData(Id = TimeZoneName.GmtL4_GmtL3)]
        [Display(Name = "Cuiabá (GMT-4 com ajuste de +1h no horário de verão)")]
        Cuiaba = 1,

        // Acre State, Amazonas, Roraima,  Roraima
        [TimeZoneData(Id = TimeZoneName.GmtL4)]
        [Display(Name = "Manaus (GMT-4 sem ajuste de horário de verão)")]
        Manaus = 2,

        //  Alagoas, Amapá, Ceará, Maranhão, Pará, Paraíba, Pernambuco, Piauí, Rio Grande do Norte,  Rondônia, Sergipe, Tocantins
        [TimeZoneData(Id = TimeZoneName.GmtL3)]
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