using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CerebelloWebRole.Areas.App.Models
{
    public class SessionViewModel
    {
        public SessionViewModel()
        {
            this.ReceiptIds = new List<int>();
            this.AnamneseIds = new List<int>();
        }

        public int PatientId { get; set; }

        public DateTime Date { get; set; }

        /// <summary>
        /// Receipt ids.
        /// </summary>
        public List<int> ReceiptIds { get; set; }

        /// <summary>
        /// Anamnese ids.
        /// </summary>
        public List<int> AnamneseIds { get; set; }

        /// <summary>
        /// Certificate ids.
        /// </summary>
        public List<int> MedicalCertificateIds { get; set; }

        /// <summary>
        /// Examination request ids.
        /// </summary>
        public List<int> ExaminationRequestIds { get; set; }

        /// <summary>
        /// Examination result ids.
        /// </summary>
        public List<int> ExaminationResultIds { get; set; }
    }
}