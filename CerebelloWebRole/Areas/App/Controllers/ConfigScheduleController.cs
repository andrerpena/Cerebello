using System.Text.RegularExpressions;
using System.Web.Mvc;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Filters;
using CerebelloWebRole.Code.Mvc;
using System.Linq;
using Cerebello.Model;
using System;
using System.Collections.Generic;
using CerebelloWebRole.Code.Json;

namespace CerebelloWebRole.Areas.App.Controllers
{
    [SelfOrUserRolePermission(RoleFlags = UserRoleFlags.Administrator)]
    public class ConfigScheduleController : DoctorController
    {
        [HttpGet]
        public ActionResult Edit(string returnUrl)
        {
            var config = this.Doctor.CFG_Schedule;

            var viewModel = new ConfigScheduleViewModel();

            if (config == null)
            {
                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Domingo",
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Segunda-feira",
                    IsBusinessDay = true,
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Terça-feira",
                    IsBusinessDay = true,
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Quarta-feira",
                    IsBusinessDay = true,
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Quinta-feira",
                    IsBusinessDay = true,
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Sexta-feira",
                    IsBusinessDay = true,
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Sábado",
                });
            }
            else
            {
                viewModel.AppointmentDuration = config.AppointmentTime;

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Domingo",
                    IsBusinessDay = config.Sunday,
                    WorkdayStartTime = config.SundayWorkdayStartTime,
                    WorkdayEndTime = config.SundayWorkdayEndTime,
                    LunchStartTime = config.SundayLunchStartTime,
                    LunchEndTime = config.SundayWorkdayEndTime
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Segunda-feira",
                    IsBusinessDay = config.Monday,
                    WorkdayStartTime = config.MondayWorkdayStartTime,
                    WorkdayEndTime = config.MondayWorkdayEndTime,
                    LunchStartTime = config.MondayLunchStartTime,
                    LunchEndTime = config.MondayLunchEndTime
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Terça-feira",
                    IsBusinessDay = config.Tuesday,
                    WorkdayStartTime = config.TuesdayWorkdayStartTime,
                    WorkdayEndTime = config.TuesdayWorkdayEndTime,
                    LunchStartTime = config.TuesdayLunchStartTime,
                    LunchEndTime = config.TuesdayLunchEndTime
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Quarta-feira",
                    IsBusinessDay = config.Wednesday,
                    WorkdayStartTime = config.WednesdayWorkdayStartTime,
                    WorkdayEndTime = config.WednesdayWorkdayEndTime,
                    LunchStartTime = config.WednesdayLunchStartTime,
                    LunchEndTime = config.WednesdayLunchEndTime
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Quinta-feira",
                    IsBusinessDay = config.Thursday,
                    WorkdayStartTime = config.ThursdayWorkdayStartTime,
                    WorkdayEndTime = config.ThursdayWorkdayEndTime,
                    LunchStartTime = config.ThursdayLunchStartTime,
                    LunchEndTime = config.ThursdayLunchEndTime
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Sexta-feira",
                    IsBusinessDay = config.Friday,
                    WorkdayStartTime = config.FridayWorkdayStartTime,
                    WorkdayEndTime = config.FridayWorkdayEndTime,
                    LunchStartTime = config.FridayLunchStartTime,
                    LunchEndTime = config.FridayLunchEndTime
                });

                viewModel.DaysOfWeek.Add(new ConfigScheduleViewModel.DayOfWeek()
                {
                    Name = "Sábado",
                    IsBusinessDay = config.Saturday,
                    WorkdayStartTime = config.SaturdayWorkdayStartTime,
                    WorkdayEndTime = config.SaturdayWorkdayEndTime,
                    LunchStartTime = config.SaturdayLunchStartTime,
                    LunchEndTime = config.SaturdayLunchEndTime
                });
            }

            ViewBag.ReturnUrl = returnUrl;

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Edit(ConfigScheduleViewModel formModel, string returnUrl)
        {
            for (int i = 0; i < formModel.DaysOfWeek.Count; i++)
            {
                var dayOfWeek = formModel.DaysOfWeek[i];

                if (dayOfWeek.IsBusinessDay)
                {
                    Match workdayStartRegexMatch = null;
                    Match workdayEndRegexMatch = null;

                    if (string.IsNullOrEmpty(dayOfWeek.WorkdayStartTime))
                        this.ModelState.AddModelError(string.Format("DaysOfWeek[{0}].WorkdayStartTime", i), string.Format(ModelStrings.RequiredValidationMessage, DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.WorkdayStartTime)));
                    else
                        workdayStartRegexMatch = TimeDataTypeAttribute.Regex.Match(dayOfWeek.WorkdayStartTime);

                    if (string.IsNullOrEmpty(dayOfWeek.WorkdayEndTime))
                        this.ModelState.AddModelError(string.Format("DaysOfWeek[{0}].WorkdayEndTime", i), string.Format(ModelStrings.RequiredValidationMessage, DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.WorkdayEndTime)));
                    else
                        workdayEndRegexMatch = TimeDataTypeAttribute.Regex.Match(dayOfWeek.WorkdayEndTime);

                    if (workdayStartRegexMatch == null || workdayEndRegexMatch == null || !workdayStartRegexMatch.Success || !workdayEndRegexMatch.Success)
                        continue;

                    int workdayIntegerHourStart = int.Parse(workdayStartRegexMatch.Groups[1].Value) * 100 + int.Parse(workdayStartRegexMatch.Groups[2].Value);
                    int workdayIntegerHourEnd = int.Parse(workdayEndRegexMatch.Groups[1].Value) * 100 + int.Parse(workdayEndRegexMatch.Groups[2].Value);

                    if (workdayIntegerHourStart >= workdayIntegerHourEnd)
                        this.ModelState.AddModelError(string.Format("DaysOfWeek[{0}].WorkdayEndTime", i), string.Format("O campo '{0}' não pode ter um valor igual ou menor que o do campo '{1}'", DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.WorkdayEndTime), DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.WorkdayStartTime)));

                    // validates lunch time if it exists

                    if (!string.IsNullOrEmpty(dayOfWeek.LunchStartTime) || !string.IsNullOrEmpty(dayOfWeek.LunchEndTime))
                    {
                        Match lunchStartRegexMatch = null;
                        Match lunchEndRegexMatch = null;

                        if (string.IsNullOrEmpty(dayOfWeek.LunchStartTime))
                            this.ModelState.AddModelError(string.Format("DaysOfWeek[{0}].LunchStartTime", i), string.Format(ModelStrings.RequiredValidationMessage, DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.LunchStartTime)));
                        else
                            lunchStartRegexMatch = TimeDataTypeAttribute.Regex.Match(dayOfWeek.LunchStartTime);

                        if (string.IsNullOrEmpty(dayOfWeek.LunchEndTime))
                            this.ModelState.AddModelError(string.Format("DaysOfWeek[{0}].LunchEndTime", i), string.Format(ModelStrings.RequiredValidationMessage, DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.LunchEndTime)));
                        else
                            lunchEndRegexMatch = TimeDataTypeAttribute.Regex.Match(dayOfWeek.LunchEndTime);

                        int lunchIntegerHourStart = int.Parse(lunchStartRegexMatch.Groups[1].Value) * 100 + int.Parse(lunchStartRegexMatch.Groups[2].Value);
                        int lunchIntegerHourEnd = int.Parse(lunchEndRegexMatch.Groups[1].Value) * 100 + int.Parse(lunchEndRegexMatch.Groups[2].Value);

                        if (lunchIntegerHourStart <= workdayIntegerHourStart)
                            this.ModelState.AddModelError(string.Format("DaysOfWeek[{0}].LunchStartTime", i), string.Format("O campo '{0}' não pode ter um valor igual ou menor que o do campo '{1}'", DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.LunchStartTime), DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.WorkdayStartTime)));

                        if (lunchIntegerHourEnd >= workdayIntegerHourEnd)
                            this.ModelState.AddModelError(string.Format("DaysOfWeek[{0}].LunchEndTime", i), string.Format("O campo '{0}' não pode ter um valor igual ou maior que o do campo '{1}'", DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.LunchEndTime), DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.WorkdayEndTime)));

                        if (lunchIntegerHourStart >= lunchIntegerHourEnd)
                            this.ModelState.AddModelError(string.Format("DaysOfWeek[{0}].LunchEndTime", i), string.Format("O campo '{0}' não pode ter um valor igual ou menor que o do campo '{1}'", DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.LunchEndTime), DataAnnotationsHelper.GetDisplayName<ConfigScheduleViewModel.DayOfWeek>(model => model.LunchStartTime)));
                    }
                }
            };

            if (this.ModelState.IsValid)
            {
                var config = this.Doctor.CFG_Schedule;

                if (config == null)
                {
                    config = new Cerebello.Model.CFG_Schedule { PracticeId = this.DbUser.PracticeId, };
                    this.Doctor.CFG_Schedule = config;
                }

                config.AppointmentTime = (int)formModel.AppointmentDuration;

                config.Sunday = formModel.DaysOfWeek[0].IsBusinessDay;
                config.SundayWorkdayStartTime = formModel.DaysOfWeek[0].WorkdayStartTime;
                config.SundayWorkdayEndTime = formModel.DaysOfWeek[0].WorkdayEndTime;
                config.SundayLunchStartTime = formModel.DaysOfWeek[0].LunchStartTime;
                config.SundayLunchEndTime = formModel.DaysOfWeek[0].LunchEndTime;

                config.Monday = formModel.DaysOfWeek[1].IsBusinessDay;
                config.MondayWorkdayStartTime = formModel.DaysOfWeek[1].WorkdayStartTime;
                config.MondayWorkdayEndTime = formModel.DaysOfWeek[1].WorkdayEndTime;
                config.MondayLunchStartTime = formModel.DaysOfWeek[1].LunchStartTime;
                config.MondayLunchEndTime = formModel.DaysOfWeek[1].LunchEndTime;

                config.Tuesday = formModel.DaysOfWeek[2].IsBusinessDay;
                config.TuesdayWorkdayStartTime = formModel.DaysOfWeek[2].WorkdayStartTime;
                config.TuesdayWorkdayEndTime = formModel.DaysOfWeek[2].WorkdayEndTime;
                config.TuesdayLunchStartTime = formModel.DaysOfWeek[2].LunchStartTime;
                config.TuesdayLunchEndTime = formModel.DaysOfWeek[2].LunchEndTime;

                config.Wednesday = formModel.DaysOfWeek[3].IsBusinessDay;
                config.WednesdayWorkdayStartTime = formModel.DaysOfWeek[3].WorkdayStartTime;
                config.WednesdayWorkdayEndTime = formModel.DaysOfWeek[3].WorkdayEndTime;
                config.WednesdayLunchStartTime = formModel.DaysOfWeek[3].LunchStartTime;
                config.WednesdayLunchEndTime = formModel.DaysOfWeek[3].LunchEndTime;

                config.Thursday = formModel.DaysOfWeek[4].IsBusinessDay;
                config.ThursdayWorkdayStartTime = formModel.DaysOfWeek[4].WorkdayStartTime;
                config.ThursdayWorkdayEndTime = formModel.DaysOfWeek[4].WorkdayEndTime;
                config.ThursdayLunchStartTime = formModel.DaysOfWeek[4].LunchStartTime;
                config.ThursdayLunchEndTime = formModel.DaysOfWeek[4].LunchEndTime;

                config.Friday = formModel.DaysOfWeek[5].IsBusinessDay;
                config.FridayWorkdayStartTime = formModel.DaysOfWeek[5].WorkdayStartTime;
                config.FridayWorkdayEndTime = formModel.DaysOfWeek[5].WorkdayEndTime;
                config.FridayLunchStartTime = formModel.DaysOfWeek[5].LunchStartTime;
                config.FridayLunchEndTime = formModel.DaysOfWeek[5].LunchEndTime;

                config.Saturday = formModel.DaysOfWeek[6].IsBusinessDay;
                config.SaturdayWorkdayStartTime = formModel.DaysOfWeek[6].WorkdayStartTime;
                config.SaturdayWorkdayEndTime = formModel.DaysOfWeek[6].WorkdayEndTime;
                config.SaturdayLunchStartTime = formModel.DaysOfWeek[6].LunchStartTime;
                config.SaturdayLunchEndTime = formModel.DaysOfWeek[6].LunchEndTime;

                db.SaveChanges();

                if (!string.IsNullOrEmpty(returnUrl))
                    return this.Redirect(returnUrl);

                return this.RedirectToAction("index", "config");
            }
            else
                return this.View(formModel);
        }

        private ConfigDaysOffViewModel GetDaysOffViewModel(bool showPast)
        {
            var currentDate = this.GetPracticeLocalNow().Date;

            var daysOffQuery = db.CFG_DayOff
                .Where(df => df.DoctorId == this.Doctor.Id);

            if (!showPast)
            {
                daysOffQuery = daysOffQuery
                    .Where(df => df.Date >= currentDate);
            }

            var daysOff = daysOffQuery
                .OrderBy(x => x.Date).ToArray();

            var viewModel = new ConfigDaysOffViewModel();

            ConfigDaysOffViewModel.DayOff prevDayOff = null;
            ConfigDaysOffViewModel.DayOff groupDayOff = null;
            foreach (var eachDayOff in daysOff)
            {
                var dayOffViewModel = new ConfigDaysOffViewModel.DayOff
                {
                    Date = eachDayOff.Date,
                    Description = eachDayOff.Description,
                    Id = eachDayOff.Id,
                };

                bool isNewGroup = prevDayOff == null
                        || dayOffViewModel.Date != prevDayOff.Date.AddDays(1.0)
                        || dayOffViewModel.Description != prevDayOff.Description;

                if (isNewGroup)
                {
                    groupDayOff = dayOffViewModel;
                    viewModel.DaysOff.Add(dayOffViewModel);
                }
                else
                {
                    if (groupDayOff.GroupItems == null)
                        groupDayOff.GroupItems = new List<ConfigDaysOffViewModel.DayOff>();

                    groupDayOff.GroupItems.Add(dayOffViewModel);
                }

                prevDayOff = dayOffViewModel;
            }

            return viewModel;
        }

        [HttpGet]
        public ActionResult DaysOff(bool? showPast, string returnUrl)
        {
            var viewModel = this.GetDaysOffViewModel(showPast ?? false);

            viewModel.Start = this.GetPracticeLocalNow();

            this.ViewBag.ReturnUrl = returnUrl;
            this.ViewBag.ShowPast = showPast;

            return this.View(viewModel);
        }

        [HttpPost]
        public ActionResult DaysOff(ConfigDaysOffViewModel formModel, bool? showPast, string returnUrl)
        {
            var start = formModel.Start.Date;
            var end = (formModel.End ?? start).Date;

            // Validando o intervalo:
            // - verifica se algum dos dias já está sendo usado;
            // - verifica se a data de início é menor ou igual a de fim.
            if (start > end)
            {
                this.ModelState.AddModelError(() => formModel.End, "Data de fim deve ser maior ou igual a de início.");
            }

            if (this.db.CFG_DayOff
                .Where(df => df.DoctorId == this.Doctor.Id)
                .Where(df => df.Date >= start && df.Date <= end)
                .Any())
            {
                this.ModelState.AddModelError(() => formModel.Start, "Já existe um dia marcado neste intervalo.");
            }

            // Salvando alterações caso esteja tudo certo.
            if (this.ModelState.IsValid)
            {
                // Adding each day in the date range, to the CFG_DayOff table.
                for (DateTime i = start; i <= end; i = i.AddDays(1.0))
                {
                    this.db.CFG_DayOff.AddObject(new CFG_DayOff
                    {
                        Date = i,
                        Description = formModel.Description,
                        Doctor = this.Doctor,
                        PracticeId = this.DbUser.PracticeId,
                    });
                }

                this.db.SaveChanges();
            }

            // Returning the view with the new elements.
            var viewModel = GetDaysOffViewModel(showPast ?? false);

            this.ViewBag.ReturnUrl = returnUrl;
            this.ViewBag.ShowPast = showPast;

            return this.View(viewModel);
        }

        public JsonResult DaysOffDelete(string items)
        {
            try
            {
                var ids = items.Split(',').Select(s => int.Parse(s)).ToArray();

                var objs = this.db.CFG_DayOff
                    .Where(df => ids.Contains(df.Id) && df.DoctorId == this.Doctor.Id)
                    .Select(df => df.Id)
                    .ToArray()
                    .Select(id => new CFG_DayOff { Id = id })
                    .ToArray();

                foreach (var eachObjToDelete in objs)
                {
                    this.db.CFG_DayOff.Attach(eachObjToDelete);
                    //old code: this.db.AttachTo("CFG_DayOff", eachObjToDelete);
                    this.db.CFG_DayOff.DeleteObject(eachObjToDelete);
                }

                this.db.SaveChanges();

                return this.Json(new JsonDeleteMessage { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new JsonDeleteMessage { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
