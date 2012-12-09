using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Json;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class UsersControllerTests : DbTestBase
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
        #endregion

        #region Create
        /// <summary>
        /// Tests the creation of an user without data.
        /// This is an invalid operation and should complete with a lot of validation messages.
        /// </summary>
        [TestMethod]
        public void Create_AllFieldsEmpty()
        {
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(new UserViewModel());
            }

            // Verifying the ActionResult, and the DB.
            // - The controller ModelState must have no validation errors related to e-mail.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should be invalid.");
        }

        /// <summary>
        /// Tests the creation of an user without e-mail.
        /// This is a valid operation and should complete without exceptions,
        /// and without validation errors.
        /// </summary>
        [TestMethod]
        public void Create_1_CreateUserWithoutEmail_HappyPath()
        {
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "milena",
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Address = new AddressViewModel
                    {
                        Street = "Nome rua",
                        CEP = "36030-060",
                        City = "Juiz de Fora",
                    },
                    IsSecretary = true,
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The controller ModelState must have no validation errors related to e-mail.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
        }

        /// <summary>
        /// Tests the creation of an user with a repeated user name.
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_2_RepeatedUserNameInSamePractice()
        {
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;
            UserViewModel viewModel;

            {
                viewModel = new UserViewModel
                {
                    UserName = "andrerpena",
                    FullName = "André Junior",
                    Gender = (int)TypeGender.Male,
                    DateOfBirth = new DateTime(1970, 11, 11),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Address = new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36000-100",
                            City = "Juiz de Fora",
                        },
                    Email = "new_email_address@not_repeated.com.xpto.br",

                    IsSecretary = true,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);
                actionResult = controller.Create(viewModel);
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(
                1,
                controller.ModelState.GetPropertyErrors(() => viewModel.UserName).Count(),
                "ModelState should contain one validation message.");
        }

        /// <summary>
        /// Tests the creation of an user with the same UserName of an user in another practice.
        /// This is a valid operation.
        /// </summary>
        [TestMethod]
        public void Create_3_RepeatedUserNameInAnotherPractice_HappyPath()
        {
            UsersController controller;
            string userNameToRepeat;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var marta = Firestarter.Create_CrmMg_Psiquiatria_DraMarta_Marta(this.db);
                userNameToRepeat = marta.Users.First().UserName;
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                var data = new UserViewModel
                {
                    UserName = userNameToRepeat,
                    FullName = "Marta Arta dos Campos",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1979, 01, 15),
                    MaritalStatus = (int)TypeMaritalStatus.Casado,
                    BirthPlace = "Juiz de Fora",
                    Address = new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "37000-200",
                            City = "Juiz de Fora",
                        },

                    Email = "new_email_address@not_repeated.com.xpto.br",
                    IsSecretary = true,
                };
                Mvc3TestHelper.SetModelStateErrors(controller, data);
                actionResult = controller.Create(data);
            }

            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
        }

        /// <summary>
        /// Tests the creation of an user without a UserName.
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_4_NoUserName()
        {
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "", // No user name.
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Address = new AddressViewModel
                    {
                        Street = "Nome rua",
                        CEP = "36030-060",
                        City = "Juiz de Fora",
                    },
                    Email = "new_email_address@not_repeated.com.xpto.br",
                    IsSecretary = true,
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(1, controller.ModelState.GetAllErrors().Count, "ModelState should contain one validation message.");
        }

        /// <summary>
        /// Tests the creation of an user without a function: not admin, not medic, not secretary... phantom user!
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_5_MustHaveAtLeastOneFunction()
        {
            // Initializing test environment.
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                mr.SetCurrentUser_Andre_CorrectPassword();
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "milena",
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Address = new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36030-060",
                            City = "Juiz de Fora",
                        },
                    Email = "new_email_address@not_repeated.com.xpto.br"
                });
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(1, controller.ModelState.GetAllErrors().Count, "ModelState should contain one validation message.");
        }

        /// <summary>
        /// Tests the creation of an user that is a medic, without the CRM.
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_6_MedicWithoutCRM()
        {
            // Initializing test environment.
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                mr.SetCurrentUser_Andre_CorrectPassword();
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                var viewModel = new UserViewModel
                {
                    UserName = "milena",
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Address = new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36030-060",
                            City = "Juiz de Fora",
                        },
                    Email = "new_email_address@not_repeated.com.xpto.br",
                    IsDoctor = true,
                    // Missing all medical information.
                    MedicCRM = "",
                    MedicalEntityId = null,
                    MedicalEntityJurisdiction = null,
                    MedicalSpecialtyId = null,
                    MedicalSpecialtyName = null,
                };

                Mvc3TestHelper.SetModelStateErrors(controller, viewModel);

                actionResult = controller.Create(viewModel);
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.AreEqual(5, controller.ModelState.GetAllErrors().Count, "ModelState should contain validation messages.");
        }

        /// <summary>
        /// Tests the creation of an user that is a medic.
        /// This is a valid operation.
        /// </summary>
        [TestMethod]
        public void Create_7_Medic_HappyPath()
        {
            // Initializing test environment.
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                mr.SetCurrentUser_Andre_CorrectPassword();
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                actionResult = controller.Create(new UserViewModel
                {
                    UserName = "milena",
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Address = new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36030-060",
                            City = "Juiz de Fora",
                        },
                    Email = "new_email_address@not_repeated.com.xpto.br",
                    IsDoctor = true,
                    MedicCRM = "98237",
                    MedicalEntityId = this.db.SYS_MedicalEntity.First().Id,
                    MedicalSpecialtyId = this.db.SYS_MedicalSpecialty.First().Id,
                    MedicalSpecialtyName = this.db.SYS_MedicalSpecialty.First().Name,
                });
            }

            // Verifying the ActionResult, and ModelState.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState should be valid.");
        }

        /// <summary>
        /// Tests the creation of an user with an invalid user-name.
        /// This is an invalid operation, and should stay in the same View, with a ModelState validation message.
        /// </summary>
        [TestMethod]
        public void Create_8_InvalidUserName()
        {
            // Initializing test environment.
            UsersController controller;
            bool hasBeenSaved = false;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                mr.SetCurrentUser_Andre_CorrectPassword();
                controller = mr.CreateController<UsersController>(
                    setupNewDb: db => db.SavingChanges += (s, e) => { hasBeenSaved = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user with the same UserName of another user in the same practice.
            ActionResult actionResult;

            {
                var data = new UserViewModel
                {
                    UserName = "milena#santos", // char '#' is not valid
                    FullName = "Milena",
                    Gender = (int)TypeGender.Female,
                    DateOfBirth = new DateTime(1986, 01, 03),
                    MaritalStatus = (int)TypeMaritalStatus.Solteiro,
                    BirthPlace = "Juiz de Fora",
                    Address = new AddressViewModel
                        {
                            Street = "Nome rua",
                            CEP = "36030-060",
                            City = "Juiz de Fora",
                        },
                    Email = "new_email_address@not_repeated.com.xpto.br"
                };
                Mvc3TestHelper.SetModelStateErrors(controller, data);
                actionResult = controller.Create(data);
            }

            // Verifying the ActionResult, and the DB.
            // - The result must be a ViewResult, with the name "Edit".
            // - The controller ModelState must have one validation message.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState should not be valid.");
            Assert.IsTrue(
                controller.ModelState.ContainsKey("UserName"),
                "ModelState must contain validation message for 'PracticeName'.");
            Assert.IsFalse(hasBeenSaved, "The database has been changed. This was not supposed to happen.");
        }
        #endregion

        #region Edit
        /// <summary>
        /// Tests the edition of an user.
        /// This is a valid operation and should complete without exceptions,
        /// and without validation errors.
        /// </summary>
        [TestMethod]
        public void Edit_1_EditUser_HappyPath()
        {
            UsersController controller;
            UserViewModel vm;
            bool isDbSaved = false;
            try
            {
                var doc = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>(
                    setupNewDb: db => db.SavingChanges += (s, e) => { isDbSaved = true; });

                var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doc);
                var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doc);

                vm = UsersController.GetViewModel(doc.Users.First(), doc.Users.FirstOrDefault().Practice, medicalEntity, medicalSpecialty);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing the user FullName.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                // When editing, the user-name cannot be changed. Must set it to null.
                vm.UserName = null;

                vm.FullName = "Andre R. Pena";

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                actionResult = controller.Edit(vm);
            }

            // Verifying the ActionResult, and the DB.
            // - The controller ModelState must have no validation errors related to e-mail.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsTrue(isDbSaved, "DB changes must be saved.");
        }

        /// <summary>
        /// Tests the edition of an user that is a doctor.
        /// This is a valid operation and should complete without exceptions,
        /// and without validation errors.
        /// </summary>
        [TestMethod]
        public void Edit_EditDoctor_HappyPath()
        {
            UsersController controller;
            UserViewModel vm;
            bool isDbSaved = false;
            try
            {
                var doc = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>(
                    setupNewDb: db => db.SavingChanges += (s, e) => { isDbSaved = true; });

                var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doc);
                var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doc);
                vm = UsersController.GetViewModel(doc.Users.First(), doc.Users.FirstOrDefault().Practice, medicalEntity, medicalSpecialty);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing the user FullName.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                // When editing, the user-name cannot be changed. Must set it to null.
                var me = Firestarter.GetMedicalEntity_Fono(this.db);
                var ms = Firestarter.GetMedicalSpecialty_Fonoaudiólogo(this.db);

                vm.UserName = null;

                vm.FullName = "Andre R. Pena";

                vm.MedicalEntityJurisdiction = (int)TypeEstadoBrasileiro.GO;
                vm.MedicCRM = "131192";
                vm.MedicalEntityId = me.Id;
                vm.MedicalSpecialtyId = ms.Id;
                vm.MedicalSpecialtyName = ms.Name;

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                actionResult = controller.Edit(vm);
            }

            // Verifying the ActionResult, and the DB.
            // - The controller ModelState must have no validation errors related to e-mail.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsTrue(isDbSaved, "DB changes must be saved.");

            // Assert database.
            using (var dba = CreateNewCerebelloEntities())
            {
                var user = dba.Users.Single(u => u.Person.FullName == "Andre R. Pena");
                Assert.IsNotNull(user.Doctor);
                Assert.AreEqual("131192", user.Doctor.CRM);
                Assert.AreEqual("CRFA", user.Doctor.MedicalEntityCode);
                Assert.AreEqual("Conselho Regional de Fonoaudiologia", user.Doctor.MedicalEntityName);
                Assert.AreEqual("GO", user.Doctor.MedicalEntityJurisdiction);
                Assert.AreEqual("2238.10", user.Doctor.MedicalSpecialtyCode);
                Assert.AreEqual("Fonoaudiólogo", user.Doctor.MedicalSpecialtyName);
            }

        }

        /// <summary>
        /// Tests the edition of an user, without changing a thing.
        /// This is a valid operation and should complete without exceptions,
        /// and without validation errors.
        /// </summary>
        [TestMethod]
        public void Edit_EditUser_NoChanges_HappyPath()
        {
            UsersController controller;
            UserViewModel vm;
            bool isDbSaved = false;
            Doctor doc;
            try
            {
                doc = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>(
                    setupNewDb: db => db.SavingChanges += (s, e) => { isDbSaved = true; });

                var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doc);
                var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doc);
                vm = UsersController.GetViewModel(doc.Users.First(), doc.Users.FirstOrDefault().Practice, medicalEntity, medicalSpecialty);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing the user FullName.
            // This must be ok, no exceptions, no validation errors.
            ActionResult actionResult;

            {
                // When editing, the user-name cannot be changed. Must set it to null.
                vm.UserName = null;

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                actionResult = controller.Edit(vm);
            }

            // Verifying the ActionResult, and the DB.
            // - The controller ModelState must have no validation errors related to e-mail.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsTrue(controller.ModelState.IsValid, "ModelState is not valid.");
            Assert.IsTrue(isDbSaved, "DB changes must be saved.");

            // Doctor UrlIdentifier must be the same as before.
            using (var db2 = new CerebelloEntities(string.Format("name={0}", Constants.CONNECTION_STRING_EF)))
            {
                var doc2 = db2.Doctors.Where(d => d.Id == doc.Id).First();
                Assert.AreEqual(doc.UrlIdentifier, doc2.UrlIdentifier);
            }
        }

        /// <summary>
        /// Tests the edition of an user, removing the value of a required field.
        /// This is an invalid operation and should have validation errors.
        /// </summary>
        [TestMethod]
        public void Edit_2_EditUserRemovingMedicalEntityJurisdiction()
        {
            UsersController controller;
            UserViewModel vm;
            bool isDbSaved = false;
            try
            {
                var doc = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>(
                    setupNewDb: db => db.SavingChanges += (s, e) => { isDbSaved = true; });

                var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doc);
                var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doc);
                vm = UsersController.GetViewModel(doc.Users.First(), doc.Users.FirstOrDefault().Practice, medicalEntity, medicalSpecialty);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing the user MedicalEntityJurisdiction, by removing the value.
            // This is not valid as this user is a doctor, and that property is required for doctors.
            ActionResult actionResult;

            {
                // When editing, the user-name cannot be changed. Must set it to null.
                vm.UserName = null;

                vm.MedicalEntityJurisdiction = null;

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                actionResult = controller.Edit(vm);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.IsInstanceOfType(viewResult.Model, typeof(UserViewModel));
            var resultViewModel = (UserViewModel)viewResult.Model;
            Assert.AreEqual("andrerpena", resultViewModel.UserName);
            Assert.IsInstanceOfType(controller.ViewBag.MedicalSpecialtyOptions, typeof(IEnumerable<SelectListItem>));
            Assert.IsInstanceOfType(controller.ViewBag.MedicalEntityOptions, typeof(IEnumerable<SelectListItem>));
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState must be invalid.");
            Assert.IsFalse(isDbSaved, "DB changes must not be saved.");
        }

        /// <summary>
        /// Tests the edition of an user, removing the administrator role from the owner of the account.
        /// This is an invalid operation.
        /// </summary>
        [TestMethod]
        public void Edit_EditOwnerRemovingAdministratorRole()
        {
            UsersController controller;
            UserViewModel vm;
            bool isDbSaved = false;
            try
            {
                var doc = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(this.db);
                var mr = new MockRepository(true);
                mr.SetCurrentUser(doc[1].Users.First(), "masban");

                var medicalEntity = UsersController.GetDoctorEntity(this.db.SYS_MedicalEntity, doc[0]);
                var medicalSpecialty = UsersController.GetDoctorSpecialty(this.db.SYS_MedicalSpecialty, doc[0]);
                vm = UsersController.GetViewModel(doc[0].Users.First(), doc[0].Users.FirstOrDefault().Practice, medicalEntity, medicalSpecialty);

                controller = mr.CreateController<UsersController>(
                    setupNewDb: db => db.SavingChanges += (s, e) => { isDbSaved = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing the user MedicalEntityJurisdiction, by removing the value.
            // This is not valid as this user is a doctor, and that property is required for doctors.
            ActionResult actionResult;

            {
                // When editing, the user-name cannot be changed. Must set it to null.
                vm.UserName = null;

                vm.IsAdministrador = false;

                Mvc3TestHelper.SetModelStateErrors(controller, vm);

                actionResult = controller.Edit(vm);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            var viewResult = (ViewResult)actionResult;
            Assert.AreEqual("Edit", viewResult.ViewName);
            Assert.IsInstanceOfType(viewResult.Model, typeof(UserViewModel));
            var resultViewModel = (UserViewModel)viewResult.Model;
            Assert.AreEqual("andrerpena", resultViewModel.UserName);
            Assert.IsInstanceOfType(controller.ViewBag.MedicalSpecialtyOptions, typeof(IEnumerable<SelectListItem>));
            Assert.IsInstanceOfType(controller.ViewBag.MedicalEntityOptions, typeof(IEnumerable<SelectListItem>));
            Assert.IsFalse(controller.ModelState.IsValid, "ModelState must be invalid.");
            Assert.AreEqual(1, controller.ModelState.GetPropertyErrors(() => vm.IsAdministrador).Count);

            Assert.IsFalse(isDbSaved, "DB changes must not be saved.");
        }
        #endregion

        #region Details
        /// <summary>
        /// Tests the visualization of a secretary.
        /// </summary>
        [TestMethod]
        public void Details_1_ViewSecretary_HappyPath()
        {
            UsersController controller;
            int userId;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var s = Firestarter.CreateSecretary_Milena(this.db, this.db.Practices.ToList().Last());
                userId = s.Users.Single().Id;
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Creating a new user without an e-mail.
            // This must be ok, no exceptions, no validation errors.
            {
                ActionResult actionResult = controller.Details(userId);

                // Verifying the ActionResult, ModelState and Model.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
                var viewResult = (ViewResult)actionResult;
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserViewModel));
                var model = (UserViewModel)viewResult.Model;
                Assert.IsTrue(model.IsSecretary, "User is not secretary.");
                Assert.IsTrue(viewResult.ViewName == "Details" || viewResult.ViewName == "", "Wrong view name.");
                Assert.IsTrue(controller.ModelState.IsValid);
            }
        }

        /// <summary>
        /// Tests the visualization of a medic.
        /// </summary>
        [TestMethod]
        public void Details_2_ViewMedic_HappyPath()
        {
            // Initializing test.
            int userId;
            UsersController controller;
            try
            {
                var medic = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                userId = medic.Users.Single().Id;
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>();
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // This must be ok, no exceptions, no validation errors.
            {
                ActionResult actionResult = controller.Details(userId);

                // Verifying the ActionResult and ModelState.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
                var viewResult = (ViewResult)actionResult;
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserViewModel));
                var model = (UserViewModel)viewResult.Model;
                Assert.IsTrue(model.IsDoctor, "User is not medic.");
                Assert.IsTrue(viewResult.ViewName == "Details" || viewResult.ViewName == "", "Wrong view name.");
                Assert.IsTrue(controller.ModelState.IsValid);
            }
        }

        /// <summary>
        /// Tests the visualization of an administrator.
        /// </summary>
        [TestMethod]
        public void Details_3_ViewAdministrator_HappyPath()
        {
            // Initializing test.
            int userId;
            UsersController controller;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(this.db);
                var admin = this.db.Users.Where(m => m.AdministratorId != null).First();
                var mr = new MockRepository(true);
                controller = mr.CreateController<UsersController>();
                userId = admin.Id;
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // This must be ok, no exceptions, no validation errors.
            {
                ActionResult actionResult = controller.Details(userId);

                // Verifying the ActionResult and ModelState.
                Assert.IsNotNull(actionResult, "The result of the controller method is null.");
                Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
                var viewResult = (ViewResult)actionResult;
                Assert.IsInstanceOfType(viewResult.Model, typeof(UserViewModel));
                var model = (UserViewModel)viewResult.Model;
                Assert.IsTrue(model.IsAdministrador, "User is not administrator.");
                Assert.IsTrue(viewResult.ViewName == "Details" || viewResult.ViewName == "", "Wrong view name.");
                Assert.IsTrue(controller.ModelState.IsValid);
            }
        }
        #endregion

        #region ChangePassword
        /// <summary>
        /// Tests the access to the ChangePassword action, when using the default password.
        /// This is a valid action, and shoud go straight to the "ChangePassword" view.
        /// </summary>
        [TestMethod]
        public void ChangePassword_1_UserWithDefaultPassword_HappyPath()
        {
            // Initializing.
            UsersController controller;
            MockRepository mr;
            int userId;
            try
            {
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var practice = this.db.Practices.FirstOrDefault();
                var s = Firestarter.CreateSecretary_Milena(this.db, practice, useDefaultPassword: true);
                var user = s.Users.Single();
                userId = user.Id;

                mr = new MockRepository(true);
                mr.SetCurrentUser_WithDefaultPassword(user, loginWithUserName: true);
                mr.SetRouteData<UsersController>(practice, null, "changepassword");

                controller = mr.CreateController<UsersController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Testing.
            ActionResult actionResult;

            {
                actionResult =
                    Mvc3TestHelper.RunOnActionExecuting(controller, mr)
                    ?? controller.ChangePassword();
            }

            // Asserting.
            Assert.IsNotNull(actionResult, "ActionResult must not be null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)actionResult;
            Assert.AreEqual("", viewResult.ViewName);
            Assert.IsTrue(viewResult.ViewBag.IsDefaultPassword == true);
        }

        /// <summary>
        /// Tests the access to the ChangePassword action, by a normal user.
        /// This is a valid action, and shoud go straight to the "ChangePassword" view.
        /// </summary>
        [TestMethod]
        public void ChangePassword_2_UserWantsToChangePassword_HappyPath()
        {
            // Initializing.
            UsersController controller;
            int userId;
            MockRepository mr;
            try
            {
                var d = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var practice = this.db.Practices.FirstOrDefault();
                var user = d.Users.Single();
                userId = user.Id;
                mr = new MockRepository(true);
                mr.SetCurrentUser_Andre_CorrectPassword(userId);
                mr.SetRouteData<UsersController>(practice, null, "changepassword");

                controller = mr.CreateController<UsersController>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            var globalFilters = new GlobalFilterCollection();
            MvcApplication.RegisterGlobalFilters(globalFilters);
            var filters = globalFilters.OrderBy(gf => gf.Scope).ThenBy(gf => gf.Order).Select(gf => gf.Instance).ToList();
            var filterToTest = filters.OfType<FirstAccessFilter>().SingleOrDefault();

            // Global filter must be registered.
            Assert.IsTrue(filterToTest != null, "FirstAccessFilter is not being registered as a global filter.");

            // Testing.
            ActionResult actionResult;

            {
                // Executing filter.
                AuthorizationContext authContext = new AuthorizationContext();
                authContext.HttpContext = mr.GetHttpContext();
                authContext.RouteData = mr.RouteData;
                filterToTest.OnAuthorization(authContext);

                actionResult =
                    authContext.Result
                    ?? Mvc3TestHelper.RunOnActionExecuting(controller, mr)
                    ?? controller.ChangePassword();
            }

            // Asserting.
            Assert.IsNotNull(actionResult, "ActionResult must not be null.");
            Assert.IsInstanceOfType(actionResult, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)actionResult;
            Assert.AreEqual("", viewResult.ViewName);
            Assert.IsTrue(viewResult.ViewBag.IsDefaultPassword == false);
        }

        /// <summary>
        /// Tests some cases of the user trying to access the software using the default password.
        /// These are invalid actions, they must redirect the user back to the "ChangePassword" view.
        /// </summary>
        [TestMethod]
        public void ChangePassword_3_AllActionsMustRedirectToChangePasswordWhenUserIsUsingDefaultPassword()
        {
            // Initializing.
            Practice practice;
            Doctor docToView;
            MockRepository mr;
            try
            {
                docToView = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                practice = this.db.Practices.FirstOrDefault();

                var d = Firestarter.CreateAdministratorDoctor_Miguel(
                    this.db,
                    this.db.SYS_MedicalEntity.FirstOrDefault(),
                    this.db.SYS_MedicalSpecialty.FirstOrDefault(),
                    practice,
                    useDefaultPassword: true);

                var user = d.Users.Single();
                var userId = user.Id;

                mr = new MockRepository(true);
                mr.SetCurrentUser_WithDefaultPassword(user, loginWithUserName: true);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            var globalFilters = new GlobalFilterCollection();
            MvcApplication.RegisterGlobalFilters(globalFilters);
            var filters = globalFilters.OrderBy(gf => gf.Scope).ThenBy(gf => gf.Order).Select(gf => gf.Instance).ToList();
            var filterToTest = filters.OfType<FirstAccessFilter>().SingleOrDefault();

            // Global filter must be registered.
            Assert.IsTrue(filterToTest != null, "FirstAccessFilter is not being registered as a global filter.");

            // The filter must deny access to everything but the User/ChangePassword screen.

            // Testing UserController.
            TestChangePassword_3_Helper<UsersController>(mr, practice, docToView, "index", filterToTest);
            TestChangePassword_3_Helper<UsersController>(mr, practice, docToView, "details", filterToTest);
            TestChangePassword_3_Helper<UsersController>(mr, practice, docToView, "create", filterToTest);
            TestChangePassword_3_Helper<UsersController>(mr, practice, docToView, "edit", filterToTest);
            TestChangePassword_3_Helper<UsersController>(mr, practice, docToView, "delete", filterToTest);

            // Testing other controllers, for which access should be denied.
            TestChangePassword_3_Helper<DoctorHomeController>(mr, practice, docToView, "index", filterToTest);
            TestChangePassword_3_Helper<PatientsController>(mr, practice, docToView, "index", filterToTest);
            TestChangePassword_3_Helper<AppController>(mr, practice, docToView, "lookupeverything", filterToTest);
            TestChangePassword_3_Helper<AnamnesesController>(mr, practice, docToView, "details", filterToTest);
        }

        private void TestChangePassword_3_Helper<T>(MockRepository mr, Practice practice, Doctor docToView, string action, IAuthorizationFilter filterToTest) where T : Controller, new()
        {
            T controller;

            try
            {
                mr.SetRouteData<T>(practice, docToView, action);
                controller = mr.CreateController<T>(callOnActionExecuting: false);
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Testing.
            AuthorizationContext authContext = new AuthorizationContext();
            authContext.HttpContext = mr.GetHttpContext();
            authContext.RouteData = mr.RouteData;
            filterToTest.OnAuthorization(authContext);

            var result = authContext.Result;

            Assert.IsNotNull(result, "ActionResult must not be null.");
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult viewResult = (RedirectToRouteResult)result;
            Assert.AreEqual(3, viewResult.RouteValues.Count);
            Assert.AreEqual("app", string.Format("{0}", viewResult.RouteValues["area"]), ignoreCase: true);
            Assert.AreEqual("users", string.Format("{0}", viewResult.RouteValues["controller"]), ignoreCase: true);
            Assert.AreEqual("changepassword", string.Format("{0}", viewResult.RouteValues["action"]), ignoreCase: true);
        }

        #endregion

        #region Delete
        [TestMethod]
        public void Delete_SecretaryDeletedByAdministrator_HappyPath()
        {
            UsersController controller;
            int idToDelete;
            bool isDbSaved = false;
            try
            {
                var docs = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(this.db);
                var mr = new MockRepository(true);

                var secretary1 = Firestarter.CreateSecretary_Milena(this.db, docs[0].Users.First().Practice);
                idToDelete = secretary1.Users.Single().Id;

                mr.SetCurrentUser(docs[1].Users.First(), "masban");

                controller = mr.CreateController<UsersController>(
                    setupNewDb: db1 => db1.SavingChanges += (s, e) => { isDbSaved = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing the user MedicalEntityJurisdiction, by removing the value.
            // This is not valid as this user is a doctor, and that property is required for doctors.
            ActionResult actionResult;

            {
                actionResult = controller.Delete(idToDelete);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var jsonResult = (JsonResult)actionResult;

            Assert.IsInstanceOfType(jsonResult.Data, typeof(JsonDeleteMessage));
            var jsonDeleteMsg = (JsonDeleteMessage)jsonResult.Data;

            Assert.IsTrue(jsonDeleteMsg.success, "Must succed when administrator deletes secretary.");

            Assert.IsTrue(controller.ModelState.IsValid, "ModelState must be valid.");
            Assert.IsTrue(isDbSaved, "DB changes must be saved.");
        }

        [TestMethod]
        public void Delete_SecretaryDeletedByAnotherSecretary()
        {
            UsersController controller;
            int idToDelete;
            bool isDbSaved = false;
            try
            {
                var docs = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(this.db);
                var mr = new MockRepository(true);

                var secretary1 = Firestarter.CreateSecretary_Milena(db, docs[0].Users.First().Practice);
                var secretary2 = Firestarter.CreateSecretary_Maricleusa(db, docs[0].Users.First().Practice);
                idToDelete = secretary2.Users.Single().Id;

                mr.SetCurrentUser(secretary1.Users.Single(), "milena");
                mr.IsAjax = true;

                controller = mr.CreateController<UsersController>(
                    setupNewDb: db1 => db1.SavingChanges += (s, e) => { isDbSaved = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing the user MedicalEntityJurisdiction, by removing the value.
            // This is not valid as this user is a doctor, and that property is required for doctors.
            ActionResult actionResult;

            {
                actionResult =
                    Mvc3TestHelper.RunOnAuthorization(controller, "Delete")
                    ?? controller.Delete(idToDelete);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var jsonResult = (JsonResult)actionResult;

            var jsonDeleteMsg = (dynamic)jsonResult.Data;

            Assert.IsFalse(jsonDeleteMsg.success, "Must not succed... only administrators have the right to delete another user.");
            Assert.AreEqual("Você não tem permissão para excluir um usuário.", jsonDeleteMsg.text);

            Assert.IsTrue(controller.ModelState.IsValid, "In this case, the model must be Ok... the wrong thing is the logged user.");
            Assert.IsFalse(isDbSaved, "DB changes must not be saved.");
        }

        [TestMethod]
        public void Delete_OwnerDeletedByAdministrator()
        {
            UsersController controller;
            int idToDelete;
            bool isDbSaved = false;
            try
            {
                var docs = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre_Miguel(this.db);
                var mr = new MockRepository(true);
                idToDelete = docs[0].Users.First().Id;

                mr.SetCurrentUser(docs[1].Users.First(), "masban");

                controller = mr.CreateController<UsersController>(
                    setupNewDb: db => db.SavingChanges += (s, e) => { isDbSaved = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing the user MedicalEntityJurisdiction, by removing the value.
            // This is not valid as this user is a doctor, and that property is required for doctors.
            ActionResult actionResult;

            {
                actionResult = controller.Delete(idToDelete);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var jsonResult = (JsonResult)actionResult;

            Assert.IsInstanceOfType(jsonResult.Data, typeof(JsonDeleteMessage));
            var jsonDeleteMsg = (JsonDeleteMessage)jsonResult.Data;

            Assert.IsFalse(jsonDeleteMsg.success, "Must not succed when deleting the owner of the account.");
            Assert.AreEqual("Não é possível excluir o usuário que é proprietário da conta.", jsonDeleteMsg.text);

            Assert.IsFalse(controller.ModelState.IsValid, "ModelState must not be valid.");
            Assert.IsFalse(isDbSaved, "DB changes must not be saved.");
        }

        [TestMethod]
        public void Delete_OwnerSelfDelete()
        {
            UsersController controller;
            int idToDelete;
            bool isDbSaved = false;
            try
            {
                var doc = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
                var mr = new MockRepository(true);
                idToDelete = doc.Users.First().Id;

                controller = mr.CreateController<UsersController>(
                    setupNewDb: db => db.SavingChanges += (s, e) => { isDbSaved = true; });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // Editing the user MedicalEntityJurisdiction, by removing the value.
            // This is not valid as this user is a doctor, and that property is required for doctors.
            ActionResult actionResult;

            {
                actionResult = controller.Delete(idToDelete);
            }

            // Verifying the ActionResult, and the DB.
            Assert.IsNotNull(actionResult, "The result of the controller method is null.");
            Assert.IsInstanceOfType(actionResult, typeof(JsonResult));
            var jsonResult = (JsonResult)actionResult;

            Assert.IsInstanceOfType(jsonResult.Data, typeof(JsonDeleteMessage));
            var jsonDeleteMsg = (JsonDeleteMessage)jsonResult.Data;

            Assert.IsFalse(jsonDeleteMsg.success, "Must not succed when deleting the owner of the account.");
            Assert.AreEqual("Não é possível excluir o usuário que é proprietário da conta.", jsonDeleteMsg.text);

            Assert.IsFalse(controller.ModelState.IsValid, "ModelState must not be valid.");
            Assert.IsFalse(isDbSaved, "DB changes must not be saved.");
        }
        #endregion
    }
}
