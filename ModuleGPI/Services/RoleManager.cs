using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ModuleGPI.Domain;

namespace ModuleGPI.Services
{
    public class RoleManager : IRoleManager
    {
        private static readonly string[] ROLE_NAMES = { "", "Viewer", "Operator", "Supervisor", "AdminDept", "SysAdmin" };

        public string GetRoleName(int typeAut)
        {
            if (typeAut < 1 || typeAut > 5) return "Unknown";
            return ROLE_NAMES[typeAut];
        }

        public void ApplyVisibility(TabControl tabMain, TabPage tabAdmin, TabPage tabConfig, int typeAut)
        {
            bool isAdmin = typeAut >= 4;
            bool isSysAdmin = typeAut >= 5;

            if (tabMain.TabPages.Contains(tabConfig) && !isAdmin)
            {
                tabMain.TabPages.Remove(tabConfig);
            }
            if (tabMain.TabPages.Contains(tabAdmin) && !isSysAdmin)
            {
                tabMain.TabPages.Remove(tabAdmin);
            }
            // Visibilidad adicional si necesario
        }

        public bool CanSeeModule(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store)
        {
            if (module == null) return false;
            bool baseVisible = userRole >= module.RolesMinTypeAut;
            var ov = store.Items.FirstOrDefault(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));
            if (ov != null)
            {
                return ov.Override == 1 ? true : (ov.Override == -1 ? false : baseVisible);
            }
            return baseVisible;
        }
    }
}