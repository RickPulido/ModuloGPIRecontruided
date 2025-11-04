using System;
using ModuleGPI;
using System.ComponentModel;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GPI.Launcher
{
    public partial class MainForm : Form
    {
        private DataTable _dtModulesAdmin;

        private readonly ToolTip toolTips = new ToolTip();
        private DataTable _dtUsers;
        private SqlDataAdapter _daUsers;
        private bool _adminCanEdit; // SysAdmin = true

        private OverridesStore _overrides = new OverridesStore();
        private DataTable _dtOverridesView; // vista para el grid por módulo seleccionado
        private DataGridView dgvOverrides;  // NUEVO



        private static readonly string[] ALLOWED_ROOTS = new string[]
        {
    @"\\USAZR3QITVFE001\Intuitive MTY\",
    @"\\USAZR3PITVFE001\Intuitive MTY\",
    @"\\srv\apps\",
    @"C:\Program Files\CorpApps\"
        };

        public MainForm()
        {
            InitializeComponent();
            WireStaticHandlers();
            this.tabMain.Selected += (s, e) =>
            {
                if (e.TabPage == this.tabAdmin && ModuleGPI.Session.TypeAut >= 4)
                {
                    LoadAdminData();
                    LoadModulesAdminFromUI();
                }
            };

            // Crear dgvOverrides si no está en el diseñador
            if (this.dgvOverrides == null)
            {
                this.dgvOverrides = new DataGridView();
                this.dgvOverrides.Name = "dgvOverrides";
                this.dgvOverrides.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                this.dgvOverrides.AllowUserToAddRows = false;
                this.dgvOverrides.AllowUserToDeleteRows = false;
                this.dgvOverrides.ReadOnly = true; // se habilita edición solo si SysAdmin en BuildOverridesViewFor
                this.dgvOverrides.RowHeadersVisible = false;

                // Usar SplitContainer para dgvModulos (arriba) y dgvOverrides (abajo)
                var host = this.dgvModulos?.Parent ?? this.tabAdmin; // fallback
                host.Controls.Remove(this.dgvModulos); // Remover dgvModulos del host si existe

                var splitContainer = new SplitContainer();
                splitContainer.Dock = DockStyle.Fill;
                splitContainer.Orientation = Orientation.Horizontal;
                splitContainer.Panel1.Controls.Add(this.dgvModulos);
                this.dgvModulos.Dock = DockStyle.Fill;
                splitContainer.Panel2.Controls.Add(this.dgvOverrides);
                this.dgvOverrides.Dock = DockStyle.Fill;
                splitContainer.SplitterDistance = host.ClientSize.Height / 2; // Mitad aproximada

                host.Controls.Add(splitContainer);
            }
            this.dgvOverrides.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvOverrides.IsCurrentCellDirty)
                    dgvOverrides.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            this.dgvOverrides.CellValueChanged += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                // módulo seleccionado
                if (dgvModulos.CurrentRow == null) return;

                var drv = dgvModulos.CurrentRow.DataBoundItem as DataRowView;
                if (drv == null) return;

                string buttonName = Convert.ToString(drv["ButtonName"]);
                // vuelca sólo la fila editada
                ApplyOneOverrideFromRow(buttonName, e.RowIndex);

                // persistir y refrescar al vuelo
                SaveOverridesToFile();
                RefreshModules();
            };

            this.dgvOverrides.DataError += (s, e) =>
            {
                // Silencia DataErrors del Combo durante resize/formateo
                e.ThrowException = false;
            };

            // ==== Zona derecha: contenedor padre donde están los módulos ====
            Control rightHost = this.dgvModulos?.Parent ?? this.tabAdmin;

            // Quita labels duplicados si ya existieran por pruebas previas
            foreach (var c in rightHost.Controls.Find("lblOverridesHint", true))
                rightHost.Controls.Remove(c);

            // Crea el label una sola vez
            var lblOverridesHint = new Label
            {
                Name = "lblOverridesHint",
                Text = "Permisos por usuario (Overrides):  -1=Denegar | 0=Heredado | 1=Permitir",
                Dock = DockStyle.Bottom,
                Height = 18
            };
            rightHost.Controls.Add(lblOverridesHint);
            lblOverridesHint.BringToFront();

            // Colorear filas por estado (sin tocar e.Value)
            this.dgvOverrides.CellFormatting -= DgvOverrides_CellFormatting;
            this.dgvOverrides.CellFormatting += DgvOverrides_CellFormatting;

            // Commit inmediato al cambiar el combo
            this.dgvOverrides.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvOverrides.IsCurrentCellDirty)
                    dgvOverrides.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

        }
        private static readonly string[] ROLE_NAMES =
{
    "", "Viewer", "Operator", "Supervisor", "AdminDept", "SysAdmin"
};


        private void DgvOverrides_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var drv = dgvOverrides.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (drv == null) return;

            int ov = 0;
            object raw = drv.Row["Override"];
            if (raw != DBNull.Value) int.TryParse(raw.ToString(), out ov);

            var row = dgvOverrides.Rows[e.RowIndex];
            if (ov == 1)
                row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(220, 255, 220); // verde claro
            else if (ov == -1)
                row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 230, 230); // rojo claro
            else
                row.DefaultCellStyle.BackColor = System.Drawing.Color.White; // heredado
        }

        private void DgvModulos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var col = dgvModulos.Columns[e.ColumnIndex].DataPropertyName;

            if (col == "ExePath" || col == "WorkingDir")
            {
                dgvModulos.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = Convert.ToString(e.Value);
            }

            if (col == "RolesMinTypeAut")
            {
                int r;
                if (int.TryParse(Convert.ToString(e.Value), out r) && r >= 1 && r <= 5)
                {
                    dgvModulos.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = ROLE_NAMES[r];
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            PositionHeaderSearchBoxes();

            // Status
            this.tsslUser.Text = "Usuario: " + Session.LogonName;
            this.tsslRole.Text = "Rol: " + GetRoleName(Session.TypeAut);
            this.tsslPlant.Text = "Planta: " + Session.Sucursal;
            this.tsslEstado.Text = "Listo";

            // Visibilidad por rol
            ApplyRoleVisibility(Session.TypeAut);

            if (ModuleGPI.Session.TypeAut >= 4)
            {
                LoadAdminData();
                LoadModulesAdminFromUI();
            }


            // LoadDashboard();
            // Categorías
            LoadCategories();


            // Tag de módulos en botones
            AssignModuleTags();
            LoadOverridesFromFile();

            // Conectar botones y context menu
            WireModuleButtons();

            LoadModulesFromFileAndApply();

        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            this.txtOpSearch.Focus();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            PositionHeaderSearchBoxes();
        }

        private void PositionHeaderSearchBoxes()
        {
            // Operación
            if (this.pnlOpHeader.ClientSize.Width > 0)
            {
                int right = this.pnlOpHeader.ClientSize.Width - 8;
                this.btnOpRefrescar.Width = 90;
                this.btnOpRefrescar.Location = new System.Drawing.Point(right - this.btnOpRefrescar.Width, 10);

                this.txtOpSearch.Width = 220;
                this.txtOpSearch.Location = new System.Drawing.Point(this.btnOpRefrescar.Left - this.txtOpSearch.Width - 8, 12);
            }

            // Consultas
            if (this.pnlConsHeader.ClientSize.Width > 0)
            {
                int right2 = this.pnlConsHeader.ClientSize.Width - 8;
                this.txtConsSearch.Width = 220;
                this.txtConsSearch.Location = new System.Drawing.Point(right2 - this.txtConsSearch.Width, 12);
            }
        }

        private static bool IsPathAllowed(string exePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(exePath)) return false;
                string path = Environment.ExpandEnvironmentVariables(exePath);
                string full = GetFullPathSafe(path);
                if (string.IsNullOrEmpty(full)) return false;

                foreach (var root in ALLOWED_ROOTS)
                {
                    string normRoot = EnsureTrailingBackslash(root);
                    if (full.StartsWith(normRoot, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch { return false; }
        }

        private static string GetFullPathSafe(string path)
        {
            try { return Path.GetFullPath(path); } catch { return null; }
        }
        private static string EnsureTrailingBackslash(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (!s.EndsWith("\\") && !s.EndsWith("/")) return s + "\\";
            return s;
        }

        private DataTable BuildModulesDataTableFromUI()
        {
            var dt = new DataTable();
            dt.Columns.Add("ButtonName", typeof(string));   // <— NUEVO: id único
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Category", typeof(string));
            dt.Columns.Add("ExePath", typeof(string));
            dt.Columns.Add("WorkingDir", typeof(string));
            dt.Columns.Add("RequiresElevation", typeof(bool));
            dt.Columns.Add("RolesMinTypeAut", typeof(int));
            dt.Columns.Add("VisibleForCurrentRole", typeof(bool));

            Action<FlowLayoutPanel> dump = (panel) =>
            {
                foreach (Control c in panel.Controls)
                {
                    var btn = c as Button;
                    if (btn == null || btn.Tag == null) continue;
                    var m = btn.Tag as ModuleDef;
                    if (m == null) continue;

                    bool visibleForMe = (ModuleGPI.Session.TypeAut >= m.RolesMinTypeAut);

                    dt.Rows.Add(new object[] {
                btn.Name, // <— botón exacto
                m.Name,
                string.IsNullOrEmpty(m.Category) ? DetectCategoryFor(btn, panel) : m.Category,
                m.ExePath ?? "",
                m.WorkingDir ?? "",
                m.RequiresElevation,
                m.RolesMinTypeAut,
                visibleForMe
            });
                }
            };

            dump(this.flpOperacion);
            dump(this.flpConsultas);
            return dt;
        }


        private string DetectCategoryFor(Button btn, FlowLayoutPanel panel)
        {
            // Por si algún ModuleDef no trae Category, inferimos por el panel host
            if (panel == this.flpOperacion) return "Operación";
            if (panel == this.flpConsultas) return "Consultas";
            return "General";
        }


        private void LoadModulesAdminFromUI()
        {
            _dtModulesAdmin = BuildModulesDataTableFromUI();
            dgvModulos.DataSource = _dtModulesAdmin;
            ConfigureModulesGrid();

            dgvModulos.CellValidating -= DgvModulos_CellValidating;
            dgvModulos.CellValidating += DgvModulos_CellValidating;

            dgvModulos.SelectionChanged -= DgvModulos_SelectionChanged;
            dgvModulos.SelectionChanged += DgvModulos_SelectionChanged;

            // Auto-resize columns to fit content after binding
            dgvModulos.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);

            // Carga overrides del primer módulo seleccionado (si hay)
            DgvModulos_SelectionChanged(null, EventArgs.Empty);
        }


        private void DgvModulos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvModulos.CurrentRow == null) { dgvOverrides.DataSource = null; return; }

            var drv = dgvModulos.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) { dgvOverrides.DataSource = null; return; }

            string buttonName = Convert.ToString(drv["ButtonName"]); // lee del DataTable
            BuildOverridesViewFor(buttonName);
        }


        private void ApplyOneOverrideFromRow(string buttonName, int rowIndex)
        {
            if (_dtOverridesView == null || rowIndex < 0 || rowIndex >= _dtOverridesView.Rows.Count) return;
            var r = _dtOverridesView.Rows[rowIndex];

            string empId = Convert.ToString(r["EmpId"]);
            int ov = Convert.ToInt32(r["Override"]);

            // elimina override previo de ese usuario/módulo
            _overrides.Items.RemoveAll(x => x.ButtonName == buttonName && x.EmpId == empId);

            if (ov != 0) // sólo guardamos Permitir/Denegar; Heredado no se guarda
            {
                _overrides.Items.Add(new ModuleUserOverride
                {
                    ButtonName = buttonName,
                    EmpId = empId,
                    Override = ov
                });
            }
        }

        private void BuildOverridesViewFor(string buttonName)
        {
            // Asegura que haya usuarios cargados
            if (_dtUsers == null || _dtUsers.Rows.Count == 0)
            {
                try { if (Session.TypeAut >= 4) LoadAdminData(); } catch { }
                if (_dtUsers == null || _dtUsers.Rows.Count == 0)
                {
                    dgvOverrides.DataSource = null;
                    return;
                }
            }


            // Construir tabla de vista (EmpId, UserLog, Override)
            _dtOverridesView = new DataTable();
            _dtOverridesView.Columns.Add("EmpId", typeof(string));
            _dtOverridesView.Columns.Add("UserLog", typeof(string));
            _dtOverridesView.Columns.Add("Override", typeof(int)); // -1=Denegar, 0=Heredado, 1=Permitir

            foreach (DataRow u in _dtUsers.Rows)
            {
                string emp = Convert.ToString(u["USU_EmpID"]);
                string log = Convert.ToString(u["USU_UserLog"]);

                var ov = _overrides.Items.FirstOrDefault(x =>
                    string.Equals(x.ButtonName, buttonName, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.EmpId, emp, StringComparison.OrdinalIgnoreCase));

                int value = (ov == null) ? 0 : ov.Override; // 0 = Heredado por defecto
                _dtOverridesView.Rows.Add(emp, log, value);
            }

            // Configurar columnas del grid (una sola vez por carga)
            dgvOverrides.AutoGenerateColumns = false;
            dgvOverrides.Columns.Clear();

            var cEmp = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "EmpId",
                HeaderText = "EmpID",
                ReadOnly = true,
                FillWeight = 20
            };
            dgvOverrides.Columns.Add(cEmp);

            var cLog = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "UserLog",
                HeaderText = "UserLog",
                ReadOnly = true,
                FillWeight = 40
            };
            dgvOverrides.Columns.Add(cLog);

            var cOv = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "Override",
                HeaderText = "Override",
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Flat,
                FillWeight = 40
            };
            cOv.DisplayMember = "Text";
            cOv.ValueMember = "Value";
            cOv.DataSource = new[]
            {
        new { Value = -1, Text = "Denegar"  },
        new { Value =  0, Text = "Heredado" },
        new { Value =  1, Text = "Permitir" }
    };
            dgvOverrides.Columns.Add(cOv);

            // Solo edita si tienes permisos
            dgvOverrides.ReadOnly = !_adminCanEdit;
            dgvOverrides.AllowUserToAddRows = false;
            dgvOverrides.AllowUserToDeleteRows = false;

            // Asignar datos
            dgvOverrides.DataSource = _dtOverridesView;
            dgvOverrides.Visible = true;
            dgvOverrides.Refresh();
        }


        private void ApplyOverridesFromView(string currentButtonName)
        {
            if (_dtOverridesView == null) return;

            // Elimina overrides previos de ese módulo
            _overrides.Items.RemoveAll(x => x.ButtonName == currentButtonName);

            foreach (DataRow r in _dtOverridesView.Rows)
            {
                int ov = Convert.ToInt32(r["Override"]);
                if (ov == 0) continue; // Heredado: no guardamos para ahorrar

                _overrides.Items.Add(new ModuleUserOverride
                {
                    ButtonName = currentButtonName,
                    EmpId = Convert.ToString(r["EmpId"]),
                    Override = ov
                });
            }
        }



        private void ConfigureModulesGrid()
        {
            dgvModulos.AutoGenerateColumns = false;
            dgvModulos.Columns.Clear();
            dgvModulos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvModulos.ScrollBars = ScrollBars.Both;
            dgvModulos.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells; // Para wrap si activamos

            bool canEdit = (ModuleGPI.Session.TypeAut >= 5);

            // ButtonName (oculta, clave)
            var cBtn = new DataGridViewTextBoxColumn();
            cBtn.DataPropertyName = "ButtonName";
            cBtn.HeaderText = "Button";
            cBtn.Name = "ButtonName";
            cBtn.Visible = false;
            dgvModulos.Columns.Add(cBtn);

            // Name (RO)
            var cName = new DataGridViewTextBoxColumn();
            cName.DataPropertyName = "Name";
            cName.HeaderText = "Módulo";
            cName.ReadOnly = true;
            cName.FillWeight = 20;
            cName.MinimumWidth = 120;
            dgvModulos.Columns.Add(cName);

            // Category (RO)
            var cCat = new DataGridViewTextBoxColumn();
            cCat.DataPropertyName = "Category";
            cCat.HeaderText = "Categoría";
            cCat.ReadOnly = true;
            cCat.FillWeight = 15;
            cCat.MinimumWidth = 100;
            dgvModulos.Columns.Add(cCat);

            // Rol mínimo
            var cRole = new DataGridViewComboBoxColumn();
            cRole.DataPropertyName = "RolesMinTypeAut";
            cRole.HeaderText = "Rol mínimo";
            cRole.DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
            cRole.FlatStyle = FlatStyle.Flat;
            cRole.Items.Add(1); cRole.Items.Add(2); cRole.Items.Add(3); cRole.Items.Add(4); cRole.Items.Add(5);
            cRole.FillWeight = 10;
            cRole.ReadOnly = !canEdit;
            cRole.MinimumWidth = 80;
            dgvModulos.Columns.Add(cRole);

            dgvModulos.ReadOnly = !canEdit; // de todos modos respeta RO por columna

            dgvModulos.CellFormatting -= DgvModulos_CellFormatting;
            dgvModulos.CellFormatting += DgvModulos_CellFormatting;

        }


        private string GetConnString()
        {
            var cs = ConfigurationManager.ConnectionStrings["DBConnectionString"];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new InvalidOperationException("No se encontró la cadena 'DBConnectionString' en App.config.");
            return cs.ConnectionString;
        }


        private void LoadAdminData()
        {
            try
            {
                string sql = @"
SELECT 
    U.USU_EmpID,
    U.USU_UserLog,
    U.USU_TypeAut,
    U.USU_Status,
    U.USU_UserPLant
FROM dbo.ModGPI_User U
ORDER BY U.USU_EmpID;";

                _dtUsers = new DataTable();

                _daUsers = new SqlDataAdapter();
                using (var cn = new SqlConnection(GetConnString()))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    _daUsers.SelectCommand = cmd;

                    // UPDATE (solo estas columnas editables)
                    _daUsers.UpdateCommand = new SqlCommand(@"
UPDATE dbo.ModGPI_User
SET    USU_TypeAut   = @USU_TypeAut,
       USU_Status    = @USU_Status,
       USU_UserPLant = @USU_UserPLant
WHERE  USU_EmpID     = @USU_EmpID;", cn);

                    _daUsers.UpdateCommand.Parameters.Add("@USU_TypeAut", SqlDbType.Int, 0, "USU_TypeAut");
                    _daUsers.UpdateCommand.Parameters.Add("@USU_Status", SqlDbType.Int, 0, "USU_Status");
                    _daUsers.UpdateCommand.Parameters.Add("@USU_UserPLant", SqlDbType.Int, 0, "USU_UserPLant");
                    var pKey = _daUsers.UpdateCommand.Parameters.Add("@USU_EmpID", SqlDbType.NVarChar, 10, "USU_EmpID");
                    pKey.SourceVersion = DataRowVersion.Original; // PK

                    _daUsers.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                    _daUsers.Fill(_dtUsers);
                }

                dgvUsuarios.DataSource = _dtUsers;

                ConfigureUsersGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando Administración: " + ex.Message, "Admin",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void ConfigureUsersGrid()
        {
            dgvUsuarios.CellValidating += DgvUsers_CellValidating;
            dgvUsuarios.AutoGenerateColumns = false;
            dgvUsuarios.Columns.Clear();



            var colEmp = new DataGridViewTextBoxColumn { DataPropertyName = "USU_EmpID", HeaderText = "EmpID", ReadOnly = true, Width = 100 };
            dgvUsuarios.Columns.Add(colEmp);

            var colUserLog = new DataGridViewTextBoxColumn { DataPropertyName = "USU_UserLog", HeaderText = "UserLog", ReadOnly = true, Width = 160 };
            dgvUsuarios.Columns.Add(colUserLog);

            var colType = new DataGridViewComboBoxColumn
            {
                DataPropertyName = "USU_TypeAut",
                HeaderText = "Rol (TypeAut)",
                DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
                FlatStyle = FlatStyle.Flat,
                Width = 140
            };
            colType.Items.Add(1); colType.Items.Add(2); colType.Items.Add(3); colType.Items.Add(4); colType.Items.Add(5);
            dgvUsuarios.Columns.Add(colType);

            dgvUsuarios.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && dgvUsuarios.Columns[e.ColumnIndex].DataPropertyName == "USU_TypeAut")
                {
                    object v = e.Value;
                    int r;
                    if (v != null && int.TryParse(v.ToString(), out r) && r >= 1 && r <= 5)
                        dgvUsuarios.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = ROLE_NAMES[r];
                }
            };

            dgvUsuarios.ReadOnly = !_adminCanEdit;
            dgvUsuarios.AllowUserToAddRows = false;
            dgvUsuarios.AllowUserToDeleteRows = false;

            if (btnAdminGuardar != null) btnAdminGuardar.Enabled = _adminCanEdit;
        }

        private void DgvUsers_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var col = dgvUsuarios.Columns[e.ColumnIndex].DataPropertyName;
            var val = Convert.ToString(e.FormattedValue ?? "");

            if (col == "USU_TypeAut")
            {
                if (!int.TryParse(val, out int n) || n < 1 || n > 5)
                {
                    e.Cancel = true;
                    MessageBox.Show("Rol debe estar entre 1 y 5.");
                }
            }
        }
        private void DgvModulos_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var col = dgvModulos.Columns[e.ColumnIndex].DataPropertyName;
            var val = Convert.ToString(e.FormattedValue ?? "");

            if (col == "RolesMinTypeAut")
            {
                int n;
                if (!int.TryParse(val, out n) || n < 1 || n > 5)
                {
                    e.Cancel = true;
                    MessageBox.Show("Rol mínimo debe estar entre 1 y 5.");
                }
            }
        }


        private void btnAdminGuardar_Click(object sender, EventArgs e)
        {
            if (!_adminCanEdit)
            {
                MessageBox.Show("No tiene permisos para guardar cambios.", "Admin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 1) aplica cambios del grid de módulos a botones
                if (_dtModulesAdmin != null)
                    UpdateModulesFromGrid(new[] { flpOperacion, flpConsultas });

                // 2) aplica overrides del grid actual (si hay módulo seleccionado)
                if (dgvModulos.CurrentRow != null)
                {
                    var drv = dgvModulos.CurrentRow.DataBoundItem as DataRowView;
                    if (drv != null)
                    {
                        string buttonName = Convert.ToString(drv["ButtonName"]);
                        ApplyOverridesFromView(buttonName);
                        SaveOverridesToFile();
                    }
                }

                // 3) guarda overrides.json
                SaveOverridesToFile();

                // 4) refresca visibilidad (aplica overrides)
                RefreshModules();

                // 5) reconstruye DataTable y guarda modules.json
                LoadModulesAdminFromUI();
                SaveModulesToFile();

                MessageBox.Show("Cambios aplicados y guardados.", "Administración",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error aplicando/guardando: " + ex.Message);
            }
        }



        private void SaveModulesToFile()
        {
            try
            {
                if (_dtModulesAdmin == null) return;

                var list = _dtModulesAdmin.AsEnumerable().Select(r => new ModuleConfig
                {
                    ButtonName = Convert.ToString(r["ButtonName"]),
                    Name = Convert.ToString(r["Name"]),
                    Category = Convert.ToString(r["Category"]),
                    ExePath = Convert.ToString(r["ExePath"]),
                    WorkingDir = Convert.ToString(r["WorkingDir"]),
                    RequiresElevation = Convert.ToBoolean(r["RequiresElevation"]),
                    RolesMinTypeAut = Convert.ToInt32(r["RolesMinTypeAut"])
                }).ToList();

                var json = JsonConvert.SerializeObject(list, Formatting.Indented);
                File.WriteAllText("modules.json", json, System.Text.Encoding.UTF8);

                // Opcional:
                this.tsslEstado.Text = "Módulos guardados en modules.json";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error guardando modules.json: " + ex.Message, "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void LoadModulesFromFileAndApply()
        {
            try
            {
                if (!File.Exists("modules.json")) return;

                var json = File.ReadAllText("modules.json");
                var list = JsonConvert.DeserializeObject<System.Collections.Generic.List<ModuleConfig>>(json);
                if (list == null || list.Count == 0) return;

                // Aplica a los botones por ButtonName
                ApplyModuleConfigList(list);

                // Refresca grid y visibilidad con los nuevos valores
                LoadModulesAdminFromUI();
                RefreshModules();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando modules.json: " + ex.Message, "Cargar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void SaveOverridesToFile()
        {
            try
            {
                // Only save non-zero overrides to optimize
                var toSave = new OverridesStore
                {
                    Items = _overrides.Items.Where(x => x.Override != 0).ToList()
                };
                string json = JsonConvert.SerializeObject(toSave);
                File.WriteAllText("overrides.json", json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving overrides: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOverridesFromFile()
        {
            try
            {
                if (File.Exists("overrides.json"))
                {
                    string json = File.ReadAllText("overrides.json");
                    _overrides = JsonConvert.DeserializeObject<OverridesStore>(json) ?? new OverridesStore();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading overrides: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _overrides = new OverridesStore(); // Reset on error
            }
        }

        private bool CanUserSeeModule(string buttonName, ModuleDef m, int userRole, string empId)
        {
            if (m == null || string.IsNullOrEmpty(empId)) return false;

            // Base: Role check
            bool baseVisible = userRole >= m.RolesMinTypeAut;

            // Find override for this user/module
            var ov = _overrides.Items.FirstOrDefault(x => x.ButtonName == buttonName && x.EmpId == empId);
            if (ov != null)
            {
                if (ov.Override == 1) return true;   // Explicit allow: always visible
                if (ov.Override == -1) return false; // Explicit deny: always hidden
            }

            // No override or inherit (0): Use base role check
            return baseVisible;
        }

        private void ApplyModuleConfigList(System.Collections.Generic.List<ModuleConfig> list)
        {
            // función local para aplicar a un panel
            Action<FlowLayoutPanel> apply = (panel) =>
            {
                foreach (Control c in panel.Controls)
                {
                    if (c is Button btn && btn.Tag is ModuleDef m)
                    {
                        var cfg = list.FirstOrDefault(x => string.Equals(x.ButtonName, btn.Name, StringComparison.OrdinalIgnoreCase));
                        if (cfg == null) continue;

                        // Validación básica (misma whitelist que LaunchModule)
                        if (!string.IsNullOrWhiteSpace(cfg.ExePath) && !ALLOWED_ROOTS.Any(r => cfg.ExePath.StartsWith(r, StringComparison.OrdinalIgnoreCase)))
                        {
                            // si la ruta viene fuera de política, la omitimos
                            continue;
                        }

                        m.ExePath = cfg.ExePath;
                        m.WorkingDir = cfg.WorkingDir;
                        m.RequiresElevation = cfg.RequiresElevation;
                        m.RolesMinTypeAut = cfg.RolesMinTypeAut;

                        ApplyButtonVisibility(btn);
                    }
                }
            };

            apply(this.flpOperacion);
            apply(this.flpConsultas);
        }


        // New helper method to reduce duplication
        private void UpdateModulesFromGrid(FlowLayoutPanel[] panels)
        {
            foreach (var panel in panels)
            {
                foreach (Control c in panel.Controls)
                {
                    if (c is Button btn && btn.Tag is ModuleDef m)
                    {
                        var rows = _dtModulesAdmin.Select("ButtonName = '" + btn.Name.Replace("'", "''") + "'");
                        if (rows.Length == 0) continue;

                        var r = rows[0];
                        m.ExePath = Convert.ToString(r["ExePath"]);
                        m.WorkingDir = Convert.ToString(r["WorkingDir"]);
                        m.RequiresElevation = Convert.ToBoolean(r["RequiresElevation"]);
                        m.RolesMinTypeAut = Convert.ToInt32(r["RolesMinTypeAut"]);

                        ApplyButtonVisibility(btn);  // Apply immediately
                    }
                }
            }
        }

        private void btnAdminRefrescar_Click(object sender, EventArgs e)
        {
            LoadAdminData();
        }


        private void WireStaticHandlers()
        {
            // Menu/Tool actions
            this.mnuArchivo_Salir.Click += delegate { this.Close(); };
            this.tsbRefrescar.Click += delegate { RefreshModules(); };
            this.mnuVer_Refrescar.Click += delegate { RefreshModules(); };
            this.tsbConfig.Click += delegate { this.tabMain.SelectedTab = this.tabConfig; };
            this.mnuHerramientas_Config.Click += delegate { this.tabMain.SelectedTab = this.tabConfig; };
            this.tsbCerrarSesion.Click += delegate { Logout(); };
            this.mnuAyuda_Acerca.Click += delegate { MessageBox.Show("GPI Lanzador v1.0", "Acerca de"); };

            // Búsquedas
            this.tsbBuscar.Click += delegate { ApplySearch(this.tstBuscar.Text, null); };
            this.tstBuscar.KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) ApplySearch(this.tstBuscar.Text, null); };
            this.txtOpSearch.KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) ApplySearch(this.txtOpSearch.Text, this.flpOperacion); };
            this.txtConsSearch.KeyDown += delegate (object s, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) ApplySearch(this.txtConsSearch.Text, this.flpConsultas); };

            // TV categorías
            this.tvCategorias.AfterSelect += delegate (object s, TreeViewEventArgs e)
            {
                string node = e.Node != null ? e.Node.Text : "";
                SwitchByCategory(node);
            };

            // Context menu
            this.cmuAbrir.Click += delegate { OpenContextSelected(false); };
            this.cmuAbrirAdmin.Click += delegate { OpenContextSelected(true); };
            this.cmuCopiarRuta.Click += delegate { CopyModulePathFromContext(); };
            this.cmuVerProp.Click += delegate { ShowModulePropertiesFromContext(); };

            // Botones Admin
            if (this.btnAdminGuardar != null)
                this.btnAdminGuardar.Click += new System.EventHandler(this.btnAdminGuardar_Click);

            if (this.btnAdminRefrescar != null)
                this.btnAdminRefrescar.Click += new System.EventHandler(this.btnAdminRefrescar_Click);

        }

        private void LoadCategories()
        {
            this.tvCategorias.BeginUpdate();
            this.tvCategorias.Nodes.Clear();
            this.tvCategorias.Nodes.Add("Dashboard");
            this.tvCategorias.Nodes.Add("Operación");
            this.tvCategorias.Nodes.Add("Consultas");
            if (ModuleGPI.Session.TypeAut >= 4)
            {
                this.tvCategorias.Nodes.Add("Administración");
                this.tvCategorias.Nodes.Add("Configuración");
            }
            this.tvCategorias.EndUpdate();
        }

        private void ApplyRoleVisibility(int typeAut)
        {
            tabDashboard.Visible = true;
            tabConsultas.Visible = typeAut >= 1;
            tabOperacion.Visible = typeAut >= 2;
            tabAdmin.Visible = typeAut >= 4;
            tabConfig.Visible = typeAut >= 4;

            _adminCanEdit = (typeAut >= 4);    // 4 y 5 editan

            if (btnAdminGuardar != null) btnAdminGuardar.Enabled = _adminCanEdit;
            if (dgvModulos != null) dgvModulos.ReadOnly = !_adminCanEdit;
            if (dgvOverrides != null) dgvOverrides.ReadOnly = !_adminCanEdit;
        }





        private void AssignModuleTags()
        {
            // ConteoPhysic (Operación)
            this.btnMod_Op_Inventario.Tag = new ModuleDef
            {
                Name = "ConteoPhysic",
                ExePath = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\MantenimientoConteosFisicos\ConteoPhysic.exe",
                WorkingDir = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\MantenimientoConteosFisicos",
                Category = "Operación",
                RequiresElevation = false,
                RolesMinTypeAut = 2
            };

            // SAPcrd (Operación) – mantenimiento a cuentas contables
            this.btnMod_Op_Logistica.Tag = new ModuleDef
            {
                Name = "SAPcrd",
                ExePath = @"\\USAZR3PITVFE001\Intuitive MTY\CustomApps\SAPcrd\SAPcrd\bin\Debug\SAPcrd.exe",
                WorkingDir = @"\\USAZR3PITVFE001\Intuitive MTY\CustomApps\SAPcrd\SAPcrd\bin\Debug",
                Category = "Operación",
                RequiresElevation = false,
                RolesMinTypeAut = 2
            };

            // PorteCart (Operación) – Carta Porte
            this.btnMod_Op_Produccion.Tag = new ModuleDef
            {
                Name = "PorteCart",
                ExePath = @"\\USAZR3QITVFE001\Intuitive MTY\CDFI40\MantenimientoCP\PorteCart.exe",
                WorkingDir = @"\\USAZR3QITVFE001\Intuitive MTY\CDFI40\MantenimientoCP",
                Category = "Operación",
                RequiresElevation = false,
                RolesMinTypeAut = 2
            };

            // ETIQUETAS MP ETIQUETASMP     EA  MP
            this.btnMod_Op_MP.Tag = new ModuleDef
            {
                Name = "ETIQUETAMP",
                ExePath = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\EtiquetasMP\bin\Debug\EtiquetasMP.exe",
                WorkingDir = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\EtiquetasMP\bin\Debug",
                Category = "Operación",
                RequiresElevation = false,
                RolesMinTypeAut = 2
            };


            // ETIQUETAS CORRUGADO
            this.btnMod_Op_corrugado.Tag = new ModuleDef
            {
                Name = "ETIQUETACORRUGADO",
                ExePath = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\EtiquetasCorrugado\bin\Debug\EtiquetasCorrugadoXCliente.exe",
                WorkingDir = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\EtiquetasCorrugado\bin\Debug",
                Category = "Operación",
                RequiresElevation = false,
                RolesMinTypeAut = 2
            };


            // ETIQUETAS CM
            this.btnMod_Op_CM.Tag = new ModuleDef
            {
                Name = "ETIQUETACM",
                ExePath = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\EtiquetasCM\bin\Debug\EtiquetasCM.exe",
                WorkingDir = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\EtiquetasCM\bin\Debug",
                Category = "Operación",
                RequiresElevation = false,
                RolesMinTypeAut = 2
            };


            // Conversor UM (fix name)
            this.btnMod_Op_ConversorUM.Tag = new ModuleDef
            {
                Name = "ConversorUM",  // Fixed from "ETIQUETACM"
                ExePath = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\ConversorUM\bin\Debug\ConversorUM.exe",
                WorkingDir = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\ConversorUM\bin\Debug",
                Category = "Operación",
                RequiresElevation = false,
                RolesMinTypeAut = 2
            };

            // Calculadora Tarimas (fix name)
            this.btnMod_Op_CalculadoraTarimas.Tag = new ModuleDef
            {
                Name = "CalculadoraTarimas",  // Fixed from "ETIQUETACM"
                ExePath = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\Calculadora Tarimas\bin\Debug\Calculadora Tarimas.exe",
                WorkingDir = @"\\USAZR3QITVFE001\Intuitive MTY\CustomApps\Calculadora Tarimas\bin\Debug",
                Category = "Operación",
                RequiresElevation = false,
                RolesMinTypeAut = 2
            };

            // KPIs (Consultas) – deja cualquiera que uses para pruebas
            this.btnMod_Cons_Reportes.Tag = new ModuleDef
            {
                Name = "Reportes",
                ExePath = @"\\srv\apps\reportes\reportes.exe", // cámbialo si quieres
                WorkingDir = @"\\srv\apps\reportes",
                Category = "Consultas",
                RequiresElevation = false,
                RolesMinTypeAut = 1
            };

            this.btnMod_Cons_KPIs.Tag = new ModuleDef
            {
                Name = "KPIs",
                ExePath = @"\\USAZR3PITVFE001\Intuitive MTY\Mantenimiento Conteos\conteophy\ConteoPhysic\bin\Debug\ConteoPhysic.exe",
                WorkingDir = @"\\USAZR3PITVFE001\Intuitive MTY\Mantenimiento Conteos\conteophy\ConteoPhysic\bin\Debug",
                Category = "Consultas",
                RequiresElevation = false,
                RolesMinTypeAut = 1
            };
        }


        private void WireModuleButtons()
        {
            // Operación
            foreach (Control c in this.flpOperacion.Controls)
            {
                Button btn = c as Button;
                if (btn != null)
                {
                    btn.ContextMenuStrip = this.cmuModulo;
                    btn.Click += new EventHandler(this.ModuleButton_Click);
                    ApplyButtonVisibility(btn);
                }
            }

            // Consultas
            foreach (Control c in this.flpConsultas.Controls)
            {
                Button btn = c as Button;
                if (btn != null)
                {
                    btn.ContextMenuStrip = this.cmuModulo;
                    btn.Click += new EventHandler(this.ModuleButton_Click);
                    ApplyButtonVisibility(btn);
                }
            }
        }

        private void ApplyButtonVisibility(Button btn)
        {
            var m = btn.Tag as ModuleDef;
            if (m == null) return;

            bool visible = CanUserSeeModule(
                btn.Name, m, Session.TypeAut, Session.EmpId ?? Session.LogonName);
            btn.Visible = visible;
            btn.Enabled = visible;
            this.toolTips.SetToolTip(btn, m.Name + Environment.NewLine + (m.ExePath ?? ""));
        }


        private void ModuleButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                ModuleDef m = btn.Tag as ModuleDef;
                if (m != null) LaunchModule(m, false);
            }
        }

        private void OpenContextSelected(bool asAdmin)
        {
            Button btn = this.cmuModulo.SourceControl as Button;
            if (btn != null)
            {
                ModuleDef m = btn.Tag as ModuleDef;
                if (m != null) LaunchModule(m, asAdmin);
            }
        }

        private void CopyModulePathFromContext()
        {
            Button btn = this.cmuModulo.SourceControl as Button;
            if (btn != null)
            {
                ModuleDef m = btn.Tag as ModuleDef;
                if (m != null && !string.IsNullOrEmpty(m.ExePath))
                {
                    Clipboard.SetText(m.ExePath);
                }
            }
        }

        private void ShowModulePropertiesFromContext()
        {
            Button btn = this.cmuModulo.SourceControl as Button;
            if (btn != null)
            {
                ModuleDef m = btn.Tag as ModuleDef;
                if (m != null)
                {
                    MessageBox.Show(m.Name + Environment.NewLine + (m.ExePath ?? "") + Environment.NewLine + (m.WorkingDir ?? ""),
                        "Propiedades", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RefreshModules()
        {
            foreach (Control c in this.flpOperacion.Controls)
            {
                Button b = c as Button;
                if (b != null) ApplyButtonVisibility(b);
            }
            foreach (Control c in this.flpConsultas.Controls)
            {
                Button b = c as Button;
                if (b != null) ApplyButtonVisibility(b);
            }
            this.tsslEstado.Text = "Módulos actualizados";
        }

        private void Logout()
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }


        private void SwitchByCategory(string node)
        {
            switch (node)
            {
                case "Dashboard": this.tabMain.SelectedTab = this.tabDashboard; break;
                case "Operación": if (this.tabOperacion.Visible) this.tabMain.SelectedTab = this.tabOperacion; break;
                case "Consultas": if (this.tabConsultas.Visible) this.tabMain.SelectedTab = this.tabConsultas; break;
                case "Administración": if (this.tabAdmin.Visible) this.tabMain.SelectedTab = this.tabAdmin; break;
                case "Configuración": if (this.tabConfig.Visible) this.tabMain.SelectedTab = this.tabConfig; break;
            }
        }

        private void ApplySearch(string text, FlowLayoutPanel scope)
        {
            string t = (text ?? string.Empty).Trim();

            if (scope == null)
            {
                ApplySearch(t, this.flpOperacion);
                ApplySearch(t, this.flpConsultas);
                return;
            }

            foreach (Control c in scope.Controls)
            {
                if (c is Button btn && btn.Tag is ModuleDef m)
                {
                    bool nameMatch = (t.Length == 0)
                                     || m.Name.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0
                                     || (!string.IsNullOrEmpty(m.ExePath) && m.ExePath.IndexOf(t, StringComparison.OrdinalIgnoreCase) >= 0);

                    bool canSee = CanUserSeeModule(btn.Name, m, Session.TypeAut, Session.EmpId ?? Session.LogonName);
                    btn.Visible = nameMatch && canSee;  // <-- respeta overrides
                }
            }
        }


        private void LaunchModule(ModuleDef m, bool asAdmin)
        {
            try
            {
                if (Session.TypeAut < m.RolesMinTypeAut)
                {
                    MessageBox.Show("No tiene permisos para ejecutar este módulo.", "Acceso denegado",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(m.ExePath) || !File.Exists(m.ExePath))
                {
                    MessageBox.Show("No se encontró el ejecutable.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }



                if (!IsPathAllowed(m.ExePath))
                {
                    MessageBox.Show("Ruta no autorizada por la política.", "Bloqueado",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }




                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = m.ExePath;
                psi.Arguments = m.Arguments ?? "";
                psi.WorkingDirectory = !string.IsNullOrEmpty(m.WorkingDir)
                    ? m.WorkingDir
                    : Path.GetDirectoryName(m.ExePath);
                psi.UseShellExecute = true;
                psi.Verb = (asAdmin || m.RequiresElevation) ? "runas" : "";

                Process p = Process.Start(psi);
                if (p != null)
                    this.tsslEstado.Text = "Lanzado: " + m.Name;
                // TODO: registrar LaunchLog en DB
            }
            catch (Win32Exception w32)
            {
                if (w32.NativeErrorCode == 1223)
                    this.tsslEstado.Text = "Elevación cancelada por el usuario.";
                else
                    MessageBox.Show("Error UAC/Win32: " + w32.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir módulo: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GetRoleName(int typeAut)
        {
            if (typeAut <= 1) return "Viewer";
            if (typeAut == 2) return "Operator";
            if (typeAut == 3) return "Supervisor";
            if (typeAut == 4) return "AdminDept";
            return "SysAdmin";
        }
    }

    // POCO
    public sealed class ModuleDef
    {
        public string Name = "";
        public string ExePath;
        public string Arguments;
        public string WorkingDir;
        public string Category;
        public bool RequiresElevation = false;
        public int RolesMinTypeAut = 1;
    }

    // Sesión mínima (ajústala a la tuya)
    public sealed class ModuleConfig
    {
        public string ButtonName { get; set; }        // clave única en UI
        public string Name { get; set; }              // solo informativo
        public string Category { get; set; }          // solo informativo
        public string ExePath { get; set; }
        public string WorkingDir { get; set; }
        public bool RequiresElevation { get; set; }
        public int RolesMinTypeAut { get; set; }
    }

    public sealed class ModuleUserOverride
    {
        public string ButtonName { get; set; } // clave del módulo (Button.Name)
        public string EmpId { get; set; }      // USU_EmpID
        public int Override { get; set; }      // -1=Denegar, 0=Heredado, 1=Permitir
    }

    public sealed class OverridesStore
    {
        public System.Collections.Generic.List<ModuleUserOverride> Items { get; set; } =
            new System.Collections.Generic.List<ModuleUserOverride>();
    }



}