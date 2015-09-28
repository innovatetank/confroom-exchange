using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Logic
{
    [Serializable]
    public class BaseResponse
    {
        public string ErrorMessage { get; set; }
        public bool Success { get; set; }

        public bool Error
        {
            get
            {
                return !Success;
            }
        }

    }
}
