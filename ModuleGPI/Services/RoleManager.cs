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
            // Manejo idéntico al código legacy
            if (typeAut <= 1) return "Viewer";
            if (typeAut == 2) return "Operator";
            if (typeAut == 3) return "Supervisor";
            if (typeAut == 4) return "AdminDept";
            if (typeAut >= 5) return "SysAdmin";

            return "Unknown";
        }

        public void ApplyVisibility(TabControl tabMain, TabPage tabAdmin, TabPage tabConfig, int typeAut)
        {
            if (tabMain == null) return;

            // Buscar tabs por nombre (como en el código legacy)
            var tabDashboard = tabMain.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Name == "tabDashboard");
            var tabOperacion = tabMain.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Name == "tabOperacion");
            var tabConsultas = tabMain.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Name == "tabConsultas");

            // Dashboard siempre visible
            if (tabDashboard != null)
                tabDashboard.Visible = true;

            // Consultas visible para Viewer (1) y superiores
            if (tabConsultas != null)
                tabConsultas.Visible = typeAut >= 1;

            // Operación visible para Operator (2) y superiores
            if (tabOperacion != null)
                tabOperacion.Visible = typeAut >= 2;

            // Admin y Config solo para AdminDept (4) y superiores
            if (typeAut < 4)
            {
                // Remover tabs de admin si existen
                if (tabAdmin != null && tabMain.TabPages.Contains(tabAdmin))
                    tabMain.TabPages.Remove(tabAdmin);
                if (tabConfig != null && tabMain.TabPages.Contains(tabConfig))
                    tabMain.TabPages.Remove(tabConfig);
            }
            else
            {
                // Agregar tabs de admin si no existen
                if (tabAdmin != null && !tabMain.TabPages.Contains(tabAdmin))
                    tabMain.TabPages.Add(tabAdmin);
                if (tabConfig != null && !tabMain.TabPages.Contains(tabConfig))
                    tabMain.TabPages.Add(tabConfig);
            }
        }

        public bool CanSeeModule(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store)
        {
            // Validación inicial
            if (module == null || string.IsNullOrWhiteSpace(buttonName))
                return false;

            // Visibilidad base según rol mínimo requerido del módulo
            bool baseVisible = userRole >= module.RolesMinTypeAut;

            // Si no hay store de overrides, usar solo visibilidad base
            if (store == null)
                return baseVisible;

            // Buscar override específico del usuario
            var userOverride = store.Items?.FirstOrDefault(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

            if (userOverride != null)
            {
                // Aplicar lógica de override exacta del código legacy:
                // Override = 1: fuerza visible (Permitir)
                // Override = -1: fuerza oculto (Denegar)
                // Override = 0: usa visibilidad base (Heredado)

                if (userOverride.Override == 1)
                    return true;  // Permitir explícitamente

                if (userOverride.Override == -1)
                    return false; // Denegar explícitamente

                // Si es 0 (heredado), usar visibilidad base
                return baseVisible;
            }

            // No hay override, usar visibilidad base por rol
            return baseVisible;
        }
    }
}