using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NaN_Api
{
    public class WebClientExtended : WebClient
    {
        Uri _responseUri;

        public Uri ResponseUri => _responseUri;

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = base.GetWebResponse(request);
            _responseUri = response.ResponseUri;
            return response;

        }
    }
}