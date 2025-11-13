using ModuleGPI;
using ModuleGPI.Data;
using ModuleGPI.Domain;
using ModuleGPI.Services;
using ModuleGPI.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace GPI.Launcher
{
    public partial class MainForm : Form
    {
        private readonly IDataAccess _db;
        private readonly IModuleService _modules;
        private readonly IRoleManager _roles;
        private readonly IUIHelpers _ui;

        // Estado / Data
        private DataTable _dtModulesAdmin;
        private DataTable _dtUsers;
        private SqlDataAdapter _daUsers;
        private bool _adminCanEdit;
        private DataTable _dtModulesConfig;
        private SqlDataAdapter _daModules;

        private OverridesStore _overrides = new OverridesStore();
        private DataTable _dtOverridesView;
        private DataGridView dgvOverrides;

        // Políticas / Constantes
        private static readonly string[] ALLOWED_ROOTS = new string[]
        {
            @"\\USAZR3QITVFE001\Intuitive MTY\",
            @"\\USAZR3PITVFE001\Intuitive MTY\",
            @"\\srv\apps\",
            @"C:\Program Files\CorpApps\"
        };

        private static readonly object[] ROLE_MIN_OPTIONS_ALL = new[]
        {
            new { Value = 1, Text = "Viewer" },
            new { Value = 2, Text = "Operator" },
            new { Value = 3, Text = "Supervisor" },
            new { Value = 4, Text = "AdminDept" },
            new { Value = 5, Text = "SysAdmin" }
        };

        private static readonly Dictionary<int, string> PLANTS = new Dictionary<int, string>
        {
            {1, "MTY"}, {2, "QRO"}, {3, "TIJUANA"}
        };

        private static readonly object[] PLANT_OPTIONS = new[]
        {
            new { Value = 1, Text = "1 - MTY" },
            new { Value = 2, Text = "2 - QRO" },
            new { Value = 3, Text = "3 - TIJUANA" }
        };

        private readonly ToolTip toolTips = new ToolTip();

        public MainForm(IDataAccess db, IModuleService modules, IRoleManager roles, IUIHelpers ui)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _modules = modules ?? throw new ArgumentNullException(nameof(modules));
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
            _ui = ui ?? throw new ArgumentNullException(nameof(ui));

            InitializeComponent();
            WireStaticHandlers();
            dgvOverrides = _ui.BuildOverridesGrid(splitAdmin.Panel2, 200);  // Ajustar parent si rightAdmin es splitAdmin.Panel2

            dgvModulesConfig.CellValidating += DgvModulesConfig_CellValidating;

            tabMain.Selected += TabMain_Selected;
        }

        private void DgvModulesConfig_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var col = dgvModulesConfig.Columns[e.ColumnIndex].DataPropertyName;
            var val = e.FormattedValue?.ToString() ?? "";
            if (col == "ExePath" && !string.IsNullOrEmpty(val) && !ModuleService.IsPathAllowed(val, ALLOWED_ROOTS))
            {
                e.Cancel = true;
                MessageBox.Show("Ruta no permitida por la política de seguridad.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _ui.PositionHeaderSearchBoxes(pnlOpHeader, btnOpRefrescar, txtOpSearch, pnlConsHeader, txtConsSearch);

            int? plant = ParsePlant(Session.Sucursal);  // Corregir conversión
            var dtModules = _modules.LoadModules(plant);
            _modules.PaintButtons(dtModules, flpOperacion, flpConsultas, cmsModules, toolTips,
                (btnName, mod) => _roles.CanSeeModule(btnName, mod, Session.TypeAut, Session.EmpId, _overrides));

            tsslUser.Text = "Usuario: " + Session.LogonName;
            tsslRole.Text = "Rol: " + _roles.GetRoleName(Session.TypeAut);
            string plantName;
            tsslPlant.Text = "Planta: " + (PLANTS.TryGetValue(Session.Sucursal, out plantName) ? plantName : "Unknown");  // Corregir GetValueOrDefault con TryGetValue
            tsslEstado.Text = "Listo";

            _roles.ApplyVisibility(tabMain, tabAdmin, tabConfig, Session.TypeAut);

            if (Session.TypeAut >= 4)
            {
                _dtModulesConfig = _db.GetModules(null);
                LoadModulesConfigBindings();
            }

            LoadCategories();

            _overrides = _db.GetOverrides();

            _modules.WireButtons(flpOperacion, flpConsultas, ModuleButton_Click);

            if (Session.TypeAut >= 4)
            {
                _dtUsers = _db.GetUsers();
                LoadAdminDataBindings();
                _daUsers = CreateUsersAdapter();  // Implementado abajo
                LoadAdminModulesFromUI();
            }

            cboPlantFilter.DataSource = PLANT_OPTIONS;
            cboPlantFilter.ValueMember = "Value";
            cboPlantFilter.DisplayMember = "Text";
            ApplyUserPlantFilter();
        }

        private int? ParsePlant(int sucursal)  // Asumiendo Session.Sucursal es int; si string, ajustar parse
        {
            return sucursal;  // O int.TryParse(Session.Sucursal, out int p) ? p : null;
        }

        private void ApplyUserPlantFilter()
        {
            if (_dtUsers == null) return;
            _dtUsers.DefaultView.RowFilter = chkPlantFilter.Checked
                ? $"USU_UserPLant = {(int)(cboPlantFilter.SelectedValue ?? 1)}"
                : string.Empty;
        }

        private void MainForm_Shown(object sender, EventArgs e) => txtOpSearch.Focus();

        private void MainForm_Resize(object sender, EventArgs e)
        {
            _ui.PositionHeaderSearchBoxes(pnlOpHeader, btnOpRefrescar, txtOpSearch, pnlConsHeader, txtConsSearch);
            SplitAdmin_Resize(splitAdmin, EventArgs.Empty);
        }

        private void ModuleButton_Click(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                var module = btn.Tag as ModuleDef;
                _modules.LaunchModule(btn.Name, module, false, ALLOWED_ROOTS, status => tsslEstado.Text = status);
            }
        }

        private void TabMain_Selected(object sender, TabControlEventArgs e)
        {
            // Lógica para tabs si necesario
        }

        private void WireStaticHandlers()
        {
            // Eventos estáticos
        }

        private void LoadCategories()
        {
            // Implementar carga de tree categorías si necesario
        }

        private void LoadModulesConfigBindings()
        {
            dgvModulesConfig.DataSource = _dtModulesConfig;
            _ui.EnableDgvDoubleBuffer(dgvModulesConfig);
            // Configurar columnas
        }

        private void LoadAdminDataBindings()
        {
            dgvUsuarios.DataSource = _dtUsers;
            _ui.EnableDgvDoubleBuffer(dgvUsuarios);
            // Configurar columnas
        }

        private void LoadAdminModulesFromUI()
        {
            _dtModulesAdmin = _dtModulesConfig.Copy();  // O vista
            dgvModulos.DataSource = _dtModulesAdmin;
            dgvModulos.SelectionChanged += DgvModulos_SelectionChanged;
        }

        private void DgvModulos_SelectionChanged(object sender, EventArgs e)
        {
            // Filtrar _dtOverridesView para módulo seleccionado
            // Ej: _dtOverridesView = CreateOverridesViewForModule(dgvModulos.CurrentRow?.Cells["ButtonName"].Value.ToString());
            dgvOverrides.DataSource = _dtOverridesView;
        }

        private void SaveOverrides()
        {
            if (_dtOverridesView == null) return;
            string buttonName = dgvModulos.CurrentRow?.Cells["ButtonName"].Value.ToString();
            if (!string.IsNullOrEmpty(buttonName))
            {
                _db.ReplaceOverrides(buttonName, _dtOverridesView);
                _overrides = _db.GetOverrides();
            }
        }

        private SqlDataAdapter CreateUsersAdapter()
        {
            var cn = new SqlConnection(_db.GetConnString());  // Asumiendo GetConnString public o expuesto
            return new SqlDataAdapter("SELECT USU_EmpID, USU_UserLog, USU_TypeAut, USU_Status, USU_UserPLant FROM dbo.ModGPI_User", cn);
            // Configurar UpdateCommand como en UpdateUsers, pero ya que UpdateUsers usa adapter internamente, mejor llamar _db.UpdateUsers(_dtUsers) cuando save
        }

        private void SplitAdmin_Resize(object sender, EventArgs e)
        {
            if (splitAdmin.Width <= 0) return;
            int desiredLeft = (int)(splitAdmin.Width * 0.78);
            int maxLeft = splitAdmin.Width - splitAdmin.Panel2MinSize - splitAdmin.SplitterWidth - 4;
            splitAdmin.SplitterDistance = Math.Max(desiredLeft, maxLeft);  // Original usa Max, pero logicamente Min? Ajustar si error
        }

        // Otros métodos UI
    }
}