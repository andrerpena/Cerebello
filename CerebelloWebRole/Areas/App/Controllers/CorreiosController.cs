using System;
using System.IO;
using System.Net;
using System.Web.Mvc;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using HtmlAgilityPack;

namespace CerebelloWebRole.Areas.App.Controllers
{
    // note: Correios is the company name
    public class CorreiosController : PracticeController
    {
        public JsonResult GetCepInfo(string cep)
        {
            // TODO: Miguel Angelo: copiei este código para o UsersController, deveria ser um método utilitário, ou criar uma classe de base.

            try
            {
                var request = WebRequest.Create("http://www.buscacep.correios.com.br/servicos/dnec/consultaEnderecoAction.do");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                using (StreamWriter requestWriter = new StreamWriter(request.GetRequestStream()))
                    requestWriter.Write(String.Format("relaxation={0}&TipoCep=ALL&semelhante=N&cfm=1&Metodo=listaLogradouro&TipoConsulta=relaxation&StartRow=1&EndRow=10", cep));

                var response = request.GetResponse();

                HtmlDocument document = new HtmlDocument();
                document.Load(response.GetResponseStream());

                CEPInfo cepInfo = new CEPInfo()
                {
                    Street = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[1]").InnerText,
                    Neighborhood = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[2]").InnerText,
                    City = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[3]").InnerText,
                    StateProvince = document.DocumentNode.SelectSingleNode("//*[@id='lamina']/div[2]/div[2]/div[2]/div/table[1]/tr/td[4]").InnerText
                };

                return Json(cepInfo, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                this.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return null;
            }
        }
    }
}
