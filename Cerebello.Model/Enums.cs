using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace CerebelloWebRole.Models
{

    public enum TypeMaritalStatus
    {
        Solteiro = 0,
        Casado = 1,
        Viuvo = 2,
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

    public enum TypeCPFOwner
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

    public enum TypeAppointment
    {
        [Display(Name = "Compromisso")]
        GenericAppointment = 0,

        [Display(Name = "Consulta")]
        MedicalAppointment = 1
    }

    public enum TypeGender
    {
        [Display(Name = "Masculino")]
        Male = 0,
        [Display(Name = "Feminino")]
        Female = 1
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