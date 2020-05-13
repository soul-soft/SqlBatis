using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis.Attributes
{
    /// <summary>
    /// 并发检查列，如果字段属性是number类型则用时间戳，否则使用GUID
    /// </summary>
    public class ConcurrencyCheckAttribute : Attribute
    {

    }
}
