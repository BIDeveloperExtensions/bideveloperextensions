using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Data.OleDb;

namespace PCDimNaturalizer
{
    static class Program
    {
        public static frmASFlattener ASFlattener;
        public static frmProgress Progress;
        public static frmSQLFlattener SQLFlattener;
        public static string LogFile = null;
    }
}
