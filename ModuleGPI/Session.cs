using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleGPI
{
    public static class Session
    {
        public static string LogonName { get; set; } = string.Empty;
        public static string Sucursal { get; set; } = string.Empty;
        public static int TypeAut { get; set; } = 1;
        public static string EmpId { get; set; } = string.Empty;  // NUEVO

    }

}
