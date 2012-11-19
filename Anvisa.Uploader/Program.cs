using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Code;
using Leaflet.Crawler;

namespace Leaflet.Processor
{
    public class Program
    {
        static void Main()
        {
            var medicines = new JavaScriptSerializer().Deserialize<List<MedicineRaw>>(new StreamReader("medicines.json").ReadToEnd());

            var db = new CerebelloEntities();

            db.ExecuteStoreCommand("delete from SYS_MedicineActiveIngredient");
            db.ExecuteStoreCommand("delete from SYS_ActiveIngredient");
            db.ExecuteStoreCommand("delete from SYS_Medicine");
            db.ExecuteStoreCommand("delete from SYS_Laboratory");

            {
                // INGREDIENTS
                var activeIngredientNames =
                    (from m in medicines
                     from ai in m.ActiveIngredient.Split('+')
                     select
                         StringHelper.CapitalizeFirstLetters(
                             Regex.Replace(ai.Trim(), @"\s+", " "),
                             new[] {"a", "o", "ao", "e", "de", "da", "do", "as", "as", "os", "das", "dos", "des"}))
                        .Distinct()
                        .ToList();

                foreach (var activeIngredientName in activeIngredientNames)
                    if (!string.IsNullOrEmpty(activeIngredientName) && activeIngredientName != "-")
                        db.SYS_ActiveIngredient.AddObject(new SYS_ActiveIngredient { Name = activeIngredientName });

                // LABORATORIES
                var laboratoryNames = (from m in medicines.ToList() select m.Laboratory).Distinct().ToList();

                foreach (var laboratoryName in laboratoryNames)
                    if (!string.IsNullOrEmpty(laboratoryName) && laboratoryName != "-")
                        db.SYS_Laboratory.AddObject(new SYS_Laboratory { Name = laboratoryName });
            }

            db.SaveChanges();

            // MEDICINES
            var medicinesList = (from m in medicines group m by new { m.Name, m.Concentration }).ToList();

            foreach (var medicinesGrouped in medicinesList)
            {
                var medicineName = medicinesGrouped.Key.Name;
                if (medicineName == "-")
                    continue;

                var medicine = new SYS_Medicine
                    {
                        Name = medicineName + " (" + medicinesGrouped.Key.Concentration + ")"
                    };

                // associating active ingredients with medicine
                var activeIngredientNames =
                    (from ai in medicinesGrouped.ElementAt(0).ActiveIngredient.Split('+')
                     select
                         StringHelper.CapitalizeFirstLetters(
                             Regex.Replace(ai.Trim(), @"\s+", " "),
                             new[] {"a", "o", "ao", "e", "de", "da", "do", "as", "as", "os", "das", "dos", "des"}))
                        .ToList();
                activeIngredientNames = activeIngredientNames
                    .Where(ain => !string.IsNullOrEmpty(ain) && ain != "-")
                    .ToList();
                foreach (var ain in activeIngredientNames)
                {
                    var activeIngredient = db.SYS_ActiveIngredient.First(ai => ai.Name == ain);
                    medicine.ActiveIngredients.Add(activeIngredient);
                }

                // associating the medicine with the laboratory
                if (!string.IsNullOrEmpty(medicinesGrouped.ElementAt(0).Laboratory))
                {
                    var laboratoryName = medicinesGrouped.ElementAt(0).Laboratory;
                    medicine.Laboratory = db.SYS_Laboratory.First(l => l.Name == laboratoryName);
                }

                foreach (var leaflet in medicinesGrouped.Select(mg => new { Description = mg.LeafletType, Url = mg.LeafletUrl }))
                    medicine.Leaflets.Add(new SYS_Leaflet { Description = leaflet.Description, Url = leaflet.Url });

                db.SYS_Medicine.AddObject(medicine);

                db.SaveChanges();
            }

            db.SaveChanges();
        }
    }
}
