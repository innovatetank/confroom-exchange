using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Logic.Exchange
{ 
    public class GetMailboxInfo
    {
        public class GetMailboxInfoRequest : BaseRequest
        {
            public string Email { get; set; }

            public GetMailboxInfoRequest(string email)
            {
                Email = email;
            }
        }

        public class GetMailboxInfoResponse : BaseResponse
        {
            public string DisplayName { get; set; }
        }

        public GetMailboxInfoResponse Execute(GetMailboxInfoRequest request)
        {
            var response = new GetMailboxInfoResponse();

            SearchResult result = FindAccountByEmail(request.Email);

            string distinguishedName = result.Properties["distinguishedName"][0] as string;
            string name = result.Properties["displayName"] != null
                            ? result.Properties["displayName"][0] as string
                            : string.Empty;

            response.Success = true;
            response.DisplayName = name;

            return response;
        }

        public SearchResult FindAccountByEmail(string email)
        {
            string filter = string.Format("(proxyaddresses=SMTP:{0})", email);

            using (DirectoryEntry gc = new DirectoryEntry("GC:"))
            {
                foreach (DirectoryEntry z in gc.Children)
                {
                    using (DirectoryEntry root = z)
                    {
                        using (DirectorySearcher searcher = new DirectorySearcher(root, filter, new string[] { "proxyAddresses", "objectGuid", "displayName", "distinguishedName" }))
                        {
                            searcher.ReferralChasing = ReferralChasingOption.All;
                            SearchResult result = searcher.FindOne();

                            return result;
                        }
                    }
                    break;
                }
            }

            return null;
        }




        //public string GetProperty(SearchResult searchResult,
        // string PropertyName)
        //{
        //    if (searchResult.Properties.Contains(PropertyName))
        //    {
        //        return searchResult.Properties[PropertyName][0].ToString();
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}
    }
}
