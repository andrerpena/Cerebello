using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Helpers;
using CerebelloWebRole.Controllers;
using CerebelloWebRole.Models;
using PayPal.Version940;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [UserRolePermission(UserRoleFlags.Owner)]
    public class ConfigAccountController : PracticeController
    {
        public ActionResult Index()
        {
            var mainContract = this.DbPractice.AccountContract;
            var viewModel = new ConfigAccountViewModel();
            // Get all the billings of this practice.
            // Billings must be grouped by year, and be in reverse order,
            // that is, newest ones come first.

            // Get plan informations of this practice.
            // Contract text, title, description, and other things to display.
            viewModel.CurrentContract = new ConfigAccountViewModel.Contract
            {
                PlanTitle = mainContract.SYS_ContractType.Name,
            };

            //viewModel.CurrentContract.Additions.AddRange(new List<ConfigAccountViewModel.Contract>
            //{
            //    new ConfigAccountViewModel.Contract
            //    {
            //        Description = "Adiciona a capacidade de envio de SMS para os paciente, para lembrá-lo de suas consultas com até 48h de antecedência.",
            //        PlanTitle = "Plano de SMS",
            //        Status = ConfigAccountViewModel.ContractStatus.Suggestion,
            //        UrlIdentifier = "SmsPlanAddition",
            //    },
            //    new ConfigAccountViewModel.Contract
            //    {
            //        Description = "Adiciona suporte on-line instantâneo via chat, na mesma janela de chat usada para se comunicar com outros membros do consultório.",
            //        PlanTitle = "Plano de suporte via chat",
            //        Status = ConfigAccountViewModel.ContractStatus.Suggestion,
            //        UrlIdentifier = "ChatSupportPlanAddition",
            //    },
            //});

            // Get available plan migrations.
            // Only migrations that can be done may be placed in this list.

            if (mainContract.SYS_ContractType.IsTrial)
            {
                viewModel.Migrations = new List<ConfigAccountViewModel.Migration>
                {
                    //RenewPlanInfo(),
                    PaidPlanInfo(),
                };
            }
            else
            {
            }

            viewModel.Migrations.Add(CancelTrialPlanInfo());

            return View(viewModel);
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
                    var partialViewModel = new EmailViewModel
                        {
                            PersonName = eachDoctorUser.Person.FullName,
                            UserName = eachDoctorUser.UserName,
                            PracticeUrlIdentifier = eachDoctorUser.Practice.UrlIdentifier,
                        };
                    var bodyText = this.RenderPartialViewToString("AccountDataEmail", partialViewModel);

                    partialViewModel.IsBodyHtml = true;
                    var bodyHtml = this.RenderPartialViewToString("AccountDataEmail", partialViewModel);

                    var toAddress = new MailAddress(eachDoctorUser.Person.Email, eachDoctorUser.Person.FullName);

                    var message = EmailHelper.CreateEmailMessage(
                        toAddress,
                        "Dados de prontuário de seus pacientes",
                        bodyText, bodyHtml);

                    // attaching pdf
                    var pdf = ReportController.ExportPatientsPdf(null, this.db, this.DbPractice, eachDoctorUser.Doctor, this.Request);

                    message.Attachments.Add(
                        new Attachment(
                            new MemoryStream(pdf.DocumentBytes), "Prontuários.pdf", pdf.MimeType));

                    // attaching xml
                    var xml = ReportController.ExportDoctorXml(this.db, this.DbPractice, eachDoctorUser.Doctor);

                    message.Attachments.Add(
                        new Attachment(
                            new MemoryStream(Encoding.UTF8.GetBytes(xml)), "Prontuários.xml", "text/xml"));

                    // sending message
                    using (message)
                    {
                        this.SendEmail(message);
                    }
                }
            }

            // sending self e-mail with user reason for canceling
            using (var message = EmailHelper.CreateEmailMessage(
                        new MailAddress("cerebello@cerebello.com.br"),
                        string.Format("Conta cancelada pelo usuário: {0}", this.DbPractice.UrlIdentifier),
                        viewModel.Reason))
            {
                this.SendEmail(message);
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
            var viewModel = new ConfigAccountViewModel();

            // Get plan informations of this practice.
            // Contract text, title, description, and other things to display.
            viewModel.CurrentContract = new ConfigAccountViewModel.Contract
            {
                PlanTitle = mainContract.SYS_ContractType.Name,
            };

            if (mainContract.SYS_ContractType.IsTrial)
            {
                var paidPlan = PaidPlanInfo();

                if (id == paidPlan.Contract.UrlIdentifier)
                {
                    viewModel.Migrations = new List<ConfigAccountViewModel.Migration>
                    {
                        paidPlan,
                    };
                }
            }

            return View(viewModel);
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

        private static ConfigAccountViewModel.Migration CancelTrialPlanInfo()
        {
            return new ConfigAccountViewModel.Migration
            {
                Type = ConfigAccountViewModel.MigrationType.Cancel,
                Contract = new ConfigAccountViewModel.Contract
                {
                    PlanTitle = "Nenhum plano",
                    Status = ConfigAccountViewModel.ContractStatus.Suggestion,
                    Text = "NENHUM", // 
                    Description = @"O cancelamento da conta de teste pode ser feito a qualquer momento.
                        Como essa é uma conta de teste, todos os dados serão apagados do sistema,
                        não sendo possível reativar a mesma, e nem fazer download dos dados após o cancelamento.",
                    UrlIdentifier = "CancelTrial",
                }
            };
        }

        private static ConfigAccountViewModel.Migration CancelProfessionalPlanInfo()
        {
            return new ConfigAccountViewModel.Migration
            {
                Type = ConfigAccountViewModel.MigrationType.Cancel,
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

        private static ConfigAccountViewModel.Migration RenewPlanInfo()
        {
            return new ConfigAccountViewModel.Migration
            {
                Type = ConfigAccountViewModel.MigrationType.Renovation,
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

        private static ConfigAccountViewModel.Migration PaidPlanInfo()
        {
            return new ConfigAccountViewModel.Migration
            {
                Type = ConfigAccountViewModel.MigrationType.Upgrade,
                Contract = new ConfigAccountViewModel.Contract
                {
                    PlanTitle = "Plano profissional",
                    Status = ConfigAccountViewModel.ContractStatus.Suggestion,
                    Text = "TEXTO DO CONTRATO", // 
                    Description = @"Plano sem os limites da conta trial. A conta trial possui um limite
                        total de 50 pacientes, o que é pouco para um consultório funcionar. Além disso
                        a conta paga possui suporte prioritário, e maior segurança dos dados.",
                    UrlIdentifier = "PaidPlan",
                }
            };
        }
    }
}
