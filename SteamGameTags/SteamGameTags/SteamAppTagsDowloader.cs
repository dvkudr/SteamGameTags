using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace SteamGameTags
{
    internal sealed class SteamAppTagsDowloader
    {
        private static readonly char[] TrimChars = {' ', '\t', '\r', '\n' };

        public static List<string> GetSteamAppTags(string appId)
        {
            var appUrl = string.Format($"http://store.steampowered.com/app/{appId}/");

            try
            {
                using (WebClient client = new WebClient())
                {
                    string htmlCode = client.DownloadString(appUrl);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(htmlCode);

                    var titleNode = doc.DocumentNode.SelectSingleNode("//div[@class='apphub_AppName']");
                    Console.WriteLine(titleNode?.InnerText);

                    var appTagNodes = doc.DocumentNode.SelectNodes("//a[@class='app_tag']");

                    return appTagNodes?.Select(n => n.InnerText.Trim(TrimChars)).ToList() ?? new List<string>();
                }
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
