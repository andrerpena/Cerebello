using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Firestarter;
using Cerebello.Model;
using CerebelloWebRole.Areas.App.Controllers;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CerebelloWebRole.Tests.Tests
{
    [TestClass]
    public class MedicinesControllerTests : DbTestBase
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

        #region Search

        [TestMethod]
        public void Search_ShouldReturnEverythingInEmptySearch()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();

                controller.Create(
                    new MedicineViewModel()
                        {
                            Name = "Pristiq",
                            LaboratoryName = "MyLab",
                            Usage = (int)TypeUsage.Cutaneo,
                            ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                                {
                                    new MedicineActiveIngredientViewModel() {Name = "P1"},
                                    new MedicineActiveIngredientViewModel() {Name = "P2"}
                                }
                        });

                controller.Create(
                    new MedicineViewModel()
                        {
                            Name = "Novalgina",
                            LaboratoryName = "MyLab2",
                            Usage = (int)TypeUsage.Cutaneo,
                            ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                                {
                                    new MedicineActiveIngredientViewModel() {Name = "P1"},
                                }
                        });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            // making an empty search
            var result = controller.Search(
                new Areas.App.Models.SearchModel()
                    {
                        Term = "",
                        Page = 1
                    });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Debug.Assert(resultAsView != null, "resultAsView must not be null");
            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<MedicineViewModel>));
            var model = resultAsView.Model as SearchViewModel<MedicineViewModel>;

            Debug.Assert(model != null, "model must not be null");
            Assert.AreEqual(2, model.Count);
        }

        [TestMethod]
        public void Search_ShouldRespectTheSearchTermWhenItsPresent()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();

                controller.Create(
                    new MedicineViewModel()
                        {
                            Name = "Pristiq",
                            LaboratoryName = "MyLab",
                            Usage = (int)TypeUsage.Cutaneo,
                            ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                                {
                                    new MedicineActiveIngredientViewModel() {Name = "P1"},
                                    new MedicineActiveIngredientViewModel() {Name = "P2"}
                                }
                        });

                controller.Create(
                    new MedicineViewModel()
                        {
                            Name = "Novalgina",
                            LaboratoryName = "MyLab2",
                            Usage = (int)TypeUsage.Cutaneo,
                            ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                                {
                                    new MedicineActiveIngredientViewModel() {Name = "P1"},
                                }
                        });
            }
            catch (Exception ex)
            {
                InconclusiveInit(ex);
                return;
            }

            const string searchTerm = "nova";

            // making an empty search
            var result = controller.Search(
                new Areas.App.Models.SearchModel()
                    {
                        Term = searchTerm,
                        Page = 1
                    });

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var resultAsView = result as ViewResult;

            Debug.Assert(resultAsView != null, "resultAsView must not null");
            Assert.IsInstanceOfType(resultAsView.Model, typeof(SearchViewModel<MedicineViewModel>));
            var model = resultAsView.Model as SearchViewModel<MedicineViewModel>;

            Debug.Assert(model != null, "model must not be null");
            Assert.AreEqual(1, model.Count);
        }

        #endregion

        #region Edit

        /// <summary>
        /// It must be possible to save a medicine just by passing the minimum information
        /// </summary>
        [TestMethod]
        public void AnvisaImport_HappyPath()
        {
            MedicinesController controller;
            SYS_Medicine sysMedicine;
            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();

                sysMedicine = this.db.SYS_Medicine.FirstOrDefault();
                if (sysMedicine == null)
                    throw new Exception("SYS_Medicines are not populated");
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            controller.AnvisaImport(
                new AnvisaImportViewModel()
                {
                    AnvisaId = sysMedicine.Id,
                    AnvisaText = sysMedicine.Name
                });

            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == sysMedicine.Name);
            Assert.IsNotNull(medicine);

            foreach (var activeIngredient in medicine.ActiveIngredients)
                Assert.IsTrue(sysMedicine.ActiveIngredients.Any(ai => ai.Name == activeIngredient.Name));

            foreach (var leaflet in medicine.Leaflets)
                Assert.IsTrue(sysMedicine.Leaflets.Any(l => l.Url == leaflet.Url));

            Assert.IsTrue(sysMedicine.Laboratory.Name == medicine.Laboratory.Name);
        }

        /// <summary>
        /// It must be possible to save a medicine just by passing the minimum information
        /// </summary>
        [TestMethod]
        public void Edit_MinimalInformation_HappyPath()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";
            controller.Create(
                new MedicineViewModel()
                    {
                        Name = medicineName
                    });

            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.IsNotNull(medicine);
        }

        /// <summary>
        /// It must be possible to save a medicine just by passing the minimum information
        /// </summary>
        [TestMethod]
        public void Edit_PassingInInvalidActiveIngredient()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";
            controller.Create(
                new MedicineViewModel()
                {
                    Name = medicineName,
                    ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                        {
                            new MedicineActiveIngredientViewModel()
                                {
                                    Id = 89
                                }
                        }
                });

            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(1, controller.ModelState["ActiveIngredients"].Errors.Count);
        }

        /// <summary>
        /// It must be possible to save a medicine just by passing the minimum information
        /// </summary>
        [TestMethod]
        public void Edit_PassingInInvalidLaboratory()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";
            controller.Create(
                new MedicineViewModel()
                {
                    Name = medicineName,
                    // INVALID LAB
                    LaboratoryId = 98
                });

            Assert.IsFalse(controller.ModelState.IsValid);
            Assert.AreEqual(1, controller.ModelState["LaboratoryId"].Errors.Count);
        }


        [TestMethod]
        public void Edit_AddingNewActiveIngredient()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";

            // all these active ingredients have already been created. 
            // now we have to associate them with the original active ingredients

            var formModel = new MedicineViewModel()
            {
                Name = medicineName,
                ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                        {
                            new MedicineActiveIngredientViewModel()
                                {
                                    Name = "AI1"
                                },
                            new MedicineActiveIngredientViewModel()
                                {
                                    Name = "AI2"
                                }
                        }
            };
            controller.Create(formModel);


            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.IsNotNull(medicine);
            Assert.AreEqual(2, medicine.ActiveIngredients.Count);

            // verify that all the active ingredients inside the medicine are those that
            // we've created here
            Assert.AreEqual(formModel.ActiveIngredients[0].Name, medicine.ActiveIngredients.ElementAt(0).Name);
            Assert.AreEqual(formModel.ActiveIngredients[1].Name, medicine.ActiveIngredients.ElementAt(1).Name);
        }

        [TestMethod]
        public void Edit_UpdatingExistingActiveIngredient()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";

            // all these active ingredients have already been created. 
            // now we have to associate them with the original active ingredients

            var formModel = new MedicineViewModel()
            {
                Name = medicineName,
                ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                        {
                            new MedicineActiveIngredientViewModel()
                                {
                                   Name = "AI1"
                                }
                        }
            };
            controller.Create(formModel);

            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.IsNotNull(medicine);
            Assert.AreEqual(1, medicine.ActiveIngredients.Count);

            formModel.Id = medicine.Id;
            formModel.ActiveIngredients[0].Id = medicine.ActiveIngredients.ElementAt(0).Id;
            formModel.ActiveIngredients[0].Name = "AI2";

            // Let's edit now and change some properties
            controller.Edit(formModel);

            // we need to refresh since the DB inside the controller is different from this
            this.db.Refresh(RefreshMode.StoreWins, medicine.ActiveIngredients);

            // verify that all the active ingredients inside the medicine are those that
            // we've EDITED here
            Assert.AreEqual(formModel.ActiveIngredients[0].Name, medicine.ActiveIngredients.ElementAt(0).Name);
        }

        [TestMethod]
        public void Edit_RemovingExistingActiveIngredient()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";

            // all these active ingredients have already been created. 
            // now we have to associate them with the original active ingredients

            var formModel = new MedicineViewModel()
            {
                Name = medicineName,
                ActiveIngredients = new List<MedicineActiveIngredientViewModel>()
                        {
                            new MedicineActiveIngredientViewModel()
                                {
                                    Name = "AI1",
                                },
                            new MedicineActiveIngredientViewModel()
                                {
                                    Name = "AI2",
                                }
                        }
            };
            controller.Create(formModel);


            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.IsNotNull(medicine);
            Assert.AreEqual(2, medicine.ActiveIngredients.Count);

            // let's put the formModel in edit mode and remove the second leaflet
            formModel.Id = medicine.Id;
            formModel.ActiveIngredients[0].Id = medicine.ActiveIngredients.ElementAt(0).Id;
            formModel.ActiveIngredients.RemoveAt(1);

            // Let's edit 
            controller.Edit(formModel);

            // we need to refresh since the DB inside the controller is different from this
            this.db.Refresh(RefreshMode.StoreWins, medicine.ActiveIngredients);

            Assert.AreEqual(1, medicine.ActiveIngredients.Count);

            // verify that all the active ingredients inside the medicine are those that
            // we've created here
            Assert.AreEqual(formModel.ActiveIngredients[0].Name, medicine.ActiveIngredients.ElementAt(0).Name);
        }

        [TestMethod]
        public void Edit_AddingNewLeaflet()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";

            // all these active ingredients have already been created. 
            // now we have to associate them with the original active ingredients

            var formModel = new MedicineViewModel()
                {
                    Name = medicineName,
                    Leaflets = new List<MedicineLeafletViewModel>()
                        {
                            new MedicineLeafletViewModel()
                                {
                                    Url = "http:\\www.google.com",
                                    Description = "desc 1"
                                },
                            new MedicineLeafletViewModel()
                                {
                                    Url = "http:\\www.google.com",
                                    Description = "desc 2"
                                }
                        }
                };
            controller.Create(formModel);


            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.IsNotNull(medicine);
            Assert.AreEqual(2, medicine.ActiveIngredients.Count);

            // verify that all the active ingredients inside the medicine are those that
            // we've created here
            Assert.AreEqual(formModel.Leaflets[0].Url, medicine.Leaflets.ElementAt(0).Url);
            Assert.AreEqual(formModel.Leaflets[0].Description, medicine.Leaflets.ElementAt(0).Description);
            Assert.AreEqual(formModel.Leaflets[1].Url, medicine.Leaflets.ElementAt(1).Url);
            Assert.AreEqual(formModel.Leaflets[1].Description, medicine.Leaflets.ElementAt(1).Description);
        }

        [TestMethod]
        public void Edit_UpdatingExistingLeaflets()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";

            // all these active ingredients have already been created. 
            // now we have to associate them with the original active ingredients

            var formModel = new MedicineViewModel()
            {
                Name = medicineName,
                Leaflets = new List<MedicineLeafletViewModel>()
                        {
                            new MedicineLeafletViewModel()
                                {
                                    Url = "http://www.google.com",
                                    Description = "desc 1"
                                }
                        }
            };
            controller.Create(formModel);

            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.IsNotNull(medicine);
            Assert.AreEqual(1, medicine.Leaflets.Count);

            formModel.Id = medicine.Id;
            formModel.Leaflets[0].Id = medicine.Leaflets.ElementAt(0).Id;
            formModel.Leaflets[0].Url = "http://www.facebook.com";
            formModel.Leaflets[0].Description = "desc 2";

            // Let's edit now and change some properties
            controller.Edit(formModel);

            // we need to refresh since the DB inside the controller is different from this
            this.db.Refresh(RefreshMode.StoreWins, medicine.Leaflets);

            // verify that all the active ingredients inside the medicine are those that
            // we've EDITED here
            Assert.AreEqual(formModel.Leaflets[0].Url, medicine.Leaflets.ElementAt(0).Url);
            Assert.AreEqual(formModel.Leaflets[0].Description, medicine.Leaflets.ElementAt(0).Description);
        }

        [TestMethod]
        public void Edit_RemovingExistingLeaflets()
        {
            MedicinesController controller;

            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";

            // all these active ingredients have already been created. 
            // now we have to associate them with the original active ingredients

            var formModel = new MedicineViewModel()
            {
                Name = medicineName,
                Leaflets = new List<MedicineLeafletViewModel>()
                        {
                            new MedicineLeafletViewModel()
                                {
                                    Url = "http:\\www.google.com",
                                    Description = "desc 1"
                                },
                            new MedicineLeafletViewModel()
                                {
                                    Url = "http:\\www.google.com",
                                    Description = "desc 2"
                                }
                        }
            };
            controller.Create(formModel);


            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.IsNotNull(medicine);
            Assert.AreEqual(2, medicine.Leaflets.Count);

            // let's put the formModel in edit mode and remove the second leaflet
            formModel.Id = medicine.Id;
            formModel.Leaflets[0].Id = medicine.Leaflets.ElementAt(0).Id;
            formModel.Leaflets.RemoveAt(1);

            // Let's edit 
            controller.Edit(formModel);

            // we need to refresh since the DB inside the controller is different from this
            this.db.Refresh(RefreshMode.StoreWins, medicine.Leaflets);

            Assert.AreEqual(1, medicine.Leaflets.Count);

            // verify that all the active ingredients inside the medicine are those that
            // we've created here
            Assert.AreEqual(formModel.Leaflets[0].Url, medicine.Leaflets.ElementAt(0).Url);
            Assert.AreEqual(formModel.Leaflets[0].Description, medicine.Leaflets.ElementAt(0).Description);
        }

        #endregion
    }
}

