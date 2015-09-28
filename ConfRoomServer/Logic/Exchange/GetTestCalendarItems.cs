using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Logic.Exchange
{
    public class GetTestCalendarItems
    {
        [Serializable]
        public class GetTestCalendarItemsRequest : BaseRequest
        {
            public string MailboxEmail { get; set; }

            public GetTestCalendarItemsRequest(string mailboxEmail)
            {
                MailboxEmail = mailboxEmail;
            }
        }

        [Serializable]
        public class GetTestCalendarItemsResponse : BaseResponse
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<Models.AppointmentItem> Appointments { get; set; }
        }

        public GetTestCalendarItemsResponse Execute(GetTestCalendarItemsRequest request)
        {
            var response = new GetTestCalendarItemsResponse();

            var service = ExchangeServiceConnector.GetService();

            // Initialize values for the start and end times, and the number of appointments to retrieve.
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(1);
            const int NUM_APPTS = 20;

            var mbx = new Mailbox(request.MailboxEmail);
            FolderId fid = new FolderId(WellKnownFolderName.Calendar, mbx);

            // Initialize the calendar folder object with only the folder ID. 
            //CalendarFolder calendar = CalendarFolder.Bind(service, WellKnownFolderName.Calendar, new PropertySet());
            CalendarFolder calendar = CalendarFolder.Bind(service, fid, new PropertySet());

            // Set the start and end time and number of appointments to retrieve.
            CalendarView cView = new CalendarView(startDate, endDate, NUM_APPTS);

            // Limit the properties returned to the appointment's subject, start time, and end time.
            cView.PropertySet = new PropertySet(AppointmentSchema.Subject, AppointmentSchema.Start, AppointmentSchema.End);

            // Retrieve a collection of appointments by using the calendar view.
            FindItemsResults<Appointment> appointments = calendar.FindAppointments(cView);

            var items = new List<Models.AppointmentItem>();
            foreach (var a in appointments)
            {
                items.Add(new Models.AppointmentItem
                {
                    Start = a.Start,
                    End = a.End,
                    Subject = a.Subject
                });
            }

            response.StartDate = startDate;
            response.EndDate = endDate;
            response.Appointments = items;
            response.Success = true; 

            return response;
        }
    }
}
