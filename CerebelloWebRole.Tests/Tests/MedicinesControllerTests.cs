using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Diagnostics;
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
                var doctor = Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
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
                                    new MedicineActiveIngredientViewModel() {ActiveIngredientName = "P1"},
                                    new MedicineActiveIngredientViewModel() {ActiveIngredientName = "P2"}
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
                                    new MedicineActiveIngredientViewModel() {ActiveIngredientName = "P1"},
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
                Firestarter.Create_CrmMg_Psiquiatria_DrHouse_Andre(this.db);
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
                                    new MedicineActiveIngredientViewModel() {ActiveIngredientName = "P1"},
                                    new MedicineActiveIngredientViewModel() {ActiveIngredientName = "P2"}
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
                                    new MedicineActiveIngredientViewModel() {ActiveIngredientName = "P1"},
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
                                    ActiveIngredientId = 89
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
        public void Edit_AddingNewActiveIngredients()
        {
            MedicinesController controller;

            var activeIngredientsCache = new List<ActiveIngredient>();
            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();

                var doctor = this.db.Doctors.First();

                // we need to add some active ingredients here
                activeIngredientsCache.Add(
                    new ActiveIngredient()
                        {
                            PracticeId = doctor.PracticeId,
                            Doctor = doctor,
                            Name = "AI1"
                        });

                activeIngredientsCache.Add(
                    new ActiveIngredient()
                    {
                        PracticeId = doctor.PracticeId,
                        Doctor = doctor,
                        Name = "AI2"
                    });

                foreach (var ai in activeIngredientsCache)
                    this.db.ActiveIngredients.AddObject(ai);

                this.db.SaveChanges();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";

            // all these active ingredients have already been created. 
            // now we have to associate them with the original active ingredients
            controller.Create(new MedicineViewModel()
                {
                    Name = medicineName,
                    ActiveIngredients = (from ai in activeIngredientsCache
                                         select new MedicineActiveIngredientViewModel()
                                             {
                                                 ActiveIngredientId = ai.Id,
                                                 ActiveIngredientName = ai.Name
                                             }).ToList()
                });


            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.IsNotNull(medicine);
            Assert.AreEqual(2, medicine.ActiveIngredients.Count);

            // verify that all the active ingredients inside the medicine are those that
            // we've created here
            foreach (var activeIngredient in medicine.ActiveIngredients)
                Assert.IsTrue(activeIngredientsCache.Any(ai => ai == activeIngredient));
        }

        /// <summary>
        /// Removing existing active ingredients
        /// </summary>
        [TestMethod]
        public void Edit_RemovingExistingActiveIngredients()
        {
            MedicinesController controller;

            var activeIngredientsCache = new List<ActiveIngredient>();
            try
            {
                var mr = new MockRepository(true);
                controller = mr.CreateController<MedicinesController>();

                var doctor = this.db.Doctors.First();

                // we need to add some active ingredients here
                activeIngredientsCache.Add(
                    new ActiveIngredient()
                    {
                        PracticeId = doctor.PracticeId,
                        Doctor = doctor,
                        Name = "AI1"
                    });

                activeIngredientsCache.Add(
                    new ActiveIngredient()
                    {
                        PracticeId = doctor.PracticeId,
                        Doctor = doctor,
                        Name = "AI2"
                    });

                foreach (var ai in activeIngredientsCache)
                    this.db.ActiveIngredients.AddObject(ai);

                this.db.SaveChanges();
            }
            catch
            {
                Assert.Inconclusive("Test initialization has failed.");
                return;
            }

            var medicineName = "My Medicine";

            // all these active ingredients have already been created. 
            // now we have to associate them with the original active ingredients
            controller.Create(new MedicineViewModel()
            {
                Name = medicineName,
                ActiveIngredients = (from ai in activeIngredientsCache
                                     select new MedicineActiveIngredientViewModel()
                                     {
                                         ActiveIngredientId = ai.Id,
                                         ActiveIngredientName = ai.Name
                                     }).ToList()
            });

            var medicine = this.db.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.IsNotNull(medicine);
            Assert.AreEqual(2, medicine.ActiveIngredients.Count);


            controller = new MockRepository(true).CreateController<MedicinesController>();

            // now we have 2 active ingredients, let's just remove 1
            controller.Edit(new MedicineViewModel()
            {
                Id = medicine.Id,
                Name = medicineName,
                // this query has a Take(1), meaning I'm editing the medicine passing just 1 
                // active ingredient and no 2, as it was previously
                ActiveIngredients = (from ai in activeIngredientsCache
                                     select new MedicineActiveIngredientViewModel()
                                     {
                                         ActiveIngredientId = ai.Id,
                                         ActiveIngredientName = ai.Name
                                     }).Take(1).ToList()
            });

            // The only way to make the "medicine" to update it's number of ActiveIngredientes
            // was to create another CerebelloEntities. I dont know why REFRESH DID NOT WORK.
            // Maybe it has something to do with the distributed transaction.
            var db2 = CreateNewCerebelloEntities();
            medicine = db2.Medicines.FirstOrDefault(m => m.Name == medicineName);
            Assert.AreEqual(1, medicine.ActiveIngredients.Count);
            // verify that all the active ingredients inside the medicine are those that
            // we've created here
            foreach (var activeIngredient in medicine.ActiveIngredients)
                Assert.IsTrue(activeIngredientsCache.Any(ai => ai.Id == activeIngredient.Id));

            // this is very important that this action WILL NOT DELETE the action active ingredients,
            // just their association with the given medicine
            Assert.AreEqual(2, db2.ActiveIngredients.Count());
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
            Assert.AreEqual(2, medicine.Leaflets.Count);

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

