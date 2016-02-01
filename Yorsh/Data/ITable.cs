using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yorsh.Data
{
    interface ITable
    {
        string TableName { get; }
        bool IsEnabled { get; set; }
    }
}
