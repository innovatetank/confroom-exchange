using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Logic.Exchange
{
    public class ReserveRoom
    {
        [Serializable]
        public class ReserveRoomRequest : BaseRequest
        {
            public string MailboxEmail { get; set; }
            public int BookMinutes { get; set; }

            public ReserveRoomRequest(string mailboxEmail, int bookMinutes)
            {
                this.MailboxEmail = mailboxEmail;
                this.BookMinutes = bookMinutes;
            }
        }

        [Serializable]
        public class ReserveRoomResponse : BaseResponse
        {
            public ItemId AppointmentId { get; set; }
        }

        public ReserveRoomResponse Execute(ReserveRoomRequest request)
        {
            var response = new ReserveRoomResponse();

            try
            {
                // Get mailbox info
                var requestMailboxInfo = new GetMailboxInfo.GetMailboxInfoRequest(
                    email: request.MailboxEmail);
                var responseMailboxInfo = new GetMailboxInfo().Execute(requestMailboxInfo);
                if (!responseMailboxInfo.Success) {
                    response.ErrorMessage = responseMailboxInfo.ErrorMessage;
                    return response;
                }

                // Connect to Exchange
                var service = ExchangeServiceConnector.GetService(request.MailboxEmail);

                var mbx = new Mailbox(request.MailboxEmail);
                FolderId fid = new FolderId(WellKnownFolderName.Calendar, mbx);
                
                //CalendarFolder calendar = CalendarFolder.Bind(service, fid, new PropertySet(FolderSchema.ManagedFolderInformation, FolderSchema.ParentFolderId, FolderSchema.ExtendedProperties));

                // Create appointment
                var appointment = new Appointment(service);

                appointment.Subject = "Impromptu Meeting";
                appointment.Body = "Booked via Conference Room app.";
                appointment.Start = DateTime.Now;
                appointment.End = DateTime.Now.AddMinutes(request.BookMinutes);
                appointment.Location = responseMailboxInfo.DisplayName;                
                appointment.Save(fid, SendInvitationsMode.SendToNone);
                
                // Verify saved 
                Item item = Item.Bind(service, appointment.Id, new PropertySet(ItemSchema.Subject));
                if (item == null)
                {
                    response.ErrorMessage = "Error saving appointment to calendar.";
                    return response;
                }
                else
                {
                    response.AppointmentId = appointment.Id;
                    response.Success = true;       
                }
            }
            catch (Exception e)
            {
                response.ErrorMessage = e.ToString();
                return response;
            }
                        
            return response;
        }
    }
}
