using System.ComponentModel.DataAnnotations;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.App.Models
{
    public class ReceiptMedicineViewModel
    {
        public int? Id { get; set; }

        [Display(Name = "Medicamento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        [Tooltip("É necessário selecionar um medicamento cadastrado da lista. Ao começar a digitar, o sistema irá sugerir medicamentos. Caso o medicamento procurado não possa ser encontrado, clique no botão +. É possível cadastrar um medicamento novo ou importar um do banco da Anvisa")]
        public int? MedicineId { get; set; }

        [Display(Name = "Medicamento")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string MedicineText { get; set; }

        [Display(Name="Quantidade")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Quantity { get; set; }

        [Display(Name = "Prescrição")]
        [Required(ErrorMessageResourceType = typeof(ModelStrings), ErrorMessageResourceName = "RequiredValidationMessage")]
        public string Prescription { get; set; }

        [Display(Name = "Observações")]
        public string Observations { get; set; }
    }
}
