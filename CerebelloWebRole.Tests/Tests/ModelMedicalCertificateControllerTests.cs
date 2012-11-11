using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    /// <summary>
    ///This is a test class for ModelMedicalCertificateControllerTest and is intended
    ///to contain all ModelMedicalCertificateControllerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ModelMedicalCertificateControllerTests : DbTestBase
    {
        #region TEST_SETUP
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            AttachCerebelloTestDatabase();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            DetachCerebelloTestDatabase();
        }

        [TestInitialize]
        public override void InitializeDb()
        {
            base.InitializeDb();
            Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
        }
        #endregion

        #region Edit

        /// <summary>
        /// Verify if it's possible to add fields with not allowed characters
        ///</summary>
        [TestMethod()]
        public void Edit_1_CannotAddFieldWithNotAllowedCharacters()
        {
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP`1%>, <%PROP%2%>",
                Fields = new System.Collections.Generic.List<ModelMedicalCertificateFieldViewModel>() {
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP`1" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP%2" }
                 }
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<ModelMedicalCertificatesController>();
            var controllerResult = controller.Edit(formModel);

            Assert.IsInstanceOfType(controllerResult, typeof(ViewResult));
            Assert.AreEqual(false, controller.ModelState.IsValid);
            Assert.AreEqual(2, controller.ModelState.Count);
        }

        /// <summary>
        /// Verify if it's not possible to place malformed placeholders
        /// </summary>
        [TestMethod]
        public void Edit_2_CannotUseMalformedPlaceHolders()
        {
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <% PROP-1%>, <%PROP_2  %>, <%   %>",
                Fields = new System.Collections.Generic.List<ModelMedicalCertificateFieldViewModel>() {
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP-1" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_2" }
                 }
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<ModelMedicalCertificatesController>();
            var controllerResult = controller.Edit(formModel);

            Assert.IsInstanceOfType(controllerResult, typeof(ViewResult));
            Assert.AreEqual(false, controller.ModelState.IsValid);

            // this is 3 because PRO-1 and PRO_2 are not referenced, which will cause additional errors
            Assert.AreEqual(3, controller.ModelState.Count);
            Assert.AreEqual(3, controller.ModelState["Text"].Errors.Count);
        }

        /// <summary>
        /// Verify if it's possible to place malformed placeholders
        /// </summary>
        [TestMethod]
        public void Edit_3_AllDeclaredFieldsMustBeReferenced()
        {
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>, <%PROP_2%>",
                Fields = new System.Collections.Generic.List<ModelMedicalCertificateFieldViewModel>() {
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_1" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_2" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_3" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_4" },
                 }
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<ModelMedicalCertificatesController>();
            var controllerResult = controller.Edit(formModel);

            Assert.IsInstanceOfType(controllerResult, typeof(ViewResult));
            Assert.AreEqual(false, controller.ModelState.IsValid);
            Assert.AreEqual(2, controller.ModelState.Count);
        }

        [TestMethod]
        public void Edit_4_AllFieldsInTheTextMustBeDeclared()
        {
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>, <%PROP_2%>",
                Fields = new System.Collections.Generic.List<ModelMedicalCertificateFieldViewModel>() {
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_1" }
                }
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<ModelMedicalCertificatesController>();
            var controllerResult = controller.Edit(formModel);

            Assert.IsInstanceOfType(controllerResult, typeof(ViewResult));
            Assert.AreEqual(false, controller.ModelState.IsValid);
            Assert.AreEqual(1, controller.ModelState.Count);
        }

        [TestMethod]
        public void Edit_4_AllFieldsInTheTextMustBeDeclaredWithTheExceptionOfPatient()
        {
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>, <%PaciENTE%>",
                Fields = new System.Collections.Generic.List<ModelMedicalCertificateFieldViewModel>() {
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_1" }
                }
            };

            var mr = new MockRepository(true);
            mr.SetCurrentUser_Andre_CorrectPassword();
            var controller = mr.CreateController<ModelMedicalCertificatesController>();
            var controllerResult = controller.Edit(formModel);

            Assert.IsInstanceOfType(controllerResult, typeof(RedirectResult));
            Assert.AreEqual(true, controller.ModelState.IsValid);
        }

        /// <summary>
        /// Verify if it's possible to create a model with no fields
        /// </summary>
        [TestMethod]
        public void Edit_5_CanProperlyCreateModelWithNoFields()
        {
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This has no references"
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<ModelMedicalCertificatesController>();
            var controllerResult = controller.Edit(formModel);

            Assert.IsInstanceOfType(controllerResult, typeof(RedirectResult));
            Assert.AreEqual(true, controller.ModelState.IsValid);
        }

        /// <summary>
        /// Verify if it's possible to create a field with a pre-defined name
        /// </summary>
        [TestMethod]
        public void Edit_CannotCreateAFieldWithPredefinedNames()
        {
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a medical certificate for <%paciente%>",
                Fields = new System.Collections.Generic.List<ModelMedicalCertificateFieldViewModel>() {
                    // 'paciente' is a pre-defined field. You can reference it but you cannot add it
                    new ModelMedicalCertificateFieldViewModel() { Name = "paciente" }
                }
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<ModelMedicalCertificatesController>();
            var controllerResult = controller.Edit(formModel);

            Assert.IsInstanceOfType(controllerResult, typeof(ViewResult));
            Assert.AreEqual(false, controller.ModelState.IsValid);
            Assert.AreEqual(1, controller.ModelState.Count);
        }

        [TestMethod]
        public void Edit_New_HappyPath()
        {
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>, <%pRoP_2%>, <%PrOP_3%>, <%ProP_4%>",
                Fields = new System.Collections.Generic.List<ModelMedicalCertificateFieldViewModel>() {
                    new ModelMedicalCertificateFieldViewModel() { Name = "prop_1" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_2" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_3" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PrOP_4" },
                 }
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<ModelMedicalCertificatesController>();
            var controllerResult = controller.Edit(formModel);

            Assert.IsInstanceOfType(controllerResult, typeof(RedirectResult));
            Assert.AreEqual(true, controller.ModelState.IsValid);
        }

        [TestMethod]
        public void Edit_Existing_HappyPath()
        {
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>, <%pRoP_2%>, <%PrOP_3%>, <%ProP_4%>",
                Fields = new System.Collections.Generic.List<ModelMedicalCertificateFieldViewModel>() {
                    new ModelMedicalCertificateFieldViewModel() { Name = "prop_1" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_2" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_3" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PrOP_4" },
                 }
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<ModelMedicalCertificatesController>();
            var controllerResult = controller.Edit(formModel);

            Assert.IsInstanceOfType(controllerResult, typeof(RedirectResult));
            Assert.AreEqual(true, controller.ModelState.IsValid);

            // edits existing

            formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model 2",
                Text = "This is a reference 2: <%PROP_1%>, <%pRoP_2%>",
                Fields = new System.Collections.Generic.List<ModelMedicalCertificateFieldViewModel>() {
                    new ModelMedicalCertificateFieldViewModel() { Name = "prop_1" },
                    new ModelMedicalCertificateFieldViewModel() { Name = "PROP_2" }
                 }
            };

            Assert.IsInstanceOfType(controllerResult, typeof(RedirectResult));
            Assert.AreEqual(true, controller.ModelState.IsValid);
        }

        #endregion

        #region Delete

        [TestMethod]
        public void Delete_HappyPath()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            // obtains a valid certificate model
            var certificateModelFormModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%FIELD_1%>",
                Fields = new List<ModelMedicalCertificateFieldViewModel>()
                {
                     new ModelMedicalCertificateFieldViewModel() { Name = "FIeLd_1" }
                }
            };
            var mr = new MockRepository(true);
            var certificateModelController = mr.CreateController<ModelMedicalCertificatesController>();
            var certificateModelControllerResult = certificateModelController.Edit(certificateModelFormModel);
            var certificateModel = this.db.ModelMedicalCertificates.First();

            // tries to save a certificate based on that model
            MedicalCertificateViewModel formModel = new MedicalCertificateViewModel()
            {
                // both EXISTING
                ModelId = certificateModel.Id,
                PatientId = patientId,
                Fields = new List<MedicalCertificateFieldViewModel>()
                {
                     new MedicalCertificateFieldViewModel() { Name = "field_1", Value = "value 1" }
                }
            };

            var certificateController = mr.CreateController<MedicalCertificatesController>();
            var certificateControllerResult = certificateController.Edit(formModel);

            var certificate = this.db.MedicalCertificates.First();

            // tries to delete the model certificate
            var jsonResult = certificateModelController.Delete(certificateModel.Id);

            JsonDeleteMessage deleteMessage = (JsonDeleteMessage)jsonResult.Data;

            Assert.AreEqual(true, deleteMessage.success, "deleteMessage.success must be true");
            Assert.AreNotEqual(null, certificate.ModelMedicalCertificate, "The medical certificate should not be deleted with the model, but it was.");
            Assert.AreEqual(0, this.db.ModelMedicalCertificateFields.Count());
        }

        #endregion
    }
}
