using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    /// <summary>
    /// The patient files group view model.
    /// </summary>
    public class PatientFilesGroupViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatientFilesGroupViewModel"/> class.
        /// </summary>
        public PatientFilesGroupViewModel()
        {
            this.Files = new List<PatientFileViewModel>(20);
        }

        public string Index { get; set; }

        /// <summary>
        /// Gets or sets the id of the files group when it already exists. If it is a new files group, this is null.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the Id of the patient associated with this files group.
        /// </summary>
        public int? PatientId { get; set; }

        /// <summary>
        /// Gets or sets the date of creation of this files group.
        /// </summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the title of the files group.
        /// </summary>
        [Display(Name = "Title")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets additional notes about this files group.
        /// </summary>
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the date these files were originated, that is, when they were first created.
        /// </summary>
        [Display(Name = "Files date")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data relacionado ao conteúdo dos arquivos. Por exemplo, se forem imagems, indica a data em que estas foram produzidas.")]
        public DateTime? FileGroupDate { get; set; }

        /// <summary>
        /// Gets or sets the date these files were received, that is, when the doctor got them.
        /// </summary>
        [Display(Name = "Receive date")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("Data de recebimento dos arquivos. Indica quando estes foram anexados ao prontuário do paciente.")]
        public DateTime? ReceiveDate { get; set; }

        /// <summary>
        /// Gets the list of files in the current files group.
        /// </summary>
        public List<PatientFileViewModel> Files { get; private set; }

        /// <summary>
        /// Gets or sets the GUID of the new patient file group, if it is new, otherwise null.
        /// </summary>
        public Guid? NewGuid { get; set; }
    }
}
