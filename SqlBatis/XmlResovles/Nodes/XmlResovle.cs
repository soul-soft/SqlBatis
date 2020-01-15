using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SqlBatis.XmlResovles
{
    public class XmlResovle : IXmlResovle
    {
        private readonly Dictionary<string, string> variables = new Dictionary<string, string>();

        private readonly Dictionary<string, CommandNode> commands = new Dictionary<string, CommandNode>();

        public void Load(string filename)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filename);
            var @namespace = document.DocumentElement.GetAttribute("namespace") ?? string.Empty;
            ResolveVariable(document);
            foreach (XmlElement item in document.DocumentElement.Cast<XmlElement>().Where(a => a.Name != "variable"))
            {
                var id = item.GetAttribute("id");
                id = string.IsNullOrEmpty(@namespace) ? $"{id}" : $"{@namespace}.{id}";
                var cmd = ResolveCommand(item);
                commands.Add(id, cmd);
            }
        }

        private void ResolveVariable(XmlDocument document)
        {
            foreach (XmlElement item in document.DocumentElement)
            {
                if (item.Name == "variable")
                {
                    var id = item.GetAttribute("id");
                    var value = string.IsNullOrEmpty(item.InnerText)
                         ? item.GetAttribute("value") : item.InnerText;
                    variables.Add(id, value);
                }
            }
        }

        private string ResolveVariable(string text)
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
            return text;
        }

        private CommandNode ResolveCommand(XmlElement element)
        {
            var cmd = new CommandNode();
            foreach (XmlNode item in element.ChildNodes)
            {
                if (item.NodeType == XmlNodeType.Text)
                {
                    var text = ResolveVariable(item.Value);
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
                            var text = ResolveVariable(iitem.Value);
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
                    cmd.Nodes.Add(new IfNode
                    {
                        Test = test,
                        Value = value
                    });
                }
            }
            return cmd;
        }

        public string Resolve<T>(string id, T parameter)
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
    }
}
