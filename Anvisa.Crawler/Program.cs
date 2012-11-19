using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using HtmlAgilityPack;

namespace Leaflet.Crawler
{
    class Program
    {
        static void Main(string[] args)
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

            Task.WaitAll(listTasks);

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
                    var medicine = new MedicineRaw();

                    medicine.ActiveIngredient = tr.ChildNodes[0].InnerText.Trim();
                    medicine.Name = Regex.Match(tr.ChildNodes[1].InnerText, @"[^\(]*").Value.Trim();
                    medicine.Laboratory = Regex.Match(tr.ChildNodes[1].InnerText, @"\((.*)\)").Groups[1].Value.Trim();
                    medicine.Concentration = tr.ChildNodes[2].InnerText.Trim();
                    medicine.LeafletType = tr.ChildNodes[3].InnerText.Trim();
                    medicine.Category = tr.ChildNodes[4].InnerText.Trim();
                    medicine.LeafletUrl = tr.ChildNodes[5].ChildNodes[0].Attributes["href"].Value.Trim();
                    // todo: commented code: this data has moved to a child page... is this really important?
                    //medicine.ApprovementDate = Convert.ToDateTime(tr.ChildNodes[6].InnerText.Trim(), CultureInfo.GetCultureInfo("pt-BR"));

                    medicines.Add(medicine);

                    Console.WriteLine("Captured: " + medicine.Name);
                }

                if (trs.Count - 2 < 20)
                    break;
            }

            using (var tw = new StreamWriter("medicines.json"))
            {
                tw.WriteLine(new JavaScriptSerializer().Serialize(medicines));
            }
        }

        private static string GetResponseText(object pagina)
        {
            return GetResponseText((int)pagina);
        }

        private static string GetResponseText(int pagina)
        {
            while (true)
                try
                {
                    var request =
                        WebRequest.Create(
                            String.Format(
                                "http://www4.anvisa.gov.br/BularioEletronico/default.asp?txtPrincipioAtivo=&txtMedicamento=&txtEmpresa=&HidLetra=&HidTipo=todos&vOrdem=&tp_bula=&vclass=&pagina={0}",
                                pagina));
                    request.Method = "GET";
                    var response = request.GetResponse();
                    var responseText = new StreamReader(response.GetResponseStream(), Encoding.Default).ReadToEnd();
                    Console.WriteLine("got page: {0}", pagina);
                    return responseText;
                }
                catch { }
        }
    }
}
