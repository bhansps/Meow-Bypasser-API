using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using AngleSharp.Parser.Html;
using Microsoft.IdentityModel.Tokens;

namespace NaN_Api
{
    public class Converter
    {
        public void Convert(Link link, int maxHops, out bool ConvertErr, out Link linkOut)
        {
            ServicePointManager.DefaultConnectionLimit = 100;
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
                            using (var client = new WebClientExtended())
                            {
                                var html = client.DownloadString(link.Url);
                                string realLink = "";
                                isShortLink(client.ResponseUri.ToString(), out var dummyStatus, out var dummy2);
                                if (!dummyStatus)
                                {
                                    realLink = client.ResponseUri.ToString();
                                    parsedURL = realLink;
                                    convertURLstatus = true;
                                    break;
                                }

                                var parser = new HtmlParser();
                                var doc = parser.Parse(html);
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

                                /* Regex Match Pattern from Nyan-API (GoLang) -> r, _:= regexp.Compile("\\?r=(.+?)?\"");
                                var pattern = "\\?r=(.+?)?\"";
                                var result = Regex.Matches(html, pattern);
                                if (result.Count >= 1)
                                {
                                    var decoded = Base64UrlEncoder.Decode(result[0].Value);
                                    parsedURL = decoded;
                                    convertURLstatus = true;
                                } */
                            }

                            break;
                        case 2:
                            using (var client = new WebClientExtended())
                            {
                                string html = "";

                                //this is the important bit...
                                client.Headers.Add("User-Agent",
                                    "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.4) Gecko/20060508 Firefox/1.5.0.4");
                                client.Headers.Add("Accept-Language", "en-us,en;q=0.5");
                                //end of the important bit...
                                
                                var parser = new HtmlParser();
                                Uri decryptParam = new Uri(link.Url);
                                string decryptLinkParamid = "";
                                if (HttpUtility.ParseQueryString(decryptParam.Query).Get("id")!=null)
                                {
                                    decryptLinkParamid = HttpUtility.ParseQueryString(decryptParam.Query).Get("id");
                                }
                                string decryptLinkParamc = "";
                                if (HttpUtility.ParseQueryString(decryptParam.Query).Get("c")!=null)
                                {
                                    decryptLinkParamc = HttpUtility.ParseQueryString(decryptParam.Query).Get("c");
                                }
                                string decryptLinkParamuser = "";
                                if (HttpUtility.ParseQueryString(decryptParam.Query).Get("user") != null)
                                {

                                    decryptLinkParamuser = HttpUtility.ParseQueryString(decryptParam.Query).Get("user");
                                }

                                var decryptLink =$"http://decrypt.safelinkconverter.com/index.php?id={decryptLinkParamid}&c={decryptLinkParamc}&user={decryptLinkParamuser}&pop=0";
                                var decryptHtml = client.DownloadString(new Uri(decryptLink));

                                var decryptdoc = parser.Parse(decryptHtml);
                                var decryptedText =
                                    Regex.Replace(
                                        decryptdoc.QuerySelector("div[class^='redirect_url'] div").TextContent,
                                        @"\t|\n|\r", "");

                                parsedURL = decryptedText;
                                convertURLstatus = true;
                                /* Regex Match Pattern from Nyan-API (GoLang) -> r, _:= regexp.Compile("\\?id=(.+?)?&");
                                var pattern = "\\?id=(.+?)?&";
                                var result = Regex.Matches(html, pattern);
                                if (result.Count >= 1)
                                {
                                    var decoded = Base64UrlEncoder.Decode(result[0].Value);
                                    parsedURL = decoded;
                                    convertURLstatus = true;
                                }*/
                            }

                            break;
                        case 3:
                        case 4:
                            using (var client = new WebClientExtended())
                            {
                                var html = client.DownloadString(link.Url);
                                var parser = new HtmlParser();
                                var doc = parser.Parse(html);
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
                                /*Regex Match Pattern from Nyan-API (GoLang) -> r, _:= regexp.Compile("<center.*href=\"(.+?)?\"");
                                var pattern = "<center.*href=\"(.+?)?\"";
                                var result = Regex.Matches(html, pattern);
                                if (result.Count >= 1 && result[1].Value.Contains("awsubs"))
                                {
                                    parsedURL = result[1].Value;
                                }*/
                            }
                            break;
                        case 5:
                            using (var client = new WebClientExtended())
                            {
                                var html = client.DownloadString(link.Url);
                                var pattern = "\\?(.*)";
                                var result = Regex.Matches(html, pattern);
                                if (result.Count >= 1)
                                {
                                    var decoded = Base64UrlEncoder.Decode(result[0].Value);
                                    parsedURL = decoded;
                                    convertURLstatus = true;
                                }
                            }

                            //                    r, _:= regexp.Compile("\\?(.*)")
                            //       
                            //            if b64String := r.FindStringSubmatch(url); len(b64String) > 1 {
                            //                        decoded, _:= b64.StdEncoding.DecodeString(b64String[1])
                            //
                            //                parsedURL = string(decoded)         
                            break;
                        case 6:
                            using (var client = new WebClientExtended())
                            {
                                var html = client.DownloadString(link.Url);
                                var pattern = "decode\\(\"(.+?)\"\\)";
                                var result = Regex.Matches(html, pattern);
                                if (result.Count >= 1)
                                {
                                    var toDecode = result[0].Value.Replace("decode(\"", "").Replace("\")", "");

                                    var decoded = Base64UrlEncoder.Decode(toDecode);
                                    parsedURL = decoded;
                                    convertURLstatus = true;
                                }
                            }

                            //                    r, _:= regexp.Compile("\\?r=(.*)")
                            //       
                            //            if b64String := r.FindStringSubmatch(url); len(b64String) > 1 {
                            //                        decoded, _:= b64.StdEncoding.DecodeString(b64String[1])
                            //
                            //                parsedURL = string(decoded)
                            break;
                        case 7:
                            using (var client = new WebClientExtended())
                            {
                                var html = client.DownloadString(link.Url);
                                var parser = new HtmlParser();
                                var doc = parser.Parse(html);
                                string realLink = "";
                                var node = doc.QuerySelectorAll("script");
                                foreach (var jsElement in node)
                                {
                                    string toRegex = jsElement.InnerHtml;
                                    if (Regex.IsMatch(toRegex, "link", RegexOptions.IgnoreCase))
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