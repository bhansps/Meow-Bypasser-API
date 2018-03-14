using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Network.Default;
using AngleSharp.Parser.Html;
using Microsoft.IdentityModel.Tokens;


namespace Meow_Api
{
    public class Converter
    {
        private static readonly HttpClient Connection = new HttpClient();

        public Converter()
        {
            #region Connection Configuration

            //set Connection SPM
            ServicePointManager.DefaultConnectionLimit = 200;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
//            //set Accept headers for HttpClient
            Connection.DefaultRequestHeaders.TryAddWithoutValidation("Accept",
                "text/html,application/xhtml+xml,application/xml,application/json");
//            //set User agent for HttpClient
            Connection.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; EN; rv:11.0) like Gecko");
            Connection.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

            #endregion
        }

        public void Convert(Link link, int maxHops, out bool ConvertErr, out Link linkOut)
        {
            string parsedURL = "undecided";
            bool convertURLstatus = false;
            linkOut = link;
            ConvertErr = false;
            if (maxHops.Equals(0))
            {
                return;
            }

            maxHops--;
            isShortLink(link.Url, out var status, out var adType);
            if (status)
            {
                try
                {
                    switch (adType)
                    {
                        case 1:
                            using (var response = Connection.GetAsync(link.Url).Result)
                            {
                                var contentBytes = response.Content.ReadAsByteArrayAsync().Result; //sync call
                                var content = Encoding.UTF8.GetString(response.Content.ReadAsByteArrayAsync().Result, 0,
                                    contentBytes.Length);
                                if (content[0] == '\uFEFF')
                                {
                                    content = content.Substring(1);
                                }

                                string realLink = "";
                                response.EnsureSuccessStatusCode();
                                string responseUri = response.RequestMessage.RequestUri.ToString();
                                isShortLink(responseUri, out var dummyStatus, out var dummy2);
                                if (!dummyStatus)
                                {
                                    realLink = responseUri;
                                    parsedURL = realLink;
                                    convertURLstatus = true;
                                    break;
                                }

                                var parser = new HtmlParser();
                                var doc = parser.Parse(content);
                                var node = doc.QuerySelectorAll("a").Where(x => x.HasAttribute("href"));
                                foreach (var nodeLink in node)
                                {
                                    if (nodeLink.Attributes.GetNamedItem("href").Value.Contains("?r="))
                                    {
                                        realLink =
                                            nodeLink.Attributes.GetNamedItem("href").Value.Split("?r=").ToList()[1];
                                        break;
                                    }
                                }

                                var decoded = Base64UrlEncoder.Decode(realLink);
                                parsedURL = decoded;
                                convertURLstatus = true;
                            }
                            break;
                        case 2:
                            var decryptLink = "";
                            try
                            {
                                Uri decryptParam = new Uri(link.Url);
                                string decryptLinkParamid = "";
                                if (HttpUtility.ParseQueryString(decryptParam.Query).Get("id") != null)
                                {
                                    decryptLinkParamid = HttpUtility.ParseQueryString(decryptParam.Query).Get("id");
                                }

                                string decryptLinkParamc = "";
                                if (HttpUtility.ParseQueryString(decryptParam.Query).Get("c") != null)
                                {
                                    decryptLinkParamc = HttpUtility.ParseQueryString(decryptParam.Query).Get("c");
                                }

                                string decryptLinkParamuser = "";
                                if (HttpUtility.ParseQueryString(decryptParam.Query).Get("user") != null)
                                {
                                    decryptLinkParamuser = HttpUtility.ParseQueryString(decryptParam.Query).Get("user");
                                }

                                decryptLink =
                                    $"http://decrypt.safelinkconverter.com/index.php?id={decryptLinkParamid}&c={decryptLinkParamc}&user={decryptLinkParamuser}&pop=0";
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Error on case 2 : decryptLink\n{e.Message}");
                                break;
                            }

                            using (var response = Connection.GetAsync(decryptLink).Result)
                            {
                                var parser = new HtmlParser();
                                var contentBytes = response.Content.ReadAsByteArrayAsync().Result; //sync call
                                var content = Encoding.UTF8.GetString(response.Content.ReadAsByteArrayAsync().Result, 0,
                                    contentBytes.Length);
                                if (content[0] == '\uFEFF')
                                {
                                    content = content.Substring(1);
                                }

                                var decryptdoc = parser.Parse(content);
                                var decryptedText =
                                    Regex.Replace(
                                        decryptdoc.QuerySelector("div[class^='redirect_url'] div").TextContent,
                                        @"\t|\n|\r", "");

                                parsedURL = decryptedText;
                                convertURLstatus = true;
                            }
                            break;
                        case 3:
                        case 4:
                            using (var response = Connection.GetAsync(link.Url).Result)
                            {
                                var contentBytes = response.Content.ReadAsByteArrayAsync().Result; //sync call
                                var content = Encoding.UTF8.GetString(response.Content.ReadAsByteArrayAsync().Result, 0,
                                    contentBytes.Length);
                                if (content[0] == '\uFEFF')
                                {
                                    content = content.Substring(1);
                                }

                                var parser = new HtmlParser();
                                var doc = parser.ParseAsync(content).Result;
                                string realLink = "";
                                var node = doc.QuerySelectorAll("div[class^='iklan'] a")
                                    .Where(x => x.HasAttribute("href"));
                                foreach (var anyElement in node)
                                {
                                    realLink = anyElement.Attributes.GetNamedItem("href").Value;
                                    break;
                                }

                                parsedURL = realLink;
                                convertURLstatus = true;
                            }
                            break;
                        case 5:
                            using (var response = Connection.GetAsync(link.Url).Result)
                            {
                                var contentBytes = response.Content.ReadAsByteArrayAsync().Result; //sync call
                                var content = Encoding.UTF8.GetString(response.Content.ReadAsByteArrayAsync().Result, 0,
                                    contentBytes.Length);
                                if (content[0] == '\uFEFF')
                                {
                                    content = content.Substring(1);
                                }

                                var pattern = "\\?(.*)";
                                var result = Regex.Matches(content, pattern);
                                if (result.Count >= 1)
                                {
                                    var decoded = Base64UrlEncoder.Decode(result[0].Value);
                                    parsedURL = decoded;
                                    convertURLstatus = true;
                                }
                            }   
                            break;
                        case 6:
                            using (var response = Connection.GetAsync(link.Url).Result)
                            {
                                var contentBytes = response.Content.ReadAsByteArrayAsync().Result; //sync call
                                var content = Encoding.UTF8.GetString(response.Content.ReadAsByteArrayAsync().Result, 0,
                                    contentBytes.Length);
                                if (content[0] == '\uFEFF')
                                {
                                    content = content.Substring(1);
                                }

                                var pattern = "decode\\(\"(.+?)\"\\)";
                                var result = Regex.Matches(content, pattern);
                                if (result.Count >= 1)
                                {
                                    var toDecode = result[0].Value.Replace("decode(\"", "").Replace("\")", "");

                                    var decoded = Base64UrlEncoder.Decode(toDecode);
                                    parsedURL = decoded;
                                    convertURLstatus = true;
                                }
                            }
                            break;
                        case 7:
                            using (var response = Connection.GetAsync(link.Url).Result)
                            {
                                var contentBytes = response.Content.ReadAsByteArrayAsync().Result; //sync call
                                var content = Encoding.UTF8.GetString(response.Content.ReadAsByteArrayAsync().Result, 0,
                                    contentBytes.Length);
                                if (content[0] == '\uFEFF')
                                {
                                    content = content.Substring(1);
                                }

                                var parser = new HtmlParser();
                                var doc = parser.ParseAsync(content).Result;
                                string realLink = "";
                                var node = doc.QuerySelectorAll("script");
                                foreach (var jsElement in node)
                                {
                                    string toRegex = jsElement.InnerHtml;
                                    if (toRegex.Contains("link-content"))
                                    {
                                        var match = Regex.Matches(toRegex, "\"(.+?)\"");
                                        foreach (var matchElement in match)
                                        {
                                            if (matchElement.ToString().Contains("http"))
                                            {
                                                realLink = matchElement.ToString().Remove(0, 1)
                                                    .Remove(matchElement.ToString().Length - 2, 1);
                                                break;
                                            }
                                        }

                                        break;
                                    }
                                }

                                parsedURL = realLink;
                                convertURLstatus = true;
                            }

                            break;
                        case 8:
                            var requester = new HttpRequester();
                            requester.Headers["X-Requested-With"] = "XMLHttpRequest";
                            var configuration = Configuration.Default
                                .WithDefaultLoader(_ => _.IsResourceLoadingEnabled = true, new[] {requester})
                                .WithCookies();
                            var context = BrowsingContext.New(configuration);
                            using (var response = context.OpenAsync(link.Url).Result)
                            {
                                var json = "";
                                using (var content = response.QuerySelector<IHtmlFormElement>("form").SubmitAsync().Result)
                                {
                                    using (var jsonResult = content.QuerySelector<IHtmlFormElement>("form")
                                        .SubmitAsync().Result)
                                    {
                                        json = jsonResult.Body.TextContent;
                                    }

                                }
                                var regexMatch = Regex.Matches(json, "\\\"(.+?)\\\"");
                                var realLink = regexMatch[regexMatch.Count - 1].ToString().Replace("\"", "")
                                    .Replace(@"\", "");

                                parsedURL = realLink;
                                convertURLstatus = true;
                            }
                            break;
                        default:
                            ConvertErr = true;
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}");
                    ConvertErr = true;
                    return;
                }
            }

            isShortLink(parsedURL, out var parsedStatus, out adType);
            if (parsedStatus)
            {
                var tempLink = new Link(parsedURL, false);
                Convert(tempLink, maxHops, out ConvertErr, out linkOut);
                return;
            }

            if (!parsedURL.Equals("undecided") && parsedURL.Length > 1 && convertURLstatus)
            {
                linkOut.Url = parsedURL;
                linkOut.IsConverted = true;
            }
            else
            {
                ConvertErr = true;
            }
        }

        private void isShortLink(string url, out bool status, out int adType)
        {
            adType = 0; // default
            status = false;
            List<List<string>> fansubAds = new List<List<string>>(new[]
                {
                    new List<string>(new[]
                    {
                        "telondasmu", "coeg.in", "siotong", "siherp"
                    }), // tested = telondasmu, siotong, siherp, coeg.in : ALL
                    new List<string>(new[] {"link.safelinkconverter"}), // tested = link.safelinkconverter : ALL/BUGGY
                    new List<string>(new[] {"short.awsubs.co"}), // tested = short.awsubs.co : ALL
                    new List<string>(new[] {"bit.ly"}), // tested = bit.ly? : Maybe
                    new List<string>(new[] {"wptech"}), // tested = ? : NONE
                    new List<string>(new[] {"lindung"}), // tested = lindung : ALL
                    new List<string>(new[] {"94lauin"}), // tested = 94lauin : ALL
                    new List<string>(new[] {"safelinku"}), // tested = safelinku : ALL
                }
            );
            foreach (var adsGroup in fansubAds)
            {
                adType++;
                foreach (var ad in adsGroup)
                {
                    if (url.Contains(ad))
                    {
                        status = true;
                        return;
                    }
                }
            }

            status = false;
            adType = 0;
        }
    }
}