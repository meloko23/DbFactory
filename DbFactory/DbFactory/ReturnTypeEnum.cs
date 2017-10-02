using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbFactory
{
    enum ReturnTypeEnum : byte
    {
        Integer = 0,
        DataSet = 1,
        DataTable = 2,
        Scalar = 3,
        Reader = 4
    }
}
