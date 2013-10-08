using System;
using AutoMapper;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App
{
    public static class ModelMappings
    {
        public static void CreateMaps()
        {
            CreateMapsFromModelToViewModel();
            CreateMapsFromViewModelToModel();
        }

        private static void CreateMapsFromViewModelToModel()
        {
            Mapper.CreateMap<PersonViewModel, Person>()
                .ForMember(dest => dest.Id, config => config.Ignore())
                .ForMember(dest => dest.Email, config => config.MapFrom(src => src.Email != null ? src.Email.ToLower() : null))
                .ForMember(dest => dest.EmailGravatarHash, config => config.MapFrom(src => GravatarHelper.GetGravatarHash(src.Email)))
                .AfterMap(
                    (src, dest) =>
                        {
                            dest.DateOfBirth = ModelDateTimeHelper.ConvertToUtcDateTime(dest.Practice, dest.DateOfBirth);
                            dest.DateOfDeath = ModelDateTimeHelper.ConvertToUtcDateTime(dest.Practice, dest.DateOfDeath);
                        });

            Mapper.CreateMap<PatientViewModel, Patient>();

            Mapper.CreateMap<AddressViewModel, Address>();

            Mapper.CreateMap<PatientBasicInformationViewModel, Patient>()
                .ForMember(dest => dest.Id, config => config.Ignore());

            Mapper.CreateMap<PastMedicalHistoryViewModel, PastMedicalHistory>()
                .ForMember(dest => dest.Id, config => config.Ignore())
                .AfterMap(
                    (src, dest) =>
                    {
                        dest.MedicalRecordDate = ModelDateTimeHelper.ConvertToUtcDateTime(dest.Practice, dest.MedicalRecordDate);
                    });

            Mapper.CreateMap<DiagnosisViewModel, Diagnosis>()
                .ForMember(dest => dest.Id, config => config.Ignore())
                .AfterMap(
                    (src, dest) =>
                    {
                        dest.StartTime = ModelDateTimeHelper.ConvertToUtcDateTime(dest.Practice, dest.StartTime);
                        dest.EndTime = ModelDateTimeHelper.ConvertToUtcDateTime(dest.Practice, dest.EndTime);
                        dest.MedicalRecordDate = ModelDateTimeHelper.ConvertToUtcDateTime(dest.Practice, dest.MedicalRecordDate);
                    });
        }

        private static void CreateMapsFromModelToViewModel()
        {
            Mapper.CreateMap<Person, PersonViewModel>()
                .AfterMap(
                    (src, dest) =>
                        {
                            dest.DateOfBirth = ModelDateTimeHelper.ConvertToLocalDateTime(src.Practice, src.DateOfBirth);
                            dest.DateOfDeath = ModelDateTimeHelper.ConvertToLocalDateTime(src.Practice, src.DateOfBirth);
                        });

            Mapper.CreateMap<Person, PatientViewModel>();

            Mapper.CreateMap<Patient, PatientViewModel>();

            Mapper.CreateMap<User, UserViewModel>()
                .ForMember(dest => dest.IsAdministrador, config => config.MapFrom(src => src.AdministratorId != null))
                .ForMember(dest => dest.IsDoctor, config => config.MapFrom(src => src.DoctorId != null))
                .ForMember(dest => dest.IsSecretary, config => config.MapFrom(src => src.SecretaryId != null));

            Mapper.CreateMap<PastMedicalHistory, PastMedicalHistoryViewModel>()
                .AfterMap(
                    (src, dest) =>
                    {
                        dest.MedicalRecordDate = ModelDateTimeHelper.ConvertToLocalDateTime(src.Practice, src.MedicalRecordDate);
                    });

            Mapper.CreateMap<Diagnosis, DiagnosisViewModel>()
                .AfterMap(
                    (src, dest) =>
                    {
                        dest.StartTime = ModelDateTimeHelper.ConvertToLocalDateTime(src.Practice, src.StartTime);
                        dest.EndTime = ModelDateTimeHelper.ConvertToLocalDateTime(src.Practice, src.EndTime);
                        dest.MedicalRecordDate = ModelDateTimeHelper.ConvertToLocalDateTime(src.Practice, src.MedicalRecordDate);
                    });

            Mapper.CreateMap<Address, AddressViewModel>();

            Mapper.CreateMap<Patient, PatientBasicInformationViewModel>();
        }
    }
}