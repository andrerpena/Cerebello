using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using PayPal.Version940;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [UserRolePermission(UserRoleFlags.Owner)]
    public class ConfigAccountController : PracticeController
    {
        private readonly IDateTimeService dateTimeService;

        public ConfigAccountController(IDateTimeService dateTimeService)
        {
            this.dateTimeService = dateTimeService;
        }

        public ActionResult Index()
        {
            // Getting the plan contract (that is the main contract)
            // other contracts can be attatched to the main,
            // like SMS, and additional support contracts.
            var mainContract = this.DbPractice.AccountContract;

            // because each plan has a different set of features,
            // this is going to be used by the view, to define the partial page that will be shown
            this.ViewBag.CurrentContractName = mainContract.SYS_ContractType.UrlIdentifier;

            if (mainContract.SYS_ContractType.IsTrial)
            {
                return this.View();
            }
            else
            {
                var viewModel = new ConfigAccountViewModel
                    {
                        CurrentContract = new ConfigAccountViewModel.Contract
                            {
                                DoctorsLimit = this.DbPractice.AccountContract.DoctorsLimit,
                            }
                    };

                var nowPlusXDays = this.dateTimeService.UtcNow.AddDays(Bus.Pro.MAX_DAYS_TO_PAY_BILLING);
                var nowLessXDays = this.dateTimeService.UtcNow.AddDays(-Bus.Pro.MAX_DAYS_TO_PAY_BILLING);

                var billings = mainContract.Billings
                    .Where(b => b.ReferenceDate != null)
                    .Select(b => new ConfigAccountViewModel.BillingCycle
                    {
                        // ReSharper disable PossibleInvalidOperationException
                        CycleStart = (DateTime)this.ConvertToLocalDateTime(b.ReferenceDate),
                        // ReSharper restore PossibleInvalidOperationException
                        CycleEnd = this.ConvertToLocalDateTime(b.ReferenceDateEnd),
                        DueDate = this.ConvertToLocalDateTime(b.DueDate),
                        Value = b.MainAmount - b.MainDiscount,
                        IsPaid = b.IsPayd,
                        EffectiveValue = b.PaydAmount,
                        CanPay = b.ReferenceDate < nowPlusXDays && nowLessXDays < b.DueDate,
                    });

                var billingYears = billings
                    .GroupBy(b => b.CycleStart.Year)
                    .Select(
                        g => new ConfigAccountViewModel.BillingYear
                            {
                                Year = g.Key,
                                Cycles = g.ToList(),
                            })
                    .ToList();

                viewModel.BillingYears = billingYears;

                return this.View(viewModel);
            }
        }

        public ActionResult Cancel()
        {
            var mainContract = this.DbPractice.AccountContract;
            var viewModel = new CancelAccountViewModel
                {
                    CurrentContract = new ConfigAccountViewModel.Contract
                        {
                            PlanTitle = mainContract.SYS_ContractType.Name,
                            UrlIdentifier = mainContract.SYS_ContractType.UrlIdentifier.Trim(),
                        }
                };

            // Get plan informations of this practice.
            // Contract text, title, description, and other things to display.

            this.ViewBag.IsTrial = mainContract.SYS_ContractType.IsTrial;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Cancel(CancelAccountViewModel viewModel)
        {
            if (!viewModel.Confirm)
            {
                var mainContract = this.DbPractice.AccountContract;

                // Get plan informations of this practice.
                // Contract text, title, description, and other things to display.
                viewModel.CurrentContract = new ConfigAccountViewModel.Contract
                {
                    PlanTitle = mainContract.SYS_ContractType.Name,
                    UrlIdentifier = mainContract.SYS_ContractType.UrlIdentifier.Trim(),
                };

                this.ModelState.AddModelError(() => viewModel.Confirm, "A caixa de checagem de confirmação precisa ser marcada para prosseguir.");

                return this.View(viewModel);
            }

            // Payd accounts are canceled manually.
            if (!this.DbPractice.AccountContract.SYS_ContractType.IsTrial)
            {
                this.DbPractice.AccountCancelRequest = true;

                this.db.SaveChanges();

                this.ViewBag.CancelRequested = true;

                return this.View(viewModel);
            }

            // Sending e-mail with data.
            if (viewModel.SendDataByEmail)
            {
                foreach (var eachDoctorUser in this.DbPractice.Users.Where(u => u.DoctorId != null))
                {
                    // Rendering message bodies from partial view.
                    var emailViewModel = new UserEmailViewModel(eachDoctorUser);
                    var toAddress = new MailAddress(eachDoctorUser.Person.Email, eachDoctorUser.Person.FullName);
                    var mailMessage = this.CreateEmailMessagePartial("AccountDataEmail", toAddress, emailViewModel);

                    // attaching pdf
                    var pdf = ReportController.ExportPatientsPdf(null, this.db, this.DbPractice, eachDoctorUser.Doctor);

                    mailMessage.Attachments.Add(
                        new Attachment(new MemoryStream(pdf), "Prontuários.pdf", "application/pdf"));

                    // attaching xml
                    var xml = ReportController.ExportDoctorXml(this.db, this.DbPractice, eachDoctorUser.Doctor);

                    mailMessage.Attachments.Add(
                        new Attachment(
                            new MemoryStream(Encoding.UTF8.GetBytes(xml)), "Prontuários.xml", "text/xml"));

                    // sending message
                    using (mailMessage)
                    {
                        this.TrySendEmail(mailMessage);
                    }
                }
            }

            var userReason = string.IsNullOrWhiteSpace(viewModel.Reason) ? "No reason provided by user." : viewModel.Reason;
            // sending self e-mail with user reason for canceling
            using (var message = EmailHelper.CreateEmailMessage(
                        new MailAddress("cerebello@cerebello.com.br"),
                        string.Format("Conta cancelada pelo usuário: {0}", this.DbPractice.UrlIdentifier),
                        userReason))
            {
                this.TrySendEmail(message);
            }

            // logging off
            FormsAuthentication.SignOut();

            // disabling account
            this.DbPractice.AccountDisabled = true;
            this.DbPractice.UrlIdentifier += " !disabled"; // change this, so that a new practice with this name can be created.
            this.db.SaveChanges();

            // redirecting user to success message (outside of the app, because the account was canceled)
            return this.RedirectToAction("AccountCanceled", "Home", new { area = "", practice = this.DbPractice.UrlIdentifier });
        }

        public ActionResult Upgrade(string id)
        {
            var mainContract = this.DbPractice.AccountContract;

            // because each plan has a different set of features,
            // this is going to be used by the view, to define the partial page that will be shown
            this.ViewBag.CurrentContractName = mainContract.SYS_ContractType.UrlIdentifier;

            if (mainContract.SYS_ContractType.IsTrial)
            {
                var viewModel = new ChangeContractViewModel
                    {
                        ContractUrlId = id,
                        CurrentDoctorsCount = this.DbPractice.Users.Count(x => x.DoctorId != null),
                    };

                return View(viewModel);
            }

            return this.HttpNotFound();
        }

        [HttpPost]
        public ActionResult Upgrade(string id, ChangeContractViewModel viewModel)
        {
            var mainContract = this.DbPractice.AccountContract;

            // because each plan has a different set of features,
            // this is going to be used by the view, to define the partial page that will be shown
            this.ViewBag.CurrentContractName = mainContract.SYS_ContractType.UrlIdentifier;

            if (!viewModel.AcceptedByUser)
            {
                this.ModelState.AddModelError(
                    () => viewModel.AcceptedByUser, "A caixa de checagem de aceitação do contrato precisa ser marcada para concluir o processo.");
            }

            if (mainContract.SYS_ContractType.IsTrial)
            {
                if (id == "ProfessionalPlan")
                {
                    // calculating values to see if the submited values are correct
                    var unitPrice = Bus.Pro.DOCTOR_PRICE;
                    Func<double, double, double> integ = (x, n) => Math.Pow(n, x) / Math.Log(n);
                    Func<double, double, double> integ0to = (x, n) => (integ(x, n) - integ(0, n));
                    Func<double, double> priceFactor = x => x * (1.0 - 0.1 * integ0to((x - 1) / 3.0, 0.75));
                    Func<double, double> price = (extraDoctors) => Math.Round(priceFactor(extraDoctors) * unitPrice * 100) / 100;

                    var dicValues = new Dictionary<string, decimal>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "MONTH", Bus.Pro.PRICE_MONTH },
                            { "3-MONTHS", Bus.Pro.PRICE_QUARTER },
                            { "6-MONTHS", Bus.Pro.PRICE_SEMESTER },
                            { "12-MONTHS", Bus.Pro.PRICE_YEAR },
                        };

                    var dicDiscount = new Dictionary<string, decimal>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "MONTH", 0 },
                            { "3-MONTHS", Bus.Pro. DISCOUNT_QUARTER },
                            { "6-MONTHS", Bus.Pro. DISCOUNT_SEMESTER },
                            { "12-MONTHS", Bus.Pro.DISCOUNT_YEAR },
                        };

                    var periodSizesDic = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "MONTH", 1 },
                            { "3-MONTHS", 3 },
                            { "6-MONTHS", 6 },
                            { "12-MONTHS", 12 },
                        };

                    var dicount = 1m - dicDiscount[viewModel.PaymentModelName] / 100m;
                    var accountValue = dicValues[viewModel.PaymentModelName];
                    var doctorsValueWithoutDiscount = (decimal)Math.Round(price(viewModel.DoctorCount - 1))
                        * periodSizesDic[viewModel.PaymentModelName];

                    var finalValue = accountValue + doctorsValueWithoutDiscount * dicount;

                    var finalValueWithoutDiscount =
                        Bus.Pro.PRICE_MONTH * periodSizesDic[viewModel.PaymentModelName]
                        + doctorsValueWithoutDiscount;

                    // tolerance of R$ 0.10 in the final value... maybe the browser could not make the calculations correctly,
                    // but we must use that value, since it is the value that the user saw
                    if (Math.Abs(finalValue - viewModel.FinalValue) >= 0.10m)
                    {
                        this.ModelState.AddModelError(
                            () => viewModel.FinalValue,
                            "Seu browser apresentou um defeito no cálculo do valor final. Não foi possível processar sua requisição de upgrade.");
                    }

                    viewModel.ContractUrlId = id;
                    viewModel.CurrentDoctorsCount = this.DbPractice.Users.Count(x => x.DoctorId != null);

                    if (this.ModelState.IsValid)
                    {
                        // sending e-mail to cerebello@cerebello.com.br
                        // to remember us to send the payment request
                        var emailViewModel = new InternalUpgradeEmailViewModel(this.DbUser, viewModel);
                        var toAddress = new MailAddress("cerebello@cerebello.com.br", this.DbUser.Person.FullName);
                        var mailMessage = this.CreateEmailMessagePartial("InternalUpgradeEmail", toAddress, emailViewModel);
                        this.SendEmailAsync(mailMessage).ContinueWith(t =>
                            {
                                // observing exception so that it is not raised
                                var ex = t.Exception;

                                // todo: should do something when e-mail is not sent
                                // 1) use a schedule table to save a serialized e-mail, and then send it later
                                // 2) log a warning message somewhere stating that this e-mail was not sent
                                // send e-mail again is not an option, SendEmailAsync already tries a lot of times
                            });

                        var utcNow = this.GetUtcNow();

                        // terminating the previous contract
                        var currentContract = this.DbPractice.AccountContract;
                        currentContract.EndDate = utcNow;

                        // setting up the professional contract
                        this.DbPractice.AccountContract = new AccountContract
                            {
                                PracticeId = this.DbPractice.Id,
                                IssuanceDate = this.GetUtcNow(),

                                ContractTypeId = (int)ContractTypes.ProfessionalContract,
                                IsTrial = false,
                                StartDate = utcNow, // contract starts NOW... without delays
                                EndDate = null, // this is an unlimited contract
                                CustomText = viewModel.WholeUserAgreement,

                                DoctorsLimit = viewModel.DoctorCount,
                                PatientsLimit = null, // there is no patients limit anymore

                                // billing informations
                                BillingAmount = viewModel.FinalValue,
                                BillingDiscountAmount = finalValueWithoutDiscount - viewModel.FinalValue,
                                BillingDueDay = viewModel.InvoceDueDayOfMonth,
                                BillingPeriodCount = null, // no limit... this contract is valid forever
                                BillingPeriodSize = periodSizesDic[viewModel.PaymentModelName],
                                BillingPeriodType = "M", // same as date-time formatter 'd' for days, 'M' for months, 'y' for years
                                BillingPaymentMethod = "PayPal Invoice",
                            };

                        this.db.SaveChanges();

                        return this.RedirectToAction("UpgradeDone");
                    }

                    return this.View(viewModel);
                }
            }

            return this.HttpNotFound();
        }

        public ActionResult PayPalCheckout()
        {
            var operation = new PayPalSetExpressCheckoutOperation();
            FillOperationDetails(operation, null, null, null);

            // Validating the request object.
            // TODO: handle the invalid object if it has errors.
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                operation,
                new System.ComponentModel.DataAnnotations.ValidationContext(operation, null, null),
                validationResults,
                validateAllProperties: true);

            var opResult = this.SetExpressCheckout(operation, "PayPalConfirm", "PayPalCancel");

            return this.RedirectToCheckout(opResult);
        }

        /// <summary>
        /// Action that termitates the payment process, and then redirects to PaymentConfirmed action.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ActionResult PayPalConfirm(PayPalExpressCheckoutConfirmation data)
        {
            var operation = new PayPalDoExpressCheckoutPaymentOperation
            {
                PayerId = data.PayerId,
                Token = data.Token,
            };
            FillOperationDetails(operation, null, null, null);

            this.DoExpressCheckoutPayment(operation);

            return this.RedirectToAction("PaymentConfirmed");
        }

        public static void FillOperationDetails(PayPalExpressCheckoutOperation operation, Practice practice, AccountContract contractInfo, Billing billing)
        {
            operation.DefaultCurrencyCode = CurrencyCode.Brazilian_Real;
            operation.PaymentRequests = new PayPalList<PayPalPaymentRequest>
            {
                new PayPalPaymentRequest
                {
                    BillingAgreementDescription = "O Cerebello é um software de gerênciamento de consultórios e clínicas médicas.",
                    //BillingType = BillingCode.RecurringPayments,
                    Description = "Cerebello - Plano profissional",
                    InvoiceNum = string.Format("{3}{2}:{0}.{1}", billing.IdentitySetName, billing.IdentitySetNumber, practice.Id, DebugConfig.PayPal.InvoiceIdPrefix),
                    Items = new PayPalList<PayPalPaymentRequestItem>
                    {
                        new PayPalPaymentRequestItem
                        {
                            Amount = contractInfo.BillingAmount,
                            Name = "Cerebello SaaS",
                            Description = "Software de gerenciamento de consultório médico.",
                            Category = ItemCategory.Digital,
                        },
                    },
                },
            };

            if (contractInfo.BillingDiscountAmount > 0)
            {
                operation.PaymentRequests[0].Items.Add(new PayPalPaymentRequestItem
                    {
                        Amount = -contractInfo.BillingDiscountAmount,
                        Name = "Desconto",
                        Category = ItemCategory.Digital,
                    });
            }
        }

        public ActionResult PaymentConfirmed(PayPalExpressCheckoutConfirmation data)
        {
            return this.View();
        }

        /// <summary>
        /// Action that cancels the payment process, and then redirects to PaymentCanceled action.
        /// </summary>
        /// <returns></returns>
        public ActionResult PayPalCancel()
        {
            return this.RedirectToAction("PaymentCanceled");
        }

        public ActionResult PaymentCanceled()
        {
            return this.View();
        }

        private static ConfigAccountViewModel.ContractChangeData CancelTrialPlanInfo()
        {
            // todo: this is not in use anymore
            return new ConfigAccountViewModel.ContractChangeData
            {
                Type = ConfigAccountViewModel.ContractChangeType.Cancel,
                Contract = new ConfigAccountViewModel.Contract
                {
                    PlanTitle = "Nenhum plano",
                    Status = ConfigAccountViewModel.ContractStatus.Suggestion,
                    Text = "NENHUM", // 
                    Description = @"",
                    UrlIdentifier = "CancelTrial",
                }
            };
        }

        private static ConfigAccountViewModel.ContractChangeData CancelProfessionalPlanInfo()
        {
            // todo: this is not in use anymore
            return new ConfigAccountViewModel.ContractChangeData
            {
                Type = ConfigAccountViewModel.ContractChangeType.Cancel,
                Contract = new ConfigAccountViewModel.Contract
                {
                    PlanTitle = "Nenhum plano",
                    Status = ConfigAccountViewModel.ContractStatus.Suggestion,
                    Text = "NENHUM", // 
                    Description = @"O cancelamento pode ser feito a qualquer momento, observando-se
                        as regras de cancelamento do contrato atual. O cancelamento da conta profissional,
                        permite que o usuário tenha acesso a opção de download dos dados em Xml ou Pdf,
                        que permanece disponível por 6 meses a contar da data do cancelamento.",
                    UrlIdentifier = "CancelProfessional",
                }
            };
        }

        private static ConfigAccountViewModel.ContractChangeData RenewPlanInfo()
        {
            // todo: this is not in use anymore
            return new ConfigAccountViewModel.ContractChangeData
            {
                Type = ConfigAccountViewModel.ContractChangeType.Renovation,
                Contract = new ConfigAccountViewModel.Contract
                {
                    PlanTitle = "Plano profissional",
                    Status = ConfigAccountViewModel.ContractStatus.Suggestion,
                    Text = "TEXTO DO CONTRATO", // 
                    Description = @"A renovação do plano já está disponível. Para continuar usando
                        o Cerebello de forma ininterrupta, é necessário fazer a renovação de seu contrato.
                        Caso a renovação não seja feita até o dia do vencimento do contrato, o acesso ao
                        software será suspenso até que seja feita a renovação. Por este motivo é recomendado
                        que a renovação seja agendada até 2 meses antes do seu contrato vencer.",
                    UrlIdentifier = "PaydPlanRenovation",
                }
            };
        }

        private static ConfigAccountViewModel.ContractChangeData ProfessionalPlanInfo()
        {
            // todo: this is not in use anymore
            return new ConfigAccountViewModel.ContractChangeData
            {
                Type = ConfigAccountViewModel.ContractChangeType.Upgrade,
                Contract = new ConfigAccountViewModel.Contract
                {
                    PlanTitle = "Plano profissional",
                    Status = ConfigAccountViewModel.ContractStatus.Suggestion,
                    Text = "TEXTO DO CONTRATO", // 
                    Description = @"",
                    UrlIdentifier = "ProfessionalPlan",
                }
            };
        }

        public ActionResult UpgradeDone(string id)
        {
            return this.View(new ChangeContractViewModel { ContractUrlId = id });
        }
    }
}
