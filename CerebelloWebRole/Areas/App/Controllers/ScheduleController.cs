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
using CerebelloWebRole.Models;

namespace CerebelloWebRole.Areas.App.Controllers
{
    public class ScheduleController : DoctorController
    {
        public ScheduleController()
        {
            this.UserNowGetter = () => DateTimeHelper.GetTimeZoneNow();
            this.UtcNowGetter = () => DateTime.UtcNow;
        }

        public Func<DateTime> UserNowGetter { get; set; }

        public Func<DateTime> UtcNowGetter { get; set; }

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
                                  title = GetAppointmentText(a),
                                  className = GetAppointmentClass(a),
                              }).ToList(), JsonRequestBehavior.AllowGet);
        }

        private static string GetAppointmentText(Appointment a)
        {
            switch ((TypeAppointment)a.Type)
            {
                case TypeAppointment.GenericAppointment:
                    return a.Description;
                case TypeAppointment.MedicalAppointment:
                    return a.Patient.Person.FullName;
                default:
                    throw new Exception("Unsupported appointment type.");
            }
        }

        private static string GetAppointmentClass(Appointment a)
        {
            switch ((TypeAppointment)a.Type)
            {
                case TypeAppointment.GenericAppointment:
                    return "generic-appointment";
                case TypeAppointment.MedicalAppointment:
                    return "medical-appointment";
                default:
                    throw new Exception("Unsupported appointment type.");
            }
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
        public ActionResult Create(DateTime? date, string start, string end, int? patientId, bool findNextAvailable)
        {
            var userNow = this.UserNowGetter();
            DateTime dateOnly = (date ?? userNow).Date;

            if (date != null)
            {
                // The behavior for 'start' and 'end' parameters are different
                // depending on 'findNextAvailable' param, when 'date' is something:
                // - case false: start must have a valid time value, end is optional.
                if (findNextAvailable)
                {
                    // When 'start' is not specified, we set it to the begining of the day.
                    if (string.IsNullOrEmpty(start))
                        start = "00:00";
                }
                else if (string.IsNullOrEmpty(start))
                {
                    // If date has something, then start and end must also be null or empty.
                    this.ModelState.AddModelError<AppointmentViewModel>(
                        m => m.Date,
                        "Ocorreu um erro nos parâmetros desta página.");

                    return this.View("Edit", new AppointmentViewModel());
                }
            }

            //var slots = GetDaySlots(dateOnly, this.Doctor);
            var slotDuration = TimeSpan.FromMinutes(this.Doctor.CFG_Schedule.AppointmentTime);

            // Getting start date and time.
            DateTime startTime =
                string.IsNullOrEmpty(start) ?
                dateOnly :
                dateOnly + DateTimeHelper.GetTimeSpan(start);

            // todo: just delete code or find a place for it?
            //FindNearestSlotStartTime(ref start, slots, ref startTime);

            // Getting end date and time.
            DateTime endTime =
                string.IsNullOrEmpty(end) ?
                startTime + slotDuration :
                dateOnly + DateTimeHelper.GetTimeSpan(end);

            if (endTime - startTime < slotDuration)
                endTime = startTime + slotDuration;

            // todo: just delete code or find a place for it?
            //FindNearestSlotEndTime(ref end, slots, ref endTime);

            // Find next available time slot.
            if (findNextAvailable)
            {
                var doctor = this.Doctor;
                var db = this.db;

                // Determining the date and time to start scanning for a free time slot.
                DateTime startingFrom = startTime;

                if (userNow > startingFrom)
                    startingFrom = userNow;

                // Finding the next available time slot, and setting the startTime and endTime.
                var slot = FindNextFreeTime(db, doctor, userNow, startingFrom);
                startTime = slot.Item1;
                endTime = slot.Item2;
            }

            dateOnly = startTime.Date;
            start = startTime.ToString("HH:mm");
            end = endTime.ToString("HH:mm");

            // Creating viewmodel.
            AppointmentViewModel viewModel = new AppointmentViewModel();

            int currentUserPracticeId = this.GetCurrentUser().PracticeId;

            var patientName = this.db.Patients
                .Where(p => p.Id == patientId)
                .Where(p => p.Doctor.Users.FirstOrDefault().PracticeId == currentUserPracticeId)
                .Select(p => p.Person.FullName)
                .FirstOrDefault();

            viewModel.PatientNameLookup = patientName;

            if (patientName != null)
                viewModel.PatientId = patientId;

            viewModel.Date = dateOnly;
            viewModel.Start = start;
            viewModel.End = end;
            viewModel.DoctorId = this.Doctor.Id;
            viewModel.DateSpelled =
                DateTimeHelper.GetDayOfWeekAsString(dateOnly) + ", "
                + DateTimeHelper.ConvertToRelative(dateOnly,
                    userNow,
                    DateTimeHelper.RelativeDateOptions.IncludePrefixes
                    | DateTimeHelper.RelativeDateOptions.IncludeSuffixes
                    | DateTimeHelper.RelativeDateOptions.ReplaceToday
                    | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow);

            ModelStateDictionary inconsistencyMessages = new ModelStateDictionary();
            var isTimeValid = ValidateTime(
                this.db,
                this.Doctor,
                dateOnly,
                start,
                end,
                this.ModelState,
                inconsistencyMessages,
                userNow);

            var isTimeAvailable = IsTimeAvailable(startTime, endTime, this.Doctor.Appointments);
            if (!isTimeAvailable)
            {
                inconsistencyMessages.AddModelError(
                    () => viewModel.Date,
                    "A data e hora já está marcada para outro compromisso.");
            }

            // Flag that tells whether the time and date are valid ot not.
            viewModel.IsTimeValid = isTimeValid && isTimeAvailable;

            // Setting the error message to display near the date and time configurations.
            var emptyErrors = new ModelErrorCollection();
            var errorsList = new List<ModelError>();
            errorsList.AddRange(this.ModelState.GetPropertyErrors(() => viewModel.Date) ?? emptyErrors);
            errorsList.AddRange(this.ModelState.GetPropertyErrors(() => viewModel.Start) ?? emptyErrors);
            errorsList.AddRange(this.ModelState.GetPropertyErrors(() => viewModel.End) ?? emptyErrors);
            errorsList.AddRange(inconsistencyMessages.GetPropertyErrors(() => viewModel.Date) ?? emptyErrors);
            errorsList.AddRange(inconsistencyMessages.GetPropertyErrors(() => viewModel.Start) ?? emptyErrors);
            errorsList.AddRange(inconsistencyMessages.GetPropertyErrors(() => viewModel.End) ?? emptyErrors);
            if (errorsList.Any())
            {
                viewModel.TimeValidationMessage = errorsList.First().ErrorMessage;
            }

            this.ModelState.Clear();

            this.ViewBag.IsEditing = false;

            return this.View("Edit", viewModel);
        }

        private static void FindNearestSlotEndTime(ref string end, List<Tuple<DateTime, DateTime>> slots, ref DateTime endTime)
        {
            // slots.Min() dispara exceção quando slot é vazio. É necessário verificar
            if (slots != null && slots.Any())
            {
                var endTime2 = endTime;
                var min = slots.Min(s => (s.Item2 > endTime2 ? s.Item2 - endTime2 : endTime2 - s.Item2));
                var findMin = slots.Where(s => (s.Item2 > endTime2 ? s.Item2 - endTime2 : endTime2 - s.Item2) == min).FirstOrDefault();
                endTime = findMin.Item2;
            }

            end = endTime.ToString("HH:mm");
        }

        private static void FindNearestSlotStartTime(ref string start, List<Tuple<DateTime, DateTime>> slots, ref DateTime startTime)
        {
            // slots.Min() dispara exceção quando slot é vazio. É necessário verificar
            if (slots != null && slots.Any())
            {
                var startTime2 = startTime;
                var min = slots.Min(s => (s.Item1 > startTime2 ? s.Item1 - startTime2 : startTime2 - s.Item1));
                var findMin = slots.Where(s => (s.Item1 > startTime2 ? s.Item1 - startTime2 : startTime2 - s.Item1) == min).FirstOrDefault();
                startTime = findMin.Item1;
            }

            start = startTime.ToString("HH:mm");
        }

        [HttpPost]
        public ActionResult Create(AppointmentViewModel formModel)
        {
            return this.Edit(formModel);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var currentUserPracticeId = this.GetCurrentUser().PracticeId;

            var appointment = db.Appointments
                .Where(a => a.Id == id)
                .Where(a => a.Doctor.Users.FirstOrDefault().PracticeId == currentUserPracticeId)
                .FirstOrDefault();

            if (appointment == null)
            {
                return this.View("NotFound");
            }

            var userNow = this.UserNowGetter();

            AppointmentViewModel viewModel = new AppointmentViewModel()
            {
                Id = appointment.Id,
                Date = appointment.Start.Date,
                Start = appointment.Start.ToString("HH:mm"),
                End = appointment.End.ToString("HH:mm"),
                DoctorId = appointment.DoctorId,
                DateSpelled = DateTimeHelper.GetDayOfWeekAsString(appointment.Start.Date) + ", "
                    + DateTimeHelper.ConvertToRelative(
                        appointment.Start.Date,
                        userNow,
                        DateTimeHelper.RelativeDateOptions.IncludePrefixes
                            | DateTimeHelper.RelativeDateOptions.IncludeSuffixes
                            | DateTimeHelper.RelativeDateOptions.ReplaceToday
                            | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow),
                IsTimeValid = true,
            };

            switch ((TypeAppointment)appointment.Type)
            {
                case TypeAppointment.GenericAppointment:
                    viewModel.IsGenericAppointment = true;
                    viewModel.Description = appointment.Description;
                    break;
                case TypeAppointment.MedicalAppointment:
                    viewModel.IsGenericAppointment = false;
                    viewModel.PatientNameLookup = appointment.Patient.Person.FullName;
                    viewModel.PatientId = appointment.PatientId;
                    break;
                default:
                    throw new Exception("Unsupported appointment type.");
            }

            this.ViewBag.IsEditing = true;

            return View("Edit", viewModel);
        }

        [HttpPost]
        public ActionResult Edit(AppointmentViewModel formModel)
        {
            // Custom model validation.
            if (formModel.IsGenericAppointment)
            {
                // This is a generic appointment, so we must clear validation for patient.
                this.ModelState.ClearPropertyErrors(() => formModel.PatientId);
                this.ModelState.ClearPropertyErrors(() => formModel.PatientName);
                this.ModelState.ClearPropertyErrors(() => formModel.PatientNameLookup);
                this.ModelState.ClearPropertyErrors(() => formModel.PatientGender);
                this.ModelState.ClearPropertyErrors(() => formModel.PatientFirstAppointment);
                this.ModelState.ClearPropertyErrors(() => formModel.PatientEmail);
                this.ModelState.ClearPropertyErrors(() => formModel.PatientDateOfBirth);
                this.ModelState.ClearPropertyErrors(() => formModel.PatientCoverageId);
            }
            else if (formModel.PatientFirstAppointment)
            {
                // This is a medical appointment, so we must clear validation for generic appointment.
                this.ModelState.ClearPropertyErrors(() => formModel.Description);

                if (string.IsNullOrEmpty(formModel.PatientName))
                    ModelState.AddModelError<AppointmentViewModel>(model => model.PatientName, ModelStrings.RequiredValidationMessage);

                if (formModel.PatientGender == null)
                    ModelState.AddModelError<AppointmentViewModel>(model => model.PatientGender, ModelStrings.RequiredValidationMessage);

                if (formModel.PatientDateOfBirth == null)
                    ModelState.AddModelError<AppointmentViewModel>(model => model.PatientDateOfBirth, ModelStrings.RequiredValidationMessage);
            }
            else
            {
                // This is a medical appointment, so we must clear validation for generic appointment.
                this.ModelState.ClearPropertyErrors(() => formModel.Description);

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

            // Verify if appoitment hours are consistent
            ModelStateDictionary inconsistencyMessages = new ModelStateDictionary();
            if (!string.IsNullOrEmpty(formModel.Start) && !string.IsNullOrEmpty(formModel.End))
            {
                var isTimeValid = ValidateTime(
                    this.db,
                    this.Doctor,
                    formModel.Date,
                    formModel.Start,
                    formModel.End,
                    this.ModelState,
                    inconsistencyMessages,
                    this.UserNowGetter());

                var isTimeAvailable = IsTimeAvailable(startTime, endTime, this.Doctor.Appointments, formModel.Id);
                if (!isTimeAvailable)
                    inconsistencyMessages.AddModelError(
                        () => formModel.Date,
                        "A data e hora já está marcada para outro compromisso.");

                // Flag that tells whether the time and date are valid ot not.
                formModel.IsTimeValid = isTimeValid && isTimeAvailable;

                // Setting the error message to display near the date and time configurations.
                var emptyErrors = new ModelErrorCollection();
                var errorsList = new List<ModelError>();
                errorsList.AddRange(this.ModelState.GetPropertyErrors(() => formModel.Date) ?? emptyErrors);
                errorsList.AddRange(this.ModelState.GetPropertyErrors(() => formModel.Start) ?? emptyErrors);
                errorsList.AddRange(this.ModelState.GetPropertyErrors(() => formModel.End) ?? emptyErrors);
                errorsList.AddRange(inconsistencyMessages.GetPropertyErrors(() => formModel.Date) ?? emptyErrors);
                errorsList.AddRange(inconsistencyMessages.GetPropertyErrors(() => formModel.Start) ?? emptyErrors);
                errorsList.AddRange(inconsistencyMessages.GetPropertyErrors(() => formModel.End) ?? emptyErrors);

                if (errorsList.Any())
                {
                    formModel.TimeValidationMessage = errorsList.First().ErrorMessage;
                }
            }

            // Saving data if model is valid.
            if (this.ModelState.IsValid)
            {
                // Creating the appointment.
                Appointment appointment = null;

                if (formModel.Id == null)
                {
                    appointment = new Appointment();
                    appointment.CreatedOn = this.UtcNowGetter();
                    appointment.DoctorId = formModel.DoctorId;
                    appointment.CreatedById = this.GetCurrentUserId();
                    this.db.Appointments.AddObject(appointment);
                }
                else
                {
                    var currentUserPracticeId = this.GetCurrentUser().PracticeId;

                    appointment = db.Appointments
                        .Where(a => a.Id == formModel.Id)
                        .Where(a => a.Doctor.Users.FirstOrDefault().PracticeId == currentUserPracticeId)
                        .FirstOrDefault();

                    // If the appointment does not exist, or does not belongs to the current practice,
                    // it should go to a view indicating that.
                    if (appointment == null)
                        return View("NotFound", formModel);
                }

                appointment.Start = formModel.Date + DateTimeHelper.GetTimeSpan(formModel.Start);
                appointment.End = formModel.Date + DateTimeHelper.GetTimeSpan(formModel.End);

                // Setting the appointment type and associated properties.
                // - generic appointment: has description, date and time interval
                // - medical appointment: has patient, date and time interval
                if (formModel.IsGenericAppointment)
                {
                    appointment.Description = formModel.Description;
                    appointment.Type = (int)TypeAppointment.GenericAppointment;
                }
                else if (formModel.PatientFirstAppointment)
                {
                    appointment.Type = (int)TypeAppointment.MedicalAppointment;

                    var patient = new Patient();
                    patient.Person = new Person();
                    patient.Person.FullName = formModel.PatientName;
                    patient.Person.UrlIdentifier = StringHelper.GenerateUrlIdentifier(formModel.PatientName);
                    patient.Person.Gender = (short)formModel.PatientGender;
                    patient.Person.DateOfBirth = formModel.PatientDateOfBirth.Value;
                    patient.Person.CreatedOn = this.UtcNowGetter();
                    patient.Doctor = this.Doctor;

                    appointment.Patient = patient;

                    if (!string.IsNullOrEmpty(formModel.PatientEmail))
                        appointment.Patient.Person.Emails.Add(new Email() { Address = formModel.PatientEmail });
                }
                else
                {
                    appointment.Type = (int)TypeAppointment.MedicalAppointment;

                    appointment.PatientId = formModel.PatientId.Value;
                }

                // Returning a JSON result, indicating what has happened.
                try
                {
                    this.db.SaveChanges();
                    return this.Json((dynamic)new { status = "success" }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return this.Json((dynamic)new { status = "error", text = "Não foi possível salvar a consulta. Erro inexperado", details = ex.Message }, JsonRequestBehavior.AllowGet);
                }
            }

            this.ViewBag.IsEditing = this.RouteData.Values["action"].ToString().ToLowerInvariant() == "edit";

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

            var userNow = this.UserNowGetter();

            // Determining the date and time to start scanning for a free time slot.
            DateTime startingFrom = userNow;

            if (!string.IsNullOrEmpty(date))
            {
                startingFrom = DateTime.Parse(date)
                    + (string.IsNullOrEmpty(time) ?
                        new TimeSpan(0, 0, 0) :
                        DateTimeHelper.GetTimeSpan(time));
            }

            if (userNow > startingFrom)
                startingFrom = userNow;

            var slot = FindNextFreeTime(db, doctor, userNow, startingFrom);
            return this.Json(new
            {
                date = slot.Item1.ToString("dd/MM/yyyy"),
                start = slot.Item1.ToString("HH:mm"),
                end = slot.Item2.ToString("HH:mm"),
                dateSpelled = DateTimeHelper.GetDayOfWeekAsString(slot.Item1) + ", "
                + DateTimeHelper.ConvertToRelative(
                    slot.Item1,
                    userNow,
                    DateTimeHelper.RelativeDateOptions.IncludePrefixes
                    | DateTimeHelper.RelativeDateOptions.IncludeSuffixes
                    | DateTimeHelper.RelativeDateOptions.ReplaceToday
                    | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow)
            }, JsonRequestBehavior.AllowGet);
        }

        public static Tuple<DateTime, DateTime> FindNextFreeTime(
            CerebelloEntities db,
            Doctor doctor,
            DateTime userNow,
            DateTime startingFrom)
        {
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
                return this.Json(
                    new
                    {
                        success = true,
                        text = DateTimeHelper.GetDayOfWeekAsString(dateParsed) + ", "
                            + DateTimeHelper.ConvertToRelative(
                                dateParsed,
                                this.UserNowGetter(),
                                DateTimeHelper.RelativeDateOptions.IncludePrefixes
                                | DateTimeHelper.RelativeDateOptions.IncludeSuffixes
                                | DateTimeHelper.RelativeDateOptions.ReplaceToday
                                | DateTimeHelper.RelativeDateOptions.ReplaceYesterdayAndTomorrow)
                    },
                    JsonRequestBehavior.AllowGet);
            else
                return this.Json(new { success = false }, JsonRequestBehavior.AllowGet);
        }

        private static bool ValidateTime(
            CerebelloEntities db,
            Doctor doctor,
            DateTime date,
            string startTimeText,
            string endTimeText,
            ModelStateDictionary modelState,
            ModelStateDictionary inconsistencyMessages,
            DateTime userNow)
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
            var isHolliday = db.SYS_Holiday.Where(h => h.MonthAndDay == monthAndDay).Any();
            if (isHolliday)
            {
                inconsistencyMessages.AddModelError<AppointmentViewModel>(
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
            //    inconsistencyMessages.AddModelError<AppointmentViewModel>(
            //        model => model.Date,
            //        "O campo '{0}' é inválido. Este dia está no intervalo de férias do médico.");
            //    hasError = true;
            //}

            // Validation: cannot set an appointment date to the past.
            var startDate = date.Date + DateTimeHelper.GetTimeSpan(startTimeText);
            if (startDate < userNow)
            {
                inconsistencyMessages.AddModelError<AppointmentViewModel>(
                    model => model.Date,
                    "A data e hora indicadas estão no passado.");
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

            Action<string, string, string, string> CheckModelTimingError = (workdayStart, workdayEnd, lunchStart, lunchEnd) =>
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
                    // Verify the lunch time.
                    {
                        var lunchStartInteger = GetTimeAsInteger(lunchStart);
                        var lunchEndInteger = GetTimeAsInteger(lunchEnd);

                        bool lunchIsAfter = integerHourStart < lunchStartInteger && integerHourEnd <= lunchStartInteger;
                        bool lunchIsBefore = integerHourStart >= lunchEndInteger && integerHourEnd > lunchEndInteger;

                        if (lunchIsAfter == lunchIsBefore)
                        {
                            inconsistencyMessages.AddModelError<AppointmentViewModel>(
                                model => model.Date,
                                "A data e hora marcada está no horário de almoço do médico.");
                            hasError = true;
                        }
                    }

                    // Verify the work time.
                    {
                        int workdayStartInteger = GetTimeAsInteger(workdayStart);
                        int workdayEndInteger = GetTimeAsInteger(workdayEnd);

                        if (integerHourStart < workdayStartInteger)
                        {
                            modelState.AddModelError<AppointmentViewModel>(
                                model => model.Start,
                                "O campo '{0}' não é um horário válido devido às configurações de horário de trabalho.");
                            hasError = true;
                        }

                        if (integerHourEnd > workdayEndInteger)
                        {
                            modelState.AddModelError<AppointmentViewModel>(
                                model => model.End,
                                "O campo '{0}' não é um horário válido devido às configurações de horário de trabalho.");
                            hasError = true;
                        }
                    }
                }
            };

            switch (date.Date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    CheckModelTimingError(
                        doctor.CFG_Schedule.SundayWorkdayStartTime,
                        doctor.CFG_Schedule.SundayWorkdayEndTime,
                        doctor.CFG_Schedule.SundayLunchStartTime,
                        doctor.CFG_Schedule.SundayLunchEndTime);
                    break;
                case DayOfWeek.Monday:
                    CheckModelTimingError(
                        doctor.CFG_Schedule.MondayWorkdayStartTime,
                        doctor.CFG_Schedule.MondayWorkdayEndTime,
                        doctor.CFG_Schedule.MondayLunchStartTime,
                        doctor.CFG_Schedule.MondayLunchEndTime);
                    break;
                case DayOfWeek.Tuesday:
                    CheckModelTimingError(
                        doctor.CFG_Schedule.TuesdayWorkdayStartTime,
                        doctor.CFG_Schedule.TuesdayWorkdayEndTime,
                        doctor.CFG_Schedule.TuesdayLunchStartTime,
                        doctor.CFG_Schedule.TuesdayLunchEndTime);
                    break;
                case DayOfWeek.Wednesday:
                    CheckModelTimingError(
                        doctor.CFG_Schedule.WednesdayWorkdayStartTime,
                        doctor.CFG_Schedule.WednesdayWorkdayEndTime,
                        doctor.CFG_Schedule.WednesdayLunchStartTime,
                        doctor.CFG_Schedule.WednesdayLunchEndTime);
                    break;
                case DayOfWeek.Thursday:
                    CheckModelTimingError(
                        doctor.CFG_Schedule.ThursdayWorkdayStartTime,
                        doctor.CFG_Schedule.ThursdayWorkdayEndTime,
                        doctor.CFG_Schedule.ThursdayLunchStartTime,
                        doctor.CFG_Schedule.ThursdayLunchEndTime);
                    break;
                case DayOfWeek.Friday:
                    CheckModelTimingError(
                        doctor.CFG_Schedule.FridayWorkdayStartTime,
                        doctor.CFG_Schedule.FridayWorkdayEndTime,
                        doctor.CFG_Schedule.FridayLunchStartTime,
                        doctor.CFG_Schedule.FridayLunchEndTime);
                    break;
                case DayOfWeek.Saturday:
                    CheckModelTimingError(
                        doctor.CFG_Schedule.SaturdayWorkdayStartTime,
                        doctor.CFG_Schedule.SaturdayWorkdayEndTime,
                        doctor.CFG_Schedule.SaturdayLunchStartTime,
                        doctor.CFG_Schedule.SaturdayLunchEndTime);
                    break;
            }

            return !hasError;
        }

        /// <summary>
        /// Converts a string containing a time to an integer.
        /// e.g.: "13:15" -> 1315
        /// </summary>
        /// <param name="strTime"></param>
        /// <returns></returns>
        private static int GetTimeAsInteger(string strTime)
        {
            var match = TimeDataTypeAttribute.Regex.Match(strTime);
            int result = int.Parse(match.Groups[1].Value) * 100 + int.Parse(match.Groups[2].Value);
            return result;
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

                var inconsistencyMessages = new ModelStateDictionary();
                var isTimeValid = ValidateTime(
                    this.db,
                    this.Doctor,
                    DateTime.Parse(date),
                    start,
                    end,
                    this.ModelState,
                    inconsistencyMessages,
                    this.UserNowGetter());

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
                var emptyErrors = new ModelErrorCollection();
                var errorsList = new List<ModelError>();
                errorsList.AddRange(this.ModelState.GetPropertyErrors<AppointmentViewModel>(m => m.Date) ?? emptyErrors);
                errorsList.AddRange(this.ModelState.GetPropertyErrors<AppointmentViewModel>(m => m.Start) ?? emptyErrors);
                errorsList.AddRange(this.ModelState.GetPropertyErrors<AppointmentViewModel>(m => m.End) ?? emptyErrors);
                errorsList.AddRange(inconsistencyMessages.GetPropertyErrors<AppointmentViewModel>(m => m.Date) ?? emptyErrors);
                errorsList.AddRange(inconsistencyMessages.GetPropertyErrors<AppointmentViewModel>(m => m.Start) ?? emptyErrors);
                errorsList.AddRange(inconsistencyMessages.GetPropertyErrors<AppointmentViewModel>(m => m.End) ?? emptyErrors);
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
