using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Logic.Exchange
{
    public class GetCalendarItems
    {
        [Serializable]
        public class GetCalendarItemsRequest : BaseRequest
        {
            public string MailboxEmail { get; set; }
            public int DaysToLoad { get; set; }

            public GetCalendarItemsRequest(string mailboxEmail, int daysToLoad)
            {
                MailboxEmail = mailboxEmail;
                DaysToLoad = daysToLoad;
            }
        }

        [Serializable]
        public class GetCalendarItemsResponse : BaseResponse
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<Models.AppointmentItem> Appointments { get; set; }
        }

        public GetCalendarItemsResponse Execute(GetCalendarItemsRequest request)
        {
            var response = new GetCalendarItemsResponse();

            var service = ExchangeServiceConnector.GetService();

            // Initialize values for the start and end times, and the number of appointments to retrieve.
            DateTime startDate = DateTime.Today;
            DateTime endDate = startDate.AddDays(request.DaysToLoad);
            const int NUM_APPTS = 20;

            var mbx = new Mailbox(request.MailboxEmail);
            

            FolderId fid = new FolderId(WellKnownFolderName.Calendar, mbx);

            // Initialize the calendar folder object with only the folder ID. 
            //CalendarFolder calendar = CalendarFolder.Bind(service, WellKnownFolderName.Calendar, new PropertySet());
            CalendarFolder calendar = CalendarFolder.Bind(service, fid, new PropertySet(FolderSchema.ManagedFolderInformation, FolderSchema.ParentFolderId, FolderSchema.ExtendedProperties));

            // Set the start and end time and number of appointments to retrieve.
            CalendarView cView = new CalendarView(startDate, endDate, NUM_APPTS);

            // Limit the properties returned to the appointment's subject, start time, and end time.
            cView.PropertySet = new PropertySet(AppointmentSchema.Subject, AppointmentSchema.Start, AppointmentSchema.End);

            // Retrieve a collection of appointments by using the calendar view.
            FindItemsResults<Appointment> appointments = calendar.FindAppointments(cView);
            
            var items = new List<Models.AppointmentItem>();
            foreach (var a in appointments)
            {
                //find appointments will only give basic properties.
                //in order to get more properties (such as BODY), we need to call call EWS again
                //Appointment appointmentDetailed = Appointment.Bind(service, a.Id, new PropertySet(BasePropertySet.FirstClassProperties) { RequestedBodyType = BodyType.Text });

                
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

        /*
        /// <summary>
        /// Gets the all calendar data as well as extended property data of a specific resource mailbox.
        /// </summary>
        /// <param name="mailbox">The specified resource mailbox - boardroom email address.</param>
        /// <param name="startDate">The start date of the calendar view.</param>
        /// <param name="endDate">The end date of the calendar view.</param>
        /// <param name="boardRoomID">The board room ID.</param>
        /// <returns>
        /// returns a collection of appointments related as well as the extended property data to the specified resource mailbox
        /// </returns>
        /// <exception cref="System.Exception">Represents errors that occur during application execution.</exception>
        public List<ExtendedAppointmentData> GetExtendedPropertyData(string mailbox, DateTime startDate, DateTime endDate, string boardRoomID)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["remotecertificatevalidation"]))
                ServicePointManager.ServerCertificateValidationCallback =
                    delegate(Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                    {
                        return true;
                    };
            ExchangeServiceBinding esb = new ExchangeServiceBinding();
            esb.Url = ConfigurationManager.AppSettings["exchangeService2010"];
            esb.Credentials = new NetworkCredential("test", "password", "domain.local");
            List<ExtendedAppointmentData> tmp = new List<ExtendedAppointmentData>();
            // Form the FindItem request.
            FindItemType findItemRequest = new FindItemType();
            CalendarViewType calendarView = new CalendarViewType();
            calendarView.StartDate = startDate;
            calendarView.EndDate = endDate;
            calendarView.MaxEntriesReturned = 100;
            calendarView.MaxEntriesReturnedSpecified = true;
            findItemRequest.Item = calendarView;
            // Define which item properties are returned in the response.
            ItemResponseShapeType itemProperties = new ItemResponseShapeType();
            // Use the Default shape for the response.
            //itemProperties.BaseShape = DefaultShapeNamesType.IdOnly;
            itemProperties.BaseShape = DefaultShapeNamesType.AllProperties;
            findItemRequest.ItemShape = itemProperties;
            DistinguishedFolderIdType[] folderIDArray = new DistinguishedFolderIdType[1];
            folderIDArray[0] = new DistinguishedFolderIdType();
            folderIDArray[0].Id = DistinguishedFolderIdNameType.calendar;
            folderIDArray[0].Mailbox = new EmailAddressType();
            folderIDArray[0].Mailbox.EmailAddress = mailbox;
            findItemRequest.ParentFolderIds = folderIDArray;
            // Define the traversal type.
            findItemRequest.Traversal = ItemQueryTraversalType.Shallow;
            try
            {
                // Send the FindItem request and get the response.
                FindItemResponseType findItemResponse = esb.FindItem(findItemRequest);
                // Access the response message.
                ArrayOfResponseMessagesType responseMessages = findItemResponse.ResponseMessages;
                ResponseMessageType[] rmta = responseMessages.Items;
                int folderNumber = 0;
                foreach (ResponseMessageType rmt in rmta)
                {
                    // One FindItemResponseMessageType per folder searched.
                    FindItemResponseMessageType firmt = rmt as FindItemResponseMessageType;
                    if (firmt.RootFolder == null)
                        continue;
                    FindItemParentType fipt = firmt.RootFolder;
                    object obj = fipt.Item;
                    // FindItem contains an array of items.
                    if (obj is ArrayOfRealItemsType)
                    {
                        ArrayOfRealItemsType items = (obj as ArrayOfRealItemsType);
                        if (items.Items == null)
                            folderNumber++;
                        else
                        {
                            foreach (ItemType it in items.Items)
                            {
                                if (it is CalendarItemType)
                                {
                                    CalendarItemType cal = (CalendarItemType)it;
                                    ExtendedPropertyType[] extendedProperties = GetExtendedProperties(cal.ItemId, esb);
                                    if (extendedProperties != null)
                                        tmp.Add(new ExtendedAppointmentData()
                                        {
                                            EndDate = cal.End.ToLocalTime(),
                                            StartDate = cal.Start.ToLocalTime(),
                                            EventID = extendedProperties[0].Item.ToString(),
                                            AppointmentStatus = cal.LegacyFreeBusyStatus.ToString(),
                                            BoardroomID = boardRoomID,
                                            FreeBusyInfo = this.GetFreeBusyTimes2010(30, mailbox, startDate, endDate, esb)
                                        });
                                }
                            }
                            folderNumber++;
                        }
                    }
                }
                return tmp;
            }
            catch
            {
                throw;
            }
        }
        /// <summary>
        /// Gets the extended properties of a resource / boardroom.
        /// </summary>
        /// <param name="itemID">The item ID.</param>
        /// <param name="serviceBinding">The exchange service binding.</param>
        /// <returns>
        /// returns an array of extended properties; otherwise null if none existed
        /// </returns>
        private ExtendedPropertyType[] GetExtendedProperties(ItemIdType itemID, ExchangeServiceBinding serviceBinding)
        {
            PathToExtendedFieldType pathClassification = new PathToExtendedFieldType();
            pathClassification.DistinguishedPropertySetId = DistinguishedPropertySetType.PublicStrings;
            pathClassification.DistinguishedPropertySetIdSpecified = true;
            pathClassification.PropertyName = STR_Guid;
            pathClassification.PropertyType = MapiPropertyTypeType.String;

            GetItemType getExPropertiesRequest = new GetItemType();
            ItemIdType iiItemId = new ItemIdType();
            iiItemId = itemID;
            ItemResponseShapeType getResponseShape = new ItemResponseShapeType();
            getResponseShape.BaseShape = DefaultShapeNamesType.AllProperties;
            getResponseShape.IncludeMimeContent = true;
            getExPropertiesRequest.ItemShape = getResponseShape;
            getExPropertiesRequest.ItemShape.AdditionalProperties = new BasePathToElementType[1];
            getExPropertiesRequest.ItemShape.AdditionalProperties[0] = pathClassification;

            getExPropertiesRequest.ItemIds = new ItemIdType[1];
            getExPropertiesRequest.ItemIds[0] = iiItemId;
            getExPropertiesRequest.ItemShape.BaseShape = DefaultShapeNamesType.AllProperties;
            GetItemResponseType giResponse = serviceBinding.GetItem(getExPropertiesRequest);
            if (giResponse.ResponseMessages.Items[0].ResponseClass == ResponseClassType.Error)
            {
                return null;
            }
            else
            {
                ItemInfoResponseMessageType rmResponseMessage = giResponse.ResponseMessages.Items[0] as ItemInfoResponseMessageType;
                MessageType message = rmResponseMessage.Items.Items[0] as MessageType;
                //return (message.ExtendedProperty);
                return (rmResponseMessage.Items.Items[0].ExtendedProperty);
            }
        }

        */


    }
}
