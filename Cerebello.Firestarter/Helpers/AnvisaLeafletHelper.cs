using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Cerebello.Model;
using CerebelloWebRole.Code;
using HtmlAgilityPack;

namespace Cerebello.Firestarter.Helpers
{
    // ReSharper disable LocalizableElement
    public static class AnvisaLeafletHelper
    {
        public static void DownloadAndCreateMedicinesJson()
        {
            var medicines = GetMedicines();

            using (var tw = new StreamWriter("medicines.json"))
            {
                tw.WriteLine(new JavaScriptSerializer().Serialize(medicines));
            }
        }

        private static List<MedicineRaw> GetMedicines()
        {
            var page1 = GetResponseText(1);
            var document1 = new HtmlDocument();
            document1.LoadHtml(page1);
            var linkFim = document1.DocumentNode.SelectSingleNode("//a[contains(text(), 'Fim')]");
            var hrefLinkFim = linkFim.GetAttributeValue("href", "");
            var regexCountPages = int.Parse(Regex.Match(hrefLinkFim, @"(?<=&pagina=)\d+").Value);

            var listTasks = new Task<string>[regexCountPages];
            listTasks[0] = new Task<string>(() => page1);
            for (int pagina = 2; pagina <= regexCountPages; pagina++)
                listTasks[pagina - 1] = new Task<string>(GetResponseText, pagina);

            for (int pagina = 0; pagina < regexCountPages; pagina++)
                listTasks[pagina].Start();

            Task.WaitAll(listTasks.OfType<Task>().ToArray());

            string[] pages = listTasks.Select(t => t.Result).ToArray();

            var medicines = new List<MedicineRaw>();

            foreach (var eachPage in pages)
            {
                var responseText = eachPage;

                var document = new HtmlDocument();
                document.LoadHtml(responseText);

                var trs = document.DocumentNode.SelectNodes("//*[@class='grid']/tr");

                foreach (var tr in trs.Skip(1).Reverse().Skip(1).Reverse())
                {
                    var medicine = new MedicineRaw
                        {
                            ActiveIngredient = tr.ChildNodes[0].InnerText.Trim(),
                            Name = Regex.Match(tr.ChildNodes[1].InnerText, @"[^\(]*").Value.Trim(),
                            Laboratory = Regex.Match(tr.ChildNodes[1].InnerText, @"\((.*)\)").Groups[1].Value.Trim(),
                            Concentration = tr.ChildNodes[2].InnerText.Trim(),
                            LeafletType = tr.ChildNodes[3].InnerText.Trim(),
                            Category = tr.ChildNodes[4].InnerText.Trim(),
                            LeafletUrl = tr.ChildNodes[5].ChildNodes[0].Attributes["href"].Value.Trim(),
                            // todo: commented code: this data has moved to a child page... is this really important?
                            //ApprovementDate = Convert.ToDateTime(tr.ChildNodes[6].InnerText.Trim(), CultureInfo.GetCultureInfo("pt-BR")),
                        };

                    medicines.Add(medicine);

                    Console.WriteLine("Captured: {0}", medicine.Name);
                }

                if (trs.Count - 2 < 20)
                    break;
            }
            return medicines;
        }

        private static string GetResponseText(object pagina)
        {
            return GetResponseText((int)pagina);
        }

        private static string GetResponseText(int pagina)
        {
            for (int itTry = 0; itTry <= 10; itTry++)
                try
                {
                    var request =
                        WebRequest.Create(
                            String.Format(
                                "http://www4.anvisa.gov.br/BularioEletronico/default.asp?txtPrincipioAtivo=&txtMedicamento=&txtEmpresa=&HidLetra=&HidTipo=todos&vOrdem=&tp_bula=&vclass=&pagina={0}",
                                pagina));
                    request.Method = "GET";
                    var response = request.GetResponse();
                    var responseStream = response.GetResponseStream();
                    Debug.Assert(responseStream != null, "responseStream must not be null");
                    var responseText = new StreamReader(responseStream, Encoding.Default).ReadToEnd();
                    Console.WriteLine("got page: {0}", pagina);
                    return responseText;
                }
                catch
                {
                    if (itTry == 10) throw;
                }
            return null;
        }

        public static void SaveLeafletsInMedicinesJsonToDb(CerebelloEntities db, Action<int, int> progress = null)
        {
            var medicines = new JavaScriptSerializer()
                .Deserialize<List<MedicineRaw>>(File.ReadAllText("medicines.json"));

            SaveLeafletsToDb(db, medicines, progress);
        }

        private static void SaveLeafletsToDb(CerebelloEntities db, List<MedicineRaw> medicines, Action<int, int> progress = null)
        {
            db.ExecuteStoreCommand("delete from SYS_MedicineActiveIngredient");
            db.ExecuteStoreCommand("delete from SYS_ActiveIngredient");
            db.ExecuteStoreCommand("delete from SYS_Medicine");
            db.ExecuteStoreCommand("delete from SYS_Laboratory");

            int count, max;
            {
                // INGREDIENTS
                Console.WriteLine("SYS_ActiveIngredient");
                var activeIngredientNames =
                    (from m in medicines
                     from ai in m.ActiveIngredient.Split('+')
                     select
                         StringHelper.CapitalizeFirstLetters(
                             Regex.Replace(ai.Trim(), @"\s+", " "),
                             new[] { "a", "o", "ao", "e", "de", "da", "do", "as", "as", "os", "das", "dos", "des" }))
                        .Distinct()
                        .ToList();

                count = 0;
                max = activeIngredientNames.Count;
                foreach (var activeIngredientName in activeIngredientNames)
                {
                    if (progress != null) progress(count, max);

                    if (!string.IsNullOrEmpty(activeIngredientName) && activeIngredientName != "-")
                        db.SYS_ActiveIngredient.AddObject(new SYS_ActiveIngredient { Name = activeIngredientName });

                    if (count % 100 == 0)
                        db.SaveChanges();

                    count++;
                }
                if (progress != null) progress(count, max);

                // LABORATORIES
                Console.WriteLine("SYS_Laboratory");
                var laboratoryNames = (from m in medicines.ToList() select m.Laboratory).Distinct().ToList();

                count = 0;
                max = laboratoryNames.Count;
                foreach (var laboratoryName in laboratoryNames)
                {
                    if (progress != null) progress(count, max);

                    if (!string.IsNullOrEmpty(laboratoryName) && laboratoryName != "-")
                        db.SYS_Laboratory.AddObject(new SYS_Laboratory { Name = laboratoryName });

                    if (count % 100 == 0)
                        db.SaveChanges();

                    count++;
                }
                if (progress != null) progress(count, max);
            }

            db.SaveChanges();

            // MEDICINES
            Console.WriteLine("SYS_Medicine");
            var medicinesList = (from m in medicines group m by new { m.Name, m.Concentration })
                .Where(mg => mg.Key.Name != "-")
                .ToList();

            count = 0;
            max = medicinesList.Count;
            foreach (var medicinesGrouped in medicinesList)
            {
                if (progress != null) progress(count, max);

                var medicineName = medicinesGrouped.Key.Name;

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
                             new[] { "a", "o", "ao", "e", "de", "da", "do", "as", "as", "os", "das", "dos", "des" }))
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

                count++;
            }
            if (progress != null) progress(count, max);

            db.SaveChanges();
        }
    }
    // ReSharper restore LocalizableElement
}
