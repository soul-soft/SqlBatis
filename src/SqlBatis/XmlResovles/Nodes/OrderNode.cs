using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBatis.XmlResovles
{
    internal class OrderNode : INodeList
    {
        public List<INode> Nodes { get; private set; } = new List<INode>();
    }
}
