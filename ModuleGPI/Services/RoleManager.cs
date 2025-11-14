using System;
using System.Linq;
using System.Windows.Forms;
using ModuleGPI.Domain;

namespace ModuleGPI.Services
{
    public sealed class RoleManager : IRoleManager
    {
        private static readonly string[] ROLE_NAMES =
        {
            "", "Viewer", "Operator", "Supervisor", "AdminDept", "SysAdmin"
        };

        public string GetRoleName(int typeAut)
        {
            if (typeAut < 0 || typeAut >= ROLE_NAMES.Length)
                return "Unknown";
            return ROLE_NAMES[typeAut];
        }

        public void ApplyVisibility(TabControl tabMain, TabPage tabAdmin, TabPage tabConfig, int typeAut)
        {
            // Tabs básicos siempre visibles según rol mínimo
            var tabDashboard = tabMain.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Name == "tabDashboard");
            var tabOperacion = tabMain.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Name == "tabOperacion");
            var tabConsultas = tabMain.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Name == "tabConsultas");

            if (tabDashboard != null) tabDashboard.Visible = true;
            if (tabConsultas != null) tabConsultas.Visible = typeAut >= 1;
            if (tabOperacion != null) tabOperacion.Visible = typeAut >= 2;

            // Admin y Config solo para roles >= 4
            if (typeAut < 4)
            {
                if (tabMain.TabPages.Contains(tabAdmin))
                    tabMain.TabPages.Remove(tabAdmin);
                if (tabMain.TabPages.Contains(tabConfig))
                    tabMain.TabPages.Remove(tabConfig);
            }
            else
            {
                if (!tabMain.TabPages.Contains(tabAdmin))
                    tabMain.TabPages.Add(tabAdmin);
                if (!tabMain.TabPages.Contains(tabConfig))
                    tabMain.TabPages.Add(tabConfig);
            }
        }

        public bool CanSeeModule(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store)
        {
            if (module == null || string.IsNullOrWhiteSpace(buttonName))
                return false;

            // Visibilidad base por rol
            bool baseVisible = userRole >= module.RolesMinTypeAut;

            // Buscar override específico del usuario
            var userOverride = store?.Items?.FirstOrDefault(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

            if (userOverride != null)
            {
                // Override = 1: fuerza visible
                // Override = -1: fuerza oculto
                // Override = 0: usa visibilidad base
                return userOverride.Override == 1 ? true :
                       userOverride.Override == -1 ? false :
                       baseVisible;
            }

            return baseVisible;
        }
    }
}