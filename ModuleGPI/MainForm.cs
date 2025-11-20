using ModuleGPI.Data;
using ModuleGPI.Domain;
using ModuleGPI.Services;
using ModuleGPI.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        private DataTable _dtModulesConfig;
        private DataTable _dtUsers;
        private DataTable _dtOverridesView;
        private readonly ToolTip _toolTips = new ToolTip();
        private OverridesStore _overrides;
        private bool _adminCanEdit;
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

            // Inicializar servicios
            _dataAccess = new SqlDataAccess();
            _moduleService = new ModuleService(_dataAccess);
            _roleManager = new RoleManager();
            _uiHelpers = new UIHelpers();
            _overrides = new OverridesStore();

            // Configurar formulario
            SetupEventHandlers();
            SetupOverridesGrid();
            ConnectAllButtonEvents(); // NUEVO: Conectar TODOS los eventos
        }
        #endregion

        #region Form Events
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Posicionar controles
                _uiHelpers.PositionHeaderSearchBoxes(pnlOpHeader, btnOpRefrescar, txtOpSearch, pnlConsHeader, txtConsSearch);

                // Actualizar barra de estado
                UpdateStatusBar();

                // Aplicar visibilidad según rol
                _roleManager.ApplyVisibility(tabMain, tabAdmin, tabConfig, Session.TypeAut);
                _adminCanEdit = Session.TypeAut >= 5; // SysAdmin

                // Cargar datos iniciales
                LoadOverrides();
                LoadModules();

                // Cargar datos admin si tiene permisos
                if (Session.TypeAut >= 4)
                {
                    LoadAdminData();
                    LoadConfigData(); // NUEVO: Cargar módulos en Config
                }

                // Cargar categorías
                LoadCategories();

                // Conectar botones de módulos
                WireModuleButtons();

                UpdateStatus("Sistema listo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar formulario: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (txtOpSearch != null) txtOpSearch.Focus();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            _uiHelpers.PositionHeaderSearchBoxes(pnlOpHeader, btnOpRefrescar, txtOpSearch, pnlConsHeader, txtConsSearch);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show(
                    "¿Está seguro que desea salir?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                e.Cancel = (result != DialogResult.Yes);
            }
        }
        #endregion

        #region Setup Methods - CORREGIDO
        private void ConnectAllButtonEvents()
        {
            // === MENU PRINCIPAL ===
            if (mnuArchivo_Salir != null)
                mnuArchivo_Salir.Click += (s, e) => Application.Exit();

            if (mnuVer_Refrescar != null)
                mnuVer_Refrescar.Click += (s, e) => RefreshAll();

            if (mnuHerramientas_Config != null)
                mnuHerramientas_Config.Click += (s, e) => ShowConfigTab();

            if (mnuAyuda_Acerca != null)
                mnuAyuda_Acerca.Click += (s, e) => ShowAboutDialog();

            // === TOOLBAR ===
            if (tsbRefrescar != null)
                tsbRefrescar.Click += (s, e) => RefreshAll();

            if (tsbBuscar != null)
                tsbBuscar.Click += (s, e) => PerformGlobalSearch();

            if (tstBuscar != null)
                tstBuscar.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) PerformGlobalSearch(); };

            if (tsbConfig != null)
                tsbConfig.Click += (s, e) => ShowConfigTab();

            if (tsbCerrarSesion != null)
                tsbCerrarSesion.Click += (s, e) => Logout();

            // === TAB CONFIG - BOTONES DE MÓDULOS ===
            if (btnNewModule != null)
                btnNewModule.Click += BtnNewModule_Click;

            if (btnSaveModule != null)
                btnSaveModule.Click += BtnSaveModule_Click;

            if (btnDeleteModule != null)
                btnDeleteModule.Click += BtnDeleteModule_Click;

            if (btnGuardarConfig != null)
            {
                btnGuardarConfig.Enabled = _adminCanEdit;
                btnGuardarConfig.Click += (s, e) => SaveAllConfigChanges();
            }

            // === TAB ADMIN - BOTONES ===
            if (btnAdminGuardar != null)
            {
                btnAdminGuardar.Enabled = _adminCanEdit;
                btnAdminGuardar.Click += BtnAdminGuardar_Click;
            }

            if (btnAdminRefrescar != null)
                btnAdminRefrescar.Click += (s, e) => LoadAdminData();

            // === CONTEXT MENU DE MÓDULOS ===
            if (cmuAbrir != null)
                cmuAbrir.Click += (s, e) => OpenContextSelected(false);

            if (cmuAbrirAdmin != null)
                cmuAbrirAdmin.Click += (s, e) => OpenContextSelected(true);

            if (cmuCopiarRuta != null)
                cmuCopiarRuta.Click += (s, e) => CopyModulePathFromContext();

            if (cmuVerProp != null)
                cmuVerProp.Click += (s, e) => ShowModulePropertiesFromContext();
        }

        private void SetupEventHandlers()
        {
            // Tab events
            if (tabMain != null)
            {
                tabMain.Selected += TabMain_Selected;
            }

            // Search boxes
            if (txtOpSearch != null)
            {
                txtOpSearch.TextChanged += (s, e) => ApplySearch(txtOpSearch.Text, flpOperacion);
            }

            if (txtConsSearch != null)
            {
                txtConsSearch.TextChanged += (s, e) => ApplySearch(txtConsSearch.Text, flpConsultas);
            }

            // Refresh button in Operation tab
            if (btnOpRefrescar != null)
            {
                btnOpRefrescar.Click += (s, e) => RefreshModules();
            }

            // Category tree
            if (treeCategories != null)
            {
                treeCategories.AfterSelect += (s, e) => SwitchByCategory(e.Node?.Text);
            }

            // Admin grids
            if (dgvModulos != null)
            {
                dgvModulos.SelectionChanged += DgvModulos_SelectionChanged;
                dgvModulos.CellFormatting += DgvModulos_CellFormatting;
                dgvModulos.CellEndEdit += DgvModulos_CellEndEdit;
                dgvModulos.DataError += (s, e) => e.ThrowException = false;
                _uiHelpers.EnableDgvDoubleBuffer(dgvModulos);
            }

            if (dgvUsuarios != null)
            {
                dgvUsuarios.CellEndEdit += DgvUsuarios_CellEndEdit;
                dgvUsuarios.DataError += (s, e) => e.ThrowException = false;
                _uiHelpers.EnableDgvDoubleBuffer(dgvUsuarios);
            }

            // Config grid
            if (dgvModulesConfig != null)
            {
                dgvModulesConfig.SelectionChanged += DgvModulesConfig_SelectionChanged;
                dgvModulesConfig.CellDoubleClick += DgvModulesConfig_CellDoubleClick;
                dgvModulesConfig.DataError += (s, e) => e.ThrowException = false;
                _uiHelpers.EnableDgvDoubleBuffer(dgvModulesConfig);
            }

            // Plant filter in Admin
            if (chkPlantFilter != null && cboPlantFilter != null)
            {
                chkPlantFilter.CheckedChanged += (s, e) =>
                {
                    cboPlantFilter.Enabled = chkPlantFilter.Checked;
                    FilterUsersByPlant();
                };

                cboPlantFilter.SelectedIndexChanged += (s, e) => FilterUsersByPlant();
            }

            if (dgvOverrides != null)
            {
                dgvOverrides.CurrentCellDirtyStateChanged += DgvOverrides_CurrentCellDirtyStateChanged;
                dgvOverrides.CellValueChanged += DgvOverrides_CellValueChanged;
                dgvOverrides.CellFormatting += DgvOverrides_CellFormatting; // ✅ Agregar esto
                dgvOverrides.DataError += (s, e) => { e.ThrowException = false; };
                _uiHelpers.EnableDgvDoubleBuffer(dgvOverrides);
            }
        }

        private void SetupOverridesGrid()
        {
            if (dgvOverrides == null)
            {
                dgvOverrides = new DataGridView
                {
                    Name = "dgvOverrides",
                    AutoGenerateColumns = true, // ✅ Importante: dejar que genere automáticamente
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    ReadOnly = false, // Permitir edición
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    Dock = DockStyle.Fill,
                    BackgroundColor = SystemColors.Window,
                    BorderStyle = BorderStyle.None
                };

                // Eventos
                dgvOverrides.CurrentCellDirtyStateChanged += DgvOverrides_CurrentCellDirtyStateChanged;
                dgvOverrides.CellValueChanged += DgvOverrides_CellValueChanged;
                dgvOverrides.CellFormatting += DgvOverrides_CellFormatting;
                dgvOverrides.DataError += (s, e) => { e.ThrowException = false; };

                _uiHelpers.EnableDgvDoubleBuffer(dgvOverrides);

                // Insertar en el panel derecho de Admin
                if (rightAdmin != null && dgvModulos != null)
                {
                    rightAdmin.Controls.Clear();

                    var splitContainer = new SplitContainer
                    {
                        Dock = DockStyle.Fill,
                        Orientation = Orientation.Horizontal,
                        SplitterDistance = rightAdmin.Height / 2
                    };

                    // Panel superior: módulos
                    splitContainer.Panel1.Controls.Add(dgvModulos);
                    dgvModulos.Dock = DockStyle.Fill;

                    // Panel inferior: overrides
                    var lblOverrides = new Label
                    {
                        Text = "🔐 Permisos Personalizados por Usuario",
                        Dock = DockStyle.Top,
                        Height = 28,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                        Padding = new Padding(8, 0, 0, 0),
                        BackColor = SystemColors.Control
                    };

                    splitContainer.Panel2.Controls.Add(dgvOverrides);
                    splitContainer.Panel2.Controls.Add(lblOverrides);

                    rightAdmin.Controls.Add(splitContainer);
                }
            }
        }

        #endregion

        #region Tab Navigation
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

                if (e.TabPage == tabAdmin)
                    LoadAdminData();
                else if (e.TabPage == tabConfig)
                    LoadConfigData();
            }
        }

        private void ShowConfigTab()
        {
            if (Session.TypeAut >= 4)
            {
                tabMain.SelectedTab = tabConfig;
                LoadConfigData();
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a configuración.",
                    "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Module Management
        private void LoadModules()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                var dt = _moduleService.LoadModules(Session.Sucursal);
                _moduleService.PaintButtons(dt, flpOperacion, flpConsultas, cmuModulo, _toolTips,
                    (btnName, module) => _roleManager.CanSeeModule(btnName, module, Session.TypeAut,
                        Session.EmpId ?? Session.LogonName, _overrides));

                UpdateStatus($"Módulos cargados: {dt.Rows.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar módulos: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void RefreshModules()
        {
            LoadOverrides(); // Recargar overrides

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

        #region TabConfig - Module Configuration
        private void LoadConfigData()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Cargar todos los módulos en el grid de configuración
                _dtModulesConfig = _dataAccess.GetModules(null); // null = todos

                if (dgvModulesConfig != null)
                {
                    dgvModulesConfig.DataSource = _dtModulesConfig;
                    dgvModulesConfig.ReadOnly = false; // Permitir edición inline

                    // Configurar columnas
                    ConfigureModulesConfigGrid();

                    // Habilitar botones según permisos
                    bool canEdit = Session.TypeAut >= 5; // Solo SysAdmin
                    btnNewModule.Enabled = canEdit;
                    btnSaveModule.Enabled = canEdit;
                    btnDeleteModule.Enabled = canEdit;
                }

                UpdateStatus("Configuración cargada");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar configuración: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ConfigureModulesConfigGrid()
        {
            if (dgvModulesConfig == null || dgvModulesConfig.Columns.Count == 0) return;

            // Configurar columnas
            var columns = new Dictionary<string, string>
            {
                { "ButtonName", "Nombre Botón" },
                { "Name", "Nombre Módulo" },
                { "ExePath", "Ruta Ejecutable" },
                { "WorkingDir", "Directorio Trabajo" },
                { "Category", "Categoría" },
                { "RequiresElevation", "Requiere Admin" },
                { "RolesMinTypeAut", "Rol Mínimo" },
                { "Plant", "Planta" }
            };

            foreach (var col in columns)
            {
                if (dgvModulesConfig.Columns[col.Key] != null)
                {
                    dgvModulesConfig.Columns[col.Key].HeaderText = col.Value;

                    // Configurar anchos específicos
                    switch (col.Key)
                    {
                        case "ButtonName":
                            dgvModulesConfig.Columns[col.Key].Width = 120;
                            break;
                        case "Name":
                            dgvModulesConfig.Columns[col.Key].Width = 150;
                            break;
                        case "ExePath":
                            dgvModulesConfig.Columns[col.Key].Width = 300;
                            break;
                        case "WorkingDir":
                            dgvModulesConfig.Columns[col.Key].Width = 250;
                            break;
                        case "Category":
                            dgvModulesConfig.Columns[col.Key].Width = 100;
                            break;
                        case "RequiresElevation":
                            dgvModulesConfig.Columns[col.Key].Width = 80;
                            break;
                        case "RolesMinTypeAut":
                            dgvModulesConfig.Columns[col.Key].Width = 80;
                            break;
                        case "Plant":
                            dgvModulesConfig.Columns[col.Key].Width = 60;
                            break;
                    }
                }
            }
        }

        private void BtnNewModule_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new ModuleEditForm(null)) // null = nuevo módulo
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        var module = form.Module;

                        // Crear nueva fila
                        var newRow = _dtModulesConfig.NewRow();
                        newRow["ButtonName"] = module.ButtonName;
                        newRow["Name"] = module.Name;
                        newRow["ExePath"] = module.ExePath;
                        newRow["WorkingDir"] = module.WorkingDir ?? "";
                        newRow["Category"] = module.Category;
                        newRow["RequiresElevation"] = module.RequiresElevation;
                        newRow["RolesMinTypeAut"] = module.RolesMinTypeAut;
                        newRow["Plant"] = module.Plant;

                        _dtModulesConfig.Rows.Add(newRow);

                        // Guardar en BD
                        _dataAccess.UpsertModule(newRow);

                        UpdateStatus($"Módulo '{module.Name}' creado exitosamente");
                        RefreshAll();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear módulo: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSaveModule_Click(object sender, EventArgs e)
        {
            if (dgvModulesConfig?.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un módulo para editar.",
                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            EditSelectedModule();
        }

        private void BtnDeleteModule_Click(object sender, EventArgs e)
        {
            if (dgvModulesConfig?.CurrentRow == null)
            {
                MessageBox.Show("Seleccione un módulo para eliminar.",
                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var drv = dgvModulesConfig.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;

            string buttonName = Convert.ToString(drv["ButtonName"]);
            string moduleName = Convert.ToString(drv["Name"]);

            var result = MessageBox.Show(
                $"¿Está seguro de eliminar el módulo '{moduleName}'?\n\n" +
                "Esta acción no se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _dataAccess.DeleteModule(buttonName);
                    drv.Row.Delete();
                    _dtModulesConfig.AcceptChanges();

                    UpdateStatus($"Módulo '{moduleName}' eliminado");
                    RefreshAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar módulo: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void DgvModulesConfig_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                EditSelectedModule();
            }
        }

        private void DgvModulesConfig_SelectionChanged(object sender, EventArgs e)
        {
            bool hasSelection = dgvModulesConfig?.CurrentRow != null;
            if (btnSaveModule != null)
                btnSaveModule.Enabled = hasSelection && Session.TypeAut >= 5;
            if (btnDeleteModule != null)
                btnDeleteModule.Enabled = hasSelection && Session.TypeAut >= 5;
        }

        private void EditSelectedModule()
        {
            var drv = dgvModulesConfig?.CurrentRow?.DataBoundItem as DataRowView;
            if (drv == null) return;

            try
            {
                var module = new ModuleDef
                {
                    ButtonName = Convert.ToString(drv["ButtonName"]),
                    Name = Convert.ToString(drv["Name"]),
                    ExePath = Convert.ToString(drv["ExePath"]),
                    WorkingDir = Convert.ToString(drv["WorkingDir"]),
                    Category = Convert.ToString(drv["Category"]),
                    RequiresElevation = drv["RequiresElevation"] != DBNull.Value &&
                                       Convert.ToBoolean(drv["RequiresElevation"]),
                    RolesMinTypeAut = drv["RolesMinTypeAut"] == DBNull.Value ? 1 :
                                     Convert.ToInt32(drv["RolesMinTypeAut"]),
                    Plant = drv["Plant"] == DBNull.Value ? 1 : Convert.ToInt32(drv["Plant"])
                };

                using (var form = new ModuleEditForm(module))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        // Actualizar fila
                        drv["Name"] = form.Module.Name;
                        drv["ExePath"] = form.Module.ExePath;
                        drv["WorkingDir"] = form.Module.WorkingDir ?? "";
                        drv["Category"] = form.Module.Category;
                        drv["RequiresElevation"] = form.Module.RequiresElevation;
                        drv["RolesMinTypeAut"] = form.Module.RolesMinTypeAut;
                        drv["Plant"] = form.Module.Plant;

                        // Guardar en BD
                        _dataAccess.UpsertModule(drv.Row);

                        UpdateStatus($"Módulo '{form.Module.Name}' actualizado");
                        RefreshAll();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al editar módulo: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveAllConfigChanges()
        {
            try
            {
                if (_dtModulesConfig?.GetChanges() != null)
                {
                    foreach (DataRow row in _dtModulesConfig.GetChanges().Rows)
                    {
                        if (row.RowState != DataRowState.Deleted)
                        {
                            _dataAccess.UpsertModule(row);
                        }
                    }

                    _dtModulesConfig.AcceptChanges();
                    UpdateStatus("Configuración guardada exitosamente");
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show("No hay cambios pendientes para guardar.",
                        "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar configuración: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region TabAdmin - Users and Overrides
        private void LoadAdminData()
        {
            if (Session.TypeAut < 4) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // ✅ 1. Cargar módulos
                _dtModulesAdmin = _dataAccess.GetModules(null);
                dgvModulos.DataSource = null;
                dgvModulos.DataSource = _dtModulesAdmin;
                dgvModulos.ReadOnly = !_adminCanEdit;
                ConfigureModulesAdminGrid();

                // ✅ 2. Cargar usuarios (NUEVO)
                _dtUsers = _dataAccess.GetUsers();
                dgvUsuarios.DataSource = _dtUsers;
                dgvUsuarios.ReadOnly = !_adminCanEdit;
                ConfigureUsersGrid();

                // ✅ 3. Inicializar filtro de planta (NUEVO)
                LoadPlantFilter();

                // ✅ 4. Cargar overrides si hay un módulo seleccionado
                if (dgvModulos.Rows.Count > 0)
                {
                    dgvModulos.Rows[0].Selected = true;
                    var drv = dgvModulos.Rows[0].DataBoundItem as DataRowView;
                    if (drv != null)
                    {
                        string buttonName = Convert.ToString(drv["ButtonName"]);
                        BuildOverridesViewFor(buttonName);
                    }
                }

                UpdateStatus("Datos de administración cargados");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos de administración: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }
        private void ConfigureModulesAdminGrid()
        {
            if (dgvModulos == null || dgvModulos.Columns.Count == 0) return;

            // Solo configurar visibilidad y textos (seguro en cualquier momento)
            var visibleColumns = new[] { "ButtonName", "Name", "Category", "RolesMinTypeAut" };

            foreach (DataGridViewColumn col in dgvModulos.Columns)
            {
                bool show = visibleColumns.Contains(col.DataPropertyName);
                col.Visible = show;

                if (show)
                {
                    switch (col.DataPropertyName)
                    {
                        case "ButtonName":
                            col.HeaderText = "Botón";
                            col.DisplayIndex = 0;
                            break;
                        case "Name":
                            col.HeaderText = "Módulo";
                            col.DisplayIndex = 1;
                            break;
                        case "Category":
                            col.HeaderText = "Categoría";
                            col.DisplayIndex = 2;
                            break;
                        case "RolesMinTypeAut":
                            col.HeaderText = "Rol Mínimo";
                            col.DisplayIndex = 3;
                            break;
                    }
                }
            }

            if (dgvModulos.IsHandleCreated && dgvModulos.Visible)
            {
                AjustarAnchosAdmin(); 
            }
            else
            {
                
                dgvModulos.HandleCreated += (s, e) => BeginInvoke(new Action(AjustarAnchosAdmin));
                dgvModulos.VisibleChanged += (s, e) => { if (dgvModulos.Visible) BeginInvoke(new Action(AjustarAnchosAdmin)); };
            }
        }

        private void AjustarAnchosAdmin()
        {
            if (!dgvModulos.IsHandleCreated) return;

            BeginInvoke(new Action(() =>
            {
                try
                {
                    if (dgvModulos.Columns["ButtonName"] != null) dgvModulos.Columns["ButtonName"].Width = 130;
                    if (dgvModulos.Columns["Name"] != null) dgvModulos.Columns["Name"].Width = 220;
                    if (dgvModulos.Columns["Category"] != null) dgvModulos.Columns["Category"].Width = 110;
                    if (dgvModulos.Columns["RolesMinTypeAut"] != null) dgvModulos.Columns["RolesMinTypeAut"].Width = 90;

                    // Opcional: la última columna llena el espacio
                    var last = dgvModulos.Columns.Cast<DataGridViewColumn>().LastOrDefault(c => c.Visible);
                    if (last != null) last.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
                catch (Exception ex)
                {
                    // Nunca más va a explotar aquí
                    System.Diagnostics.Debug.WriteLine("Error ajustando anchos: " + ex.Message);
                }
            }));
        }

        private void ConfigureUsersGrid()
        {
            if (dgvUsuarios == null) return;

            var columns = new Dictionary<string, string>
            {
                { "USU_EmpID", "ID Empleado" },
                { "USU_UserLog", "Usuario" },
                { "USU_TypeAut", "Rol" },
                { "USU_Status", "Estado" },
                { "USU_UserPLant", "Planta" }
            };

            foreach (var col in columns)
            {
                if (dgvUsuarios.Columns[col.Key] != null)
                {
                    dgvUsuarios.Columns[col.Key].HeaderText = col.Value;
                }
            }

            // Agregar combo para roles si es editable
            if (_adminCanEdit && !dgvUsuarios.Columns.Contains("RoleCombo"))
            {
                var roleCombo = new DataGridViewComboBoxColumn
                {
                    Name = "RoleCombo",
                    HeaderText = "Rol",
                    DataPropertyName = "USU_TypeAut",
                    DataSource = new[]
                    {
                        new { Value = 1, Display = "Viewer" },
                        new { Value = 2, Display = "Operator" },
                        new { Value = 3, Display = "Supervisor" },
                        new { Value = 4, Display = "AdminDept" },
                        new { Value = 5, Display = "SysAdmin" }
                    },
                    ValueMember = "Value",
                    DisplayMember = "Display",
                    Width = 100
                };

                var index = dgvUsuarios.Columns["USU_TypeAut"].Index;
                dgvUsuarios.Columns.RemoveAt(index);
                dgvUsuarios.Columns.Insert(index, roleCombo);
            }
        }

        private void LoadPlantFilter()
        {
            if (cboPlantFilter == null) return;

            cboPlantFilter.Items.Clear();
            cboPlantFilter.Items.Add(new { Value = 0, Text = "Todas las plantas" });
            cboPlantFilter.Items.Add(new { Value = 1, Text = "Planta MTY" });
            cboPlantFilter.Items.Add(new { Value = 2, Text = "Planta GDL" });
            cboPlantFilter.Items.Add(new { Value = 3, Text = "Planta QRO" });

            cboPlantFilter.DisplayMember = "Text";
            cboPlantFilter.ValueMember = "Value";
            cboPlantFilter.SelectedIndex = 0;
            cboPlantFilter.Enabled = false;
        }

        private void FilterUsersByPlant()
        {
            if (_dtUsers == null || !chkPlantFilter.Checked)
            {
                if (dgvUsuarios != null && _dtUsers != null)
                    dgvUsuarios.DataSource = _dtUsers;
                return;
            }

            var selectedPlant = cboPlantFilter.SelectedItem as dynamic;
            if (selectedPlant?.Value == 0)
            {
                dgvUsuarios.DataSource = _dtUsers;
            }
            else if (selectedPlant != null)
            {
                var filteredView = _dtUsers.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["USU_UserPLant"]) == selectedPlant.Value)
                    .CopyToDataTable();
                dgvUsuarios.DataSource = filteredView;
            }
        }

        private void BtnAdminGuardar_Click(object sender, EventArgs e)
        {
            if (!_adminCanEdit)
            {
                MessageBox.Show("No tiene permisos para guardar cambios.",
                    "Acceso Denegado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Guardar cambios en usuarios
                if (_dtUsers?.GetChanges() != null)
                {
                    _dataAccess.UpdateUsers(_dtUsers);
                    _dtUsers.AcceptChanges();
                }

                // Guardar overrides si hay cambios
                SaveAllOverrides();

                UpdateStatus("Cambios guardados exitosamente");
                RefreshAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar cambios: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
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
            // No permitir edición en Admin, solo en Config
        }

        private void DgvModulos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (Session.TypeAut < 4 || e.RowIndex < 0) return;

            var col = dgvModulos.Columns[e.ColumnIndex].DataPropertyName;

            if (col == "RolesMinTypeAut")
            {
                int r;
                if (int.TryParse(Convert.ToString(e.Value), out r))
                {
                    e.Value = _roleManager.GetRoleName(r);
                    e.FormattingApplied = true;
                }
            }
        }

        private void DgvUsuarios_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (!_adminCanEdit || _dtUsers == null) return;

            try
            {
                UpdateStatus("Usuario modificado - Guardar para aplicar cambios");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar usuario: {ex.Message}",
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
                MessageBox.Show($"Error al cargar overrides: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _overrides = new OverridesStore();
            }
        }

        private void BuildOverridesViewFor(string buttonName)
        {
            if (_dtUsers == null || dgvOverrides == null) return;

            try
            {
                // 1. Crear tabla de datos
                _dtOverridesView = new DataTable();
                _dtOverridesView.Columns.Add("EmpId", typeof(string));
                _dtOverridesView.Columns.Add("UserName", typeof(string));
                _dtOverridesView.Columns.Add("RoleName", typeof(string));
                _dtOverridesView.Columns.Add("Override", typeof(int));

                // 2. Llenar datos
                foreach (DataRow userRow in _dtUsers.Rows)
                {
                    string empId = Convert.ToString(userRow["USU_EmpID"]);
                    string userName = Convert.ToString(userRow["USU_UserLog"]);
                    int userRole = Convert.ToInt32(userRow["USU_TypeAut"]);
                    var ov = _overrides.Get(buttonName, empId) ?? 0;
                    _dtOverridesView.Rows.Add(empId, userName, _roleManager.GetRoleName(userRole), ov);
                }

                // 3. Asignar DataSource
                dgvOverrides.DataSource = null;
                dgvOverrides.DataSource = _dtOverridesView;

                // 4. ⭐ CRÍTICO: Configurar columnas en el siguiente ciclo del mensaje
                if (dgvOverrides.IsHandleCreated)
                {
                    dgvOverrides.BeginInvoke(new Action(ConfigureOverridesColumns));
                }
                else
                {
                    // Si el handle no existe, esperar a que se cree
                    dgvOverrides.HandleCreated += (s, e) =>
                        dgvOverrides.BeginInvoke(new Action(ConfigureOverridesColumns));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al construir vista de overrides: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Método auxiliar para configurar columnas de forma segura
        private void ConfigureOverridesColumns()
        {
            // Validación defensiva
            if (dgvOverrides == null || !dgvOverrides.IsHandleCreated)
            {
                Debug.WriteLine("[ConfigureOverridesColumns] ❌ Control no inicializado");
                return;
            }

            if (dgvOverrides.Columns.Count == 0)
            {
                Debug.WriteLine("[ConfigureOverridesColumns] ❌ No hay columnas");
                return;
            }

            try
            {
                Debug.WriteLine($"[ConfigureOverridesColumns] ✅ Iniciando - Columnas: {dgvOverrides.Columns.Count}");

                // Suspender layout
                dgvOverrides.SuspendLayout();

                // Listar columnas existentes
                foreach (DataGridViewColumn col in dgvOverrides.Columns)
                {
                    Debug.WriteLine($"  📋 Columna: {col.Name}, Index: {col.Index}, Width: {col.Width}");
                }

                // 1. Configurar EmpId (con validación extra)
                var empIdCol = dgvOverrides.Columns["EmpId"];
                if (empIdCol != null)
                {
                    empIdCol.HeaderText = "ID";
                    empIdCol.ReadOnly = true;
                    empIdCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    empIdCol.Width = 80; // ⚠️ Ahora sí es seguro
                    Debug.WriteLine("  ✅ Columna EmpId configurada");
                }

                // 2. Configurar UserName
                var userNameCol = dgvOverrides.Columns["UserName"];
                if (userNameCol != null)
                {
                    userNameCol.HeaderText = "Usuario";
                    userNameCol.ReadOnly = true;
                    userNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    userNameCol.Width = 140;
                    Debug.WriteLine("  ✅ Columna UserName configurada");
                }

                // 3. Configurar RoleName
                var roleNameCol = dgvOverrides.Columns["RoleName"];
                if (roleNameCol != null)
                {
                    roleNameCol.HeaderText = "Rol Base";
                    roleNameCol.ReadOnly = true;
                    roleNameCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    roleNameCol.Width = 100;
                    Debug.WriteLine("  ✅ Columna RoleName configurada");
                }

                // 4. Reemplazar columna Override con ComboBox
                var overrideCol = dgvOverrides.Columns["Override"];
                if (overrideCol != null)
                {
                    int overrideIndex = overrideCol.Index;
                    dgvOverrides.Columns.Remove(overrideCol); // Usar Remove en vez de RemoveAt

                    var comboCol = new DataGridViewComboBoxColumn
                    {
                        Name = "Override",
                        HeaderText = "Permiso",
                        DataPropertyName = "Override",
                        DataSource = new[]
                        {
                    new { Value = -1, Display = "❌ Denegar" },
                    new { Value = 0, Display = "⚪ Heredado" },
                    new { Value = 1, Display = "✅ Permitir" }
                },
                        ValueMember = "Value",
                        DisplayMember = "Display",
                        AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                        Width = 120,
                        ReadOnly = !_adminCanEdit,
                        DisplayStyle = DataGridViewComboBoxDisplayStyle.Nothing
                    };

                    dgvOverrides.Columns.Insert(overrideIndex, comboCol);
                    Debug.WriteLine("  ✅ Columna Override configurada (ComboBox)");
                }

                // 5. Configuración general
                dgvOverrides.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                dgvOverrides.AllowUserToAddRows = false;
                dgvOverrides.AllowUserToDeleteRows = false;
                dgvOverrides.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgvOverrides.MultiSelect = false;
                dgvOverrides.RowHeadersVisible = false;
                dgvOverrides.ReadOnly = !_adminCanEdit;

                // Reanudar layout
                dgvOverrides.ResumeLayout();

                Debug.WriteLine("[ConfigureOverridesColumns] ✅ Configuración completada");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigureOverridesColumns] ❌ Error: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error al configurar columnas de overrides:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
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

            UpdateStatus("Override modificado - Guardar para aplicar cambios");
        }

        private void ApplyOneOverrideFromRow(string buttonName, int rowIndex)
        {
            if (_dtOverridesView == null || rowIndex >= _dtOverridesView.Rows.Count) return;

            var row = _dtOverridesView.Rows[rowIndex];
            string empId = Convert.ToString(row["EmpId"]);
            int overrideValue = Convert.ToInt32(row["Override"]);

            _overrides.Set(buttonName, empId, overrideValue);
        }

        private void SaveAllOverrides()
        {
            if (!_adminCanEdit || dgvModulos?.CurrentRow == null) return;

            var drv = dgvModulos.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;

            string buttonName = Convert.ToString(drv["ButtonName"]);

            if (_dtOverridesView != null)
            {
                _dataAccess.ReplaceOverrides(buttonName, _dtOverridesView);
            }
        }

        private void DgvOverrides_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || dgvOverrides.Columns[e.ColumnIndex].Name != "Override")
                return;

            try
            {
                var cellValue = dgvOverrides.Rows[e.RowIndex].Cells["Override"].Value;
                if (cellValue == null || cellValue == DBNull.Value) return;

                int overrideValue = Convert.ToInt32(cellValue);
                var row = dgvOverrides.Rows[e.RowIndex];

                switch (overrideValue)
                {
                    case 1: // Permitir
                        row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230);
                        row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                        break;
                    case -1: // Denegar
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                        break;
                    default: // Heredado (0)
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                        break;
                }
            }
            catch
            {
                // Ignorar errores de formateo
            }
        }
        #endregion

        #region Context Menu
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

                MessageBox.Show(info, "Propiedades del Módulo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region UI Helpers
        private void UpdateStatusBar()
        {
            if (tsslUser != null)
                tsslUser.Text = $"Usuario: {Session.LogonName}";
            if (tsslRole != null)
                tsslRole.Text = $"Rol: {_roleManager.GetRoleName(Session.TypeAut)}";
            if (tsslPlant != null)
                tsslPlant.Text = $"Planta: {Session.Sucursal}";
            if (tsslEstado != null)
                tsslEstado.Text = "Listo";
        }

        private void UpdateStatus(string message)
        {
            if (tsslEstado != null)
            {
                tsslEstado.Text = message;
                Application.DoEvents(); // Actualizar UI inmediatamente
            }
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

        private void PerformGlobalSearch()
        {
            string searchText = tstBuscar?.Text ?? "";

            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("Ingrese un término de búsqueda.",
                    "Búsqueda", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Buscar en ambos paneles
            ApplySearch(searchText, flpOperacion);
            ApplySearch(searchText, flpConsultas);

            // Cambiar a la pestaña apropiada si hay resultados
            int opCount = flpOperacion.Controls.OfType<Button>().Count(b => b.Visible);
            int consCount = flpConsultas.Controls.OfType<Button>().Count(b => b.Visible);

            if (opCount > 0)
                tabMain.SelectedTab = tabOperacion;
            else if (consCount > 0)
                tabMain.SelectedTab = tabConsultas;

            UpdateStatus($"Búsqueda: {opCount + consCount} módulos encontrados");
        }

        private void RefreshAll()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                LoadOverrides();
                LoadModules();

                if (Session.TypeAut >= 4)
                {
                    if (tabMain.SelectedTab == tabAdmin)
                        LoadAdminData();
                    else if (tabMain.SelectedTab == tabConfig)
                        LoadConfigData();
                }

                UpdateStatus("Sistema actualizado");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al refrescar: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ShowAboutDialog()
        {
            string info = "Sistema de Lanzamiento de Módulos GPI\n\n" +
                         "Versión: 2.0.0\n" +
                         "Desarrollado por: IT Department\n" +
                         "© 2024 Graphic Packaging International\n\n" +
                         "Para soporte técnico contacte a:\n" +
                         "soporte.it@graphicpkg.com";

            MessageBox.Show(info, "Acerca de ModuleGPI",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Public Methods
        public void Logout()
        {
            var result = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }
        #endregion
    }
}