using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.Models;
using PayPal.Version940;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [UserRolePermission(UserRoleFlags.Owner)]
    public class ConfigAccountController : PracticeController
    {
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
                return View();
            }
            else
            {
                // todo: fill in the billings
                var viewModel = new ConfigAccountViewModel
                    {
                    };

                return View();
            }
        }

        public ActionResult Cancel()
        {
            var mainContract = this.DbPractice.AccountContract;
            var viewModel = new CancelAccountViewModel();

            // Get plan informations of this practice.
            // Contract text, title, description, and other things to display.
            viewModel.CurrentContract = new ConfigAccountViewModel.Contract
            {
                PlanTitle = mainContract.SYS_ContractType.Name,
                UrlIdentifier = mainContract.SYS_ContractType.UrlIdentifier.Trim(),
            };

            this.ViewBag.IsTrial = mainContract.SYS_ContractType.IsTrial;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Cancel(CancelAccountViewModel viewModel)
        {
            // Payd accounts are canceled manually.
            if (!this.DbPractice.AccountContract.SYS_ContractType.IsTrial)
            {
                this.DbPractice.AccountCancelRequest = true;

                this.db.SaveChanges();

                this.ViewBag.CancelRequested = true;

                return this.View(viewModel);
            }

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

            // Sending e-mail with data.
            if (viewModel.SendDataByEmail)
            {
                foreach (var eachDoctorUser in this.DbPractice.Users.Where(u => u.DoctorId != null))
                {
                    // Rendering message bodies from partial view.
                    var emailViewModel = new UserEmailViewModel(eachDoctorUser);
                    var toAddress = new MailAddress(eachDoctorUser.Person.Email, eachDoctorUser.Person.FullName);
                    var mailMessage = this.CreateEmailMessage("AccountDataEmail", toAddress, emailViewModel);

                    // attaching pdf
                    var pdf = ReportController.ExportPatientsPdf(null, this.db, this.DbPractice, eachDoctorUser.Doctor, this.Request);

                    mailMessage.Attachments.Add(
                        new Attachment(
                            new MemoryStream(pdf.DocumentBytes), "Prontuários.pdf", pdf.MimeType));

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
                    var unitPrice = Buz.Pro.DOCTOR_PRICE;
                    Func<double, double, double> integ = (x, n) => Math.Pow(n, x) / Math.Log(n);
                    Func<double, double, double> integ0to = (x, n) => (integ(x, n) - integ(0, n));
                    Func<double, double> priceFactor = x => x * (1.0 - 0.1 * integ0to((x - 1) / 3.0, 0.75));
                    Func<double, double> price = (extraDoctors) => Math.Round(priceFactor(extraDoctors) * unitPrice * 100) / 100;

                    var dicValues = new Dictionary<string, decimal>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "MONTH", Buz.Pro.PRICE_MONTH },
                            { "3-MONTHS", Buz.Pro.PRICE_QUARTER },
                            { "6-MONTHS", Buz.Pro.PRICE_SEMESTER },
                            { "12-MONTHS", Buz.Pro.PRICE_YEAR },
                        };

                    var dicDiscount = new Dictionary<string, decimal>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "MONTH", 0 },
                            { "3-MONTHS", Buz.Pro. DISCOUNT_QUARTER },
                            { "6-MONTHS", Buz.Pro. DISCOUNT_SEMESTER },
                            { "12-MONTHS", Buz.Pro.DISCOUNT_YEAR },
                        };

                    var mult = new Dictionary<string, decimal>(StringComparer.InvariantCultureIgnoreCase)
                        {
                            { "MONTH", 1 },
                            { "3-MONTHS", 3 },
                            { "6-MONTHS", 6 },
                            { "12-MONTHS", 12 },
                        };

                    var dicount = 1m - dicDiscount[viewModel.PaymentModelName] / 100m;
                    var accountValue = dicValues[viewModel.PaymentModelName];
                    var doctorsValue = (decimal)Math.Round(price(viewModel.DoctorCount - 1))
                        * mult[viewModel.PaymentModelName] * dicount;

                    var finalValue = accountValue + doctorsValue;

                    if (finalValue != viewModel.FinalValue)
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
                        var mailMessage = this.CreateEmailMessage("InternalUpgradeEmail", toAddress, emailViewModel);
                        this.TrySendEmail(mailMessage);

                        return this.RedirectToAction("UpgradeRequested");
                    }

                    return this.View(viewModel);
                }
            }

            return this.HttpNotFound();
        }

        public ActionResult PayPalCheckout()
        {
            var operation = new PayPalSetExpressCheckoutOperation();
            FillOperationDetails(operation, null);

            // Validating the request object.
            // TODO: handle the invalid object if it has errors.
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                operation,
                new System.ComponentModel.DataAnnotations.ValidationContext(operation, null, null),
                validationResults,
                validateAllProperties: true);

            var opResult = this.SetExpressCheckout(operation);

            return this.RedirectToCheckout(opResult);
        }

        /// <summary>
        /// Action that termitates the payment process, and the redirects to PaymentConfirmed action.
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
            this.FillOperationDetails(operation, null);

            this.DoExpressCheckoutPayment(operation);

            return this.RedirectToAction("PaymentConfirmed");
        }

        private void FillOperationDetails<T1>(T1 operation, object contractInfo)
            where T1 : PayPalExpressCheckoutOperation, new()
        {
            bool discountFirst100 = !this.db.Practices
                .Where(p => !p.AccountContract.SYS_ContractType.IsTrial)
                .OrderBy(p => p.Id)
                .Skip(99).Any();

            operation.DefaultCurrencyCode = CurrencyCode.Brazilian_Real;
            operation.PaymentRequests = new PayPalList<PayPalPaymentRequest>
            {
                new PayPalPaymentRequest
                {
                    Description = "Cerebello - Pacote profissional",

                    Items = new PayPalList<PayPalPaymentRequestItem>
                    {
                        new PayPalPaymentRequestItem
                        {
                            Amount = 150.00m,
                            Name = "Cerebello SaaS",
                            Description = "Software de gerenciamento de consultório médico.",
                            Category = ItemCategory.Digital,
                        },
                    },
                },
            };

            // Discount for the first 100 practices.
            if (discountFirst100)
            {
                operation.PaymentRequests[0].Items.Add(new PayPalPaymentRequestItem
                {
                    Amount = -30.00m,
                    Name = "Desconto lançamento! (20%)",
                    Description = "Desconto para os 100 primeiros clientes.",
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

        public ActionResult UpgradeRequested(string id)
        {
            return this.View(new ChangeContractViewModel { ContractUrlId = id });
        }
    }
}
