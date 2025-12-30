using ModuleGPI.Data;
using ModuleGPI.Domain;
using ModuleGPI.Controls;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ModuleGPI.Services
{
    public sealed class ModuleService : IModuleService
    {
        private readonly IDataAccess _db;
        private DateTime _lastLaunch = DateTime.MinValue;

        public ModuleService(IDataAccess db) => _db = db;

        public DataTable LoadModules(int? plant) => _db.GetModules(plant);

        public void PaintButtons(
    DataTable dt,
    FlowLayoutPanel panel,      // ✅ Solo un panel
    FlowLayoutPanel _,          // ✅ Segundo panel ignorado (null)
    ContextMenuStrip cm,
    ToolTip tips,
    Func<string, ModuleDef, bool> canSee)
        {
            if (panel == null) return;

            ClearButtons(panel);

            foreach (DataRow row in dt.Rows)
            {
                string btnName = Convert.ToString(row["ButtonName"]);

                if (panel.Controls.OfType<Control>().Any(c => c.Name == btnName))
                    continue;

                var def = new ModuleDef
                {
                    ButtonName = Convert.ToString(row["ButtonName"]), 
                    Name = Convert.ToString(row["Name"]),
                    ExePath = Convert.ToString(row["ExePath"]),
                    WorkingDir = Convert.ToString(row["WorkingDir"]),
                    Arguments = row.Table.Columns.Contains("Arguments") ? Convert.ToString(row["Arguments"]) : "",
                    IconPath = row.Table.Columns.Contains("IconPath") ? Convert.ToString(row["IconPath"]) : "",
                    Category = Convert.ToString(row["Category"]),
                    RequiresElevation = row["RequiresElevation"] != DBNull.Value && Convert.ToBoolean(row["RequiresElevation"]),
                    RolesMinTypeAut = row["RolesMinTypeAut"] == DBNull.Value ? 1 : Convert.ToInt32(row["RolesMinTypeAut"]),
                    Plant = row["Plant"] == DBNull.Value ? 1 : Convert.ToInt32(row["Plant"]),
                    IsTest = row.Table.Columns.Contains("IsTest") && row["IsTest"] != DBNull.Value && Convert.ToBoolean(row["IsTest"]) 
                };


                var moduleBtn = new ModuleButton
                {
                    Name = btnName,
                    ButtonText = def.Name,
                    IconPath = string.IsNullOrEmpty(def.IconPath) ? def.ExePath : def.IconPath,
                    Plant = def.Plant,
                    Size = new System.Drawing.Size(120, 140),
                    Margin = new Padding(10),
                    Tag = def,
                    ContextMenuStrip = cm
                };

                bool v = canSee(btnName, def);
                moduleBtn.Visible = v;
                moduleBtn.Enabled = v;

                if (tips != null)
                {
                    tips.SetToolTip(moduleBtn, def.Name + Environment.NewLine + (def.ExePath ?? ""));
                }

                
                panel.Controls.Add(moduleBtn);
            }
        }

        public void RefreshVisibility(
     FlowLayoutPanel panel,     
     FlowLayoutPanel _,         
     Func<string, ModuleDef, bool> canSee)
        {
            if (panel == null) return;

            foreach (var control in panel.Controls.OfType<Control>())
            {
                if (control.Tag is ModuleDef m)
                {
                    bool v = canSee(control.Name, m);
                    control.Visible = v;
                    control.Enabled = v;
                }
            }
        }

        public void LaunchModule(string buttonName, ModuleDef m, bool asAdmin, string[] allowedRoots, Action<string> setStatus)
        {
            if (DateTime.Now - _lastLaunch < TimeSpan.FromSeconds(1.5)) return;
            _lastLaunch = DateTime.Now;

            if (m == null || string.IsNullOrWhiteSpace(m.ExePath) || !File.Exists(m.ExePath))
            {
                MessageBox.Show("No se encontró el ejecutable.");
                return;
            }

            // ✅ Verificar si ya está ejecutándose
            if (ProcessTracker.IsModuleRunning(buttonName))
            {
                MessageBox.Show($"El módulo '{m.Name}' ya está ejecutándose.\n\nNo se pueden abrir múltiples instancias.",
                    "Módulo en Ejecución",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            if (!IsPathAllowed(m.ExePath, allowedRoots))
            {
                MessageBox.Show("Ruta no autorizada por la política.", "Bloqueado");
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = m.ExePath,
                    Arguments = m.Arguments ?? "",
                    WorkingDirectory = string.IsNullOrEmpty(m.WorkingDir) ? Path.GetDirectoryName(m.ExePath) : m.WorkingDir,
                    UseShellExecute = true,
                    Verb = (asAdmin || m.RequiresElevation) ? "runas" : ""
                };

                var p = Process.Start(psi);

                if (p != null)
                {
                    ProcessTracker.RegisterProcess(buttonName, p.Id);
                    setStatus?.Invoke((psi.Verb == "runas") ? $"Lanzado: {m.Name} (Admin)" : $"Lanzado: {m.Name}");
                }
            }
            catch (Win32Exception w32)
            {
                if (w32.NativeErrorCode == 1223) setStatus?.Invoke("Elevación UAC cancelada.");
                else MessageBox.Show("Error Win32: " + w32.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir módulo: " + ex.Message);
            }
        }

        public void WireButtons(
    FlowLayoutPanel panel,     // ✅ Solo un panel
    FlowLayoutPanel _,         // ✅ Segundo panel ignorado
    EventHandler clickHandler)
        {
            if (panel == null) return;

            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl.Tag is ModuleDef)
                {
                    ctrl.Click += clickHandler;
                }
            }
        }

        private static void ClearButtons(FlowLayoutPanel flp)
        {
            Control[] controls = new Control[flp.Controls.Count];
            flp.Controls.CopyTo(controls, 0);
            foreach (Control c in controls)
            {
                c.Dispose();
            }
            flp.Controls.Clear();
        }

        public static bool IsPathAllowed(string exePath, string[] roots)
        {
            try
            {
                string full = Path.GetFullPath(Environment.ExpandEnvironmentVariables(exePath));
                foreach (var root in roots)
                {
                    var norm = root.EndsWith("\\") ? root : root + "\\";
                    if (full.StartsWith(norm, StringComparison.OrdinalIgnoreCase)) return true;
                }
                return false;
            }
            catch { return false; }
        }
    }
}