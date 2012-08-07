using System.Text.RegularExpressions;
using System.Web.Mvc;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Mvc;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ConfigScheduleController : DoctorController
    {
        [HttpGet]
        public ActionResult Edit(string returnUrl)
        {
            var config = this.Doctor.CFG_Schedule;

            var viewModel = new ConfigScheduleViewModel()
            {
                AppointmentDuration = config.AppointmentTime
            };

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
    }
}
