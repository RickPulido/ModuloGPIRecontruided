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
            // ========================================
            // VALIDACIÓN INICIAL
            // ========================================
            if (module == null || string.IsNullOrWhiteSpace(buttonName))
                return false;

            // ========================================
            // PASO 1: Verificar ACCESO A PLANTA (prioritario)
            // ========================================
            // Si el usuario NO tiene acceso a la planta del módulo, 
            // NO puede verlo bajo ninguna circunstancia (ni con override)
            bool hasPlantAccess = Session.HasAccessToPlant(module.Plant);

            if (!hasPlantAccess)
            {
                // Sin acceso a la planta = sin acceso al módulo
                // Esto bloquea incluso si tiene override = 1
                return false;
            }

            // ========================================
            // PASO 2: Verificar OVERRIDE
            // ========================================
            var userOverride = store?.Items?.FirstOrDefault(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

            if (userOverride != null)
            {
                // Override = -1: DENEGAR (bloquea incluso con acceso a planta)
                if (userOverride.Override == -1)
                    return false;

                // Override = 1: PERMITIR (ya validamos planta arriba, ahora ignoramos rol)
                if (userOverride.Override == 1)
                    return true;

                // Override = 0: HEREDADO (continúa con validación de rol)
            }

            // ========================================
            // PASO 3: Verificar ROL
            // ========================================
            bool hasRequiredRole = userRole >= module.RolesMinTypeAut;

            // Resultado final: Tiene planta + (override=1 O rol suficiente)
            return hasRequiredRole;
        }


        // Método de diagnóstico para verificar paso a paso por qué un usuario puede/no puede ver un módulo
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

            // ========================================
            // PASO 1: PLANTA
            // ========================================
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine("PASO 1: VERIFICAR ACCESO A PLANTA");
            sb.AppendLine("─────────────────────────────────────────");

            string modulePlantName = Session.GetPlantName(module.Plant);
            sb.AppendLine($"Planta del módulo: {module.Plant} ({modulePlantName})");
            sb.AppendLine();

            sb.AppendLine($"Planta principal del usuario: {Session.Sucursal} ({Session.GetPlantName(Session.Sucursal)})");
            sb.AppendLine($"  • MTY_Access: {(Session.MTY_Access ? "✅" : "❌")}");
            sb.AppendLine($"  • QRO_Access: {(Session.QRO_Access ? "✅" : "❌")}");
            sb.AppendLine($"  • TIJ_Access: {(Session.TIJ_Access ? "✅" : "❌")}");
            sb.AppendLine();

            bool hasPlantAccess = Session.HasAccessToPlant(module.Plant);

            if (hasPlantAccess)
            {
                sb.AppendLine($"✅ Usuario TIENE acceso a planta {modulePlantName}");

                if (module.Plant == Session.Sucursal)
                {
                    sb.AppendLine("   (Es su planta principal)");
                }
                else if (module.Plant == 0)
                {
                    sb.AppendLine("   (Módulo disponible para todas las plantas)");
                }
                else
                {
                    sb.AppendLine("   (Tiene acceso multi-planta)");
                }
            }
            else
            {
                sb.AppendLine($"❌ Usuario NO tiene acceso a planta {modulePlantName}");
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════");
                sb.AppendLine("RESULTADO FINAL: ❌ ACCESO DENEGADO");
                sb.AppendLine("RAZÓN: Sin acceso a la planta del módulo");
                sb.AppendLine("═══════════════════════════════════════════");
                return sb.ToString();
            }

           
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine("PASO 2: VERIFICAR OVERRIDE PERSONALIZADO");
            sb.AppendLine("─────────────────────────────────────────");

            var userOverride = store?.Items?.FirstOrDefault(x =>
                string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.EmpId, empId, StringComparison.OrdinalIgnoreCase));

            if (userOverride != null)
            {
                string overrideText = userOverride.Override == 1 ? "✅ PERMITIR" :
                                     userOverride.Override == -1 ? "❌ DENEGAR" : "⚪ HEREDADO";

                sb.AppendLine($"Override encontrado: {overrideText}");

                if (userOverride.Override == -1)
                {
                    sb.AppendLine();
                    sb.AppendLine("═══════════════════════════════════════════");
                    sb.AppendLine("RESULTADO FINAL: ❌ ACCESO DENEGADO");
                    sb.AppendLine("RAZÓN: Override configurado como DENEGAR");
                    sb.AppendLine("═══════════════════════════════════════════");
                    return sb.ToString();
                }
                else if (userOverride.Override == 1)
                {
                    sb.AppendLine("Override = PERMITIR (se omite validación de rol)");
                    sb.AppendLine();
                    sb.AppendLine("═══════════════════════════════════════════");
                    sb.AppendLine("RESULTADO FINAL: ✅ ACCESO CONCEDIDO");
                    sb.AppendLine("RAZÓN: Override PERMITIR + Acceso a planta");
                    sb.AppendLine("═══════════════════════════════════════════");
                    return sb.ToString();
                }
                else
                {
                    sb.AppendLine("Override = HEREDADO (continúa validación normal)");
                }
            }
            else
            {
                sb.AppendLine("No existe override para este usuario/módulo");
            }

            // ========================================
            // PASO 3: ROL
            // ========================================
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine("PASO 3: VERIFICAR ROL MÍNIMO REQUERIDO");
            sb.AppendLine("─────────────────────────────────────────");

            sb.AppendLine($"Rol mínimo del módulo: {module.RolesMinTypeAut} ({GetRoleName(module.RolesMinTypeAut)})");
            sb.AppendLine($"Rol del usuario: {userRole} ({GetRoleName(userRole)})");

            bool hasRequiredRole = userRole >= module.RolesMinTypeAut;

            if (hasRequiredRole)
            {
                sb.AppendLine($"✅ Usuario tiene rol suficiente ({userRole} >= {module.RolesMinTypeAut})");
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════");
                sb.AppendLine("RESULTADO FINAL: ✅ ACCESO CONCEDIDO");
                sb.AppendLine("RAZÓN: Rol suficiente + Acceso a planta");
                sb.AppendLine("═══════════════════════════════════════════");
            }
            else
            {
                sb.AppendLine($"❌ Usuario NO tiene rol suficiente ({userRole} < {module.RolesMinTypeAut})");
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════");
                sb.AppendLine("RESULTADO FINAL: ❌ ACCESO DENEGADO");
                sb.AppendLine("RAZÓN: Rol insuficiente");
                sb.AppendLine("═══════════════════════════════════════════");
            }

            return sb.ToString();
        }
    }
}