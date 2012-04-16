using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Leaflet.Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            List<MedicineRaw> medicines = new List<MedicineRaw>();

            for (int pagina = 1; ; pagina++)
            {
                Console.WriteLine("getting page: " + pagina);

                var request = HttpWebRequest.Create(String.Format("http://www4.anvisa.gov.br/BularioEletronico/default.asp?txtPrincipioAtivo=&txtMedicamento=&txtEmpresa=&HidLetra=&HidTipo=todos&vOrdem=&tp_bula=&vclass=&pagina={0}", pagina));
                request.Method = "GET";
                var response = request.GetResponse();
                var responseText = new StreamReader(response.GetResponseStream(), Encoding.Default).ReadToEnd();
                
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(responseText);

                var trs = document.DocumentNode.SelectNodes("//*[@class='grid']/tr");

                foreach (var tr in trs.Skip(1).Reverse().Skip(1).Reverse())
                {
                    MedicineRaw medicine = new MedicineRaw();

                    medicine.ActiveIngredient = tr.ChildNodes[0].InnerText.Trim();
                    medicine.Name = Regex.Match(tr.ChildNodes[1].InnerText, @"[^\(]*").Value.Trim();
                    medicine.Laboratory = Regex.Match(tr.ChildNodes[1].InnerText, @"\((.*)\)").Groups[1].Value.Trim();
                    medicine.Concentration = tr.ChildNodes[2].InnerText.Trim();
                    medicine.LeafletType = tr.ChildNodes[3].InnerText.Trim();
                    medicine.Category = tr.ChildNodes[4].InnerText.Trim();
                    medicine.LeafletUrl = tr.ChildNodes[5].ChildNodes[0].Attributes["href"].Value.Trim();
                    medicine.ApprovementDate = Convert.ToDateTime(tr.ChildNodes[6].InnerText.Trim(), CultureInfo.GetCultureInfo("pt-BR"));

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
    }
}
