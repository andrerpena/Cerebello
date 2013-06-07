using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class MedicalCertificatesControllerTests : DbTestBase
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
        public override void TestInitialize()
        {
            base.TestInitialize();
            Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
        }
        #endregion

        #region Edit

        [TestMethod]
        public void Edit_1_CannotSaveWithInvalidModelId()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            MedicalCertificateViewModel formModel = new MedicalCertificateViewModel()
            {
                // this probably doesn't exist
                ModelId = 9999,
                PatientId = patientId
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<MedicalCertificatesController>();
            var controllerResult = controller.Edit(new[] { formModel });

            Assert.IsInstanceOfType(controllerResult, typeof(ViewResult));
            Assert.AreEqual(false, controller.ModelState.IsValid);
            Assert.AreEqual(1, controller.ModelState.Count);
        }

        [TestMethod]
        public void Edit_2_CannotSaveWithInvalidPatient()
        {
            var mr = new MockRepository(true);
            int modelId = this.db.ModelMedicalCertificates.First().Id;

            // tries to save
            var formModel = new MedicalCertificateViewModel()
            {
                ModelId = modelId,
                // this probably doesn't exist
                PatientId = 9999
            };

            var controller = mr.CreateController<MedicalCertificatesController>();
            var controllerResult = controller.Edit(new[] { formModel });

            Assert.IsInstanceOfType(controllerResult, typeof(ViewResult));
            Assert.AreEqual(false, controller.ModelState.IsValid);
            Assert.AreEqual(1, controller.ModelState.Count);
        }

        [TestMethod]
        public void Edit_3_CannotSaveWithInvalidModelIdAndId()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            // tries to save
            var formModel = new MedicalCertificateViewModel()
            {
                // both EXISTING
                ModelId = null,
                Id = null,
                PatientId = patientId,
                Fields = new List<MedicalCertificateFieldViewModel>()
                {
                     new MedicalCertificateFieldViewModel() { Name = "field_1" },
                     new MedicalCertificateFieldViewModel() { Name = "field_2" }
                }
            };

            var mr = new MockRepository(true);
            var controller = mr.CreateController<MedicalCertificatesController>();
            var controllerResult = controller.Edit(new[] { formModel });

            Assert.IsInstanceOfType(controllerResult, typeof(ViewResult));
            Assert.AreEqual(false, controller.ModelState.IsValid);
            Assert.AreEqual(1, controller.ModelState.Count);
        }

        [TestMethod]
        public void Edit_4_CannotSaveACertificateWithInvalidFields()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            // obtains a valid certificate model
            ModelMedicalCertificateViewModel certificateModelFormModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%FIELD_1%>"
            };
            var mr = new MockRepository(true);
            var certificateModelTarget = mr.CreateController<ModelMedicalCertificatesController>();
            var certificateModelResult = certificateModelTarget.Edit(certificateModelFormModel);
            var modelId = this.db.ModelMedicalCertificates.First().Id;

            // tries to save
            MedicalCertificateViewModel formModel = new MedicalCertificateViewModel()
            {
                // both EXISTING
                ModelId = modelId,
                PatientId = patientId
            };

            var target = mr.CreateController<MedicalCertificatesController>();
            var result = target.Edit(new[] { formModel });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(false, target.ModelState.IsValid);
            Assert.AreEqual(1, target.ModelState.Count);
        }

        [TestMethod]
        public void Edit_5_CanSaveAllFieldsWhenModelDoesNotExist()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            // obtains a valid certificate model
            ModelMedicalCertificateViewModel certificateModelFormModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%FIELD_1%>"
            };
            var mr = new MockRepository(true);
            var certificateModelTarget = mr.CreateController<ModelMedicalCertificatesController>();
            var certificateModelResult = certificateModelTarget.Edit(certificateModelFormModel);
            var modelId = this.db.ModelMedicalCertificates.First().Id;

            // tries to save
            MedicalCertificateViewModel formModel = new MedicalCertificateViewModel()
            {
                // both EXISTING
                ModelId = modelId,
                PatientId = patientId,
                Fields = new List<MedicalCertificateFieldViewModel>()
                {
                     new MedicalCertificateFieldViewModel() { Name = "field_1", Value = "value 1" }
                }
            };

            var target = mr.CreateController<MedicalCertificatesController>();
            var result = target.Edit(new[] { formModel });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(true, target.ModelState.IsValid);

            // now, edit to remove the model and add a field

            var medicalCerticate = this.db.MedicalCertificates.First();
            formModel = new MedicalCertificateViewModel()
            {
                // both EXISTING
                Id = medicalCerticate.Id,
                ModelId = null,
                PatientId = patientId,
                Fields = new List<MedicalCertificateFieldViewModel>()
                {
                     new MedicalCertificateFieldViewModel() { Name = "field_1" },
                     new MedicalCertificateFieldViewModel() { Name = "field_2" },
                }
            };

            target = mr.CreateController<MedicalCertificatesController>();
            result = target.Edit(new[] { formModel });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(true, target.ModelState.IsValid);
        }

        [TestMethod]
        public void Edit_HappyPath()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();
            var patientId = this.db.Patients.First().Id;

            // obtains a valid certificate model
            var certificateModelFormModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%FIELD_1%>"
            };
            var mr = new MockRepository(true);
            var certificateModelTarget = mr.CreateController<ModelMedicalCertificatesController>();
            var certificateModelResult = certificateModelTarget.Edit(certificateModelFormModel);
            var modelId = this.db.ModelMedicalCertificates.First().Id;

            // tries to save
            MedicalCertificateViewModel formModel = new MedicalCertificateViewModel()
            {
                // both EXISTING
                ModelId = modelId,
                PatientId = patientId,
                Fields = new List<MedicalCertificateFieldViewModel>()
                {
                     new MedicalCertificateFieldViewModel() { Name = "field_1", Value ="Este é o valor" }
                }
            };

            var target = mr.CreateController<MedicalCertificatesController>();
            var result = target.Edit(new[] { formModel });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(true, target.ModelState.IsValid);
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
            ModelMedicalCertificateViewModel certificateModelFormModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%FIELD_1%>"
            };

            var mr = new MockRepository(true);
            var certificateModelController = mr.CreateController<ModelMedicalCertificatesController>();
            var certificateModelControllerResult = certificateModelController.Edit(certificateModelFormModel);
            var certificateModel = this.db.ModelMedicalCertificates.First();

            // tries to save a certificate based on that model
            MedicalCertificateViewModel formModel = new MedicalCertificateViewModel()
            {
                ModelId = certificateModel.Id,
                PatientId = patientId,
                Fields = new List<MedicalCertificateFieldViewModel>()
                {
                     new MedicalCertificateFieldViewModel() { Name = "field_1", Value = "value 1" }
                }
            };

            var certificateController = mr.CreateController<MedicalCertificatesController>();
            var certificateControllerResult = certificateController.Edit(new[] { formModel });
            var certificate = this.db.MedicalCertificates.First();

            // tries to delete the certificate
            var result = certificateController.Delete(certificate.Id);
            JsonDeleteMessage deleteMessage = (JsonDeleteMessage)result.Data;

            Assert.AreEqual(true, deleteMessage.success, "deleteMessage.success must be true");
            Assert.AreEqual(0, this.db.MedicalCertificates.Count());
        }

        #endregion

        #region GetCertificateText

        [TestMethod]
        public void GetCertificateText_HappyPath()
        {
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();

            var patient = this.db.Patients.First();
            var patientId = patient.Id;
            var patientName = patient.Person.FullName;

            // obtains a valid certificate model
            ModelMedicalCertificateViewModel certificateModelFormModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%FIELD_1%>. This is the patient name: <%paCIENTE%>"
            };
            var mr = new MockRepository(true);
            var certificateModelController = mr.CreateController<ModelMedicalCertificatesController>();
            var certificateModelControllerResult = certificateModelController.Edit(certificateModelFormModel);
            var modelId = this.db.ModelMedicalCertificates.First().Id;

            // tries to save
            MedicalCertificateViewModel formModel = new MedicalCertificateViewModel()
            {
                // both EXISTING
                ModelId = modelId,
                PatientId = patientId,
                Fields = new List<MedicalCertificateFieldViewModel>()
                {
                     new MedicalCertificateFieldViewModel() { Name = "field_1", Value = "This is a value" }
                }
            };

            var certificateController = mr.CreateController<MedicalCertificatesController>();
            var certificateControllerResult = certificateController.Edit(new[] { formModel });

            Assert.IsInstanceOfType(certificateControllerResult, typeof(ViewResult));
            Assert.AreEqual(true, certificateController.ModelState.IsValid);

            // Now verifies whether the result is the expected
            var newlyCreatedCertificate = this.db.MedicalCertificates.First();

            var certificateText = certificateController.GetCertificateText(newlyCreatedCertificate.Id);

            Assert.AreEqual("This is a reference: This is a value. This is the patient name: " + patientName, certificateText);
        }

        #endregion

        #region MedicalCertificateFieldsEditor

        [TestMethod]
        public void MedicalCertificateFieldsEditor_1_NoModelIdSupplied()
        {
            // create a model
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>, <%pRoP_2%>, <%PrOP_3%>, <%ProP_4%>"
            };

            var mr = new MockRepository(true);
            var certificateModelcontroller = mr.CreateController<ModelMedicalCertificatesController>();
            certificateModelcontroller.Edit(formModel);

            // create a certificate
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();

            var patientId = this.db.Patients.First().Id;
            var controller = mr.CreateController<MedicalCertificatesController>();
            var controllerResult = controller.MedicalCertificateFieldsEditor(null, 37, null);

            // obtaining the view-model
            ViewResult view = (ViewResult)controllerResult;
            var viewModel = (MedicalCertificateViewModel)view.Model;

            Assert.AreEqual(0, viewModel.Fields.Count);
            Assert.AreEqual(null, viewModel.ModelId);
            Assert.AreEqual(null, viewModel.ModelName);
        }

        [TestMethod]
        public void MedicalCertificateFieldsEditor_2_ModelIdIsSuppliedAndCertficateIdIsNot()
        {
            // create a model
            ModelMedicalCertificateViewModel formModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>, <%pRoP_2%>, <%PrOP_3%>, <%ProP_4%>"
            };

            var mr = new MockRepository(true);
            var certificateModelcontroller = mr.CreateController<ModelMedicalCertificatesController>();
            certificateModelcontroller.Edit(formModel);
            var modelId = this.db.ModelMedicalCertificates.Select(m => m.Id).First();

            // create a certificate
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();

            var patientId = this.db.Patients.First().Id;
            var controller = mr.CreateController<MedicalCertificatesController>();
            var controllerResult = controller.MedicalCertificateFieldsEditor(modelId, null, null);

            // obtaining the view-model
            ViewResult view = (ViewResult)controllerResult;
            var viewModel = (MedicalCertificateViewModel)view.Model;

            Assert.AreEqual(4, viewModel.Fields.Count);
        }

        [TestMethod]
        public void MedicalCertificateFieldsEditor_3_BothParametersAreSuppliedButTheyDontMatch()
        {
            // create a model
            var certificateModelFormModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>"
            };

            var mr = new MockRepository(true);
            var certificateModelcontroller = mr.CreateController<ModelMedicalCertificatesController>();
            certificateModelcontroller.Edit(certificateModelFormModel);
            var modelId = this.db.ModelMedicalCertificates.Select(m => m.Id).First();

            // create another model
            certificateModelFormModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>, <%ProP_4%>"
            };

            certificateModelcontroller = mr.CreateController<ModelMedicalCertificatesController>();
            certificateModelcontroller.Edit(certificateModelFormModel);
            var anotherModelId = this.db.ModelMedicalCertificates.Select(m => m.Id).ToList().AsEnumerable().Reverse().First();

            // create a certificate
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();

            var patientId = this.db.Patients.First().Id;

            var controller = mr.CreateController<MedicalCertificatesController>();
            MedicalCertificateViewModel formModel = new MedicalCertificateViewModel()
            {
                // both EXISTING
                ModelId = modelId,
                PatientId = patientId,
                Fields = new List<MedicalCertificateFieldViewModel>()
                {
                     new MedicalCertificateFieldViewModel() { Name = "prop_1", Value ="Este é o valor" }
                }
            };

            // save the certificate
            controller.Edit(new[] { formModel });
            var certificateId = this.db.MedicalCertificates.Select(c => c.Id).First();

            // at this point we have 2 certificate models, "modelId" and "anotherModelId" and we have a certificate using "modelId". The point is 
            // to call MedicalCertificateFieldsEditor passing "anotherModelId" 

            var controllerResult = controller.MedicalCertificateFieldsEditor(anotherModelId, certificateId, null);

            // obtaining the view-model
            ViewResult view = (ViewResult)controllerResult;
            var viewModel = (MedicalCertificateViewModel)view.Model;

            Assert.AreEqual(2, viewModel.Fields.Count);
            Assert.AreEqual(null, viewModel.Fields[0].Value);
            Assert.AreEqual(null, viewModel.Fields[1].Value);
        }

        [TestMethod]
        public void MedicalCertificateFieldsEditor_4_BothParametersAreSuppliedAndTheyDontMatch()
        {
            // create a model
            ModelMedicalCertificateViewModel certificateModelFormModel = new ModelMedicalCertificateViewModel()
            {
                Name = "My Model",
                Text = "This is a reference: <%PROP_1%>"
            };

            var mr = new MockRepository(true);
            var certificateModelcontroller = mr.CreateController<ModelMedicalCertificatesController>();
            certificateModelcontroller.Edit(certificateModelFormModel);
            var modelId = this.db.ModelMedicalCertificates.Select(m => m.Id).First();

            // create a certificate
            // obtains a valid patient
            Firestarter.CreateFakePatients(this.db.Doctors.First(), this.db);
            this.db.SaveChanges();

            var patientId = this.db.Patients.First().Id;

            var controller = mr.CreateController<MedicalCertificatesController>();
            MedicalCertificateViewModel formModel = new MedicalCertificateViewModel()
            {
                // both EXISTING
                ModelId = modelId,
                PatientId = patientId,
                Fields = new List<MedicalCertificateFieldViewModel>()
                {
                     new MedicalCertificateFieldViewModel() { Name = "prop_1", Value ="Este é o valor" }
                }
            };

            // save the certificate
            controller.Edit(new[] { formModel });
            var certificateId = this.db.MedicalCertificates.Select(c => c.Id).First();

            // at this point we have 2 certificate models, "modelId" and "anotherModelId" and we have a certificate using "modelId". The point is 
            // to call MedicalCertificateFieldsEditor passing "anotherModelId" 

            var controllerResult = controller.MedicalCertificateFieldsEditor(modelId, certificateId, null);

            // obtaining the view-model
            ViewResult view = (ViewResult)controllerResult;
            var viewModel = (MedicalCertificateViewModel)view.Model;

            Assert.AreEqual(1, viewModel.Fields.Count);
            Assert.AreEqual("Este é o valor", viewModel.Fields[0].Value);
        }

        #endregion
    }
}
