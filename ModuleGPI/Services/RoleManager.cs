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

            var tabDashboard = tabMain.TabPages.Cast<TabPage>()
                .FirstOrDefault(t => t.Name == "tabDashboard");
            var tabModulos = tabMain.TabPages.Cast<TabPage>()
                .FirstOrDefault(t => t.Name == "tabModulos");
            // ✅ NUEVO: Buscar tab de módulos TEST
            var tabModulosTest = tabMain.TabPages.Cast<TabPage>()
                .FirstOrDefault(t => t.Name == "tabModulosTest");

            // Dashboard siempre visible
            if (tabDashboard != null)
                tabDashboard.Visible = true;

            // Módulos PRD visible para todos (Viewer y superiores)
            if (tabModulos != null)
                tabModulos.Visible = typeAut >= 1;

            // ✅ NUEVO: Módulos TEST visible SOLO para SysAdmin
            if (tabModulosTest != null)
                tabModulosTest.Visible = typeAut >= 5;

            // Admin y Config solo para AdminDept (4) y superiores
            if (typeAut < 4)
            {
                if (tabAdmin != null && tabMain.TabPages.Contains(tabAdmin))
                    tabMain.TabPages.Remove(tabAdmin);
                if (tabConfig != null && tabMain.TabPages.Contains(tabConfig))
                    tabMain.TabPages.Remove(tabConfig);
            }
            else
            {
                if (tabAdmin != null && !tabMain.TabPages.Contains(tabAdmin))
                    tabMain.TabPages.Add(tabAdmin);
                if (tabConfig != null && !tabMain.TabPages.Contains(tabConfig))
                    tabMain.TabPages.Add(tabConfig);
            }
        }

        public bool CanSeeModule(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store)
        {
            if (module == null || string.IsNullOrWhiteSpace(buttonName))
                return false;

            // PASO 1: Verificar acceso a planta
            bool hasPlantAccess = Session.HasAccessToPlant(module.Plant);
            if (!hasPlantAccess)
                return false;

            // ✅ NUEVO: PASO 2: Si es módulo TEST, validar acceso especial
            if (module.IsTest)
            {
                // Solo SysAdmin o usuarios con override=1 pueden ver TEST
                if (userRole < 5)
                {
                    var testOverride = store?.Items?.FirstOrDefault(x =>
                        string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

                    if (testOverride?.Override == 1)
                        return true;

                    if (testOverride?.Override == -1)
                        return false;

                    return false;  // Sin override no puede ver TEST
                }
            }

            // PASO 3: Verificar override
            var userOverride = store?.Items?.FirstOrDefault(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

            if (userOverride != null)
            {
                if (userOverride.Override == -1)
                    return false;

                if (userOverride.Override == 1)
                    return true;
            }

            // PASO 4: Verificar rol
            return userRole >= module.RolesMinTypeAut;
        }

        public string DiagnoseModuleAccess(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine("   DIAGNÓSTICO DE ACCESO A MÓDULO");
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine($"Módulo: {module?.Name}");
            sb.AppendLine($"Botón: {buttonName}");
            sb.AppendLine($"Usuario: {empId} ({Session.LogonName})");
            sb.AppendLine($"Rol: {userRole} - {GetRoleName(userRole)}");
            sb.AppendLine();

            if (module == null)
            {
                sb.AppendLine("❌ ERROR: Módulo no existe");
                return sb.ToString();
            }

            if (module.IsTest)
            {
                sb.AppendLine("⚠️ MÓDULO DE PRUEBA (TEST)");
                sb.AppendLine("Solo accesible por SysAdmin o con override=1");
                sb.AppendLine();
            }

            // PASO 1: PLANTA
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine("PASO 1: VERIFICAR ACCESO A PLANTA");
            sb.AppendLine("─────────────────────────────────────────");

            bool hasPlantAccess = Session.HasAccessToPlant(module.Plant);
            sb.AppendLine($"Planta del módulo: {module.Plant} ({Session.GetPlantName(module.Plant)})");
            sb.AppendLine($"Usuario {(hasPlantAccess ? "✅ TIENE" : "❌ NO TIENE")} acceso");

            if (!hasPlantAccess)
            {
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════");
                sb.AppendLine("RESULTADO: ❌ ACCESO DENEGADO (Sin acceso a planta)");
                sb.AppendLine("═══════════════════════════════════════════");
                return sb.ToString();
            }

            // ✅ PASO 2: Si es TEST
            if (module.IsTest)
            {
                sb.AppendLine();
                sb.AppendLine("─────────────────────────────────────────");
                sb.AppendLine("PASO 2: VERIFICAR ACCESO A MÓDULO TEST");
                sb.AppendLine("─────────────────────────────────────────");

                if (userRole >= 5)
                {
                    sb.AppendLine($"✅ Usuario es SysAdmin (TypeAut={userRole})");
                }
                else
                {
                    var testOverride = store?.Items?.FirstOrDefault(x =>
                        string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

                    if (testOverride?.Override == 1)
                    {
                        sb.AppendLine("✅ Override PERMITIR encontrado");
                        sb.AppendLine();
                        sb.AppendLine("═══════════════════════════════════════════");
                        sb.AppendLine("RESULTADO: ✅ ACCESO CONCEDIDO (Override en TEST)");
                        sb.AppendLine("═══════════════════════════════════════════");
                        return sb.ToString();
                    }
                    else
                    {
                        sb.AppendLine($"❌ Usuario NO es SysAdmin y sin override=1");
                        sb.AppendLine();
                        sb.AppendLine("═══════════════════════════════════════════");
                        sb.AppendLine("RESULTADO: ❌ ACCESO DENEGADO (No puede ver TEST)");
                        sb.AppendLine("═══════════════════════════════════════════");
                        return sb.ToString();
                    }
                }
            }

            // PASO 3: Override
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine("PASO " + (module.IsTest ? "3" : "2") + ": VERIFICAR OVERRIDE");
            sb.AppendLine("─────────────────────────────────────────");

            var userOverride = store?.Items?.FirstOrDefault(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

            if (userOverride?.Override == -1)
            {
                sb.AppendLine("❌ Override DENEGAR");
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════");
                sb.AppendLine("RESULTADO: ❌ ACCESO DENEGADO (Override)");
                sb.AppendLine("═══════════════════════════════════════════");
                return sb.ToString();
            }
            else if (userOverride?.Override == 1)
            {
                sb.AppendLine("✅ Override PERMITIR");
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════");
                sb.AppendLine("RESULTADO: ✅ ACCESO CONCEDIDO (Override)");
                sb.AppendLine("═══════════════════════════════════════════");
                return sb.ToString();
            }

            // PASO 4: Rol
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine("PASO " + (module.IsTest ? "4" : "3") + ": VERIFICAR ROL");
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine($"Rol mínimo: {module.RolesMinTypeAut} ({GetRoleName(module.RolesMinTypeAut)})");
            sb.AppendLine($"Rol usuario: {userRole} ({GetRoleName(userRole)})");

            bool hasRole = userRole >= module.RolesMinTypeAut;
            sb.AppendLine($"{(hasRole ? "✅" : "❌")} Usuario {(hasRole ? "tiene" : "NO tiene")} rol suficiente");
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════");
            sb.AppendLine($"RESULTADO: {(hasRole ? "✅ ACCESO CONCEDIDO" : "❌ ACCESO DENEGADO")} (Rol)");
            sb.AppendLine("═══════════════════════════════════════════");

            return sb.ToString();
        }
    }
}
