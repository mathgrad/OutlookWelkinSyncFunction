using System;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace OutlookWelkinSyncFunction
{
    public class WelkinEvent
    {
        public static WelkinEvent CreateDefaultForCalendar(string calendarId)
        {
            WelkinEvent evt = new WelkinEvent();
            evt.CalendarId = calendarId;
            evt.IsAllDay = true;
            evt.Day = DateTime.UtcNow.Date;
            evt.Modality = Constants.DefaultModality;
            evt.AppointmentType = Constants.DefaultAppointmentType;
            evt.PatientId = Environment.GetEnvironmentVariable("WelkinDummyPatientId");
            evt.IgnoreUnavailableTimes = true;
            evt.IgnoreWorkingHours = true;
            
            return evt;
        }

        public bool SyncWith(Event outlookEvent)
        {
            bool keepMine = 
                (outlookEvent.LastModifiedDateTime == null) || 
                (this.Updated != null && this.Updated > outlookEvent.LastModifiedDateTime);

            if (keepMine)
            {
                outlookEvent.IsAllDay = this.IsAllDay;
                if (this.IsAllDay)
                {
                    outlookEvent.Start.DateTime = this.Day.Value.ToString("o");
                    outlookEvent.End.DateTime = this.Day.Value.AddDays(1).ToString("o");
                }
                else 
                {
                    outlookEvent.Start.DateTime = this.Start.Value.ToString("o");
                    outlookEvent.End.DateTime = this.End.Value.ToString("o");
                }
            }
            else
            {
                this.IsAllDay = outlookEvent.IsAllDay.HasValue? outlookEvent.IsAllDay.Value : false;
                
                if (this.IsAllDay)
                {
                    this.Day = DateTime.Parse(outlookEvent.Start.DateTime).Date;
                    this.IgnoreUnavailableTimes = true;
                    this.IgnoreWorkingHours = true;
                    this.Start = outlookEvent.StartUtc();
                    this.End = outlookEvent.EndUtc();
                }
                else 
                {
                    this.Day = null;
                    this.Start = outlookEvent.StartUtc();
                    this.End = outlookEvent.EndUtc();
                }
            }

            return !keepMine; // was changed
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("is_all_day")]
        public bool IsAllDay { get; set; }

        [JsonProperty("ignore_unavailable_times", NullValueHandling=NullValueHandling.Ignore)]
        public bool IgnoreUnavailableTimes { get; set; }

        [JsonProperty("ignore_working_hours", NullValueHandling=NullValueHandling.Ignore)]
        public bool IgnoreWorkingHours { get; set; }

        [JsonProperty("calendar_id")]
        public string CalendarId { get; set; }

        [JsonProperty("patient_id")]
        public string PatientId { get; set; }

        [JsonProperty("outcome", NullValueHandling=NullValueHandling.Ignore)]
        public string Outcome { get; set; }

        [JsonProperty("modality")]
        public string Modality { get; set; }

        [JsonProperty("appointment_type")]
        public string AppointmentType { get; set; }

        [JsonProperty("updated_at", NullValueHandling=NullValueHandling.Ignore)]
        public DateTimeOffset? Updated { get; set; }

        [JsonProperty("created_at", NullValueHandling=NullValueHandling.Ignore)]
        public DateTimeOffset? Created { get; set; }

        [JsonProperty("start_time")]
        public DateTimeOffset? Start { get; set; }

        [JsonProperty("end_time")]
        public DateTimeOffset? End { get; set; }

        // If this is an all-day event, this is the date it's on
        [JsonProperty("day", NullValueHandling=NullValueHandling.Ignore)]
        [JsonConverter(typeof(JsonDateFormatConverter), "yyyy-MM-dd")]
        public DateTimeOffset? Day { get; set; }
    }
}