using System.Web.Mvc;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Areas.Docs.Controllers
{
    public class HomeDocsController : CerebelloSiteController
    {
        //
        // GET: /Docs/HomeDocs/

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult CreatingAnAccount()
        {
            return this.View();
        }

        public ActionResult ConfiguringDocuments()
        {
            return this.View();
        }

        public ActionResult ManagingUsers()
        {
            return this.View();
        }
        
        public ActionResult ConfiguringTheSchedule()
        {
            return this.View();
        }

        public ActionResult RegisteringNewAppointments()
        {
            return this.View();
        }

        public ActionResult ConsultingTheSchedule()
        {
            return this.View();
        }

        public ActionResult TheAppointmentLine()
        {
            return this.View();
        }

        public ActionResult RegisteringPatients()
        {
            return this.View();
        }

        public ActionResult PerformingAnAppointment()
        {
            return this.View();
        }

        public ActionResult RegisteringMedicines()
        {
            return this.View();
        }

        public ActionResult RegisteringHealthInsurances()
        {
            return this.View();
        }

        public ActionResult RegisteringMedicalCertificates()
        {
            return this.View();
        }

        public ActionResult ReferenceFields()
        {
            return this.View();
        }
    }
}
