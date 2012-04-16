using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cerebello.Model;
using Leaflet.Crawler;
using System.Web.Script.Serialization;
using System.IO;
using CerebelloWebRole.Code;

namespace Leaflet.Processor
{
    public class Program
    {
        static void Main(string[] args)
        {
            List<MedicineRaw> medicines = new JavaScriptSerializer().Deserialize<List<MedicineRaw>>(new StreamReader("medicines.json").ReadToEnd());

            CerebelloEntities db = new CerebelloEntities();

            db.ExecuteStoreCommand("delete from SYS_MedicineActiveIngredient");
            db.ExecuteStoreCommand("delete from SYS_ActiveIngredient");
            db.ExecuteStoreCommand("delete from SYS_Medicine");
            db.ExecuteStoreCommand("delete from SYS_Laboratory");

            {
                // INGREDIENTS
                var activeIngredientNames = (from m in medicines from ai in m.ActiveIngredient.Split('+') select StringHelper.CapitalizeFirstLetters(Regex.Replace(ai.Trim(), @"\s+", " "), new string[] { "a", "o", "ao", "e", "de", "da", "do", "as", "as", "os", "das", "dos", "des" })).Distinct().ToList();

                foreach (var activeIngredientName in activeIngredientNames)
                    if (!string.IsNullOrEmpty(activeIngredientName) && activeIngredientName != "-")
                        db.SYS_ActiveIngredient.AddObject(new SYS_ActiveIngredient() { Name = activeIngredientName });

                // LABORATORIES
                var laboratoryNames = (from m in medicines.ToList() select m.Laboratory).Distinct().ToList();

                foreach (var laboratoryName in laboratoryNames)
                    if (!string.IsNullOrEmpty(laboratoryName) && laboratoryName != "-")
                        db.SYS_Laboratory.AddObject(new SYS_Laboratory() { Name = laboratoryName });
            }

            db.SaveChanges();

            // MEDICINES
            var medicinesList = (from m in medicines group m by new { m.Name, m.Concentration }).ToList();

            foreach (var medicinesGrouped in medicinesList)
            {
                var medicineName = medicinesGrouped.Key.Name;
                if (medicineName == "-")
                    continue;

                var medicine = new SYS_Medicine();

                // name
                medicine.Name = medicineName + " (" + medicinesGrouped.Key.Concentration + ")";

                // associating active ingredients with medicine
                var activeIngredientNames = (from ai in medicinesGrouped.ElementAt(0).ActiveIngredient.Split('+') select StringHelper.CapitalizeFirstLetters(Regex.Replace(ai.Trim(), @"\s+", " "), new string[] { "a", "o", "ao", "e", "de", "da", "do", "as", "as", "os", "das", "dos", "des" })).ToList();
                activeIngredientNames = activeIngredientNames.Where(ain => !string.IsNullOrEmpty(ain) && ain != "-").ToList();
                foreach (var ain in activeIngredientNames)
                {
                    var activeIngredient = db.SYS_ActiveIngredient.Where(ai => ai.Name == ain).First();
                    medicine.ActiveIngredients.Add(activeIngredient);
                }

                // associating the medicine with the laboratory
                if (!string.IsNullOrEmpty(medicinesGrouped.ElementAt(0).Laboratory))
                {
                    var laboratoryName = medicinesGrouped.ElementAt(0).Laboratory;
                    medicine.Laboratory = db.SYS_Laboratory.Where(l => l.Name == laboratoryName).First();
                }

                foreach (var leaflet in medicinesGrouped.Select(mg => new { Description = mg.LeafletType, Url = mg.LeafletUrl }))
                    medicine.Leaflets.Add(new SYS_Leaflet() { Description = leaflet.Description, Url = leaflet.Url });

                db.SYS_Medicine.AddObject(medicine);

                db.SaveChanges();
            }

            db.SaveChanges();
        }
    }
}
