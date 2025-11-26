using ModuleGPI.Data;
using ModuleGPI.Domain;
using ModuleGPI.Services;
using ModuleGPI.UI;
using System;
using System.Data;
using System.Diagnostics;
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

            _dataAccess = new SqlDataAccess();
            _moduleService = new ModuleService(_dataAccess);
            _roleManager = new RoleManager();
            _uiHelpers = new UIHelpers();
            _overrides = new OverridesStore();

            this.Load += MainForm_Load;
            this.Shown += MainForm_Shown;
            this.Resize += MainForm_Resize;
            this.FormClosing += MainForm_FormClosing;

            SetupEventHandlers();
            SetupOverridesGrid();
            ConnectAllButtonEvents();
        }
        #endregion

        #region Form Events
        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                _uiHelpers.PositionHeaderSearchBoxes(pnlOpHeader, btnOpRefrescar, txtOpSearch, pnlConsHeader, txtConsSearch);

                // ✅ Actualizar status bar con datos de sesión
                UpdateStatusBar();

                // ✅ Aplicar visibilidad según rol
                _roleManager.ApplyVisibility(tabMain, tabAdmin, tabConfig, Session.TypeAut);
                _adminCanEdit = Session.TypeAut >= 5;

                // Cargar datos iniciales
                LoadOverrides();
                LoadModules();

                // Cargar datos admin si tiene permisos
                if (Session.TypeAut >= 4)
                {
                    LoadConfigData();
                }

                LoadCategories();
                WireModuleButtons();

                UpdateStatus("Sistema listo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar formulario: {ex.Message}\n\n{ex.StackTrace}",
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
                var result = MessageBox.Show("¿Está seguro que desea salir?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                e.Cancel = (result != DialogResult.Yes);
            }
        }
        #endregion

        #region Setup Methods
        private void ConnectAllButtonEvents()
        {
            if (mnuArchivo_Salir != null)
                mnuArchivo_Salir.Click += (s, e) => Application.Exit();

            if (mnuVer_Refrescar != null)
                mnuVer_Refrescar.Click += (s, e) => RefreshAll();

            if (mnuHerramientas_Config != null)
                mnuHerramientas_Config.Click += (s, e) => ShowConfigTab();

            if (mnuAyuda_Acerca != null)
                mnuAyuda_Acerca.Click += (s, e) => ShowAboutDialog();

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

            if (btnAdminGuardar != null)
            {
                btnAdminGuardar.Enabled = _adminCanEdit;
                btnAdminGuardar.Click += BtnAdminGuardar_Click;
            }

            if (btnAdminRefrescar != null)
                btnAdminRefrescar.Click += (s, e) => LoadAdminData();

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
            if (tabMain != null)
                tabMain.Selected += TabMain_Selected;

            if (txtOpSearch != null)
                txtOpSearch.TextChanged += (s, e) => ApplySearch(txtOpSearch.Text, flpOperacion);

            if (txtConsSearch != null)
                txtConsSearch.TextChanged += (s, e) => ApplySearch(txtConsSearch.Text, flpConsultas);

            if (btnOpRefrescar != null)
                btnOpRefrescar.Click += (s, e) => RefreshModules();

            if (treeCategories != null)
                treeCategories.AfterSelect += (s, e) => SwitchByCategory(e.Node?.Text);

            if (dgvModulos != null)
            {
                dgvModulos.SelectionChanged += DgvModulos_SelectionChanged;
                dgvModulos.CellFormatting += DgvModulos_CellFormatting;
                dgvModulos.DataError += (s, e) => e.ThrowException = false;
                _uiHelpers.EnableDgvDoubleBuffer(dgvModulos);
            }

            if (dgvUsuarios != null)
            {
                dgvUsuarios.CellValueChanged += DgvUsuarios_CellValueChanged;
                dgvUsuarios.CurrentCellDirtyStateChanged += DgvUsuarios_CurrentCellDirtyStateChanged;
                dgvUsuarios.CellEndEdit += DgvUsuarios_CellEndEdit;
                dgvUsuarios.CellContentClick += DgvUsuarios_CellContentClick;

                //dgvUsuarios.CellClick += DgvUsuarios_CellClick;

                dgvUsuarios.DataError += (s, e) => e.ThrowException = false;
                _uiHelpers.EnableDgvDoubleBuffer(dgvUsuarios);
            }

            if (dgvModulesConfig != null)
            {
                dgvModulesConfig.SelectionChanged += DgvModulesConfig_SelectionChanged;
                dgvModulesConfig.CellDoubleClick += DgvModulesConfig_CellDoubleClick;
                dgvModulesConfig.DataError += (s, e) => e.ThrowException = false;
                _uiHelpers.EnableDgvDoubleBuffer(dgvModulesConfig);
            }

            if (chkPlantFilter != null && cboPlantFilter != null)
            {
                chkPlantFilter.CheckedChanged += (s, e) =>
                {
                    cboPlantFilter.Enabled = chkPlantFilter.Checked;
                    FilterUsersByPlant();
                };

                cboPlantFilter.SelectedIndexChanged += (s, e) => FilterUsersByPlant();
            }
        }





        private void DgvUsuarios_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var columnName = dgvUsuarios.Columns[e.ColumnIndex].Name;

            // ✅ Si es un checkbox de planta, hacer cambio inmediato
            if (columnName == "MTY_Access" || columnName == "QRO_Access" || columnName == "TIJ_Access")
            {
                try
                {
                    // ✅ PASO 1: Obtener el valor actual
                    var currentCell = dgvUsuarios[e.ColumnIndex, e.RowIndex];
                    var currentValue = currentCell.Value;

                    // ✅ PASO 2: Calcular el nuevo valor (invertir)
                    bool newValue;
                    if (currentValue == null || currentValue == DBNull.Value)
                    {
                        newValue = true;  // Si es null, marcar como true
                    }
                    else
                    {
                        newValue = !Convert.ToBoolean(currentValue);
                    }

                    // ✅ PASO 3: Asignar el nuevo valor directamente
                    currentCell.Value = newValue;

                    // ✅ PASO 4: Forzar commit del cambio
                    dgvUsuarios.CommitEdit(DataGridViewDataErrorContexts.Commit);

                    // ✅ PASO 5: Refrescar la celda para que se vea el cambio
                    dgvUsuarios.RefreshEdit();

                    // ✅ PASO 6: Marcar que hay cambios pendientes
                    UpdateStatus($"⚠️ Acceso a {columnName.Replace("_Access", "")} modificado - Presione GUARDAR para aplicar");

                    if (btnAdminGuardar != null && _adminCanEdit)
                    {
                        btnAdminGuardar.Enabled = true;
                        btnAdminGuardar.BackColor = Color.FromArgb(255, 235, 180);
                    }

                    // ✅ PASO 7: Log para debugging (temporal)
                    Debug.WriteLine($"Checkbox {columnName} cambiado a: {newValue} para fila {e.RowIndex}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Error en CellContentClick: {ex.Message}");
                    MessageBox.Show($"Error al cambiar checkbox: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void DgvUsuarios_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvUsuarios.IsCurrentCellDirty)
            {
                // Commit inmediatamente el cambio
                dgvUsuarios.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        // ✅ NUEVO: Handler para cuando cambia el valor de una celda
        private void DgvUsuarios_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                // Marcar que hay cambios pendientes
                UpdateStatus("⚠️ Cambios sin guardar - Presione GUARDAR para aplicar");

                // ✅ Asegurar que el botón Guardar esté habilitado
                if (btnAdminGuardar != null && _adminCanEdit)
                {
                    btnAdminGuardar.Enabled = true;
                    btnAdminGuardar.BackColor = Color.FromArgb(255, 235, 180); // Color de advertencia
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en CellValueChanged: {ex.Message}");
            }
        }

        private void SetupOverridesGrid()
        {
            if (dgvOverrides == null)
            {
                dgvOverrides = new DataGridView
                {
                    Name = "dgvOverrides",
                    AutoGenerateColumns = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    ReadOnly = false,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.CellSelect,
                    MultiSelect = false,
                    Dock = DockStyle.Fill,
                    BackgroundColor = SystemColors.Window,
                    BorderStyle = BorderStyle.None,
                    EditMode = DataGridViewEditMode.EditOnEnter
                };

                dgvOverrides.CurrentCellDirtyStateChanged += DgvOverrides_CurrentCellDirtyStateChanged;
                dgvOverrides.CellValueChanged += DgvOverrides_CellValueChanged;
                dgvOverrides.CellFormatting += DgvOverrides_CellFormatting;
                dgvOverrides.DataError += (s, e) => { e.ThrowException = false; };

                _uiHelpers.EnableDgvDoubleBuffer(dgvOverrides);

                if (rightAdmin != null && dgvModulos != null)
                {
                    rightAdmin.Controls.Clear();

                    var splitContainer = new SplitContainer
                    {
                        Dock = DockStyle.Fill,
                        Orientation = Orientation.Horizontal,
                        SplitterDistance = rightAdmin.Height / 2
                    };

                    splitContainer.Panel1.Controls.Add(dgvModulos);
                    dgvModulos.Dock = DockStyle.Fill;

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
            if (e.TabPage == tabAdmin)
                LoadAdminData();
            else if (e.TabPage == tabConfig)
                LoadConfigData();
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
            LoadOverrides();
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
            // ✅ Funciona tanto para Button como para ModuleButton (ambos heredan de Control)
            if (sender is Control ctrl && ctrl.Tag is ModuleDef m)
            {
                _moduleService.LaunchModule(ctrl.Name, m, false, ALLOWED_ROOTS, UpdateStatus);
            }
        }
        #endregion

        #region TabConfig - Module Configuration
        private void LoadConfigData()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                _dtModulesConfig = _dataAccess.GetModules(null);

                // ✅ FORZAR que IconPath NO sea ReadOnly
                if (_dtModulesConfig.Columns.Contains("IconPath"))
                {
                    _dtModulesConfig.Columns["IconPath"].ReadOnly = false;
                }

                if (dgvModulesConfig != null)
                {
                    dgvModulesConfig.DataSource = _dtModulesConfig;
                    dgvModulesConfig.ReadOnly = false;

                    ConfigureModulesConfigGrid();

                    bool canEdit = Session.TypeAut >= 5;
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

            try
            {
                var columns = new System.Collections.Generic.Dictionary<string, string>
        {
            { "ButtonName", "Nombre Botón" },
            { "Name", "Nombre Módulo" },
            { "ExePath", "Ruta Ejecutable" },
            { "WorkingDir", "Directorio Trabajo" },
           // { "Arguments", "Argumentos" },
            { "IconPath", "Ruta Icono" },
            { "Category", "Categoría" },
            { "RequiresElevation", "Requiere Admin" },
            { "RolesMinTypeAut", "Rol Mínimo" },
            { "Plant", "Planta" }
        };

                // Paso 1: Solo configurar nombres (SEGURO)
                foreach (var col in columns)
                {
                    if (dgvModulesConfig.Columns[col.Key] != null)
                    {
                        dgvModulesConfig.Columns[col.Key].HeaderText = col.Value;
                    }
                }

                // Ocultar CreatedDate
                if (dgvModulesConfig.Columns["CreatedDate"] != null)
                    dgvModulesConfig.Columns["CreatedDate"].Visible = false;

                // Paso 2: Configurar anchos de forma diferida
                if (dgvModulesConfig.IsHandleCreated && dgvModulesConfig.Visible)
                {
                    AjustarAnchosModulosConfig();
                }
                else
                {
                    dgvModulesConfig.HandleCreated += (s, e) =>
                        dgvModulesConfig.BeginInvoke(new Action(AjustarAnchosModulosConfig));

                    dgvModulesConfig.VisibleChanged += (s, e) =>
                    {
                        if (dgvModulesConfig.Visible)
                            dgvModulesConfig.BeginInvoke(new Action(AjustarAnchosModulosConfig));
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error configurando grid de módulos config: {ex.Message}");
            }
        }

        private void AjustarAnchosModulosConfig()
        {
            if (dgvModulesConfig == null || !dgvModulesConfig.IsHandleCreated) return;

            try
            {
                dgvModulesConfig.SuspendLayout();

                var widths = new System.Collections.Generic.Dictionary<string, int>
        {
            { "ButtonName", 120 },
            { "Name", 150 },
            { "ExePath", 300 },
            { "WorkingDir", 250 },
            { "Arguments", 150 },
            { "IconPath", 250 }, // ✅ AGREGAR
            { "Category", 100 },
            { "RequiresElevation", 80 },
            { "RolesMinTypeAut", 80 },
            { "Plant", 60 }
        };

                foreach (var kvp in widths)
                {
                    if (dgvModulesConfig.Columns[kvp.Key] != null)
                    {
                        dgvModulesConfig.Columns[kvp.Key].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        dgvModulesConfig.Columns[kvp.Key].Width = kvp.Value;
                    }
                }

                dgvModulesConfig.ResumeLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ajustando anchos de columnas config: {ex.Message}");
            }
        }

        private void BtnNewModule_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new ModuleEditForm(null))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        var module = form.Module;

                        var newRow = _dtModulesConfig.NewRow();
                        newRow["ButtonName"] = module.ButtonName;
                        newRow["Name"] = module.Name;
                        newRow["ExePath"] = module.ExePath;
                        newRow["WorkingDir"] = string.IsNullOrEmpty(module.WorkingDir) ? "" : module.WorkingDir;
                       // newRow["Arguments"] = string.IsNullOrEmpty(module.Arguments) ? "" : module.Arguments;
                        newRow["IconPath"] = string.IsNullOrEmpty(module.IconPath) ? "" : module.IconPath; // ✅ AGREGAR
                        newRow["Category"] = module.Category;
                        newRow["RequiresElevation"] = module.RequiresElevation;
                        newRow["RolesMinTypeAut"] = module.RolesMinTypeAut;
                        newRow["Plant"] = module.Plant;

                        _dtModulesConfig.Rows.Add(newRow);
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
                $"¿Está seguro de eliminar el módulo '{moduleName}'?\n\nEsta acción no se puede deshacer.",
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
                   // Arguments = Convert.ToString(drv["Arguments"]),
                    IconPath = Convert.ToString(drv["IconPath"]), // ✅ LEER IconPath
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
                        drv["Name"] = form.Module.Name;
                        drv["ExePath"] = form.Module.ExePath;
                        drv["WorkingDir"] = form.Module.WorkingDir ?? "";
                       // drv["Arguments"] = form.Module.Arguments ?? "";
                        drv["IconPath"] = form.Module.IconPath ?? ""; // ✅ GUARDAR IconPath
                        drv["Category"] = form.Module.Category;
                        drv["RequiresElevation"] = form.Module.RequiresElevation;
                        drv["RolesMinTypeAut"] = form.Module.RolesMinTypeAut;
                        drv["Plant"] = form.Module.Plant;

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

                // ✅ FORZAR LAYOUT CORRECTO
                if (grpRoles != null && barFiltroPlanta != null && dgvUsuarios != null)
                {
                    grpRoles.SuspendLayout();

                    // Remover controles temporalmente
                    grpRoles.Controls.Clear();

                    // Configurar filtro
                    barFiltroPlanta.Dock = DockStyle.Top;
                    barFiltroPlanta.Height = 40;

                    // Configurar grid
                    dgvUsuarios.Dock = DockStyle.Fill;

                    // Agregar en orden correcto
                    grpRoles.Controls.Add(dgvUsuarios);      // Primero el que va Fill
                    grpRoles.Controls.Add(barFiltroPlanta);  // Después el que va Top

                    grpRoles.ResumeLayout();
                }

                if (dgvUsuarios.Columns.Contains("MTY_Access"))
                    dgvUsuarios.Columns["MTY_Access"].ReadOnly = false;

                if (dgvUsuarios.Columns.Contains("QRO_Access"))
                    dgvUsuarios.Columns["QRO_Access"].ReadOnly = false;

                if (dgvUsuarios.Columns.Contains("TIJ_Access"))
                    dgvUsuarios.Columns["TIJ_Access"].ReadOnly = false;

                Debug.WriteLine("✅ LoadAdminData: Forzado ReadOnly=false en checkboxes");

                // Cargar módulos
                _dtModulesAdmin = _dataAccess.GetModules(null);
                dgvModulos.DataSource = _dtModulesAdmin;
                dgvModulos.ReadOnly = true;
                ConfigureModulesAdminGrid();

                // Cargar usuarios
                _dtUsers = _dataAccess.GetUsers();
                dgvUsuarios.DataSource = _dtUsers;
                ConfigureUsersGrid();

              //  DiagnosticarCheckbox();

                if (btnAdminGuardar != null)
                {
                    btnAdminGuardar.Enabled = _adminCanEdit;
                }

                LoadPlantFilter();

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

            try
            {
                var visibleColumns = new[] { "ButtonName", "Name", "Category", "RolesMinTypeAut" };

                // Paso 1: Solo configurar visibilidad y textos (SEGURO)
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
                                break;
                            case "Name":
                                col.HeaderText = "Módulo";
                                break;
                            case "Category":
                                col.HeaderText = "Categoría";
                                break;
                            case "RolesMinTypeAut":
                                col.HeaderText = "Rol Mínimo";
                                break;
                        }
                    }
                }

                // Paso 2: Configurar anchos de forma diferida (SEGURO)
                if (dgvModulos.IsHandleCreated && dgvModulos.Visible)
                {
                    AjustarAnchosModulosAdmin();
                }
                else
                {
                    // Esperar a que el control esté completamente inicializado
                    dgvModulos.HandleCreated += (s, e) =>
                        dgvModulos.BeginInvoke(new Action(AjustarAnchosModulosAdmin));

                    dgvModulos.VisibleChanged += (s, e) =>
                    {
                        if (dgvModulos.Visible)
                            dgvModulos.BeginInvoke(new Action(AjustarAnchosModulosAdmin));
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error configurando grid de módulos admin: {ex.Message}");
            }
        }

        private void AjustarAnchosModulosAdmin()
        {
            if (dgvModulos == null || !dgvModulos.IsHandleCreated) return;

            try
            {
                dgvModulos.SuspendLayout();

                if (dgvModulos.Columns["ButtonName"] != null)
                {
                    dgvModulos.Columns["ButtonName"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvModulos.Columns["ButtonName"].Width = 130;
                }

                if (dgvModulos.Columns["Name"] != null)
                {
                    dgvModulos.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                if (dgvModulos.Columns["Category"] != null)
                {
                    dgvModulos.Columns["Category"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvModulos.Columns["Category"].Width = 110;
                }

                if (dgvModulos.Columns["RolesMinTypeAut"] != null)
                {
                    dgvModulos.Columns["RolesMinTypeAut"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvModulos.Columns["RolesMinTypeAut"].Width = 90;
                }

                dgvModulos.ResumeLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ajustando anchos de columnas: {ex.Message}");
            }
        }

        private void ConfigureUsersGrid()
        {
            if (dgvUsuarios == null || dgvUsuarios.Columns.Count == 0)
            {
                Debug.WriteLine("⚠️ ConfigureUsersGrid: Grid vacío");
                return;
            }

            try
            {
                dgvUsuarios.SuspendLayout();

                // ✅ Configuración general del grid
                dgvUsuarios.ReadOnly = false;
                dgvUsuarios.AllowUserToAddRows = false;
                dgvUsuarios.AllowUserToDeleteRows = false;
                dgvUsuarios.EditMode = DataGridViewEditMode.EditOnEnter;
                dgvUsuarios.SelectionMode = DataGridViewSelectionMode.CellSelect;

                // ========================================
                // COLUMNAS SIMPLES (ReadOnly)
                // ========================================
                if (dgvUsuarios.Columns["USU_EmpID"] != null)
                {
                    dgvUsuarios.Columns["USU_EmpID"].HeaderText = "ID Empleado";
                    dgvUsuarios.Columns["USU_EmpID"].ReadOnly = true;
                }

                if (dgvUsuarios.Columns["USU_UserLog"] != null)
                {
                    dgvUsuarios.Columns["USU_UserLog"].HeaderText = "Usuario";
                    dgvUsuarios.Columns["USU_UserLog"].ReadOnly = true;
                }

                // ========================================
                // COMBOBOXES (Reemplazar columnas)
                // ========================================

                // ✅ TypeAut - ComboBox
                if (dgvUsuarios.Columns["USU_TypeAut"] != null)
                {
                    int roleIndex = dgvUsuarios.Columns["USU_TypeAut"].Index;
                    dgvUsuarios.Columns.RemoveAt(roleIndex);

                    var roleCombo = new DataGridViewComboBoxColumn
                    {
                        Name = "USU_TypeAut",
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
                        ReadOnly = false,
                        DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
                    };

                    dgvUsuarios.Columns.Insert(roleIndex, roleCombo);
                    Debug.WriteLine("✅ ComboBox TypeAut creado");
                }

                // ✅ Status - ComboBox
                if (dgvUsuarios.Columns["USU_Status"] != null)
                {
                    int statusIndex = dgvUsuarios.Columns["USU_Status"].Index;
                    dgvUsuarios.Columns.RemoveAt(statusIndex);

                    var statusCombo = new DataGridViewComboBoxColumn
                    {
                        Name = "USU_Status",
                        HeaderText = "Estado",
                        DataPropertyName = "USU_Status",
                        DataSource = new[]
                        {
                    new { Value = 0, Display = "Inactivo" },
                    new { Value = 1, Display = "Activo" }
                },
                        ValueMember = "Value",
                        DisplayMember = "Display",
                        ReadOnly = false,
                        DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
                    };

                    dgvUsuarios.Columns.Insert(statusIndex, statusCombo);
                    Debug.WriteLine("✅ ComboBox Status creado");
                }

                // ✅ UserPLant - ComboBox
                if (dgvUsuarios.Columns["USU_UserPLant"] != null)
                {
                    int plantIndex = dgvUsuarios.Columns["USU_UserPLant"].Index;
                    dgvUsuarios.Columns.RemoveAt(plantIndex);

                    var plantCombo = new DataGridViewComboBoxColumn
                    {
                        Name = "USU_UserPLant",
                        HeaderText = "Planta",
                        DataPropertyName = "USU_UserPLant",
                        DataSource = new[]
                        {
                    new { Value = 1, Display = "MTY" },
                    new { Value = 2, Display = "QRO" },
                    new { Value = 3, Display = "TIJ" }
                },
                        ValueMember = "Value",
                        DisplayMember = "Display",
                        ReadOnly = false,
                        DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox
                    };

                    dgvUsuarios.Columns.Insert(plantIndex, plantCombo);
                    Debug.WriteLine("✅ ComboBox UserPLant creado");
                }

                // ========================================
                // ✅ CHECKBOXES - CREAR SI NO EXISTEN
                // ========================================

                // ✅ MTY_Access
                if (dgvUsuarios.Columns.Contains("MTY_Access") && dgvUsuarios.Columns["MTY_Access"] != null)
                {
                    // La columna existe, reemplazarla
                    int mtyIndex = dgvUsuarios.Columns["MTY_Access"].Index;
                    dgvUsuarios.Columns.RemoveAt(mtyIndex);

                    var mtyCheckbox = new DataGridViewCheckBoxColumn
                    {
                        Name = "MTY_Access",
                        HeaderText = "✓ MTY",
                        DataPropertyName = "MTY_Access",
                        TrueValue = true,
                        FalseValue = false,
                        ReadOnly = false,
                        Width = 60
                    };

                    dgvUsuarios.Columns.Insert(mtyIndex, mtyCheckbox);
                    Debug.WriteLine("✅ Checkbox MTY_Access creado");
                }
                else
                {
                    // La columna NO existe, agregarla al final
                    var mtyCheckbox = new DataGridViewCheckBoxColumn
                    {
                        Name = "MTY_Access",
                        HeaderText = "✓ MTY",
                        DataPropertyName = "MTY_Access",
                        TrueValue = true,
                        FalseValue = false,
                        ReadOnly = false,
                        Width = 60
                    };

                    dgvUsuarios.Columns.Add(mtyCheckbox);
                    Debug.WriteLine("⚠️ Checkbox MTY_Access NO existía, se agregó al final");
                }

                // ✅ QRO_Access
                if (dgvUsuarios.Columns.Contains("QRO_Access") && dgvUsuarios.Columns["QRO_Access"] != null)
                {
                    int qroIndex = dgvUsuarios.Columns["QRO_Access"].Index;
                    dgvUsuarios.Columns.RemoveAt(qroIndex);

                    var qroCheckbox = new DataGridViewCheckBoxColumn
                    {
                        Name = "QRO_Access",
                        HeaderText = "✓ QRO",
                        DataPropertyName = "QRO_Access",
                        TrueValue = true,
                        FalseValue = false,
                        ReadOnly = false,
                        Width = 60
                    };

                    dgvUsuarios.Columns.Insert(qroIndex, qroCheckbox);
                    Debug.WriteLine("✅ Checkbox QRO_Access creado");
                }
                else
                {
                    var qroCheckbox = new DataGridViewCheckBoxColumn
                    {
                        Name = "QRO_Access",
                        HeaderText = "✓ QRO",
                        DataPropertyName = "QRO_Access",
                        TrueValue = true,
                        FalseValue = false,
                        ReadOnly = false,
                        Width = 60
                    };

                    dgvUsuarios.Columns.Add(qroCheckbox);
                    Debug.WriteLine("⚠️ Checkbox QRO_Access NO existía, se agregó al final");
                }

                // ✅ TIJ_Access
                if (dgvUsuarios.Columns.Contains("TIJ_Access") && dgvUsuarios.Columns["TIJ_Access"] != null)
                {
                    int tijIndex = dgvUsuarios.Columns["TIJ_Access"].Index;
                    dgvUsuarios.Columns.RemoveAt(tijIndex);

                    var tijCheckbox = new DataGridViewCheckBoxColumn
                    {
                        Name = "TIJ_Access",
                        HeaderText = "✓ TIJ",
                        DataPropertyName = "TIJ_Access",
                        TrueValue = true,
                        FalseValue = false,
                        ReadOnly = false,
                        Width = 60
                    };

                    dgvUsuarios.Columns.Insert(tijIndex, tijCheckbox);
                    Debug.WriteLine("✅ Checkbox TIJ_Access creado");
                }
                else
                {
                    var tijCheckbox = new DataGridViewCheckBoxColumn
                    {
                        Name = "TIJ_Access",
                        HeaderText = "✓ TIJ",
                        DataPropertyName = "TIJ_Access",
                        TrueValue = true,
                        FalseValue = false,
                        ReadOnly = false,
                        Width = 60
                    };

                    dgvUsuarios.Columns.Add(tijCheckbox);
                    Debug.WriteLine("⚠️ Checkbox TIJ_Access NO existía, se agregó al final");
                }

                dgvUsuarios.ResumeLayout();

                // ========================================
                // ✅ AJUSTAR ANCHOS DE COLUMNAS
                // ========================================
                if (dgvUsuarios.IsHandleCreated && dgvUsuarios.Visible)
                {
                    AjustarAnchosUsuarios();
                }
                else
                {
                    // Si el grid no está visible aún, diferir el ajuste
                    EventHandler handlerCreated = null;
                    EventHandler handlerVisible = null;

                    handlerCreated = (s, e) =>
                    {
                        dgvUsuarios.HandleCreated -= handlerCreated;
                        dgvUsuarios.BeginInvoke(new Action(AjustarAnchosUsuarios));
                    };

                    handlerVisible = (s, e) =>
                    {
                        if (dgvUsuarios.Visible)
                        {
                            dgvUsuarios.VisibleChanged -= handlerVisible;
                            dgvUsuarios.BeginInvoke(new Action(AjustarAnchosUsuarios));
                        }
                    };

                    dgvUsuarios.HandleCreated += handlerCreated;
                    dgvUsuarios.VisibleChanged += handlerVisible;
                }

                Debug.WriteLine("✅ ConfigureUsersGrid completado exitosamente");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ ERROR en ConfigureUsersGrid: {ex.Message}");
                Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                MessageBox.Show($"Error configurando grid: {ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void AjustarAnchosUsuarios()
        {
            if (dgvUsuarios == null || !dgvUsuarios.IsHandleCreated) return;

            try
            {
                dgvUsuarios.SuspendLayout();

                if (dgvUsuarios.Columns["USU_EmpID"] != null)
                {
                    dgvUsuarios.Columns["USU_EmpID"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvUsuarios.Columns["USU_EmpID"].Width = 100;
                }

                if (dgvUsuarios.Columns["USU_UserLog"] != null)
                {
                    dgvUsuarios.Columns["USU_UserLog"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvUsuarios.Columns["USU_UserLog"].MinimumWidth = 120;
                }

                if (dgvUsuarios.Columns["USU_TypeAut"] != null)
                {
                    dgvUsuarios.Columns["USU_TypeAut"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvUsuarios.Columns["USU_TypeAut"].Width = 120;
                }

                if (dgvUsuarios.Columns["USU_Status"] != null)
                {
                    dgvUsuarios.Columns["USU_Status"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvUsuarios.Columns["USU_Status"].Width = 80;
                }

                if (dgvUsuarios.Columns["USU_UserPLant"] != null)
                {
                    dgvUsuarios.Columns["USU_UserPLant"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvUsuarios.Columns["USU_UserPLant"].Width = 120;
                }

                // ✅ Checkboxes de plantas
                if (dgvUsuarios.Columns["MTY_Access"] != null)
                {
                    dgvUsuarios.Columns["MTY_Access"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvUsuarios.Columns["MTY_Access"].Width = 60;
                }

                if (dgvUsuarios.Columns["QRO_Access"] != null)
                {
                    dgvUsuarios.Columns["QRO_Access"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvUsuarios.Columns["QRO_Access"].Width = 60;
                }

                if (dgvUsuarios.Columns["TIJ_Access"] != null)
                {
                    dgvUsuarios.Columns["TIJ_Access"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvUsuarios.Columns["TIJ_Access"].Width = 60;
                }

                dgvUsuarios.ResumeLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error ajustando anchos de columnas usuarios: {ex.Message}");
            }
        }




        //private void DiagnosticarCheckbox()
        //{
        //    if (dgvUsuarios == null || dgvUsuarios.Columns.Count == 0) return;

        //    var diagnostico = new System.Text.StringBuilder();
        //    diagnostico.AppendLine("=== DIAGNÓSTICO DE CHECKBOXES ===\n");

        //    // Verificar configuración del grid
        //    diagnostico.AppendLine($"Grid ReadOnly: {dgvUsuarios.ReadOnly}");
        //    diagnostico.AppendLine($"Grid EditMode: {dgvUsuarios.EditMode}");
        //    diagnostico.AppendLine($"Grid Enabled: {dgvUsuarios.Enabled}\n");

        //    // Verificar columnas de checkboxes
        //    foreach (var colName in new[] { "MTY_Access", "QRO_Access", "TIJ_Access" })
        //    {
        //        if (dgvUsuarios.Columns.Contains(colName))
        //        {
        //            var col = dgvUsuarios.Columns[colName];
        //            diagnostico.AppendLine($"Columna: {colName}");
        //            diagnostico.AppendLine($"  Tipo: {col.GetType().Name}");
        //            diagnostico.AppendLine($"  ReadOnly: {col.ReadOnly}");
        //            diagnostico.AppendLine($"  Visible: {col.Visible}");

        //            if (col is DataGridViewCheckBoxColumn chkCol)
        //            {
        //                diagnostico.AppendLine($"  TrueValue: {chkCol.TrueValue}");
        //                diagnostico.AppendLine($"  FalseValue: {chkCol.FalseValue}");
        //                diagnostico.AppendLine($"  ThreeState: {chkCol.ThreeState}");
        //            }
        //            diagnostico.AppendLine();
        //        }
        //    }

        //    // Verificar datos de la primera fila
        //    if (dgvUsuarios.Rows.Count > 0)
        //    {
        //        diagnostico.AppendLine("Primera fila:");
        //        var row = dgvUsuarios.Rows[0];
        //        foreach (var colName in new[] { "MTY_Access", "QRO_Access", "TIJ_Access" })
        //        {
        //            if (dgvUsuarios.Columns.Contains(colName))
        //            {
        //                var value = row.Cells[colName].Value;
        //                var readOnly = row.Cells[colName].ReadOnly;
        //                diagnostico.AppendLine($"  {colName}: Value={value}, ReadOnly={readOnly}");
        //            }
        //        }
        //    }

        //    MessageBox.Show(diagnostico.ToString(), "Diagnóstico",
        //        MessageBoxButtons.OK, MessageBoxIcon.Information);
        //}
        private void LoadPlantFilter()
        {
            if (cboPlantFilter == null) return;

            cboPlantFilter.Items.Clear();
            cboPlantFilter.Items.Add(new { Value = 0, Text = "Todas las plantas" });
            cboPlantFilter.Items.Add(new { Value = 1, Text = "MTY" });
            cboPlantFilter.Items.Add(new { Value = 2, Text = "QRO" });
            cboPlantFilter.Items.Add(new { Value = 3, Text = "TIJ" });

            cboPlantFilter.DisplayMember = "Text";
            cboPlantFilter.ValueMember = "Value";
            cboPlantFilter.SelectedIndex = 0;
            cboPlantFilter.Enabled = false;
        }

        private void FilterUsersByPlant()
        {
            if (_dtUsers == null) return;

            if (!chkPlantFilter.Checked)
            {
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
                var filtered = _dtUsers.AsEnumerable()
                    .Where(r => Convert.ToInt32(r["USU_UserPLant"]) == selectedPlant.Value);

                if (filtered.Any())
                {
                    dgvUsuarios.DataSource = filtered.CopyToDataTable();
                }
                else
                {
                    dgvUsuarios.DataSource = _dtUsers.Clone();
                }
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

                // Guardar overrides
                SaveAllOverrides();

                // ✅ Resetear botón Guardar
                if (btnAdminGuardar != null)
                {
                    btnAdminGuardar.BackColor = SystemColors.Control;
                }

                UpdateStatus("✅ Cambios guardados exitosamente");
                MessageBox.Show("Cambios guardados correctamente", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void DgvModulos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (Session.TypeAut < 4 || e.RowIndex < 0) return;

            var col = dgvModulos.Columns[e.ColumnIndex].DataPropertyName;

            if (col == "RolesMinTypeAut")
            {
                if (int.TryParse(Convert.ToString(e.Value), out int r))
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
                UpdateStatus("⚠️ Usuario modificado - Presione GUARDAR para aplicar cambios");
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

                dgvOverrides.DataSource = null;
                dgvOverrides.DataSource = _dtOverridesView;

                if (dgvOverrides.IsHandleCreated)
                {
                    dgvOverrides.BeginInvoke(new Action(ConfigureOverridesColumns));
                }
                else
                {
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

        private void ConfigureOverridesColumns()
        {
            if (dgvOverrides == null || !dgvOverrides.IsHandleCreated || dgvOverrides.Columns.Count == 0)
                return;

            try
            {
                dgvOverrides.SuspendLayout();

                // Configurar EmpId
                var empIdCol = dgvOverrides.Columns["EmpId"];
                if (empIdCol != null)
                {
                    empIdCol.HeaderText = "ID";
                    empIdCol.ReadOnly = true;
                    empIdCol.Width = 80;
                }

                // Configurar UserName
                var userNameCol = dgvOverrides.Columns["UserName"];
                if (userNameCol != null)
                {
                    userNameCol.HeaderText = "Usuario";
                    userNameCol.ReadOnly = true;
                    userNameCol.Width = 140;
                }

                // Configurar RoleName
                var roleNameCol = dgvOverrides.Columns["RoleName"];
                if (roleNameCol != null)
                {
                    roleNameCol.HeaderText = "Rol Base";
                    roleNameCol.ReadOnly = true;
                    roleNameCol.Width = 100;
                }

                // ✅ Reemplazar columna Override con ComboBox EDITABLE
                var overrideCol = dgvOverrides.Columns["Override"];
                if (overrideCol != null)
                {
                    int overrideIndex = overrideCol.Index;
                    dgvOverrides.Columns.Remove(overrideCol);

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
                        Width = 130,
                        ReadOnly = false, // ✅ EDITABLE
                        DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox, // ✅ Mostrar siempre como ComboBox
                        FlatStyle = FlatStyle.Flat
                    };

                    dgvOverrides.Columns.Insert(overrideIndex, comboCol);
                }

                // ✅ Configuración general - PERMITIR EDICIÓN
                dgvOverrides.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                dgvOverrides.AllowUserToAddRows = false;
                dgvOverrides.AllowUserToDeleteRows = false;
                dgvOverrides.SelectionMode = DataGridViewSelectionMode.CellSelect;
                dgvOverrides.MultiSelect = false;
                dgvOverrides.RowHeadersVisible = false;
                dgvOverrides.ReadOnly = false; // ✅ Grid NO es ReadOnly
                dgvOverrides.EditMode = DataGridViewEditMode.EditOnEnter;

                dgvOverrides.ResumeLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error configurando columnas de overrides: {ex.Message}");
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
            if (e.RowIndex < 0 || dgvModulos?.CurrentRow == null) return;

            var drv = dgvModulos.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;

            string buttonName = Convert.ToString(drv["ButtonName"]);
            ApplyOneOverrideFromRow(buttonName, e.RowIndex);

            // ✅ Habilitar botón Guardar y cambiar color
            UpdateStatus("⚠️ Override modificado - Presione GUARDAR para aplicar cambios");

            if (btnAdminGuardar != null && _adminCanEdit)
            {
                btnAdminGuardar.Enabled = true;
                btnAdminGuardar.BackColor = Color.FromArgb(255, 235, 180); // Color de advertencia
            }
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
            if (e.RowIndex < 0) return;

            try
            {
                var row = dgvOverrides.Rows[e.RowIndex];
                var cellValue = row.Cells["Override"].Value;

                if (cellValue == null || cellValue == DBNull.Value) return;

                int overrideValue = Convert.ToInt32(cellValue);

                switch (overrideValue)
                {
                    case 1:
                        row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230);
                        row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                        break;
                    case -1:
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                        break;
                    default:
                        row.DefaultCellStyle.BackColor = Color.White;
                        row.DefaultCellStyle.ForeColor = Color.Black;
                        break;
                }
            }
            catch { }
        }
        #endregion

        #region Context Menu
        private void OpenContextSelected(bool asAdmin)
        {
            // ✅ Funciona con cualquier control que tenga Tag de tipo ModuleDef
            if (cmuModulo?.SourceControl is Control ctrl && ctrl.Tag is ModuleDef m)
            {
                _moduleService.LaunchModule(ctrl.Name, m, asAdmin, ALLOWED_ROOTS, UpdateStatus);
            }
        }

        private void CopyModulePathFromContext()
        {
            if (cmuModulo?.SourceControl is Control ctrl && ctrl.Tag is ModuleDef m && !string.IsNullOrEmpty(m.ExePath))
            {
                Clipboard.SetText(m.ExePath);
                UpdateStatus("Ruta copiada al portapapeles");
            }
        }

        private void ShowModulePropertiesFromContext()
        {
            if (cmuModulo?.SourceControl is Control ctrl && ctrl.Tag is ModuleDef m)
            {
                var info = $"Nombre: {m.Name}\n" +
                          $"Ruta: {m.ExePath ?? "(no especificada)"}\n" +
                          $"Directorio: {m.WorkingDir ?? "(no especificado)"}\n" +
                          $"Icono: {m.IconPath ?? "(usar icono del ejecutable)"}\n" +
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
                tsslUser.Text = $"Usuario: {Session.LogonName ?? "—"}";
            if (tsslRole != null)
                tsslRole.Text = $"Rol: {_roleManager.GetRoleName(Session.TypeAut)}";
            if (tsslPlant != null)
            {
                string plantName = Session.Sucursal == 1 ? "MTY" :
                                  Session.Sucursal == 2 ? "QRO" :
                                  Session.Sucursal == 3 ? "TIJ" : "—";
                tsslPlant.Text = $"Planta: {plantName}";
            }
            if (tsslEstado != null)
                tsslEstado.Text = "Listo";
        }

        private void UpdateStatus(string message)
        {
            if (tsslEstado != null)
            {
                tsslEstado.Text = message;
                Application.DoEvents();
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

            ApplySearch(searchText, flpOperacion);
            ApplySearch(searchText, flpConsultas);

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
                         "© 2024 Graphic Packaging International";

            MessageBox.Show(info, "Acerca de ModuleGPI",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Public Methods
        public void Logout()
        {
            var result = MessageBox.Show("¿Está seguro que desea cerrar sesión?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }
        #endregion
    }
}