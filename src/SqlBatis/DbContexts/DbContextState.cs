using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBatis
{
    public enum DbContextState
    {
        Closed = 0,
        Open = 1,
        Commit = 2,
        Rollback = 3,
    }
}
