using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SqlBatis.XmlResovles
{
    internal class CommandNode : INode
    {
        public List<INode> Nodes { get; set; } = new List<INode>();

        private string ResolveTextNode(TextNode node)
        {
            return node.Value;
        }

        private string ResolveIfNode<T>(IfNode node, T parameter)
        {
            if (node.Delegate == null)
            {
                lock (this)
                {
                    var context = new ExpressionContext();
                    var result = context.Create<T>(node.Test);
                    node.Delegate = result.Func;
                }
            }
            var func = node.Delegate as Func<T, bool>;
            if (func(parameter))
            {
                return ResolveTextNode(new TextNode { Value = node.Value });
            }
            return string.Empty;
        }

        private string ResolveWhereNode<T>(WhereNode node, T parameter)
        {
            var buffer = new StringBuilder();
            foreach (var item in node.Nodes)
            {
                if (item is IfNode)
                {
                    buffer.Append(ResolveIfNode<T>(item as IfNode, parameter));
                }
                else if (item is TextNode)
                {
                    buffer.Append(ResolveTextNode(item as TextNode));
                }
            }
            var sql = buffer.ToString();
            sql = Regex.Replace(sql, @"\s+", " ").Trim(' ');
            if (sql.StartsWith("and", StringComparison.OrdinalIgnoreCase))
            {
                sql = sql.Remove(0, 3).Trim(' ');
            }
            else if (sql.StartsWith("or", StringComparison.OrdinalIgnoreCase))
            {
                sql = sql.Remove(0, 2).Trim(' ');
            }
            return sql.Length > 0 ? " WHERE " + sql : string.Empty;
        }

        public string Resolve<T>(CommandNode command, T parameter)
        {
            var buffer = new StringBuilder();
            foreach (var item in command.Nodes)
            {
                if (item is TextNode)
                {
                    buffer.Append(ResolveTextNode(item as TextNode));
                }
                else if (item is WhereNode)
                {
                    buffer.Append(ResolveWhereNode(item as WhereNode, parameter));
                }
                else if (item is IfNode)
                {
                    buffer.Append(ResolveIfNode(item as IfNode, parameter));
                }
            }
            var sql = buffer.ToString();
            sql = Regex.Replace(sql, @"\s+", " ").Trim(' ');
            return sql;
        }
    }
}
