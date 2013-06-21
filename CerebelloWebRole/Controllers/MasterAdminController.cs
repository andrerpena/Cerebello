using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using CerebelloWebRole.WorkerRole.Code.Workers;

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

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult GenerateInvoice(GenerateInvoiceViewModel viewModel)
        {
            if (viewModel.Step == null && string.IsNullOrEmpty(viewModel.PracticeIdentifier))
            {
                this.ModelState.Clear();
                return this.View();
            }

            var step = viewModel.Step ?? 0;

            using (var db = this.CreateNewCerebelloEntities())
            {
                // Getting the practice indicated in the view-model.
                Practice practice = null;
                if (!string.IsNullOrWhiteSpace(viewModel.PracticeIdentifier))
                {
                    practice = db.Practices
                        .SingleOrDefault(p => p.UrlIdentifier == viewModel.PracticeIdentifier);

                    if (practice == null)
                        this.ModelState.AddModelError(() => viewModel.PracticeIdentifier, "Consultório inexistente.");

                    if (practice != null && practice.AccountContract.IsTrial)
                        this.ModelState.AddModelError(() => viewModel.PracticeIdentifier, "Consultório possui conta trial.");
                }

                if (practice == null || this.ModelState.HasPropertyErrors(() => viewModel.PracticeIdentifier))
                {
                    // the current step failed
                    this.ModelState.ClearPropertyErrors(() => viewModel.Amount);
                    this.ModelState.ClearPropertyErrors(() => viewModel.DueDate);

                    return this.View(viewModel);
                }

                var utcNow = this.GetUtcNow();
                viewModel.MissingInvoices = GetAccountMissingInvoices(practice, utcNow);

                if (step == 0)
                {
                    if (this.Request.HttpMethod == "POST")
                        return this.RedirectToAction("GenerateInvoice", new { viewModel.PracticeIdentifier });

                    // going to the next step
                    this.ModelState.Clear();

                    viewModel.Step = 1;
                    return this.View(viewModel);
                }

                var localNow = PracticeController.ConvertToLocalDateTime(practice, utcNow);

                if (viewModel.DueDate != null)
                {
                    if (practice.AccountContract.BillingDueDay != viewModel.DueDate.Value.Day)
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

                if (this.Request.HttpMethod == "POST"
                    && (viewModel.DueDate == null || this.ModelState.HasPropertyErrors(() => viewModel.DueDate)))
                {
                    // the current step failed
                    this.ModelState.ClearPropertyErrors(() => viewModel.PracticeIdentifier);

                    return this.View(viewModel);
                }

                if (this.Request.HttpMethod == "POST" && step == 1)
                {
                    // going to the next step
                    this.ModelState.Clear();

                    viewModel.Step = 2;
                    return this.View(viewModel);
                }

                Billing billing = null;
                if (viewModel.DueDate != null)
                {
                    var idSet = string.Format(
                        "CEREB.{1}{2}.{0}",
                        localNow.Year,
                        practice.AccountContract.BillingPeriodSize,
                        practice.AccountContract.BillingPeriodType);

                    billing = new Billing
                        {
                            PracticeId = practice.Id,
                            AfterDueMonthlyTax = 1.00m, // 1%
                            AfterDueTax = 2.00m, // 2%
                            IssuanceDate = utcNow,
                            MainAmount = viewModel.Amount ?? 0m,
                            DueDate = PracticeController.ConvertToUtcDateTime(practice, viewModel.DueDate.Value),
                            IdentitySetName = idSet,
                            IdentitySetNumber = db.Billings.Count(b => b.PracticeId == practice.Id && b.IdentitySetName == idSet) + 1,
                            //ReferenceDate = 
                        };
                }

                if (practice.AccountContract.BillingPaymentMethod == "PayPal Invoice")
                    this.ViewBag.IsPayPalInvoice = true;

                if (this.ModelState.IsValid && billing != null && viewModel.DueDate != null && viewModel.Amount != null)
                {
                    db.Billings.AddObject(billing);
                    db.SaveChanges();

                    // adding PayPal invoice info
                    viewModel.Invoice = new GenerateInvoiceViewModel.PayPalInvoiceInfo
                        {
                            UserEmail = practice.Owner.Person.Email,
                            IssuanceDate = localNow.ToString("dd-MM-yyyy"),
                            Currency = "BRL - Reais",
                            Number = string.Format("{0}.{1}", billing.IdentitySetName, billing.IdentitySetNumber),
                            DuaDate = viewModel.DueDate.Value.ToString("dd-MM-yyyy"),
                            Terms = "Vencimento na data especificada",
                            Items = new List<GenerateInvoiceViewModel.PayPalInvoiceItem>
                                {
                                    new GenerateInvoiceViewModel.PayPalInvoiceItem
                                        {
                                            NameId = "Assinatura Cerebello",
                                            Date = "",
                                            Quantity = 1,
                                            UnitPrice = viewModel.Amount.Value.ToString("0.00"),
                                            Description = string.Format("Assinatura do plano profissional do Cerebello (01/01/2013 até 01/04/2013)")
                                        }
                                }
                        };

                    viewModel.Step = 3;
                    return this.View(viewModel);
                }
            }

            return this.View(viewModel);
        }

        private static List<BillingInfo> GetAccountMissingInvoices(Practice practice, DateTime utcNow)
        {
            // getting info about the services that are being used, that is all active AccountContracts
            var activeAccountContracts = new[] { practice.AccountContract }
                .Concat(
                    practice.AccountContracts
                        .Where(ac => ac.EndDate == null || ac.EndDate >= utcNow)
                        .Where(ac => ac.Id != practice.AccountContract.Id))
                .ToList();

            // The main account contract dictates the billing periods of subcontracts.
            // We must see what products (active account contracts) don't have
            // billing items in any period.
            var mainIntervals = GetAccountContractBillingCycles(practice.AccountContract)
                .TakeWhile(it => it.Start < utcNow)
                .ToList();

            var dateSetBillingCycles = new ContinuousSet<DateTime>(mainIntervals.Count * 2);
            foreach (var dateTimeInterval in mainIntervals)
                dateSetBillingCycles.AddInterval(dateTimeInterval.Start, true, dateTimeInterval.End, false);
            dateSetBillingCycles.Flatten(mergeRedundantTrues: false);

            var billingsDic = new Dictionary<DateTime, BillingInfo>();
            foreach (var activeAccountContract in activeAccountContracts)
            {
                // Getting intervals that already have invoices.
                var dateSetBillings = new ContinuousSet<DateTime>();
                foreach (var eachBilling in activeAccountContract.Billings)
                {
                    if (eachBilling.ReferenceDateEnd.HasValue)
                        dateSetBillings.AddInterval(eachBilling.ReferenceDate, true, eachBilling.ReferenceDateEnd.Value, false);
                    else
                        dateSetBillings.AddPoint(eachBilling.ReferenceDate, false, true, true);
                }

                dateSetBillings.Flatten();

                // Merging date sets, to see where there are holes without any invoice.
                var dateSetResult = !dateSetBillings & dateSetBillingCycles;

                // Getting the intervals that represents missing invoices.
                foreach (var eachMissingInterval in dateSetResult.PositiveIntervals)
                {
                    BillingInfo billingInfo;
                    if (!billingsDic.TryGetValue(eachMissingInterval.Start, out billingInfo))
                        billingsDic[eachMissingInterval.Start] = billingInfo = new BillingInfo();

                    billingInfo.Start = eachMissingInterval.Start;
                    billingInfo.End = eachMissingInterval.End;
                    billingInfo.DueDate = DateTimeHelper.Range(billingInfo.Start.AddDays(10), 31, d => d.AddDays(1))
                        .Single(d => d.Day == practice.AccountContract.BillingDueDay);
                    billingInfo.Items.Add(
                        new BillingInfo.Item
                            {
                                Value = activeAccountContract.BillingAmount ?? 0 - activeAccountContract.BillingDiscountAmount ?? 0,
                                ContractType = (ContractTypes)activeAccountContract.ContractTypeId,
                            });
                }
            }

            var result = billingsDic.Values.OrderBy(bi => bi.Start).ToList();
            return result;
        }

        private static IEnumerable<DateTimeInterval> GetAccountContractBillingCycles(AccountContract accountContract)
        {
            var start = accountContract.StartDate;
            var periodType = accountContract.BillingPeriodType;
            var periodSize = accountContract.BillingPeriodSize ?? 1;
            var periodCount = accountContract.BillingPeriodCount ?? 7305; // = 365.25 * 20

            var mainIntervals = DateTimeHelper.IntervalRange(
                start,
                periodCount,
                d =>
                {
                    if (periodType == "M") return d.AddMonths(periodSize, start.Day);
                    if (periodType == "d") return d.AddDays(periodSize);
                    if (periodType == "y") return d.AddYears(periodSize);
                    throw new Exception("Unknown period type.");
                });

            return mainIntervals;
        }

        public static DateTime GetBillingDueDate(DateTime localNow, DateTime currentBillingCycleStart, AccountContract accountContract, int minDaysBeforeDueDate = 10)
        {
            if (accountContract.BillingDueDay == null)
                throw new ArgumentException("accountContract.BillingDueDay must not be null");

            if (accountContract.BillingPeriodIndex == null)
                throw new ArgumentException("accountContract.BillingPeriodIndex must not be null");

            var dueDay = accountContract.BillingDueDay.Value;
            var index = accountContract.BillingPeriodIndex.Value;

            if (accountContract.BillingPeriodType == "M")
            {
                // getting next day that is the same as the indicated in dueDay
                var nextDate = currentBillingCycleStart.Date.AddDays(dueDay - currentBillingCycleStart.Day);
                if (nextDate < currentBillingCycleStart) index++;
                nextDate = nextDate.AddMonths(index, dueDay);

                if (localNow <= nextDate && nextDate < localNow.Date.AddDays(minDaysBeforeDueDate))
                    nextDate = nextDate.AddMonths(1, dueDay);

                return nextDate;
            }
            else
            {
                throw new NotImplementedException("Support for this BillingPeriodType is not implemented yet.");
            }
        }

        public ActionResult ViewBinFolder()
        {
            var binFolder = this.Server.MapPath("~/bin");
            var folderContents = Directory.GetFiles(binFolder).Aggregate("", (current, file) => current + (file + Environment.NewLine));
            return this.Content(folderContents, "text/plain");
        }

        public string StartFullBackup()
        {
            var errors = new List<string>();
            using (var db = new CerebelloEntities())
            {
                foreach (var patient in db.Patients)
                    patient.IsBackedUp = false;
                db.SaveChanges();

                BackupHelper.BackupEverything(db, errors);
            }

            return "Errors : " + string.Join(",", errors);
        }

        public ActionResult Log(LogViewModel formModel)
        {
            if (!string.IsNullOrEmpty(formModel.Message))
                Trace.TraceInformation("MasterAdminController.Log(formModel): " + formModel.Message);

            var levels = formModel.GetLevels();
            IEnumerable<WindowsAzureLogHelper.TraceLogsEntity> logsSource = WindowsAzureLogHelper.GetLogEvents(
                formModel.Page ?? 1, levels.ToArray(), formModel.FilterRoleInstance, formModel.FilterPath);

            var logs = logsSource
                .Select(
                    item => new LogViewModel.TraceLogItem(item.Message)
                        {
                            Timestamp = DateTime.Parse(item.Timestamp),
                            Level = item.Level,
                            RoleInstance = item.RoleInstance,
                            Role = item.Role,
                        });

            if (formModel.HasPathFilter())
                logs = logs.Where(l => l.Path.StartsWith(formModel.FilterPath));

            if (formModel.HasSourceFilter())
                logs = logs.Where(l => l.Source.Equals(formModel.FilterSource, StringComparison.InvariantCultureIgnoreCase));

            if (formModel.HasTextFilter())
                logs = logs.Where(l => l.Text.IndexOf(formModel.FilterText, StringComparison.InvariantCultureIgnoreCase) > 0);

            if (formModel.HasRoleInstanceFilter())
                logs = logs.Where(l => formModel.FilterRoleInstance == l.RoleInstance);

            if (formModel.HasLevelFilter())
                logs = logs.Where(l => levels.Contains(l.Level));

            if (formModel.HasSpecialFilter())
                logs = logs.Where(l => l.SpecialStrings.Any(s => formModel.FilterSpecial.Contains(s)));

            if (formModel.HasRoleFilter())
                logs = logs.Where(l => formModel.FilterRole == l.Role);

            if (formModel.HasTimestampFilter())
            {
                if (formModel.FilterTimestamp == "now")
                {
                    var vm = formModel.Clone();
                    vm.FilterLevel = null;
                    var q = UrlBuilder.GetListQueryParams("FilterSpecial", vm.FilterSpecial);
                    vm.FilterTimestamp = this.GetUtcNow().ToString("yyyy'-'MM'-'dd'-'HH'-'mm");

                    return this.Redirect(UrlBuilder.AppendQuery(this.Url.Action("Log", vm), q));
                }

                var date =
                    DateTime.Parse(
                        Regex.Replace(
                            formModel.FilterTimestamp,
                            @"(\d{4})\-(\d{2})\-(\d{2})\-(\d{2})\-(\d{2})",
                            @"$1-$2-$3T$4:$5:00"));

                logs = logs.Where(l => l.Timestamp > date);
            }

            formModel.Logs = logs.ToList();

            return this.View(formModel);
        }

        public ActionResult ThrowException()
        {
            throw new Exception("THIS IS AN INTENTIONAL ERROR TO TEST LOGGING");
        }
    }
}
