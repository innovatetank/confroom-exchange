using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Exchange.Dtos
{
    public class ReserveRoomRequestDto
    {
        public string MailboxEmail { get; set; }
        public int BookMinutes { get; set; }
    }
}
