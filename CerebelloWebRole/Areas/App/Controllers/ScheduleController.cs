using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CerebelloWebRole.Code;
using CerebelloWebRole.Models;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code.Controllers;
using Cerebello.Model;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using CommonLib.Mvc;
using CerebelloWebRole.App_GlobalResources;
using CommonLib.Mvc.DataTypes;
using System.Data.Objects;
using System.Threading;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ScheduleController : DoctorController
    {
        public JsonResult GetAppointments(int start, int end)
        {
            System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            var startAsDateTime = origin.AddSeconds(start);
            var endAsDateTime = origin.AddSeconds(end);

            var appointments = (from a in db.Appointments.Include("Patient").Include("Patient.Person") where a.Start >= startAsDateTime && a.End <= endAsDateTime select a).ToList();

            return this.Json((from a in appointments
                              select new ScheduleEventViewModel()
                              {
                                  id = a.Id,
                                  start = a.Start.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                  end = a.End.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                                  title = a.Patient.Person.FullName,
                                  className = "myTestClass"
                              }).ToList(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index(int? doctorId)
        {
            // verify min and max times
            List<string> minTimes = new List<string>();
            List<string> maxTimes = new List<string>();

            if (this.Doctor.CFG_Schedule.Sunday)
            {
                minTimes.Add(this.Doctor.CFG_Schedule.SundayWorkdayStartTime);
                maxTimes.Add(this.Doctor.CFG_Schedule.SundayWorkdayEndTime);
            }
            if (this.Doctor.CFG_Schedule.Monday)
            {
                minTimes.Add(this.Doctor.CFG_Schedule.MondayWorkdayStartTime);
                maxTimes.Add(this.Doctor.CFG_Schedule.MondayWorkdayEndTime);
            }
            if (this.Doctor.CFG_Schedule.Tuesday)
            {
                minTimes.Add(this.Doctor.CFG_Schedule.TuesdayWorkdayStartTime);
                maxTimes.Add(this.Doctor.CFG_Schedule.TuesdayWorkdayEndTime);
            }
            if (this.Doctor.CFG_Schedule.Wednesday)
            {
                minTimes.Add(this.Doctor.CFG_Schedule.WednesdayWorkdayStartTime);
                maxTimes.Add(this.Doctor.CFG_Schedule.WednesdayWorkdayEndTime);
            }
            if (this.Doctor.CFG_Schedule.Thursday)
            {
                minTimes.Add(this.Doctor.CFG_Schedule.ThursdayWorkdayStartTime);
                maxTimes.Add(this.Doctor.CFG_Schedule.ThursdayWorkdayEndTime);
            }
            if (this.Doctor.CFG_Schedule.Friday)
            {
                minTimes.Add(this.Doctor.CFG_Schedule.FridayWorkdayStartTime);
                maxTimes.Add(this.Doctor.CFG_Schedule.FridayWorkdayEndTime);
            }
            if (this.Doctor.CFG_Schedule.Saturday)
            {
                minTimes.Add(this.Doctor.CFG_Schedule.SaturdayWorkdayStartTime);
                maxTimes.Add(this.Doctor.CFG_Schedule.SaturdayWorkdayEndTime);
            }

            string minMinTime = minTimes.Min();
            var maxMaxTime = maxTimes.Max();

            ScheduleViewModel viewModel = new ScheduleViewModel()
            {
                SlotMinutes = this.Doctor.CFG_Schedule.AppointmentTime,
                MinTime = minMinTime,
                MaxTime = maxMaxTime,
                Weekends = this.Doctor.CFG_Schedule.Saturday || this.Doctor.CFG_Schedule.Sunday
            };
            if (doctorId != null)
                viewModel.DoctorId = doctorId.Value;
            else
            {
                // se não foi informado qual é o médico, eu tenho que verificar.
                // se o usuário atual for um médico, então eu vou pegar a agenda dele
                var user = this.GetCurrentUser();
                var userPractice = (from up in user.UserPractices where up.PracticeId == this.Practice.Id select up).FirstOrDefault();

                if (userPractice == null)
                    throw new Exception("Não foi possível estabelecer uma relação entre o usuário atual e o consultório atual");

                // se o usuário atual for um médico deste consultório, eu vou usar a agenda dele
                if (userPractice.Doctor != null)
                    viewModel.DoctorId = user.Id;
                else
                    throw new Exception("Não é possível determinar o médico do qual se deseja acessar a agenda");
            }

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Create(DateTime date, string start, string end, int doctorId)
        {
            var startTime = date + DateTimeHelper.GetTimeSpan(start);
            var endTime = date + DateTimeHelper.GetTimeSpan(end);

            AppointmentViewModel viewModel = new AppointmentViewModel();
            viewModel.Date = date;
            viewModel.Start = start;
            viewModel.End = end;
            viewModel.DoctorId = doctorId;
            viewModel.DateSpelled = DateTimeHelper.GetDayOfWeekAsString(date) + ", " + DateTimeHelper.ConvertToRelative(date, DateTimeHelper.GetTimeZoneNow(), DateTimeHelper.RelativeDateOptions.IncludePrefixes | DateTimeHelper.RelativeDateOptions.IncludeSuffixes | DateTimeHelper.RelativeDateOptions.ReplaceToday | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow);
            viewModel.IsTimeValid = this.ValidateTime(date, start, end) && this.IsTimeAvailable(startTime, endTime);

            ModelState.Clear();

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Create(AppointmentViewModel formModel)
        {
            return this.Edit(formModel);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var appointment = db.Appointments.Where(a => a.Id == id).FirstOrDefault();

            AppointmentViewModel viewModel = new AppointmentViewModel()
            {
                Id = appointment.Id,
                Date = appointment.Start.Date,
                Start = appointment.Start.ToString("HH:mm"),
                End = appointment.End.ToString("HH:mm"),
                PatientNameLookup = appointment.Patient.Person.FullName,
                PatientId = appointment.PatientId,
                DoctorId = appointment.DoctorId,
                DateSpelled = DateTimeHelper.GetDayOfWeekAsString(appointment.Start.Date) + ", " + DateTimeHelper.ConvertToRelative(appointment.Start.Date, DateTimeHelper.GetTimeZoneNow(), DateTimeHelper.RelativeDateOptions.IncludePrefixes | DateTimeHelper.RelativeDateOptions.IncludeSuffixes | DateTimeHelper.RelativeDateOptions.ReplaceToday | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow),
                IsTimeValid = true
            };

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(AppointmentViewModel formModel)
        {
            if (formModel.PatientFirstAppointment)
            {
                if (string.IsNullOrEmpty(formModel.PatientName))
                    ModelState.AddModelError<AppointmentViewModel>(model => model.PatientName, ModelStrings.RequiredValidationMessage);

                if (formModel.PatientGender == null)
                    ModelState.AddModelError<AppointmentViewModel>(model => model.PatientGender, ModelStrings.RequiredValidationMessage);

                if (formModel.PatientDateOfBirth == null)
                    ModelState.AddModelError<AppointmentViewModel>(model => model.PatientDateOfBirth, ModelStrings.RequiredValidationMessage);
            }
            else
            {
                if (formModel.PatientId == null)
                    ModelState.AddModelError<AppointmentViewModel>(model => model.PatientNameLookup, ModelStrings.RequiredValidationMessage);

                else
                {
                    var patient = db.Patients.Where(p => p.Id == formModel.PatientId).FirstOrDefault();

                    if (patient == null)
                        ModelState.AddModelError<AppointmentViewModel>(model => model.PatientNameLookup, "O paciente informado não foi encontrado no banco de dados");

                    else if (patient.Person.FullName != formModel.PatientNameLookup)
                        ModelState.AddModelError<AppointmentViewModel>(model => model.PatientNameLookup, "O paciente informado foi encontrado mas o nome não coincide");
                }
            }

            var startTime = formModel.Date + DateTimeHelper.GetTimeSpan(formModel.Start);
            var endTime = formModel.Date + DateTimeHelper.GetTimeSpan(formModel.End);

            // verify if appoitment hours are consistent
            if (!string.IsNullOrEmpty(formModel.Start) && !string.IsNullOrEmpty(formModel.End))
                formModel.IsTimeValid = this.ValidateTime(formModel.Date, formModel.Start, formModel.End) && this.IsTimeAvailable(startTime, endTime, patientId: formModel.PatientId);

            if (ModelState.IsValid)
            {
                Appointment appointment = null;

                if (formModel.Id == null)
                {
                    appointment = new Appointment();
                    appointment.CreatedOn = DateTime.UtcNow;
                    appointment.DoctorId = formModel.DoctorId;
                    appointment.CreatedById = this.GetCurrentUserId();
                    this.db.Appointments.AddObject(appointment);
                }
                else
                    appointment = db.Appointments.Where(a => a.Id == formModel.Id).FirstOrDefault();

                appointment.Start = formModel.Date + DateTimeHelper.GetTimeSpan(formModel.Start);
                appointment.End = formModel.Date + DateTimeHelper.GetTimeSpan(formModel.End);

                if (formModel.PatientFirstAppointment)
                {
                    appointment.Patient = new Patient();
                    appointment.Patient.Person = new Person();
                    appointment.Patient.Person.FullName = formModel.PatientName;
                    appointment.Patient.Person.UrlIdentifier = StringHelper.GenerateUrlIdentifier(formModel.PatientName);
                    appointment.Patient.Person.Gender = (short)formModel.PatientGender;
                    appointment.Patient.Person.DateOfBirth = formModel.PatientDateOfBirth.Value;
                    appointment.Patient.Person.CreatedOn = DateTime.UtcNow;
                    appointment.Patient.Doctor = this.Doctor;

                    if (!string.IsNullOrEmpty(formModel.PatientEmail))
                        appointment.Patient.Person.Emails.Add(new Email() { Address = formModel.PatientEmail });
                }
                else
                    appointment.PatientId = formModel.PatientId.Value;


                try
                {
                    this.db.SaveChanges();
                    return Json(new { status = "success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { status = "error", text = "Não foi possível salvar a consulta. Erro inexperado", details = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }

            return View("Edit", formModel);
        }

        [HttpGet]
        public JsonResult Delete(int id)
        {
            var medicine = db.Appointments.Where(m => m.Id == id).First();
            try
            {
                this.db.Appointments.DeleteObject(medicine);
                this.db.SaveChanges();
                return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return this.Json(new { success = false, text = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Returns the series of all day slots ordered
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        private List<Tuple<DateTime, DateTime>> GetDaySlots(DateTime day)
        {
            DateTime todayBeginning = new DateTime(day.Year, day.Month, day.Day);

            string workdayStartTimeAsString = null;
            string workdayEndTimeAsString = null;
            string lunchStartTimeAsString = null;
            string lunchEndTimeAsString = null;

            switch (day.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    if (!this.Doctor.CFG_Schedule.Sunday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = this.Doctor.CFG_Schedule.SundayWorkdayStartTime;
                    workdayEndTimeAsString = this.Doctor.CFG_Schedule.SundayWorkdayEndTime;
                    lunchStartTimeAsString = this.Doctor.CFG_Schedule.SundayLunchStartTime;
                    lunchEndTimeAsString = this.Doctor.CFG_Schedule.SundayLunchEndTime;
                    break;
                case DayOfWeek.Monday:
                    if (!this.Doctor.CFG_Schedule.Monday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = this.Doctor.CFG_Schedule.MondayWorkdayStartTime;
                    workdayEndTimeAsString = this.Doctor.CFG_Schedule.MondayWorkdayEndTime;
                    lunchStartTimeAsString = this.Doctor.CFG_Schedule.MondayLunchStartTime;
                    lunchEndTimeAsString = this.Doctor.CFG_Schedule.MondayLunchEndTime;
                    break;
                case DayOfWeek.Tuesday:
                    if (!this.Doctor.CFG_Schedule.Tuesday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = this.Doctor.CFG_Schedule.TuesdayWorkdayStartTime;
                    workdayEndTimeAsString = this.Doctor.CFG_Schedule.TuesdayWorkdayEndTime;
                    lunchStartTimeAsString = this.Doctor.CFG_Schedule.TuesdayLunchStartTime;
                    lunchEndTimeAsString = this.Doctor.CFG_Schedule.TuesdayLunchEndTime;
                    break;
                case DayOfWeek.Wednesday:
                    if (!this.Doctor.CFG_Schedule.Wednesday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = this.Doctor.CFG_Schedule.WednesdayWorkdayStartTime;
                    workdayEndTimeAsString = this.Doctor.CFG_Schedule.WednesdayWorkdayEndTime;
                    lunchStartTimeAsString = this.Doctor.CFG_Schedule.WednesdayLunchStartTime;
                    lunchEndTimeAsString = this.Doctor.CFG_Schedule.WednesdayLunchEndTime;
                    break;
                case DayOfWeek.Thursday:
                    if (!this.Doctor.CFG_Schedule.Thursday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = this.Doctor.CFG_Schedule.ThursdayWorkdayStartTime;
                    workdayEndTimeAsString = this.Doctor.CFG_Schedule.ThursdayWorkdayEndTime;
                    lunchStartTimeAsString = this.Doctor.CFG_Schedule.ThursdayLunchStartTime;
                    lunchEndTimeAsString = this.Doctor.CFG_Schedule.ThursdayLunchEndTime;
                    break;
                case DayOfWeek.Friday:
                    if (!this.Doctor.CFG_Schedule.Friday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = this.Doctor.CFG_Schedule.FridayWorkdayStartTime;
                    workdayEndTimeAsString = this.Doctor.CFG_Schedule.FridayWorkdayEndTime;
                    lunchStartTimeAsString = this.Doctor.CFG_Schedule.FridayLunchStartTime;
                    lunchEndTimeAsString = this.Doctor.CFG_Schedule.FridayLunchEndTime;
                    break;
                case DayOfWeek.Saturday:
                    if (!this.Doctor.CFG_Schedule.Saturday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = this.Doctor.CFG_Schedule.SaturdayWorkdayStartTime;
                    workdayEndTimeAsString = this.Doctor.CFG_Schedule.SaturdayWorkdayEndTime;
                    lunchStartTimeAsString = this.Doctor.CFG_Schedule.SaturdayLunchStartTime;
                    lunchEndTimeAsString = this.Doctor.CFG_Schedule.SaturdayLunchEndTime;
                    break;
            }

            var workdayStartTime = todayBeginning + DateTimeHelper.GetTimeSpan(workdayStartTimeAsString);
            var workdayEndTime = todayBeginning + DateTimeHelper.GetTimeSpan(workdayEndTimeAsString);
            var lunchStartTime = todayBeginning + DateTimeHelper.GetTimeSpan(lunchStartTimeAsString);
            var lunchEndTime = todayBeginning + DateTimeHelper.GetTimeSpan(lunchEndTimeAsString);

            // ok. Now with all the info we need, let' start building these slots

            List<Tuple<DateTime, DateTime>> result = new List<Tuple<DateTime, DateTime>>();

            var time = workdayStartTime;
            var appointmentMinutes = this.Doctor.CFG_Schedule.AppointmentTime;

            while (true)
            {
                var timeEnd = time + new TimeSpan(0, appointmentMinutes, 0);
                if ((time >= workdayStartTime && timeEnd <= lunchStartTime) || (time >= lunchEndTime && timeEnd <= workdayEndTime))
                {
                    // in this case this span (time to timeEnd) is absolutely valid and we must add it to the slots
                    result.Add(new Tuple<DateTime, DateTime>(time, timeEnd));
                    time = time + new TimeSpan(0, appointmentMinutes, 0);
                }

                else if (time >= workdayStartTime && timeEnd > lunchStartTime && timeEnd < workdayEndTime)
                {
                    // this is an exception case in which the appointment would and in the middle of the lunch time
                    time = lunchEndTime;
                }
                else
                    break;
            }

            return result;
        }

        [HttpGet]
        public JsonResult FindNextFreeTime(string date, string time)
        {
            // the reference time begins by today with no 

            DateTime startingFrom;

            if (!string.IsNullOrEmpty(date))
                startingFrom = DateTime.Parse(date) + (string.IsNullOrEmpty(time) ? new TimeSpan(0, 0, 0) : DateTimeHelper.GetTimeSpan(time));
            else
                startingFrom = DateTimeHelper.GetTimeZoneNow();

            var currentDateStart = startingFrom.Date;
            var currentDateEnd = currentDateStart.AddDays(1).AddMinutes(-1);
            // take all appointments of that day 

            while (true)
            {
                var appointments = this.db.Appointments.Where(a => a.Start >= currentDateStart && a.End <= currentDateEnd).OrderBy(a => a.Start).ToList();
                var slots = this.GetDaySlots(currentDateStart).Where(s => s.Item1 >= startingFrom);

                foreach (var slot in slots)
                {
                    if (!this.IsTimeAvailable(slot.Item1, slot.Item2, appointments))
                        continue;
                    return this.Json(new { date = slot.Item1.ToString("dd/MM/yyyy"), start = slot.Item1.ToString("HH:mm"), end = slot.Item2.ToString("HH:mm"), dateSpelled = DateTimeHelper.GetDayOfWeekAsString(slot.Item1) + ", " + DateTimeHelper.ConvertToRelative(slot.Item1, DateTimeHelper.GetTimeZoneNow(), DateTimeHelper.RelativeDateOptions.IncludePrefixes | DateTimeHelper.RelativeDateOptions.IncludeSuffixes | DateTimeHelper.RelativeDateOptions.ReplaceToday | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow) }, JsonRequestBehavior.AllowGet);
                };

                currentDateStart = currentDateStart.AddDays(1);
                currentDateEnd = currentDateStart.AddDays(1).AddMinutes(-1);
            }
        }

        [HttpGet]
        public JsonResult GetSpelledDate(string date)
        {
            DateTime dateParsed;
            if (DateTime.TryParse(date, out dateParsed))
                return this.Json(new { success = true, text = DateTimeHelper.GetDayOfWeekAsString(dateParsed) + ", " + DateTimeHelper.ConvertToRelative(dateParsed, DateTimeHelper.GetTimeZoneNow(), DateTimeHelper.RelativeDateOptions.IncludePrefixes | DateTimeHelper.RelativeDateOptions.IncludeSuffixes | DateTimeHelper.RelativeDateOptions.ReplaceToday | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow) }, JsonRequestBehavior.AllowGet);
            else
                return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        private bool ValidateTime(DateTime date, string start, string end)
        {
            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                return false;

            bool hasError = false;

            var startRegexMatch = TimeDataTypeAttribute.Regex.Match(start);
            var endRegexMatch = TimeDataTypeAttribute.Regex.Match(end);

            int integerHourStart = int.Parse(startRegexMatch.Groups[1].Value) * 100 + int.Parse(startRegexMatch.Groups[2].Value);
            int integerHourEnd = int.Parse(endRegexMatch.Groups[1].Value) * 100 + int.Parse(endRegexMatch.Groups[2].Value);

            var startDate = date.Date + DateTimeHelper.GetTimeSpan(start);
            if (startDate < DateTimeHelper.GetTimeZoneNow())
            {
                this.ModelState.AddModelError<AppointmentViewModel>(model => model.Date, "O campo '{0}' é inválido. Não é permitido marcar uma consulta para o passado");
                hasError = true;
            }

            // validate
            if (integerHourStart == integerHourEnd)
            {
                this.ModelState.AddModelError<AppointmentViewModel>(model => model.End, "O campo '{0}' não pode ser igual ao horário de início");
                hasError = true;
            }

            else if (integerHourStart > integerHourEnd)
            {
                this.ModelState.AddModelError<AppointmentViewModel>(model => model.End, "O campo '{0}' não pode ser menor que o horário de início");
                hasError = true;
            }

            Action<string, string> CheckModelTimingError = (workdayStart, workdayEnd) =>
            {
                if (string.IsNullOrEmpty(workdayStart) || string.IsNullOrEmpty(workdayEnd))
                {
                    this.ModelState.AddModelError<AppointmentViewModel>(model => model.Date, "O campo '{0}' é inválido. Não existem configurações de horário para esta data");
                    hasError = true;
                }
                else
                {
                    var dbStartRegexMatch = TimeDataTypeAttribute.Regex.Match(workdayStart);
                    int dbIntegerHourStart = int.Parse(dbStartRegexMatch.Groups[1].Value) * 100 + int.Parse(dbStartRegexMatch.Groups[2].Value);

                    var dbEndRegexMatch = TimeDataTypeAttribute.Regex.Match(workdayEnd);
                    int dbIntegerHourEnd = int.Parse(dbEndRegexMatch.Groups[1].Value) * 100 + int.Parse(dbEndRegexMatch.Groups[2].Value);

                    if (integerHourStart < dbIntegerHourStart)
                    {
                        this.ModelState.AddModelError<AppointmentViewModel>(model => model.Start, "O campo '{0}' não é um horário válido devido às configurações de horário de trabalho");
                        hasError = true;
                    }
                    if (integerHourEnd > dbIntegerHourEnd)
                    {
                        this.ModelState.AddModelError<AppointmentViewModel>(model => model.End, "O campo '{0}' não é um horário válido devido às configurações de horário de trabalho");
                        hasError = true;
                    }
                }
            };

            switch (date.Date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    CheckModelTimingError(this.Doctor.CFG_Schedule.SundayWorkdayStartTime, this.Doctor.CFG_Schedule.SundayWorkdayEndTime);
                    break;
                case DayOfWeek.Monday:
                    CheckModelTimingError(this.Doctor.CFG_Schedule.MondayWorkdayStartTime, this.Doctor.CFG_Schedule.MondayWorkdayEndTime);
                    break;
                case DayOfWeek.Tuesday:
                    CheckModelTimingError(this.Doctor.CFG_Schedule.TuesdayWorkdayStartTime, this.Doctor.CFG_Schedule.TuesdayWorkdayEndTime);
                    break;
                case DayOfWeek.Wednesday:
                    CheckModelTimingError(this.Doctor.CFG_Schedule.WednesdayWorkdayStartTime, this.Doctor.CFG_Schedule.WednesdayWorkdayEndTime);
                    break;
                case DayOfWeek.Thursday:
                    CheckModelTimingError(this.Doctor.CFG_Schedule.ThursdayWorkdayStartTime, this.Doctor.CFG_Schedule.ThursdayWorkdayEndTime);
                    break;
                case DayOfWeek.Friday:
                    CheckModelTimingError(this.Doctor.CFG_Schedule.FridayWorkdayStartTime, this.Doctor.CFG_Schedule.FridayWorkdayEndTime);
                    break;
                case DayOfWeek.Saturday:
                    CheckModelTimingError(this.Doctor.CFG_Schedule.SaturdayWorkdayStartTime, this.Doctor.CFG_Schedule.SaturdayWorkdayEndTime);
                    break;
            }

            return !hasError;
        }

        public bool IsTimeAvailable(DateTime startTime, DateTime endTime, IEnumerable<Appointment> appointments = null, int? patientId = null)
        {
            if (appointments == null)
                appointments = db.Appointments;

            var query = from a in appointments where (a.Start <= startTime && a.End > startTime) || (a.Start < endTime && a.End >= endTime) || (a.Start > startTime && a.End < endTime) || (a.Start == startTime && a.End == endTime) select a;

            if (patientId != null)
                query = query.Where(a => a.PatientId != patientId);

            return !query.Any();
        }

        /// <summary>
        /// Verifies whether it's a valid time
        /// </summary>
        /// <param name="date"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="patientId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult VerifyTimeAvailability(string date, string start, string end, int? patientId = null)
        {
            string error;

            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                error = "Sem informações suficientes";
            else
            {
                var dateParsed = DateTime.Parse(date);

                if (ValidateTime(DateTime.Parse(date), start, end))
                {
                    var startTime = dateParsed + DateTimeHelper.GetTimeSpan(start);
                    var endTime = dateParsed + DateTimeHelper.GetTimeSpan(end);

                    if (this.IsTimeAvailable(startTime, endTime, patientId: patientId))
                        return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    else
                        error = "O horário já possui pelo menos uma consulta agendada";
                }
                else
                    error = "O horário não é válido";
            }

            return this.Json(new { success = false, text = error }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public JsonResult GetDatesWithAppointments(int year, int month)
        {
            var first = new DateTime(year, month, 1);
            var last = first.AddMonths(1);

            var result = (from a in db.Appointments where a.Start >= first && a.End < last select a).ToList().Select(a => a.Start.ToString("'d'dd_MM_yyyy")).Distinct().ToArray();
            return this.Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
