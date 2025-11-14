using ModuleGPI.Data;
using ModuleGPI.Domain;
using ModuleGPI.Services;
using ModuleGPI.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ModuleGPI
{
    public partial class MainForm : Form
    {
        #region Fields
        private readonly IDataAccess _dataAccess;
        private readonly IModuleService _moduleService;
        private readonly IRoleManager _roleManager;
        private readonly IUIHelpers _uiHelpers;

        private DataTable _dtModulesAdmin;
        private DataTable _dtUsers;
        private DataTable _dtOverridesView;
        private readonly ToolTip _toolTips = new ToolTip();
        private OverridesStore _overrides;
        private bool _adminCanEdit;

        private ToolStripMenuItem tsmiAbrir;
        private ToolStripMenuItem tsmiAbrirAdmin;
        private ToolStripMenuItem tsmiCopiarRuta;
        private ToolStripMenuItem tsmiPropiedades;

        private DataGridView dgvOverrides;

        private static readonly string[] ALLOWED_ROOTS = new string[]
        {
            @"\\USAZR3QITVFE001\Intuitive MTY\",
            @"\\USAZR3PITVFE001\Intuitive MTY\",
            @"\\srv\apps\",
            @"C:\Program Files\CorpApps\"
        };
        #endregion

        #region Constructor
        public MainForm()
        {
            InitializeComponent();

            // Inicializar servicios primero (dependencias para handlers y grids)
            _dataAccess = new SqlDataAccess();
            _moduleService = new ModuleService(_dataAccess);
            _roleManager = new RoleManager();
            _uiHelpers = new UIHelpers();

            _overrides = new OverridesStore();

            SetupEventHandlers();
            SetupOverridesGrid();
        }
        #endregion

        #region Form Events
        private void MainForm_Load(object sender, EventArgs e)
        {
            _uiHelpers.PositionHeaderSearchBoxes(pnlOpHeader, btnOpRefrescar, txtOpSearch, pnlConsHeader, txtConsSearch);

            UpdateStatusBar();

            _roleManager.ApplyVisibility(tabMain, tabAdmin, tabConfig, Session.TypeAut);
            _adminCanEdit = Session.TypeAut >= 5;

            LoadOverrides();
            LoadModules();

            if (Session.TypeAut >= 4)
            {
                LoadAdminData();
            }

            LoadCategories();

            WireModuleButtons();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (txtOpSearch != null) txtOpSearch.Focus();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            _uiHelpers.PositionHeaderSearchBoxes(pnlOpHeader, btnOpRefrescar, txtOpSearch, pnlConsHeader, txtConsSearch);
        }
        #endregion

        #region Setup Methods
        private void SetupEventHandlers()
        {
            if (tabMain != null)
            {
                tabMain.Selected += TabMain_Selected;
            }

            if (txtOpSearch != null)
            {
                txtOpSearch.TextChanged += (s, e) => ApplySearch(txtOpSearch.Text, flpOperacion);
            }
            if (txtConsSearch != null)
            {
                txtConsSearch.TextChanged += (s, e) => ApplySearch(txtConsSearch.Text, flpConsultas);
            }

            if (btnOpRefrescar != null)
            {
                btnOpRefrescar.Click += (s, e) => RefreshModules();
            }

            if (treeCategories != null)
            {
                treeCategories.AfterSelect += (s, e) => SwitchByCategory(e.Node?.Text);
            }

            SetupContextMenu();

            if (dgvModulos != null)
            {
                dgvModulos.SelectionChanged += DgvModulos_SelectionChanged;
                dgvModulos.CellFormatting += DgvModulos_CellFormatting;
                dgvModulos.CellEndEdit += DgvModulos_CellEndEdit;
                _uiHelpers.EnableDgvDoubleBuffer(dgvModulos);
            }

            if (dgvUsuarios != null)
            {
                dgvUsuarios.CellEndEdit += DgvUsuarios_CellEndEdit;
                _uiHelpers.EnableDgvDoubleBuffer(dgvUsuarios);
            }
        }

        private void SetupContextMenu()
        {
            if (cmuModulo == null)
            {
                cmuModulo = new ContextMenuStrip();
                tsmiAbrir = new ToolStripMenuItem("Abrir");
                tsmiAbrirAdmin = new ToolStripMenuItem("Abrir como Administrador");
                tsmiCopiarRuta = new ToolStripMenuItem("Copiar ruta");
                tsmiPropiedades = new ToolStripMenuItem("Propiedades");

                cmuModulo.Items.AddRange(new ToolStripItem[]
                {
                    tsmiAbrir,
                    tsmiAbrirAdmin,
                    new ToolStripSeparator(),
                    tsmiCopiarRuta,
                    tsmiPropiedades
                });

                tsmiAbrir.Click += (s, e) => OpenContextSelected(false);
                tsmiAbrirAdmin.Click += (s, e) => OpenContextSelected(true);
                tsmiCopiarRuta.Click += (s, e) => CopyModulePathFromContext();
                tsmiPropiedades.Click += (s, e) => ShowModulePropertiesFromContext();
            }
        }

        private void SetupOverridesGrid()
        {
            if (dgvOverrides == null)
            {
                dgvOverrides = new DataGridView
                {
                    Name = "dgvOverrides",
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    ReadOnly = true,
                    RowHeadersVisible = false,
                    Dock = DockStyle.Fill
                };

                dgvOverrides.CurrentCellDirtyStateChanged += DgvOverrides_CurrentCellDirtyStateChanged;
                dgvOverrides.CellValueChanged += DgvOverrides_CellValueChanged;
                dgvOverrides.DataError += (s, e) => { e.ThrowException = false; };
                dgvOverrides.CellFormatting += DgvOverrides_CellFormatting;

                _uiHelpers.EnableDgvDoubleBuffer(dgvOverrides);

                if (dgvModulos != null && tabAdmin != null)
                {
                    var host = dgvModulos.Parent ?? tabAdmin;
                    host.Controls.Remove(dgvModulos);

                    var splitContainer = new SplitContainer
                    {
                        Dock = DockStyle.Fill,
                        Orientation = Orientation.Horizontal
                    };
                    splitContainer.Panel1.Controls.Add(dgvModulos);
                    dgvModulos.Dock = DockStyle.Fill;
                    splitContainer.Panel2.Controls.Add(dgvOverrides);
                    splitContainer.SplitterDistance = host.ClientSize.Height / 2;

                    host.Controls.Add(splitContainer);
                }
            }
        }
        #endregion

        #region Tab Management
        private void TabMain_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == tabAdmin || e.TabPage == tabConfig)
            {
                if (Session.TypeAut < 4)
                {
                    tabMain.SelectedTab = tabDashboard;
                    MessageBox.Show("Acceso denegado: Requiere rol AdminDept o superior.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                LoadAdminData();
            }
        }
        #endregion

        #region Module Loading and Management
        private void LoadModules()
        {
            try
            {
                var dt = _moduleService.LoadModules(Session.Sucursal);
                _moduleService.PaintButtons(dt, flpOperacion, flpConsultas, cmuModulo, _toolTips,
                    (btnName, module) => _roleManager.CanSeeModule(btnName, module, Session.TypeAut,
                        Session.EmpId ?? Session.LogonName, _overrides));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar módulos: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshModules()
        {
            _moduleService.RefreshVisibility(flpOperacion, flpConsultas,
                (btnName, module) => _roleManager.CanSeeModule(btnName, module, Session.TypeAut,
                    Session.EmpId ?? Session.LogonName, _overrides));
            UpdateStatus("Módulos actualizados");
        }

        private void WireModuleButtons()
        {
            _moduleService.WireButtons(flpOperacion, flpConsultas, ModuleButton_Click);
        }

        private void ModuleButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is ModuleDef m)
            {
                _moduleService.LaunchModule(btn.Name, m, false, ALLOWED_ROOTS, UpdateStatus);
            }
        }
        #endregion

        #region Context Menu Actions
        private void OpenContextSelected(bool asAdmin)
        {
            if (cmuModulo?.SourceControl is Button btn && btn.Tag is ModuleDef m)
            {
                _moduleService.LaunchModule(btn.Name, m, asAdmin, ALLOWED_ROOTS, UpdateStatus);
            }
        }

        private void CopyModulePathFromContext()
        {
            if (cmuModulo?.SourceControl is Button btn && btn.Tag is ModuleDef m && !string.IsNullOrEmpty(m.ExePath))
            {
                Clipboard.SetText(m.ExePath);
                UpdateStatus("Ruta copiada al portapapeles");
            }
        }

        private void ShowModulePropertiesFromContext()
        {
            if (cmuModulo?.SourceControl is Button btn && btn.Tag is ModuleDef m)
            {
                var info = $"Nombre: {m.Name}\n" +
                           $"Ruta: {m.ExePath ?? "(no especificada)"}\n" +
                           $"Directorio: {m.WorkingDir ?? "(no especificado)"}\n" +
                           $"Categoría: {m.Category}\n" +
                           $"Rol mínimo: {_roleManager.GetRoleName(m.RolesMinTypeAut)}\n" +
                           $"Requiere elevación: {(m.RequiresElevation ? "Sí" : "No")}";

                MessageBox.Show(info, "Propiedades del Módulo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Admin Functions
        private void LoadAdminData()
        {
            if (Session.TypeAut < 4) return;

            try
            {
                _dtModulesAdmin = _dataAccess.GetModules(null);
                if (dgvModulos != null)
                {
                    dgvModulos.DataSource = _dtModulesAdmin;
                    dgvModulos.ReadOnly = !_adminCanEdit;
                }

                _dtUsers = _dataAccess.GetUsers();
                if (dgvUsuarios != null)
                {
                    dgvUsuarios.DataSource = _dtUsers;
                    dgvUsuarios.ReadOnly = !_adminCanEdit;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos administrativos: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvModulos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvModulos?.CurrentRow?.DataBoundItem is DataRowView drv)
            {
                string buttonName = Convert.ToString(drv["ButtonName"]);
                BuildOverridesViewFor(buttonName);
            }
        }

        private void DgvModulos_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (!_adminCanEdit)
                return;

            var current = dgvModulos?.CurrentRow?.DataBoundItem as DataRowView;
            if (current == null)
                return;

            try
            {
                _dataAccess.UpsertModule(current.Row);
                UpdateStatus("Módulo actualizado");
                RefreshModules();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar módulo: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvModulos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (Session.TypeAut < 4 || e.RowIndex < 0) return;

            var col = dgvModulos.Columns[e.ColumnIndex].DataPropertyName;

            if (col == "ExePath" || col == "WorkingDir")
            {
                dgvModulos.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = Convert.ToString(e.Value);
            }
            else if (col == "RolesMinTypeAut")
            {
                int r;
                if (int.TryParse(Convert.ToString(e.Value), out r) && r >= 1 && r <= 5)
                {
                    dgvModulos.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = _roleManager.GetRoleName(r);
                }
            }
        }

        private void DgvUsuarios_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (!_adminCanEdit || _dtUsers == null) return;

            try
            {
                _dataAccess.UpdateUsers(_dtUsers);
                UpdateStatus("Usuario actualizado");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al actualizar usuario: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Overrides Management
        private void LoadOverrides()
        {
            try
            {
                _overrides = _dataAccess.GetOverrides();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar overrides: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _overrides = new OverridesStore();
            }
        }

        private void BuildOverridesViewFor(string buttonName)
        {
            if (_dtUsers == null) return;

            _dtOverridesView = new DataTable();
            _dtOverridesView.Columns.Add("EmpId", typeof(string));
            _dtOverridesView.Columns.Add("UserName", typeof(string));
            _dtOverridesView.Columns.Add("RoleName", typeof(string));
            _dtOverridesView.Columns.Add("Override", typeof(int));

            foreach (DataRow userRow in _dtUsers.Rows)
            {
                string empId = Convert.ToString(userRow["USU_EmpID"]);
                string userName = Convert.ToString(userRow["USU_UserLog"]);
                int userRole = Convert.ToInt32(userRow["USU_TypeAut"]);

                var ov = _overrides.Get(buttonName, empId) ?? 0;

                _dtOverridesView.Rows.Add(empId, userName, _roleManager.GetRoleName(userRole), ov);
            }

            dgvOverrides.DataSource = _dtOverridesView;

            if (!dgvOverrides.Columns.Contains("OverrideCombo"))
            {
                var comboCol = new DataGridViewComboBoxColumn
                {
                    Name = "OverrideCombo",
                    HeaderText = "Override",
                    DataPropertyName = "Override",
                    DataSource = new[]
                    {
                        new { Value = -1, Display = "Denegar" },
                        new { Value = 0, Display = "Heredado" },
                        new { Value = 1, Display = "Permitir" }
                    },
                    ValueMember = "Value",
                    DisplayMember = "Display"
                };

                var index = dgvOverrides.Columns["Override"].Index;
                dgvOverrides.Columns.RemoveAt(index);
                dgvOverrides.Columns.Insert(index, comboCol);
            }

            dgvOverrides.ReadOnly = !_adminCanEdit;
        }

        private void DgvOverrides_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvOverrides.IsCurrentCellDirty)
            {
                dgvOverrides.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvOverrides_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!_adminCanEdit || e.RowIndex < 0 || dgvModulos?.CurrentRow == null) return;

            var drv = dgvModulos.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;

            string buttonName = Convert.ToString(drv["ButtonName"]);
            ApplyOneOverrideFromRow(buttonName, e.RowIndex);
            SaveOverrides(buttonName);
            RefreshModules();
        }

        private void ApplyOneOverrideFromRow(string buttonName, int rowIndex)
        {
            if (_dtOverridesView == null || rowIndex >= _dtOverridesView.Rows.Count) return;

            var row = _dtOverridesView.Rows[rowIndex];
            string empId = Convert.ToString(row["EmpId"]);
            int overrideValue = Convert.ToInt32(row["Override"]);

            _overrides.Set(buttonName, empId, overrideValue);
        }

        private void SaveOverrides(string buttonName)
        {
            if (!_adminCanEdit || _dtOverridesView == null) return;

            try
            {
                _dataAccess.ReplaceOverrides(buttonName, _dtOverridesView);
                UpdateStatus("Overrides guardados");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar overrides: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DgvOverrides_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var drv = dgvOverrides.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (drv == null) return;

            int ov = 0;
            object raw = drv.Row["Override"];
            if (raw != DBNull.Value)
            {
                int.TryParse(raw.ToString(), out ov);
            }

            var row = dgvOverrides.Rows[e.RowIndex];
            switch (ov)
            {
                case 1:
                    row.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220); // Verde claro
                    break;
                case -1:
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230); // Rojo claro
                    break;
                default:
                    row.DefaultCellStyle.BackColor = Color.White; // Heredado
                    break;
            }
        }
        #endregion

        #region UI Helpers
        private void UpdateStatusBar()
        {
            if (tsslUser != null) tsslUser.Text = $"Usuario: {Session.LogonName}";
            if (tsslRole != null) tsslRole.Text = $"Rol: {_roleManager.GetRoleName(Session.TypeAut)}";
            if (tsslPlant != null) tsslPlant.Text = $"Planta: {Session.Sucursal}";
            if (tsslEstado != null) tsslEstado.Text = "Listo";
        }

        private void UpdateStatus(string message)
        {
            if (tsslEstado != null)
                tsslEstado.Text = message;
        }

        private void LoadCategories()
        {
            if (treeCategories == null) return;

            treeCategories.Nodes.Clear();
            treeCategories.Nodes.Add("Dashboard");
            treeCategories.Nodes.Add("Operación");
            treeCategories.Nodes.Add("Consultas");
            if (Session.TypeAut >= 4)
            {
                treeCategories.Nodes.Add("Administración");
                treeCategories.Nodes.Add("Configuración");
            }

            treeCategories.ExpandAll();
        }

        private void SwitchByCategory(string node)
        {
            if (string.IsNullOrEmpty(node)) return;

            switch (node)
            {
                case "Dashboard":
                    if (tabDashboard != null) tabMain.SelectedTab = tabDashboard;
                    break;
                case "Operación":
                    if (tabOperacion?.Visible == true) tabMain.SelectedTab = tabOperacion;
                    break;
                case "Consultas":
                    if (tabConsultas?.Visible == true) tabMain.SelectedTab = tabConsultas;
                    break;
                case "Administración":
                    if (tabAdmin?.Visible == true) tabMain.SelectedTab = tabAdmin;
                    break;
                case "Configuración":
                    if (tabConfig?.Visible == true) tabMain.SelectedTab = tabConfig;
                    break;
            }
        }

        private void ApplySearch(string text, FlowLayoutPanel scope)
        {
            if (scope == null) return;

            string searchText = (text ?? string.Empty).Trim();

            foreach (Control control in scope.Controls)
            {
                if (control is Button btn && btn.Tag is ModuleDef m)
                {
                    bool nameMatch = string.IsNullOrEmpty(searchText) ||
                                     m.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                     (!string.IsNullOrEmpty(m.ExePath) &&
                                      m.ExePath.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);

                    bool canSee = _roleManager.CanSeeModule(btn.Name, m, Session.TypeAut,
                        Session.EmpId ?? Session.LogonName, _overrides);

                    btn.Visible = nameMatch && canSee;
                }
            }
        }
        #endregion

        #region Public Methods
        public void Logout()
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }
        #endregion
    }
}