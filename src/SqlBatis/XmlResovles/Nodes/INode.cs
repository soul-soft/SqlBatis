using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis.XmlResovles
{
    interface INode
    {

    }
    interface INodeList: INode
    {
        List<INode> Nodes { get; }
    }
}
