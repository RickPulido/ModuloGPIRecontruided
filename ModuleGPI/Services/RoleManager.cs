using ModuleGPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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

        public void ApplyVisibility(
     TabControl tabMain,
     TabPage tabAdmin,
     TabPage tabConfig,
     int typeAut,
     string empId,
     OverridesStore store,
     IEnumerable<ModuleDef> modules)
        {
            if (tabMain == null) return;

            // --- Referencias base por nombre (si existen en TabPages hoy) ---
            var tabDashboard = tabMain.TabPages.Cast<TabPage>()
                .FirstOrDefault(t => t.Name == "tabDashboard");

            var tabModulos = tabMain.TabPages.Cast<TabPage>()
                .FirstOrDefault(t => t.Name == "tabModulos");

            // OJO: tabModulosTest puede estar removida; si no está, intenta tomarla del diseñador:
            // Si tú tienes el field tabModulosTest en MainForm, pásalo como parámetro sería ideal,
            // pero aquí lo localizamos si existe. Si no existe, NO truena.
            var tabModulosTest = tabMain.TabPages.Cast<TabPage>()
                .FirstOrDefault(t => t.Name == "tabModulosTest");

            // --- Reglas ---
            bool allowAdminConfig = typeAut >= 4; // Ajusta si quieres que sea >= 5
            bool allowTestTab = HasAccessToAnyTestModule(typeAut, empId, store, modules);

            // --- Dashboard: tú querías eliminarlo -> fuera siempre ---
            EnsureTab(tabMain, tabDashboard, shouldBePresent: false, desiredIndex: 0);

            // --- PRD: siempre presente (o >=1 si quieres) ---
            EnsureTab(tabMain, tabModulos, shouldBePresent: (typeAut >= 1), desiredIndex: 0);

            // --- TEST: solo si SysAdmin o override=1 en algún módulo TEST consumible ---
            // (Si tu tabModulosTest NO está en TabPages porque la removiste antes, este locator no la encontrará.
            // Para que esto sea 100% robusto, lo ideal es PASAR tabModulosTest como parámetro, igual que Admin/Config.)
            // Si hoy sí está declarada como field en el diseñador, la forma más limpia es ampliar la firma.
            EnsureTab(tabMain, tabModulosTest, shouldBePresent: allowTestTab, desiredIndex: 1);

            // --- Admin/Config: solo si >=4 (o >=5 si cambias regla) ---
            EnsureTab(tabMain, tabAdmin, shouldBePresent: allowAdminConfig, desiredIndex: 2);
            EnsureTab(tabMain, tabConfig, shouldBePresent: allowAdminConfig, desiredIndex: 3);
        }

        private static void EnsureTab(TabControl tabMain, TabPage tab, bool shouldBePresent, int desiredIndex)
        {
            if (tabMain == null || tab == null) return;

            bool isPresent = tabMain.TabPages.Contains(tab);

            if (shouldBePresent)
            {
                if (!isPresent)
                {
                    int idx = Math.Max(0, Math.Min(desiredIndex, tabMain.TabPages.Count));
                    tabMain.TabPages.Insert(idx, tab);
                }
            }
            else
            {
                if (isPresent)
                    tabMain.TabPages.Remove(tab);
            }
        }



        public bool HasAccessToAnyTestModule(int userRole, string empId, OverridesStore store, IEnumerable<ModuleDef> modules)
        {
            // SysAdmin siempre ve la pestaña TEST
            if (userRole >= 5) return true;

            if (store?.Items == null || modules == null) return false;

            // Identidades posibles del usuario (por si en DB guardaste distinto)
            var ids = new[] { empId, Session.LogonName }
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ids.Count == 0) return false;

            // Módulos TEST existentes
            var testModules = modules.Where(m => m != null && m.IsTest && !string.IsNullOrWhiteSpace(m.ButtonName)).ToList();
            if (testModules.Count == 0) return false;

            // Para cada módulo TEST, revisar override=1 para el usuario
            foreach (var m in testModules)
            {
                // ✅ Si quieres que el tab solo aparezca si el módulo es "consumible" por planta:
                if (!Session.HasAccessToPlant(m.Plant))
                    continue;

                var ov = store.Items.FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(x.ButtonName) &&
                    string.Equals(x.ButtonName, m.ButtonName, StringComparison.OrdinalIgnoreCase) &&
                    ids.Any(id => string.Equals(x.EmpId, id, StringComparison.OrdinalIgnoreCase)));

                if (ov?.Override == 1) return true;
            }

            return false;
        }


        public bool CanSeeModule(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store)
        {
            if (module == null || string.IsNullOrWhiteSpace(buttonName))
                return false;

            var ids = new[] { empId, Session.LogonName }
    .Where(s => !string.IsNullOrWhiteSpace(s))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToList();



            // PASO 1: Verificar acceso a planta
            bool hasPlantAccess = Session.HasAccessToPlant(module.Plant);
            if (!hasPlantAccess)
                return false;

            // ✅ PASO 2: Si es módulo TEST, validar acceso especial
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
                // SysAdmin (userRole >= 5) continúa a verificación de rol normal
            }

            // PASO 3: Verificar override para módulos NO-TEST
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

            // PASO 1:  PLANTA
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
                sb.AppendLine("RESULTADO:  ✅ ACCESO CONCEDIDO (Override)");
                sb.AppendLine("═══════════════════════════════════════════");
                return sb.ToString();
            }

            // PASO 4: Rol
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine("PASO " + (module.IsTest ? "4" : "3") + ": VERIFICAR ROL");
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine($"Rol mínimo:  {module.RolesMinTypeAut} ({GetRoleName(module.RolesMinTypeAut)})");
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