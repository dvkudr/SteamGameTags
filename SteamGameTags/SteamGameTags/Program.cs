using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamGameTags
{
    class Program
    {
        static void Main(string[] args)
        {
            var steamConfig = new SteamSharedConfigManager("sharedconfig.vdf");

            var appList =
                steamConfig.RootNode?.GetChildNode("Software")?
                    .GetChildNode("Valve")?.GetChildNode("Steam")?.GetChildNode("apps").ChildNodes;

            var count = 0;
            foreach (var app in appList)
            {
                Console.Write($"{count++} - {app.Name} - ");

                var appTags = SteamAppTagsDowloader.GetSteamAppTags(app.Name);
                foreach (var tag in appTags)
                {
                    app.AddTagNode(tag);
                }
            }

            using (StreamWriter writer = new StreamWriter("out.vdf"))
                steamConfig.RootNode.WriteNode(0, writer);
        }
    }
}
