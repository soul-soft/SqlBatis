using SqlBatis.XmlResovles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SqlBatis
{
    public interface IXmlResovle
    {
        string Resolve<T>(string id, T parameter) where T : class;
        string Resolve(string id);
        void Load(string filename);
        void Load(string path, string pattern);
    }
  
    public class XmlResovle : IXmlResovle
    {
        private readonly Dictionary<string, CommandNode> commands = new Dictionary<string, CommandNode>();

        private Dictionary<string, string> ResolveVariables(XmlDocument document)
        {
            var variables = new Dictionary<string, string>();
            var elements = document.DocumentElement.Cast<XmlNode>().Where(a => a.Name == "variable");
            foreach (XmlElement item in elements)
            {
                if (item.Name == "variable")
                {
                    var id = item.GetAttribute("id");
                    var value = string.IsNullOrEmpty(item.InnerText)
                         ? item.GetAttribute("value") : item.InnerText;
                    variables.Add(id, value);
                }
            }
            return variables;
        }

        private string ReplaceVariable(Dictionary<string, string> variables, string text)
        {
            var matches = Regex.Matches(text, @"\${(?<key>.*?)}");
            foreach (Match item in matches)
            {
                var key = item.Groups["key"].Value;
                if (variables.ContainsKey(key))
                {
                    var value = variables[key];
                    text = text.Replace("${" + key + "}", value);
                }
            }
            return Regex.Replace(text, @"\s+", " ").Trim(' ');
        }

        private CommandNode ResolveCommand(Dictionary<string, string> variables, XmlElement element)
        {
            var cmd = new CommandNode();
            foreach (XmlNode item in element.ChildNodes)
            {
                if (item.NodeType == XmlNodeType.Text)
                {
                    var text = ReplaceVariable(variables, item.Value);
                    cmd.Nodes.Add(new TextNode
                    {
                        Value = text
                    });
                }
                else if (item.NodeType == XmlNodeType.Element && item.Name == "where")
                {
                    var whereNode = new WhereNode();
                    foreach (XmlNode iitem in item.ChildNodes)
                    {
                        if (iitem.NodeType == XmlNodeType.Text)
                        {
                            var text = ReplaceVariable(variables, iitem.Value);
                            whereNode.Nodes.Add(new TextNode
                            {
                                Value = text
                            });
                        }
                        else if (iitem.NodeType == XmlNodeType.Element && iitem.Name == "if")
                        {
                            var test = iitem.Attributes["test"].Value;
                            var value = string.IsNullOrEmpty(iitem.InnerText) ?
                                (iitem.Attributes["value"]?.Value ?? string.Empty) : iitem.InnerText;
                            value = ReplaceVariable(variables, value);
                            whereNode.Nodes.Add(new IfNode
                            {
                                Test = test,
                                Value = value
                            });
                        }
                    }
                    cmd.Nodes.Add(whereNode);
                }
                else if (item.NodeType == XmlNodeType.Element && item.Name == "if")
                {
                    var test = item.Attributes["test"].Value;
                    var value = string.IsNullOrEmpty(item.InnerText) ?
                             (item.Attributes["value"]?.Value ?? string.Empty) : item.InnerText;
                    value = ReplaceVariable(variables, value);
                    cmd.Nodes.Add(new IfNode
                    {
                        Test = test,
                        Value = value
                    });
                }
            }
            return cmd;
        }

        public string Resolve<T>(string id, T parameter) where T : class
        {
            if (!commands.ContainsKey(id))
            {
                return null;
            }
            var cmd = commands[id];
            return cmd.Resolve(cmd, parameter);
        }

        public string Resolve(string id)
        {
            return Resolve(id, (object)null);
        }

        public void Load(string filename)
        {
            lock (this)
            {
                XmlDocument document = new XmlDocument();
                document.Load(filename);
                var @namespace = document.DocumentElement.GetAttribute("namespace")
                    ?? string.Empty;
                var variables = ResolveVariables(document);
                var elements = document.DocumentElement
                    .Cast<XmlNode>()
                    .Where(a => a.Name != "variable" && a is XmlElement);
                foreach (XmlElement item in elements)
                {
                    var id = item.GetAttribute("id");
                    id = string.IsNullOrEmpty(@namespace) ? $"{id}" : $"{@namespace}.{id}";
                    var cmd = ResolveCommand(variables, item);
                    if (commands.ContainsKey(id))
                    {
                        commands[id] = cmd;
                    }
                    else
                    {
                        commands.Add(id, cmd);
                    }
                }
            }
        }

        public void Load(string path, string pattern)
        {
            var files = System.IO.Directory.GetFiles(path, pattern);
            foreach (var item in files)
            {
                Load(item);
            }
        }
    }
}
