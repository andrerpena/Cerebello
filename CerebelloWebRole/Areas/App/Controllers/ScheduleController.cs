using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Cerebello.Model;
using CerebelloWebRole.App_GlobalResources;
using CerebelloWebRole.Areas.App.Models;
using CerebelloWebRole.Code;
using CerebelloWebRole.Code.Controllers;
using CerebelloWebRole.Code.Mvc;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ScheduleController : DoctorController
    {
        public JsonResult GetAppointments(int start, int end)
        {
            System.DateTime origin = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            var startAsDateTime = origin.AddSeconds(start);
            var endAsDateTime = origin.AddSeconds(end);

            var appointments =
                this.db.Appointments
                .Include("Patient")
                .Include("Patient.Person")
                .Where(a => a.DoctorId == this.Doctor.Id)
                .Where(a => a.Start >= startAsDateTime && a.End <= endAsDateTime)
                .ToList();

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

        public ActionResult Index()
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

            viewModel.DoctorId = this.Doctor.Id;

            return View(viewModel);
        }

        [HttpGet]
        public ActionResult Create(DateTime? date, string start, string end, int? patientId)
        {
            var now = DateTimeHelper.GetTimeZoneNow();
            DateTime date2 = date ?? now.Date;

            var slots = GetDaySlots(date2, this.Doctor);
            var slotDuration = TimeSpan.FromMinutes(this.Doctor.CFG_Schedule.AppointmentTime);

            // Getting start date and time.
            DateTime startTime =
                string.IsNullOrEmpty(start) ?
                now :
                date2 + DateTimeHelper.GetTimeSpan(start);

            // slots.Min() dispara exceção quando slot é vazio. É necessário verificar
            if (slots.Any())
            {
                var min = slots.Min(s => (s.Item1 > startTime ? s.Item1 - startTime : startTime - s.Item1));
                var findMin = slots.Where(s => (s.Item1 > startTime ? s.Item1 - startTime : startTime - s.Item1) == min).FirstOrDefault();
                startTime = findMin.Item1;
            }

            start = startTime.ToString("HH:mm");

            // Getting end date and time.
            DateTime endTime =
                string.IsNullOrEmpty(end) ?
                startTime + slotDuration :
                date2 + DateTimeHelper.GetTimeSpan(end);

            if (endTime - startTime < slotDuration)
                endTime = startTime + slotDuration;

            // slots.Min() dispara exceção quando slot é vazio. É necessário verificar
            if (slots.Any())
            {
                var min = slots.Min(s => (s.Item2 > endTime ? s.Item2 - endTime : endTime - s.Item2));
                var findMin = slots.Where(s => (s.Item2 > endTime ? s.Item2 - endTime : endTime - s.Item2) == min).FirstOrDefault();
                endTime = findMin.Item2;
            }

            end = endTime.ToString("HH:mm");

            // Creating viewmodel.
            AppointmentViewModel viewModel = new AppointmentViewModel();
            viewModel.PatientId = patientId;

            viewModel.PatientNameLookup = this.db.Patients
                .Where(p => p.Id == patientId)
                .Select(p => p.Person.FullName)
                .FirstOrDefault();

            viewModel.Date = date2;
            viewModel.Start = start;
            viewModel.End = end;
            viewModel.DoctorId = this.Doctor.Id;
            viewModel.DateSpelled =
                DateTimeHelper.GetDayOfWeekAsString(date2) + ", "
                + DateTimeHelper.ConvertToRelative(date2,
                    DateTimeHelper.GetTimeZoneNow(),
                    DateTimeHelper.RelativeDateOptions.IncludePrefixes
                    | DateTimeHelper.RelativeDateOptions.IncludeSuffixes
                    | DateTimeHelper.RelativeDateOptions.ReplaceToday
                    | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow);

            var isTimeValid = ValidateTime(this.db, this.Doctor, date2, start, end, this.ModelState);

            var isTimeAvailable = IsTimeAvailable(startTime, endTime, this.Doctor.Appointments);
            if (!isTimeAvailable)
                this.ModelState.AddModelError(() => viewModel.Date, "A data e hora já está marcada para outro compromisso.");

            // Flag that tells whether the time and date are valid ot not.
            viewModel.IsTimeValid = isTimeValid && isTimeAvailable;

            // Setting the error message to display near the date and time configurations.
            var dateAndTimeErrors = this.ModelState.GetPropertyErrors(() => viewModel.Date);
            if (dateAndTimeErrors.Any())
            {
                viewModel.TimeValidationMessage = dateAndTimeErrors.First().ErrorMessage;
            }

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
            {
                var isTimeValid = ValidateTime(this.db, this.Doctor, formModel.Date, formModel.Start, formModel.End, this.ModelState);

                var isTimeAvailable = IsTimeAvailable(startTime, endTime, this.Doctor.Appointments, formModel.Id);
                if (!isTimeAvailable)
                    this.ModelState.AddModelError(() => formModel.Date, "A data e hora já está marcada para outro compromisso.");

                // Flag that tells whether the time and date are valid ot not.
                formModel.IsTimeValid = isTimeValid && isTimeAvailable;

                // Setting the error message to display near the date and time configurations.
                var dateAndTimeErrors = this.ModelState.GetPropertyErrors(() => formModel.Date);
                if (dateAndTimeErrors.Any())
                {
                    formModel.TimeValidationMessage = dateAndTimeErrors.First().ErrorMessage;
                }
            }

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
        private static List<Tuple<DateTime, DateTime>> GetDaySlots(DateTime day, Doctor doctor)
        {
            DateTime todayBeginning = new DateTime(day.Year, day.Month, day.Day);

            string workdayStartTimeAsString = null;
            string workdayEndTimeAsString = null;
            string lunchStartTimeAsString = null;
            string lunchEndTimeAsString = null;

            switch (day.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    if (!doctor.CFG_Schedule.Sunday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = doctor.CFG_Schedule.SundayWorkdayStartTime;
                    workdayEndTimeAsString = doctor.CFG_Schedule.SundayWorkdayEndTime;
                    lunchStartTimeAsString = doctor.CFG_Schedule.SundayLunchStartTime;
                    lunchEndTimeAsString = doctor.CFG_Schedule.SundayLunchEndTime;
                    break;
                case DayOfWeek.Monday:
                    if (!doctor.CFG_Schedule.Monday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = doctor.CFG_Schedule.MondayWorkdayStartTime;
                    workdayEndTimeAsString = doctor.CFG_Schedule.MondayWorkdayEndTime;
                    lunchStartTimeAsString = doctor.CFG_Schedule.MondayLunchStartTime;
                    lunchEndTimeAsString = doctor.CFG_Schedule.MondayLunchEndTime;
                    break;
                case DayOfWeek.Tuesday:
                    if (!doctor.CFG_Schedule.Tuesday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = doctor.CFG_Schedule.TuesdayWorkdayStartTime;
                    workdayEndTimeAsString = doctor.CFG_Schedule.TuesdayWorkdayEndTime;
                    lunchStartTimeAsString = doctor.CFG_Schedule.TuesdayLunchStartTime;
                    lunchEndTimeAsString = doctor.CFG_Schedule.TuesdayLunchEndTime;
                    break;
                case DayOfWeek.Wednesday:
                    if (!doctor.CFG_Schedule.Wednesday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = doctor.CFG_Schedule.WednesdayWorkdayStartTime;
                    workdayEndTimeAsString = doctor.CFG_Schedule.WednesdayWorkdayEndTime;
                    lunchStartTimeAsString = doctor.CFG_Schedule.WednesdayLunchStartTime;
                    lunchEndTimeAsString = doctor.CFG_Schedule.WednesdayLunchEndTime;
                    break;
                case DayOfWeek.Thursday:
                    if (!doctor.CFG_Schedule.Thursday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = doctor.CFG_Schedule.ThursdayWorkdayStartTime;
                    workdayEndTimeAsString = doctor.CFG_Schedule.ThursdayWorkdayEndTime;
                    lunchStartTimeAsString = doctor.CFG_Schedule.ThursdayLunchStartTime;
                    lunchEndTimeAsString = doctor.CFG_Schedule.ThursdayLunchEndTime;
                    break;
                case DayOfWeek.Friday:
                    if (!doctor.CFG_Schedule.Friday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = doctor.CFG_Schedule.FridayWorkdayStartTime;
                    workdayEndTimeAsString = doctor.CFG_Schedule.FridayWorkdayEndTime;
                    lunchStartTimeAsString = doctor.CFG_Schedule.FridayLunchStartTime;
                    lunchEndTimeAsString = doctor.CFG_Schedule.FridayLunchEndTime;
                    break;
                case DayOfWeek.Saturday:
                    if (!doctor.CFG_Schedule.Saturday)
                        return new List<Tuple<DateTime, DateTime>>();
                    workdayStartTimeAsString = doctor.CFG_Schedule.SaturdayWorkdayStartTime;
                    workdayEndTimeAsString = doctor.CFG_Schedule.SaturdayWorkdayEndTime;
                    lunchStartTimeAsString = doctor.CFG_Schedule.SaturdayLunchStartTime;
                    lunchEndTimeAsString = doctor.CFG_Schedule.SaturdayLunchEndTime;
                    break;
            }

            var workdayStartTime = todayBeginning + DateTimeHelper.GetTimeSpan(workdayStartTimeAsString);
            var workdayEndTime = todayBeginning + DateTimeHelper.GetTimeSpan(workdayEndTimeAsString);
            var lunchStartTime = todayBeginning + DateTimeHelper.GetTimeSpan(lunchStartTimeAsString);
            var lunchEndTime = todayBeginning + DateTimeHelper.GetTimeSpan(lunchEndTimeAsString);

            // ok. Now with all the info we need, let' start building these slots

            List<Tuple<DateTime, DateTime>> result = new List<Tuple<DateTime, DateTime>>();

            var time = workdayStartTime;
            var appointmentMinutes = doctor.CFG_Schedule.AppointmentTime;

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
            var doctor = this.Doctor;
            var db = this.db;

            var slot = FindNextFreeTime(db, doctor, date, time);
            return this.Json(new
            {
                date = slot.Item1.ToString("dd/MM/yyyy"),
                start = slot.Item1.ToString("HH:mm"),
                end = slot.Item2.ToString("HH:mm"),
                dateSpelled = DateTimeHelper.GetDayOfWeekAsString(slot.Item1) + ", "
                + DateTimeHelper.ConvertToRelative(
                    slot.Item1,
                    DateTimeHelper.GetTimeZoneNow(),
                    DateTimeHelper.RelativeDateOptions.IncludePrefixes
                    | DateTimeHelper.RelativeDateOptions.IncludeSuffixes
                    | DateTimeHelper.RelativeDateOptions.ReplaceToday
                    | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow)
            }, JsonRequestBehavior.AllowGet);
        }

        public static Tuple<DateTime, DateTime> FindNextFreeTime(CerebelloEntities db, Doctor doctor, string date = null, string time = null)
        {
            DateTime startingFrom;

            // Determining the date and time to start scanning for a free time slot.
            var now = DateTimeHelper.GetTimeZoneNow();
            if (!string.IsNullOrEmpty(date))
            {
                startingFrom = DateTime.Parse(date) + (string.IsNullOrEmpty(time) ?
                    new TimeSpan(0, 0, 0) :
                    DateTimeHelper.GetTimeSpan(time));

                if (now > startingFrom)
                    startingFrom = now;
            }
            else
                startingFrom = now;

            var currentDateStart = startingFrom.Date;
            var currentDateEnd = currentDateStart.AddDays(1).Date;

            while (true)
            {
                // take all appointments of that day
                var appointments = db.Appointments
                    .Where(a => a.DoctorId == doctor.Id)
                    .Where(a => a.End >= currentDateStart && a.Start <= currentDateEnd)
                    .OrderBy(a => a.Start)
                    .ToList();

                var slots = GetDaySlots(currentDateStart, doctor).Where(s => s.Item1 >= startingFrom);

                // Looking for available slots of time in the current day.
                foreach (var slot in slots)
                {
                    if (!IsTimeAvailable(slot.Item1, slot.Item2, appointments))
                        continue;

                    return slot;
                };

                // Moving to the next day.
                currentDateStart = currentDateEnd;
                currentDateEnd = currentDateStart.AddDays(1).Date;
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

        private static bool ValidateTime(CerebelloEntities db, Doctor doctor, DateTime date, string startTimeText, string endTimeText, ModelStateDictionary modelState)
        {
            if (string.IsNullOrEmpty(startTimeText) || string.IsNullOrEmpty(endTimeText))
                return false;

            bool hasError = false;

            var startRegexMatch = TimeDataTypeAttribute.Regex.Match(startTimeText);
            var endRegexMatch = TimeDataTypeAttribute.Regex.Match(endTimeText);

            int integerHourStart = int.Parse(startRegexMatch.Groups[1].Value) * 100 + int.Parse(startRegexMatch.Groups[2].Value);
            int integerHourEnd = int.Parse(endRegexMatch.Groups[1].Value) * 100 + int.Parse(endRegexMatch.Groups[2].Value);

            var monthAndDay = date.Month * 100 + date.Day;

            // Validation: cannot be holliday.
            var isHolliday = db.SYS_Holliday.Where(h => h.MonthAndDay == monthAndDay).Any();
            if (isHolliday)
            {
                modelState.AddModelError<AppointmentViewModel>(
                    model => model.Date,
                    "O campo '{0}' é inválido. Este dia é um feriado.");
                hasError = true;
            }

            //var doctor = this.Doctor;

            //// Validation: cannot be day-off.
            //var isDayOff = this.db.DayOffs
            //    .Where(d => d.DoctorId == doctor.Id)
            //    .Where(d => d.StartDate <= date && date <= d.EndDate)
            //    .Any();

            //if (isDayOff)
            //{
            //    modelState.AddModelError<AppointmentViewModel>(
            //        model => model.Date,
            //        "O campo '{0}' é inválido. Este dia está no intervalo de férias do médico.");
            //    hasError = true;
            //}

            // Validation: cannot set an appointment data to the past.
            var startDate = date.Date + DateTimeHelper.GetTimeSpan(startTimeText);
            if (startDate < DateTimeHelper.GetTimeZoneNow())
            {
                modelState.AddModelError<AppointmentViewModel>(
                    model => model.Date,
                    "O campo '{0}' é inválido. Não é permitido marcar uma consulta para o passado.");
                hasError = true;
            }

            // Validation: end time cannot be the same as start time.
            if (integerHourStart == integerHourEnd)
            {
                modelState.AddModelError<AppointmentViewModel>(
                    model => model.End,
                    "O campo '{0}' não pode ser igual ao horário de início.");
                hasError = true;
            }

            // Validation: end time must come after the start time.
            else if (integerHourStart > integerHourEnd)
            {
                modelState.AddModelError<AppointmentViewModel>(
                    model => model.End,
                    "O campo '{0}' não pode ser menor que o horário de início.");
                hasError = true;
            }

            Action<string, string> CheckModelTimingError = (workdayStart, workdayEnd) =>
            {
                if (string.IsNullOrEmpty(workdayStart) || string.IsNullOrEmpty(workdayEnd))
                {
                    modelState.AddModelError<AppointmentViewModel>(
                        model => model.Date,
                        "O campo '{0}' é inválido. Não existem configurações de horário para esta data.");
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
                        modelState.AddModelError<AppointmentViewModel>(
                            model => model.Start,
                            "O campo '{0}' não é um horário válido devido às configurações de horário de trabalho.");
                        hasError = true;
                    }
                    if (integerHourEnd > dbIntegerHourEnd)
                    {
                        modelState.AddModelError<AppointmentViewModel>(
                            model => model.End,
                            "O campo '{0}' não é um horário válido devido às configurações de horário de trabalho.");
                        hasError = true;
                    }
                }
            };

            switch (date.Date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    CheckModelTimingError(doctor.CFG_Schedule.SundayWorkdayStartTime, doctor.CFG_Schedule.SundayWorkdayEndTime);
                    break;
                case DayOfWeek.Monday:
                    CheckModelTimingError(doctor.CFG_Schedule.MondayWorkdayStartTime, doctor.CFG_Schedule.MondayWorkdayEndTime);
                    break;
                case DayOfWeek.Tuesday:
                    CheckModelTimingError(doctor.CFG_Schedule.TuesdayWorkdayStartTime, doctor.CFG_Schedule.TuesdayWorkdayEndTime);
                    break;
                case DayOfWeek.Wednesday:
                    CheckModelTimingError(doctor.CFG_Schedule.WednesdayWorkdayStartTime, doctor.CFG_Schedule.WednesdayWorkdayEndTime);
                    break;
                case DayOfWeek.Thursday:
                    CheckModelTimingError(doctor.CFG_Schedule.ThursdayWorkdayStartTime, doctor.CFG_Schedule.ThursdayWorkdayEndTime);
                    break;
                case DayOfWeek.Friday:
                    CheckModelTimingError(doctor.CFG_Schedule.FridayWorkdayStartTime, doctor.CFG_Schedule.FridayWorkdayEndTime);
                    break;
                case DayOfWeek.Saturday:
                    CheckModelTimingError(doctor.CFG_Schedule.SaturdayWorkdayStartTime, doctor.CFG_Schedule.SaturdayWorkdayEndTime);
                    break;
            }

            return !hasError;
        }

        public static bool IsTimeAvailable(DateTime startTime, DateTime endTime, IEnumerable<Appointment> appointments, int? excludeAppointmentId = null)
        {
            // Not overlap condition:
            // The whole body of A is before the start of B   (a.start < b.start && a.end <= b.start)
            //   OR                                             ||
            // The whole body of A is after the end of B      (a.start >= b.end && a.end > b.end)
            var query = from a in appointments
                        where
                        !(
                               (a.Start < startTime && a.End <= startTime)
                                 ||
                               (a.Start >= endTime && a.End > endTime)
                        )
                        select a;

            // When moving the appointment to another date or time,
            // we must exclude it from the selection... you can
            // move it to the position where it is now.
            if (excludeAppointmentId != null)
                query = query.Where(a => a.Id != excludeAppointmentId);

            return !query.Any();
        }

        /// <summary>
        /// Verifies whether it's a valid time
        /// </summary>
        /// <param name="date"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="excludeAppointmentId"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult VerifyTimeAvailability(string date, string start, string end, int? excludeAppointmentId = null)
        {
            string error;

            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                error = "Sem informações suficientes";
            else
            {
                var dateParsed = DateTime.Parse(date);

                var isTimeValid = ValidateTime(this.db, this.Doctor, DateTime.Parse(date), start, end, this.ModelState);

                var startTime = dateParsed + DateTimeHelper.GetTimeSpan(start);
                var endTime = dateParsed + DateTimeHelper.GetTimeSpan(end);

                var isTimeAvailable = IsTimeAvailable(startTime, endTime, this.Doctor.Appointments, excludeAppointmentId);
                if (!isTimeAvailable)
                {
                    this.ModelState.AddModelError<AppointmentViewModel>(
                        m => m.Date,
                        "A data e hora já está marcada para outro compromisso.");
                }

                // Setting the error message to display near the date and time configurations.
                var dateAndTimeErrors = this.ModelState.GetPropertyErrors<AppointmentViewModel>(m => m.Date);

                if (isTimeValid && isTimeAvailable)
                {
                    return this.Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else if (dateAndTimeErrors.Any())
                {
                    error = dateAndTimeErrors.First().ErrorMessage;
                }
                else
                {
                    error = "O horário não é válido.";
                }
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
