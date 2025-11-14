using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleGPI
{
    public static class Session
    {
      


        public static string LogonName { get; set; }
        public static string EmpId { get; set; }
        public static int TypeAut { get; set; } = 1;  // 1=Viewer, 2=Operator, 3=Supervisor, 4=AdminDept, 5=SysAdmin
        public static int Sucursal { get; set; }
        public static int Status { get; set; } = 1;

        /// <summary>
        /// Limpia la sesión actual
        /// </summary>
        public static void Clear()
        {
            LogonName = null;
            EmpId = null;
            TypeAut = 1;
            Sucursal = 0;
            Status = 1;
        }

    }

}



