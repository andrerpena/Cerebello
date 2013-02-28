using System;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Models;
using CerebelloWebRole.WorkerRole.Code.Workers;
using System.Linq;
using CerebelloWebRole.Code;

namespace CerebelloWebRole.Controllers
{
    public class MasterAdminController : RootController
    {
        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult SendReminderEmails()
        {
            using (this.CreateNewCerebelloEntities())
            {
                EmailSenderWorker emailSender;
                try
                {
                    emailSender = new EmailSenderWorker();
                    emailSender.RunOnce();
                }
                catch (Exception ex)
                {
                    return this.Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
                }

                return this.Json(new { success = true, message = "E-mails enviados com sucesso: " + emailSender.EmailsCount }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GenerateInvoice()
        {
            this.ViewBag.OnlyPractice = true;
            return this.View();
        }

        [HttpPost]
        public ActionResult GenerateInvoice(GenerateInvoiceViewModel viewModel)
        {
            using (var db = this.CreateNewCerebelloEntities())
            {
                Practice practice = null;
                if (!string.IsNullOrWhiteSpace(viewModel.PracticeIdentifier))
                {
                    practice = db.Practices
                        .SingleOrDefault(p => p.UrlIdentifier == viewModel.PracticeIdentifier);

                    if (practice == null)
                        this.ModelState.AddModelError(viewModel.PracticeIdentifier, "Consultório inexistente.");
                }

                if (this.ModelState.HasPropertyErrors(() => viewModel.PracticeIdentifier))
                {
                    this.ModelState.ClearPropertyErrors(() => viewModel.Amount);
                    this.ModelState.ClearPropertyErrors(() => viewModel.DueDate);

                    this.ViewBag.OnlyPractice = true;
                    return this.View(viewModel);
                }

                var utcNow = this.GetUtcNow();
                var localNow = PracticeController.ConvertToLocalDateTime(practice, utcNow);

                var fillFields = this.Request.Params["FillFields"] == "true";

                if (fillFields)
                {
                    this.ModelState.Clear();

                    viewModel.Amount = practice.AccountContract.BillingAmount;
                    viewModel.DueDate = PracticeController.ConvertToLocalDateTime(practice, utcNow).Date;
                }

                if (viewModel.DueDate != null)
                {
                    if (practice != null && practice.AccountContract.BillingDueDay != viewModel.DueDate.Value.Day)
                    {
                        this.ModelState.AddModelError(
                              () => viewModel.DueDate,
                              "Dia da data de vencimento deve ser {0}",
                              practice.AccountContract.BillingDueDay);
                    }

                    if (viewModel.DueDate < localNow.Date)
                    {
                        this.ModelState.AddModelError(
                            () => viewModel.DueDate,
                            "Data de vencimento está no passado");
                    }
                    else if (viewModel.DueDate < localNow.Date.AddDays(10))
                    {
                        this.ModelState.AddModelError(
                          () => viewModel.DueDate,
                          "Data de vencimento não pode estar nos próximos 10 dias");
                    }
                }

                Billing billing = null;
                if (viewModel.DueDate != null)
                {
                    billing = new Billing
                        {
                            PracticeId = practice.Id,
                            AfterDueMonthlyTax = 1.00m, // 1%
                            AfterDueTax = 2.00m, // 2%
                            IssuanceDate = this.GetUtcNow(),
                            Amount = viewModel.Amount ?? 0m,
                            DueDate = PracticeController.ConvertToUtcDateTime(practice, viewModel.DueDate.Value),
                        };
                }

                if (practice.AccountContract.BillingPaymentMethod == "PayPal Invoice")
                    this.ViewBag.IsPayPalInvoice = true;

                if (this.ModelState.IsValid && !fillFields && billing != null)
                {
                    db.Billings.AddObject(billing);
                    db.SaveChanges();

                    this.ViewBag.BillingOk = true;
                    return this.View(viewModel);
                }
            }

            return this.View(viewModel);
        }
    }
}
