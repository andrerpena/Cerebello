using System;
using System.Collections.Concurrent;
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
using File = System.IO.File;

namespace Cerebello.Firestarter.Helpers
{
    // ReSharper disable LocalizableElement
    public class AnvisaLeafletHelper
    {
        private readonly ConcurrentDictionary<int, string> cacheOfGetResponseText = new ConcurrentDictionary<int, string>();

        public List<MedicineRaw> DownloadAndCreateMedicinesJson()
        {
            List<MedicineRaw> medicines = this.GetMedicines();

            using (var tw = new StreamWriter("medicines.json"))
            {
                tw.WriteLine(new JavaScriptSerializer().Serialize(medicines));
            }

            return medicines;
        }

        private List<MedicineRaw> GetMedicines()
        {
            string page1 = this.GetResponseTextWithCache(1);
            var document1 = new HtmlDocument();
            document1.LoadHtml(page1);
            HtmlNode linkFim = document1.DocumentNode.SelectSingleNode("//a[contains(text(), 'Fim')]");
            string hrefLinkFim = linkFim.GetAttributeValue("href", "");
            int regexCountPages = int.Parse(Regex.Match(hrefLinkFim, @"(?<=&pagina=)\d+").Value);

            var listTasks = new Task<List<MedicineRaw>>[regexCountPages];
            for (int pagina = 0; pagina < regexCountPages; pagina++)
                listTasks[pagina] = new Task<List<MedicineRaw>>(this.GetItems, pagina + 1);

            foreach (var eachTask in listTasks)
                eachTask.Start();

            Task.WaitAll(listTasks.OfType<Task>().ToArray());

            List<MedicineRaw> medicines = listTasks.SelectMany(t => t.Result).ToList();

            return medicines;
        }

        private List<MedicineRaw> GetItems(object pagina)
        {
            return this.GetItems((int)pagina);
        }

        private List<MedicineRaw> GetItems(int pagina)
        {
            HtmlNodeCollection trs;
            while (true)
            {
                var document = new HtmlDocument();
                string responseText = this.GetResponseTextWithCache(pagina);
                document.LoadHtml(responseText);

                trs = document.DocumentNode.SelectNodes("//*[@class='grid']/tr");

                if (trs.Count > 2)
                    break;
            }

            var medicines = new List<MedicineRaw>(20);

            var trs2 = trs.Skip(1).Reverse().Skip(1).Reverse().ToArray();
            foreach (HtmlNode tr in trs2)
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

                //Console.WriteLine("Captured: {0}", medicine.Name);
            }

            Console.WriteLine("got page {0}, with {1} items", pagina, trs2.Length);

            return medicines;
        }

        private string GetResponseTextWithCache(int pagina)
        {
            string result = this.cacheOfGetResponseText.GetOrAdd(pagina, GetResponseText);
            return result;
        }

        private static string GetResponseText(int pagina)
        {
            for (int itTry = 0; itTry <= 10; itTry++)
                try
                {
                    WebRequest request =
                        WebRequest.Create(
                            String.Format(
                                "http://www4.anvisa.gov.br/BularioEletronico/default.asp?txtPrincipioAtivo=&txtMedicamento=&txtEmpresa=&HidLetra=&HidTipo=todos&vOrdem=&tp_bula=&vclass=&pagina={0}",
                                pagina));

                    request.Method = "GET";
                    WebResponse response = request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    Debug.Assert(responseStream != null, "responseStream must not be null");
                    string responseText = new StreamReader(responseStream, Encoding.Default).ReadToEnd();
                    return responseText;
                }
                catch
                {
                    if (itTry == 10) throw;
                }
            return null;
        }

        public void SaveLeafletsInMedicinesJsonToDb(CerebelloEntities db, Action<int, int> progress = null)
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
                List<string> activeIngredientNames =
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
                foreach (string activeIngredientName in activeIngredientNames)
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
                List<string> laboratoryNames = (from m in medicines.ToList() select m.Laboratory).Distinct().ToList();

                count = 0;
                max = laboratoryNames.Count;
                foreach (string laboratoryName in laboratoryNames)
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

                string medicineName = medicinesGrouped.Key.Name;

                var medicine = new SYS_Medicine
                    {
                        Name = medicineName + " (" + medicinesGrouped.Key.Concentration + ")"
                    };

                // associating active ingredients with medicine
                List<string> activeIngredientNames =
                    (from ai in medicinesGrouped.ElementAt(0).ActiveIngredient.Split('+')
                     select
                         StringHelper.CapitalizeFirstLetters(
                             Regex.Replace(ai.Trim(), @"\s+", " "),
                             new[] { "a", "o", "ao", "e", "de", "da", "do", "as", "as", "os", "das", "dos", "des" }))
                        .ToList();
                activeIngredientNames = activeIngredientNames
                    .Where(ain => !string.IsNullOrEmpty(ain) && ain != "-")
                    .ToList();
                foreach (string ain in activeIngredientNames)
                {
                    SYS_ActiveIngredient activeIngredient = db.SYS_ActiveIngredient.First(ai => ai.Name == ain);
                    medicine.ActiveIngredients.Add(activeIngredient);
                }

                // associating the medicine with the laboratory
                if (!string.IsNullOrEmpty(medicinesGrouped.ElementAt(0).Laboratory))
                {
                    string laboratoryName = medicinesGrouped.ElementAt(0).Laboratory;
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
