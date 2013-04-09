using Cerebello.Model;

namespace CerebelloWebRole.Code
{
    /// <summary>
    /// Helps filling business objects with valid data.
    /// </summary>
    public class BusHelper
    {
        /// <summary>
        /// Fills a new doctor with common useful data: default medical-certificates and a default health-insurance.
        /// </summary>
        /// <param name="doctor">The new doctor to receive some useful objects to work with.</param>
        public static void FillNewDoctorUtilityBelt(Doctor doctor)
        {
            {
                var medicalCertificate = new ModelMedicalCertificate
                {
                    Name = "Comparecimento em consulta",
                    Text = "O paciente <%Paciente%> compareceu a uma consulta no horário de <%HoraInicio%> até <%HoraFim%>.",
                    PracticeId = doctor.PracticeId,
                };
                medicalCertificate.Fields.Add(new ModelMedicalCertificateField { Name = "HoraInicio", PracticeId = doctor.PracticeId, });
                medicalCertificate.Fields.Add(new ModelMedicalCertificateField { Name = "HoraFim", PracticeId = doctor.PracticeId, });

                doctor.ModelMedicalCertificates.Add(medicalCertificate);
            }

            {
                var medicalCertificate = new ModelMedicalCertificate
                {
                    Name = "Paciente necessita repouso",
                    Text = "O paciente <%Paciente%> necessita de repouso do dia <%DiaInicio%> até o dia <%DiaFim%>.",
                    PracticeId = doctor.PracticeId,
                };
                medicalCertificate.Fields.Add(new ModelMedicalCertificateField { Name = "DiaInicio", PracticeId = doctor.PracticeId, });
                medicalCertificate.Fields.Add(new ModelMedicalCertificateField { Name = "DiaFim", PracticeId = doctor.PracticeId, });

                doctor.ModelMedicalCertificates.Add(medicalCertificate);
            }

            doctor.HealthInsurances.Add(new HealthInsurance
            {
                Name = "Particular",
                IsParticular = true,
                IsActive = true,
                PracticeId = doctor.PracticeId,
                ReturnTimeInterval = 30,
            });
        }
    }
}