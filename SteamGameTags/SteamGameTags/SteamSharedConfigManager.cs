using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace SteamGameTags
{
    internal sealed class SteamVDFNode
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public List<SteamVDFNode> ChildNodes { get; set; }

        public SteamVDFNode GetChildNode(string nodeName)
        {
            if (ChildNodes == null)
                return null;

            foreach (var child in ChildNodes)
            {
                if (child.Name == nodeName)
                    return child;
            }

            return null;
        }

        public SteamVDFNode GetChildNodeValue(string nodeValue)
        {
            if (ChildNodes == null)
                return null;

            foreach (var child in ChildNodes)
            {
                if (child.Value == nodeValue)
                    return child;
            }

            return null;
        }

        public void AddTagNode(string tagName)
        {
            if (ChildNodes == null)
                ChildNodes = new List<SteamVDFNode>();

            var tagNode = GetChildNode("tags");
            if (tagNode == null)
            {
                tagNode = new SteamVDFNode { Name = "tags" };
                ChildNodes.Add(tagNode);
            }
            if (tagNode.ChildNodes == null)
                tagNode.ChildNodes = new List<SteamVDFNode>();

            if (tagNode.GetChildNodeValue(tagName) == null)
            {
                tagNode.ChildNodes.Add(new SteamVDFNode { Name = tagNode.ChildNodes.Count.ToString(), Value = tagName} );
            }
        }

        public void WriteNode(int level, StreamWriter writer)
        {
            if (ChildNodes != null)
            {
                for (var i = 0; i < level; i++) writer.Write("\t"); writer.WriteLine($"\"{Name}\"");
                for (var i = 0; i < level; i++) writer.Write("\t"); writer.WriteLine("{");
                foreach (var child in ChildNodes)
                {
                    child.WriteNode(level + 1, writer);
                }
                for (var i = 0; i < level; i++) writer.Write("\t"); writer.WriteLine("}");
            }
            else
            {
                for (var i = 0; i < level; i++) writer.Write("\t"); writer.WriteLine($"\"{Name}\"\t\t\"{Value}\"");
            }
        }
    }

    internal sealed class SteamSharedConfigManager
    {
        private readonly string _sharedConfigFileName;

        private SteamVDFNode _rootNode;

        public SteamSharedConfigManager(string sharedConfigFileName)
        {
            _sharedConfigFileName = sharedConfigFileName;
        }

        public SteamVDFNode RootNode
        {
            get
            {
                if (_rootNode == null)
                    ReadFile();

                return _rootNode;
            }
        }

        private void ReadFile()
        {
            using (StreamReader reader = File.OpenText(_sharedConfigFileName))
            {
                _rootNode = ReadVDFNode(null, reader);
            }
        }

        private SteamVDFNode ReadVDFNode(string foreRead, StreamReader reader)
        {
            var s = foreRead ?? ReadNextLine(reader);

            var split = s.Split(new [] { '"' }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length == 1)
            {
                var res = new SteamVDFNode { Name = split[0], Value = null, ChildNodes = new List<SteamVDFNode>() };

                ReadNextLine(reader); // "{"
                while (true)
                {
                    var t = ReadNextLine(reader);
                    if (t.StartsWith("}"))
                    {
                        break;
                    }
                    res.ChildNodes.Add(ReadVDFNode(t, reader));
                }

                return res;
            }
            return new SteamVDFNode { Name = split[0], Value = split[2], ChildNodes = null };
        }

        private string ReadNextLine(StreamReader reader)
        {
            var s = default(string);

            while (string.IsNullOrEmpty(s))
            {
                s = reader.ReadLine().Trim(new[] { ' ', '\t' });
            }

            return s;
        }
    }
}
