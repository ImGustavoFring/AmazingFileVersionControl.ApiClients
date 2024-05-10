using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazingFileVersionControl.ApiClients.Helpers
{
    public static class TokenHelper
    {
        public static string ExtractToken(string jsonResponse)
        {
            return JObject.Parse(jsonResponse)["token"].ToString();
        }
    }
}
