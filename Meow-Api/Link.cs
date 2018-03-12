using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Rewrite.Internal;

namespace Meow_Api
{
    public class Link
    {
        public Link()
        {
            // default ctor
        }

        public Link(string url, bool isConverted)
        {
            Url = url;
            IsConverted = isConverted;
        }

        public string Url { get; set; }
        public bool IsConverted { get; set; }
    }
}