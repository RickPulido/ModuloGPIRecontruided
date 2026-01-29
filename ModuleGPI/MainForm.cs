using ModuleGPI.Data;
using ModuleGPI.Domain;
using ModuleGPI.Services;
using ModuleGPI.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ModuleGPI
{
    /// <summary>
    /// Formulario principal del sistema de lanzamiento de módulos GPI
    /// </summary>
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

        private FavoritesManager _favoritesManager;
        private bool _isLoadingFavorites = false;
        private TreeNode _favoritesNode;

        private readonly BindingSource _bsUsers = new BindingSource();
        private readonly BindingSource _modulosBinding = new BindingSource();

        private List<ModuleDef> _allModules = new List<ModuleDef>();
        private bool _isRefreshingAll = false;

        private static readonly string[] ALLOWED_ROOTS = new string[]
        {
            @"\\USAZR3QITVFE001\",   // Servidor 1 (TEST)
            @"\\USAZR3PITVFE001\",   // Servidor 2 (PRODUCCION)
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
            _favoritesManager = new FavoritesManager();

            // Suscribir eventos del formulario
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;

            // Configurar handlers
            SetupEventHandlers();
            ConnectAllButtonEvents();
        }

        #endregion

        #region Form Events

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                UpdateStatusBar();
                _adminCanEdit = Session.TypeAut >= 5;

                LoadOverrides();
                LoadModules();

                _roleManager.ApplyVisibility(
                    tabMain, tabAdmin, tabConfig,
                    Session.TypeAut,
                    Session.EmpId ?? Session.LogonName,
                    _overrides,
                    _allModules
                );

                if (Session.TypeAut >= 4)
                {
                    LoadConfigData();
                }

                LoadFavorites();

                if (treeFavoritos != null)
                {
                    treeFavoritos.SelectedNode = null;
                }

                WireModuleButtons();
                UpdateStatus("Sistema listo");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Error al cargar formulario: {0}\n\n{1}", ex.Message, ex.StackTrace),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                var result = MessageBox.Show(
                    "¿Está seguro que desea salir?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );
                e.Cancel = (result != DialogResult.Yes);
            }
        }

        #endregion

        #region Setup Methods

        private void ConnectAllButtonEvents()
        {
            if (btnCerrarSesion != null)
                btnCerrarSesion.Click += (s, e) => Logout();

            if (btnRefreshAll != null)
                btnRefreshAll.Click += (s, e) => RefreshAll();

            if (btnNewModule != null)
                btnNewModule.Click += BtnNewModule_Click;

            if (btnSaveModule != null)
                btnSaveModule.Click += BtnSaveModule_Click;

            if (btnDeleteModule != null)
                btnDeleteModule.Click += BtnDeleteModule_Click;

            if (btnAdminGuardar != null)
            {
                btnAdminGuardar.Enabled = _adminCanEdit;
                btnAdminGuardar.Click += BtnAdminGuardar_Click;
            }

            if (cmuAbrir != null)
                cmuAbrir.Click += (s, e) => OpenContextSelected(false);

            if (cmuAbrirAdmin != null)
                cmuAbrirAdmin.Click += (s, e) => OpenContextSelected(true);

            if (cmuCopiarRuta != null)
                cmuCopiarRuta.Click += (s, e) => CopyModulePathFromContext();

            if (cmuVerProp != null)
                cmuVerProp.Click += (s, e) => ShowModulePropertiesFromContext();

            // Agregar opción de favoritos al menú contextual
            var cmuFavorito = new ToolStripMenuItem("⭐ Agregar/Quitar de Favoritos");
            cmuFavorito.Click += (s, e) => ToggleFavoriteFromContext();
            cmuModulo.Items.Insert(0, cmuFavorito);
            cmuModulo.Items.Insert(1, new ToolStripSeparator());
        }

        private void SetupEventHandlers()
        {
            if (tabMain != null)
                tabMain.Selected += TabMain_Selected;

            if (treeFavoritos != null)
            {
                treeFavoritos.BeforeSelect += (s, e) =>
                {
                    Debug.WriteLine(string.Format("🔍 BeforeSelect: Node={0}, Action={1}",
                        e.Node != null ? e.Node.Text : "null", e.Action));
                };
                treeFavoritos.AfterSelect += TreeFavoritos_AfterSelect;
            }

            // DataGridView eventos
            if (dgvModulos != null)
            {
                dgvModulos.SelectionChanged += DgvModulos_SelectionChanged;
                dgvModulos.CellFormatting += DgvModulos_CellFormatting;
                dgvModulos.DataError += (s, e) => e.ThrowException = false;
                dgvModulos.RowPrePaint += DgvModulos_RowPrePaint;
                _uiHelpers.EnableDgvDoubleBuffer(dgvModulos);
            }

            if (dgvModulesConfig != null)
            {
                dgvModulesConfig.SelectionChanged += DgvModulesConfig_SelectionChanged;
                dgvModulesConfig.CellDoubleClick += DgvModulesConfig_CellDoubleClick;
                dgvModulesConfig.DataError += (s, e) => e.ThrowException = false;
                dgvModulesConfig.RowPrePaint -= DgvModulesConfig_RowPrePaint;
                dgvModulesConfig.RowPrePaint += DgvModulesConfig_RowPrePaint;
                _uiHelpers.EnableDgvDoubleBuffer(dgvModulesConfig);
            }

            if (dgvUsuarios != null)
            {
                dgvUsuarios.CellValueChanged += DgvUsuarios_CellValueChanged;
                dgvUsuarios.CurrentCellDirtyStateChanged += DgvUsuarios_CurrentCellDirtyStateChanged;
                dgvUsuarios.CellEndEdit += DgvUsuarios_CellEndEdit;
                dgvUsuarios.CellContentClick += DgvUsuarios_CellContentClick;
                dgvUsuarios.DataError += (s, e) => e.ThrowException = false;
                _uiHelpers.EnableDgvDoubleBuffer(dgvUsuarios);
            }

            if (chkPlantFilter != null && cboPlantFilter != null)
            {
                chkPlantFilter.CheckedChanged += (s, e) =>
                {
                    cboPlantFilter.Enabled = chkPlantFilter.Checked;
                    FilterUsersByPlant();
                };
                cboPlantFilter.SelectedIndexChanged += (s, e) => FilterUsersByPlant();
                cboPlantFilter.SelectedValueChanged -= CboPlantFilter_SelectedIndexChanged;
                cboPlantFilter.SelectedValueChanged += CboPlantFilter_SelectedIndexChanged;
            }

            if (dgvOverrides != null)
            {
                dgvOverrides.CurrentCellDirtyStateChanged += DgvOverrides_CurrentCellDirtyStateChanged;
                dgvOverrides.CellValueChanged += DgvOverrides_CellValueChanged;
                dgvOverrides.CellFormatting += DgvOverrides_CellFormatting;
                dgvOverrides.DataError += (s, e) => { e.ThrowException = false; };
            }

            if (chkPlantP != null && cboPlantP != null)
            {
                chkPlantP.CheckedChanged += chkFiltrarPorPlanta_CheckedChanged;
                cboPlantP.SelectedIndexChanged += (s, e) =>
                {
                    if (!_isRefreshingAll)
                        LoadModules();
                };
            }

            if (txtSearchMod != null)
            {
                txtSearchMod.TextChanged += (s, e) =>
                {
                    ApplySearch(txtSearchMod.Text, flpModulos);
                    ApplySearch(txtSearchMod.Text, flpModulosTest);
                };
            }
        }

        #endregion

        #region DataGridView Eventos - Diseño Dinámico

        private void RefreshOverridesForCurrentModule()
        {
            if (dgvModulos == null || dgvModulos.CurrentRow == null) return;

            if (dgvModulos.CurrentRow.DataBoundItem is DataRowView drv)
            {
                string buttonName = Convert.ToString(drv["ButtonName"]);
                BuildOverridesViewFor(buttonName);
            }
        }

        private void DgvModulesConfig_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dgvModulesConfig == null || e.RowIndex < 0) return;

            var row = dgvModulesConfig.Rows[e.RowIndex];

            if (!dgvModulesConfig.Columns.Contains("IsTest")) return;

            bool isTest = false;
            var v = row.Cells["IsTest"].Value;
            if (v != null && v != DBNull.Value)
                bool.TryParse(v.ToString(), out isTest);

            if (isTest)
            {
                var amber = Color.FromArgb(255, 245, 200);
                row.DefaultCellStyle.BackColor = amber;
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 230, 160);
                row.DefaultCellStyle.ForeColor = Color.Black;
                row.DefaultCellStyle.SelectionForeColor = Color.Black;
            }
            else
            {
                row.DefaultCellStyle.BackColor = dgvModulesConfig.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.SelectionBackColor = dgvModulesConfig.DefaultCellStyle.SelectionBackColor;
                row.DefaultCellStyle.ForeColor = dgvModulesConfig.DefaultCellStyle.ForeColor;
                row.DefaultCellStyle.SelectionForeColor = dgvModulesConfig.DefaultCellStyle.SelectionForeColor;
            }
        }

        private void TreeFavoritos_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_isLoadingFavorites)
            {
                Debug.WriteLine("⚠️ Evento AfterSelect ignorado: Cargando favoritos");
                return;
            }

            if (e.Action != TreeViewAction.ByMouse && e.Action != TreeViewAction.ByKeyboard)
            {
                Debug.WriteLine(string.Format("⚠️ Evento AfterSelect ignorado: Action={0}", e.Action));
                return;
            }

            if (e.Node != null && e.Node.Tag is string buttonName)
            {
                Debug.WriteLine(string.Format("✅ Lanzando favorito desde TreeView: {0}", buttonName));
                LaunchFavoriteModule(buttonName);
            }
            else
            {
                Debug.WriteLine("⚠️ Nodo seleccionado no tiene módulo asociado");
            }
        }

        private void ToggleFavoriteFromContext()
        {
            if (cmuModulo != null && cmuModulo.SourceControl is Control ctrl && ctrl.Tag is ModuleDef m)
            {
                _favoritesManager.ToggleFavorite(ctrl.Name);

                bool isFav = _favoritesManager.IsFavorite(ctrl.Name);

                UpdateStatus(isFav
                    ? string.Format("'{0}' agregado a favoritos", m.Name)
                    : string.Format("'{0}' removido de favoritos", m.Name));

                RefreshFavorites();
            }
        }

        private void DgvUsuarios_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var columnName = dgvUsuarios.Columns[e.ColumnIndex].Name;

            if (columnName == "MTY_Access" || columnName == "QRO_Access" || columnName == "TIJ_Access")
            {
                try
                {
                    var currentCell = dgvUsuarios[e.ColumnIndex, e.RowIndex];
                    var currentValue = currentCell.Value;

                    bool newValue;
                    if (currentValue == null || currentValue == DBNull.Value)
                    {
                        newValue = true;
                    }
                    else
                    {
                        newValue = !Convert.ToBoolean(currentValue);
                    }

                    currentCell.Value = newValue;

                    dgvUsuarios.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    dgvUsuarios.RefreshEdit();

                    UpdateStatus(string.Format("Acceso a {0} modificado - Presione GUARDAR para aplicar",
                        columnName.Replace("_Access", "")));

                    if (btnAdminGuardar != null && _adminCanEdit)
                    {
                        btnAdminGuardar.Enabled = true;
                        btnAdminGuardar.BackColor = Color.FromArgb(255, 235, 180);
                    }

                    Debug.WriteLine(string.Format("Checkbox {0} cambiado a: {1} para fila {2}",
                        columnName, newValue, e.RowIndex));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("Error en CellContentClick: {0}", ex.Message));
                    MessageBox.Show(
                        string.Format("Error al cambiar checkbox: {0}", ex.Message),
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void DgvUsuarios_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvUsuarios.IsCurrentCellDirty)
            {
                dgvUsuarios.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DgvUsuarios_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                UpdateStatus("Cambios sin guardar - Presione GUARDAR para aplicar");

                if (btnAdminGuardar != null && _adminCanEdit)
                {
                    btnAdminGuardar.Enabled = true;
                    btnAdminGuardar.BackColor = Color.FromArgb(255, 235, 180);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error en CellValueChanged: {0}", ex.Message));
            }
        }

        private void DgvUsuarios_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Evento adicional para manejar fin de edición si es necesario
        }

        private void BtnDiagnosticar_Click(object sender, EventArgs e)
        {
            if (dgvModulos == null || dgvModulos.CurrentRow == null) return;

            if (dgvModulos.CurrentRow.DataBoundItem is DataRowView drv)
            {
                try
                {
                    string buttonName = Convert.ToString(drv["ButtonName"]);

                    var module = new ModuleDef
                    {
                        ButtonName = buttonName,
                        Name = Convert.ToString(drv["Name"]),
                        RolesMinTypeAut = Convert.ToInt32(drv["RolesMinTypeAut"]),
                        Plant = Convert.ToInt32(drv["Plant"])
                    };

                    string diagnostico = _roleManager.DiagnoseModuleAccess(
                        buttonName,
                        module,
                        Session.TypeAut,
                        Session.EmpId,
                        _overrides
                    );

                    var formDiag = new Form
                    {
                        Text = "Diagnóstico de Acceso",
                        Size = new Size(600, 500),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.SizableToolWindow
                    };

                    var txtDiag = new TextBox
                    {
                        Multiline = true,
                        ReadOnly = true,
                        ScrollBars = ScrollBars.Vertical,
                        Dock = DockStyle.Fill,
                        Font = new Font("Consolas", 9F),
                        Text = diagnostico
                    };

                    formDiag.Controls.Add(txtDiag);
                    formDiag.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format("Error en diagnóstico: {0}", ex.Message),
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
            else
            {
                MessageBox.Show(
                    "Seleccione un módulo primero",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void DgvModulos_SelectionChanged(object sender, EventArgs e)
        {
            RefreshOverridesForCurrentModule();
        }

        private void DgvModulos_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvModulos == null || e.RowIndex < 0) return;

            var row = dgvModulos.Rows[e.RowIndex];
            if (row.DataBoundItem is DataRowView drv)
            {
                bool isTest = false;
                if (drv.Row.Table.Columns.Contains("IsTest") &&
                    drv["IsTest"] != DBNull.Value)
                {
                    isTest = Convert.ToBoolean(drv["IsTest"]);
                }

                if (isTest)
                {
                    e.CellStyle.BackColor = Color.FromArgb(255, 245, 200);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(255, 230, 160);
                    e.CellStyle.ForeColor = Color.Black;
                    e.CellStyle.SelectionForeColor = Color.Black;
                }
            }
        }

        private void DgvModulos_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (dgvModulos == null || e.RowIndex < 0) return;

            var row = dgvModulos.Rows[e.RowIndex];
            var isTestCol = dgvModulos.Columns["IsTest"];
            if (isTestCol == null) return;

            bool isTest = false;
            var v = row.Cells[isTestCol.Index].Value;
            if (v != null && v != DBNull.Value)
                bool.TryParse(v.ToString(), out isTest);

            if (isTest)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 200);
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 230, 160);
                row.DefaultCellStyle.ForeColor = Color.Black;
                row.DefaultCellStyle.SelectionForeColor = Color.Black;
            }
            else
            {
                row.DefaultCellStyle.BackColor = dgvModulos.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.SelectionBackColor = dgvModulos.DefaultCellStyle.SelectionBackColor;
                row.DefaultCellStyle.ForeColor = dgvModulos.DefaultCellStyle.ForeColor;
                row.DefaultCellStyle.SelectionForeColor = dgvModulos.DefaultCellStyle.SelectionForeColor;
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
            if (e.RowIndex < 0 || dgvOverrides.Columns.Count == 0) return;

            try
            {
                var row = dgvOverrides.Rows[e.RowIndex];
                string empId = Convert.ToString(row.Cells["USU_UserLog"].Value);
                int? overrideValue = null;

                for (int i = 1; i < dgvOverrides.Columns.Count; i++)
                {
                    if (dgvOverrides.Columns[i] is DataGridViewCheckBoxColumn && e.ColumnIndex == i)
                    {
                        var val = row.Cells[i].Value;
                        if (val != null && val != DBNull.Value && Convert.ToBoolean(val))
                        {
                            overrideValue = 1;
                        }
                        else
                        {
                            overrideValue = null;
                        }

                        string buttonName = dgvOverrides.Columns[i].Name.Replace("chk_", "");
                        _overrides.Set(buttonName, empId, overrideValue ?? 0);
                        break;
                        // ov == 0 ? 0 : ov
                    }
                }

                UpdateStatus("Override modificado - Presione GUARDAR para aplicar");
                if (btnAdminGuardar != null && _adminCanEdit)
                {
                    btnAdminGuardar.Enabled = true;
                    btnAdminGuardar.BackColor = Color.FromArgb(255, 235, 180);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error en DgvOverrides_CellValueChanged: {0}", ex.Message));
            }
        }

        private void DgvOverrides_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvOverrides == null || e.RowIndex < 0) return;

            if (e.RowIndex % 2 == 0)
            {
                e.CellStyle.BackColor = Color.White;
            }
            else
            {
                e.CellStyle.BackColor = Color.FromArgb(245, 245, 245);
            }
        }

        #endregion

        #region Favoritos

        private void LoadFavorites()
        {
            if (treeFavoritos == null) return;

            _isLoadingFavorites = true;
            try
            {
                treeFavoritos.Nodes.Clear();

                _favoritesNode = new TreeNode("⭐ Favoritos")
                {
                    Name = "nodeFavoritos",
                    NodeFont = new Font(treeFavoritos.Font, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 102, 204)
                };

                treeFavoritos.Nodes.Add(_favoritesNode);

                var favorites = _favoritesManager.GetFavorites();
                foreach (string buttonName in favorites)
                {
                    Control moduleControl = FindModuleControl(buttonName);
                    if (moduleControl != null && moduleControl.Tag is ModuleDef module)
                    {
                        var favNode = new TreeNode(module.Name) { Tag = buttonName };
                        _favoritesNode.Nodes.Add(favNode);
                    }
                }

                _favoritesNode.Expand();
            }
            finally
            {
                _isLoadingFavorites = false;
            }
        }

        private void RefreshFavorites()
        {
            LoadFavorites();
            if (_favoritesNode == null) return;

            _favoritesNode.Nodes.Clear();

            var favorites = _favoritesManager.GetFavorites();

            foreach (string buttonName in favorites)
            {
                Control moduleControl = FindModuleControl(buttonName);

                if (moduleControl != null && moduleControl.Tag is ModuleDef module)
                {
                    var favNode = new TreeNode(module.Name) { Tag = buttonName };
                    _favoritesNode.Nodes.Add(favNode);
                }
            }
        }

        private Control FindModuleControl(string buttonName)
        {
            if (flpModulos != null)
            {
                foreach (Control ctrl in flpModulos.Controls)
                {
                    if (ctrl.Name == buttonName)
                        return ctrl;
                }
            }

            if (flpModulosTest != null)
            {
                foreach (Control ctrl in flpModulosTest.Controls)
                {
                    if (ctrl.Name == buttonName)
                        return ctrl;
                }
            }

            return null;
        }

        private void LaunchFavoriteModule(string buttonName)
        {
            Control moduleControl = FindModuleControl(buttonName);

            if (moduleControl != null && moduleControl.Tag is ModuleDef module)
            {
                _moduleService.LaunchModule(buttonName, module, false, ALLOWED_ROOTS, UpdateStatus);
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
                MessageBox.Show(
                    "No tiene permisos para acceder a configuración.",
                    "Acceso Denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        #endregion

        #region Module Management

        /// <summary>
        /// Carga los módulos desde la base de datos o cache
        /// </summary>
        public void LoadModules()
        {
            LoadingSpinner spinner = null;
            try
            {
                DataTable dt;
                int? plant = null;

                // Intentar usar cache precargada
                if (ModulesCache.TryGetModules(out dt))
                {
                    spinner = ShowLoadingSpinner("Aplicando permisos...");
                    Debug.WriteLine("✅ Usando módulos precargados");
                }
                else
                {
                    // Cache no disponible - cargar ahora (fallback)
                    spinner = ShowLoadingSpinner("Cargando módulos...");
                    Debug.WriteLine("⚠️ Cache no disponible, cargando ahora...");
                    
                    // ✅ CORREGIDO: Cargar módulos real
                    dt = _moduleService.LoadModules(plant);
                   // ModulesCache.GetModules(dt);
                }

                // Aplicar filtro de planta si está habilitado
                if (chkPlantP != null && chkPlantP.Checked &&
                    cboPlantP != null && cboPlantP.SelectedItem != null)
                {
                    dynamic selected = cboPlantP.SelectedItem;
                    plant = (int)selected.Value;
                    if (plant == 0) plant = null; // "Todas las plantas"
                }

                // Construir lista completa de módulos
                _allModules = dt.AsEnumerable()
                    .Select(r => new ModuleDef
                    {
                        ButtonName = Convert.ToString(r["ButtonName"]),
                        Name = Convert.ToString(r["Name"]),
                        ExePath = Convert.ToString(r["ExePath"]),
                        Arguments = dt.Columns.Contains("Arguments") ? Convert.ToString(r["Arguments"]) : "",
                        WorkingDir = Convert.ToString(r["WorkingDir"]),
                        IconPath = Convert.ToString(r["IconPath"]),
                        RequiresElevation = r["RequiresElevation"] != DBNull.Value && Convert.ToBoolean(r["RequiresElevation"]),
                        RolesMinTypeAut = r["RolesMinTypeAut"] == DBNull.Value ? 1 : Convert.ToInt32(r["RolesMinTypeAut"]),
                        Plant = r["Plant"] == DBNull.Value ? 1 : Convert.ToInt32(r["Plant"]),
                        IsTest = r["IsTest"] != DBNull.Value && Convert.ToBoolean(r["IsTest"]),
                    })
                    .ToList();

                // Cargar módulos PRD (IsTest = false)
                if (flpModulos != null)
                {
                    var prdRows = dt.AsEnumerable()
                        .Where(r => r["IsTest"] == DBNull.Value || Convert.ToBoolean(r["IsTest"]) == false);

                    if (prdRows.Any())
                    {
                        var dtPRD = prdRows.CopyToDataTable();

                        _moduleService.PaintButtons(
                            dtPRD,
                            flpModulos,
                            null,
                            cmuModulo,
                            _toolTips,
                            (btnName, module) => _roleManager.CanSeeModule(
                                btnName,
                                module,
                                Session.TypeAut,
                                Session.EmpId ?? Session.LogonName,
                                _overrides
                            )
                        );
                    }
                }

                // Cargar módulos TEST (IsTest = true)
                if (flpModulosTest != null)
                {
                    var testRows = dt.AsEnumerable()
                        .Where(r => r["IsTest"] != DBNull.Value && Convert.ToBoolean(r["IsTest"]));

                    if (testRows.Any())
                    {
                        var dtTEST = testRows.CopyToDataTable();

                        _moduleService.PaintButtons(
                            dtTEST,
                            flpModulosTest,
                            null,
                            cmuModulo,
                            _toolTips,
                            (btnName, module) => _roleManager.CanSeeModule(
                                btnName,
                                module,
                                Session.TypeAut,
                                Session.EmpId ?? Session.LogonName,
                                _overrides
                            )
                        );
                    }
                    else
                    {
                        if (!tabModulosTest.Visible && flpModulosTest != null)
                            flpModulosTest.Controls.Clear();
                    }
                }

                // Aplicar búsqueda si hay texto
                if (txtSearchMod != null && !string.IsNullOrEmpty(txtSearchMod.Text))
                {
                    ApplySearch(txtSearchMod.Text, flpModulos);
                    ApplySearch(txtSearchMod.Text, flpModulosTest);
                }

                UpdateStatus("Módulos cargados correctamente");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Error cargando módulos: {0}", ex.Message),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                HideLoadingSpinner(spinner);
            }
        }

        private void RefreshModules()
        {
            LoadModules();

            _roleManager.ApplyVisibility(
                tabMain,
                tabAdmin,
                tabConfig,
                Session.TypeAut,
                Session.EmpId ?? Session.LogonName,
                _overrides,
                _allModules
            );

            if (tabModulosTest != null && flpModulosTest != null &&
                flpModulosTest.Controls.Count == 0 && Session.TypeAut < 5)
            {
                tabModulosTest.Visible = false;
            }
        }

        private void LoadOverrides()
        {
            try
            {
                _overrides = _dataAccess.GetOverrides();
                Debug.WriteLine(string.Format("Overrides cargados: {0} registros",
                    _overrides != null ? "OK" : "NULL"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error cargando overrides: {0}", ex.Message));
                _overrides = new OverridesStore();
            }
        }

        private void WireModuleButtons()
        {
            _moduleService.WireButtons(flpModulos, null, ModuleButton_Click);

            if (flpModulosTest != null)
            {
                _moduleService.WireButtons(flpModulosTest, null, ModuleButton_Click);
            }
        }

        private void ModuleButton_Click(object sender, EventArgs e)
        {
            if (sender is Control ctrl && ctrl.Tag is ModuleDef m)
            {
                _moduleService.LaunchModule(ctrl.Name, m, false, ALLOWED_ROOTS, UpdateStatus);
            }
        }

        private void ApplySearch(string searchText, FlowLayoutPanel panel)
        {
            if (panel == null || string.IsNullOrWhiteSpace(searchText))
            {
                if (panel != null)
                {
                    foreach (Control ctrl in panel.Controls)
                    {
                        if (ctrl is Button btn && btn.Tag is ModuleDef m)
                        {
                            bool canSee = _roleManager.CanSeeModule(
                                btn.Name,
                                m,
                                Session.TypeAut,
                                Session.EmpId ?? Session.LogonName,
                                _overrides
                            );
                            btn.Visible = canSee;
                        }
                    }
                }
                return;
            }

            searchText = searchText.ToLower();
            foreach (Control ctrl in panel.Controls)
            {
                if (ctrl is Button btn && btn.Tag is ModuleDef m)
                {
                    bool nameMatch = (!string.IsNullOrEmpty(m.Name) &&
                                     m.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (!string.IsNullOrEmpty(m.ExePath) &&
                                     m.ExePath.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);

                    bool canSee = _roleManager.CanSeeModule(
                        btn.Name,
                        m,
                        Session.TypeAut,
                        Session.EmpId ?? Session.LogonName,
                        _overrides
                    );

                    btn.Visible = nameMatch && canSee;
                }
            }
        }

        #endregion

        #region RefreshAll

        private void RefreshAll()
        {
            if (_isRefreshingAll) return;
            _isRefreshingAll = true;

            string desiredTabName = tabMain != null && tabMain.SelectedTab != null
                ? tabMain.SelectedTab.Name : null;
            int desiredTabIndex = tabMain != null ? tabMain.SelectedIndex : -1;

            if (tabMain != null)
            {
                tabMain.Selected -= TabMain_Selected;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Limpiar cache y recargar desde BD
                ModulesCache.Clear();

                LoadOverrides();
                LoadModules();

                _roleManager.ApplyVisibility(
                    tabMain, tabAdmin, tabConfig,
                    Session.TypeAut,
                    Session.EmpId ?? Session.LogonName,
                    _overrides,
                    _allModules
                );

                if (tabMain != null)
                {
                    tabMain.BeginInvoke(new Action(() =>
                    {
                        RestoreSelectedTab(desiredTabName, desiredTabIndex);
                    }));
                }

                if (Session.TypeAut >= 4)
                {
                    if (tabMain != null && tabMain.SelectedTab == tabAdmin)
                        LoadAdminData();
                    else if (tabMain != null && tabMain.SelectedTab == tabConfig)
                        LoadConfigData();
                }

                UpdateStatus("Sistema actualizado");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Error al refrescar: {0}", ex.Message),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                if (tabMain != null)
                {
                    tabMain.Selected += TabMain_Selected;
                }

                this.Cursor = Cursors.Default;
                _isRefreshingAll = false;
            }
        }

        private void RestoreSelectedTab(string tabName, int fallbackIndex)
        {
            if (tabMain == null) return;

            // Estrategia 1: Restaurar por nombre
            if (!string.IsNullOrWhiteSpace(tabName))
            {
                var desired = tabMain.TabPages
                    .Cast<TabPage>()
                    .FirstOrDefault(tp => string.Equals(tp.Name, tabName,
                        StringComparison.OrdinalIgnoreCase));

                if (desired != null && desired.Visible)
                {
                    tabMain.SelectedTab = desired;
                    Debug.WriteLine(string.Format("✅ Tab restaurado por nombre: {0}", tabName));
                    return;
                }

                Debug.WriteLine(string.Format("⚠️ Tab '{0}' no disponible (invisible o eliminado)", tabName));
            }

            // Estrategia 2: Restaurar por índice
            if (fallbackIndex >= 0 && fallbackIndex < tabMain.TabPages.Count)
            {
                var tabAtIndex = tabMain.TabPages[fallbackIndex];
                if (tabAtIndex.Visible)
                {
                    tabMain.SelectedTab = tabAtIndex;
                    Debug.WriteLine(string.Format("✅ Tab restaurado por índice: {0} ({1})",
                        fallbackIndex, tabAtIndex.Name));
                    return;
                }
            }

            // Estrategia 3: Buscar tab similar
            var similarTab = FindSimilarTab(tabName);
            if (similarTab != null)
            {
                tabMain.SelectedTab = similarTab;
                Debug.WriteLine(string.Format("✅ Tab similar encontrado: {0}", similarTab.Name));
                return;
            }

            Debug.WriteLine("⚠️ No se pudo restaurar tab, usando selección automática");
        }

        private TabPage FindSimilarTab(string lostTabName)
        {
            if (tabMain == null || string.IsNullOrWhiteSpace(lostTabName))
                return null;

            bool ContainsI(string text, string value)
            {
                if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
                    return false;
                return text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (ContainsI(lostTabName, "Test"))
            {
                var prdTab = tabMain.TabPages
                    .Cast<TabPage>()
                    .FirstOrDefault(t => t.Name == "tabModulos" && t.Visible);

                if (prdTab != null) return prdTab;
            }

            if (ContainsI(lostTabName, "Admin") || ContainsI(lostTabName, "Config"))
            {
                var modulosTab = tabMain.TabPages
                    .Cast<TabPage>()
                    .FirstOrDefault(t => (t.Name == "tabModulos" || t.Name == "tabModulosTest")
                        && t.Visible);

                if (modulosTab != null) return modulosTab;
            }

            return null;
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

        #region TabConfig - Module Configuration

        private void LoadConfigData()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                _dtModulesConfig = _dataAccess.GetModules(null);

                if (!_dtModulesConfig.Columns.Contains("IsTest"))
                {
                    _dtModulesConfig.Columns.Add("IsTest", typeof(bool));
                    _dtModulesConfig.Columns["IsTest"].DefaultValue = false;
                    _dtModulesConfig.Columns["IsTest"].ReadOnly = false;
                }

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
                MessageBox.Show(
                    string.Format("Error al cargar configuración: {0}", ex.Message),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
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
                dgvModulesConfig.SuspendLayout();

                // Ocultar columnas que no deben mostrarse
                var hiddenColumns = new[] { "Category", "ExePath", "WorkingDir", "IconPath", "Arguments" };
                foreach (var colName in hiddenColumns)
                {
                    if (dgvModulesConfig.Columns[colName] != null)
                    {
                        dgvModulesConfig.Columns[colName].Visible = false;
                    }
                }

                // Configurar headers
                if (dgvModulesConfig.Columns["ButtonName"] != null)
                {
                    dgvModulesConfig.Columns["ButtonName"].HeaderText = "ID Botón";
                    dgvModulesConfig.Columns["ButtonName"].ReadOnly = true;
                }

                if (dgvModulesConfig.Columns["Name"] != null)
                {
                    dgvModulesConfig.Columns["Name"].HeaderText = "Nombre";
                }

                if (dgvModulesConfig.Columns["IsTest"] != null)
                {
                    dgvModulesConfig.Columns["IsTest"].HeaderText = "Es Test";
                }

                if (dgvModulesConfig.Columns["RequiresElevation"] != null)
                {
                    dgvModulesConfig.Columns["RequiresElevation"].HeaderText = "Requiere Admin";
                }

                if (dgvModulesConfig.Columns["RolesMinTypeAut"] != null)
                {
                    dgvModulesConfig.Columns["RolesMinTypeAut"].HeaderText = "Rol Mínimo";
                }

                if (dgvModulesConfig.Columns["Plant"] != null)
                {
                    dgvModulesConfig.Columns["Plant"].HeaderText = "Planta";
                }

                dgvModulesConfig.ResumeLayout();

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
                Debug.WriteLine(string.Format("Error configurando grid de módulos config: {0}", ex.Message));
            }
        }

        private static int ClampSplitterDistance(SplitContainer sc, int desired)
        {
            int total = sc.Orientation == Orientation.Horizontal ? sc.Height : sc.Width;

            int min = sc.Panel1MinSize;
            int max = total - sc.Panel2MinSize - sc.SplitterWidth;

            if (max < min) max = min;
            if (desired < min) return min;
            if (desired > max) return max;
            return desired;
        }

        private void AjustarAnchosModulosConfig()
        {
            if (dgvModulesConfig == null || !dgvModulesConfig.IsHandleCreated) return;

            try
            {
                dgvModulesConfig.SuspendLayout();

                var widths = new Dictionary<string, int>
                {
                    { "ButtonName", 120 },
                    { "Name", 200 },
                    { "IsTest", 70 },
                    { "RequiresElevation", 100 },
                    { "RolesMinTypeAut", 90 },
                    { "Plant", 70 }
                };

                foreach (var kvp in widths)
                {
                    if (dgvModulesConfig.Columns[kvp.Key] != null)
                    {
                        dgvModulesConfig.Columns[kvp.Key].AutoSizeMode =
                            DataGridViewAutoSizeColumnMode.None;
                        dgvModulesConfig.Columns[kvp.Key].Width = kvp.Value;
                    }
                }

                if (dgvModulesConfig.Columns["Name"] != null)
                {
                    dgvModulesConfig.Columns["Name"].AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.Fill;
                    dgvModulesConfig.Columns["Name"].MinimumWidth = 150;
                }

                dgvModulesConfig.ResumeLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error ajustando anchos de columnas config: {0}", ex.Message));
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
                        newRow["IconPath"] = string.IsNullOrEmpty(module.IconPath) ? "" : module.IconPath;
                        newRow["Category"] = "";
                        newRow["RequiresElevation"] = module.RequiresElevation;
                        newRow["RolesMinTypeAut"] = module.RolesMinTypeAut;
                        newRow["Plant"] = module.Plant;
                        newRow["IsTest"] = module.IsTest;

                        _dtModulesConfig.Rows.Add(newRow);
                        _dataAccess.UpsertModule(newRow);

                        UpdateStatus(string.Format("Módulo '{0}' creado exitosamente", module.Name));
                        RefreshAll();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Error al crear módulo: {0}", ex.Message),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnSaveModule_Click(object sender, EventArgs e)
        {
            if (dgvModulesConfig == null || dgvModulesConfig.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un módulo para editar.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            EditSelectedModule();
        }

        private void BtnDeleteModule_Click(object sender, EventArgs e)
        {
            if (dgvModulesConfig == null || dgvModulesConfig.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un módulo para eliminar.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            var drv = dgvModulesConfig.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;

            string buttonName = Convert.ToString(drv["ButtonName"]);
            string moduleName = Convert.ToString(drv["Name"]);

            var result = MessageBox.Show(
                string.Format("¿Está seguro de eliminar el módulo '{0}'?\n\nEsta acción no se puede deshacer.", moduleName),
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    _dataAccess.DeleteModule(buttonName);
                    drv.Row.Delete();
                    _dtModulesConfig.AcceptChanges();

                    UpdateStatus(string.Format("Módulo '{0}' eliminado", moduleName));
                    RefreshAll();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        string.Format("Error al eliminar módulo: {0}", ex.Message),
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
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
            bool hasSelection = dgvModulesConfig != null && dgvModulesConfig.CurrentRow != null;
            if (btnSaveModule != null)
                btnSaveModule.Enabled = hasSelection && Session.TypeAut >= 5;
            if (btnDeleteModule != null)
                btnDeleteModule.Enabled = hasSelection && Session.TypeAut >= 5;
        }

        private void EditSelectedModule()
        {
            var drv = dgvModulesConfig == null ? null :
                dgvModulesConfig.CurrentRow == null ? null :
                dgvModulesConfig.CurrentRow.DataBoundItem as DataRowView;

            if (drv == null) return;

            try
            {
                var module = new ModuleDef
                {
                    ButtonName = Convert.ToString(drv["ButtonName"]),
                    Name = Convert.ToString(drv["Name"]),
                    ExePath = Convert.ToString(drv["ExePath"]),
                    WorkingDir = Convert.ToString(drv["WorkingDir"]),
                    IconPath = Convert.ToString(drv["IconPath"]),
                    Category = "",
                    RequiresElevation = drv["RequiresElevation"] != DBNull.Value &&
                       Convert.ToBoolean(drv["RequiresElevation"]),
                    RolesMinTypeAut = drv["RolesMinTypeAut"] == DBNull.Value ? 1 :
                     Convert.ToInt32(drv["RolesMinTypeAut"]),
                    Plant = drv["Plant"] == DBNull.Value ? 1 : Convert.ToInt32(drv["Plant"]),
                    IsTest = drv["IsTest"] != DBNull.Value && Convert.ToBoolean(drv["IsTest"])
                };

                using (var form = new ModuleEditForm(module))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        drv["Name"] = form.Module.Name;
                        drv["ExePath"] = form.Module.ExePath;
                        drv["WorkingDir"] = form.Module.WorkingDir ?? "";
                        drv["IconPath"] = form.Module.IconPath ?? "";
                        drv["Category"] = "";
                        drv["RequiresElevation"] = form.Module.RequiresElevation;
                        drv["RolesMinTypeAut"] = form.Module.RolesMinTypeAut;
                        drv["Plant"] = form.Module.Plant;
                        drv["IsTest"] = form.Module.IsTest;

                        _dataAccess.UpsertModule(drv.Row);

                        UpdateStatus(string.Format("Módulo '{0}' actualizado", form.Module.Name));
                        RefreshAll();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Error al editar módulo: {0}", ex.Message),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void SaveAllConfigChanges()
        {
            try
            {
                if (_dtModulesConfig != null && _dtModulesConfig.GetChanges() != null)
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
                    MessageBox.Show(
                        "No hay cambios pendientes para guardar.",
                        "Información",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Error al guardar configuración: {0}", ex.Message),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
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

                if (grpRoles != null && barFiltroPlanta != null && dgvUsuarios != null)
                {
                    grpRoles.SuspendLayout();

                    grpRoles.Controls.Clear();

                    barFiltroPlanta.Dock = DockStyle.Top;
                    barFiltroPlanta.Height = 40;

                    dgvUsuarios.Dock = DockStyle.Fill;

                    grpRoles.Controls.Add(dgvUsuarios);
                    grpRoles.Controls.Add(barFiltroPlanta);

                    grpRoles.ResumeLayout();
                }

                if (dgvUsuarios != null)
                {
                    if (dgvUsuarios.Columns.Contains("MTY_Access"))
                        dgvUsuarios.Columns["MTY_Access"].ReadOnly = false;

                    if (dgvUsuarios.Columns.Contains("QRO_Access"))
                        dgvUsuarios.Columns["QRO_Access"].ReadOnly = false;

                    if (dgvUsuarios.Columns.Contains("TIJ_Access"))
                        dgvUsuarios.Columns["TIJ_Access"].ReadOnly = false;

                    Debug.WriteLine("✅ LoadAdminData: Forzado ReadOnly=false en checkboxes");
                }

                _dtModulesAdmin = _dataAccess.GetModules(null);
                if (dgvModulos != null)
                {
                    dgvModulos.DataSource = _dtModulesAdmin;
                    dgvModulos.ReadOnly = true;
                }
                ConfigureModulesAdminGrid();

                _dtUsers = _dataAccess.GetUsers();

                _bsUsers.DataSource = _dtUsers.DefaultView;
                if (dgvUsuarios != null)
                {
                    dgvUsuarios.DataSource = _bsUsers;
                }

                if (btnAdminGuardar != null)
                {
                    btnAdminGuardar.Enabled = _adminCanEdit;
                }

                LoadPlantFilter();

                if (dgvModulos != null && dgvModulos.Rows.Count > 0)
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
                MessageBox.Show(
                    string.Format("Error al cargar datos de administración: {0}", ex.Message),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
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
                var visibleColumns = new[] { "Name", "RolesMinTypeAut", "Plant" };

                foreach (DataGridViewColumn col in dgvModulos.Columns)
                {
                    bool show = visibleColumns.Contains(col.DataPropertyName) ||
                               visibleColumns.Contains(col.Name);

                    col.Visible = show;

                    if (show)
                    {
                        var key = !string.IsNullOrWhiteSpace(col.DataPropertyName)
                            ? col.DataPropertyName : col.Name;

                        switch (key)
                        {
                            case "Name":
                                col.HeaderText = "Nombre Módulo";
                                col.ReadOnly = true;
                                break;
                            case "RolesMinTypeAut":
                                col.HeaderText = "Rol Mínimo";
                                col.ReadOnly = true;
                                break;
                            case "Plant":
                                col.HeaderText = "Planta";
                                col.ReadOnly = true;
                                break;
                        }
                    }
                }

                if (dgvModulos.IsHandleCreated && dgvModulos.Visible)
                {
                    AjustarAnchosModulosAdmin();
                }
                else
                {
                    EventHandler handlerCreated = null;
                    EventHandler handlerVisible = null;

                    handlerCreated = (s, e) =>
                    {
                        dgvModulos.HandleCreated -= handlerCreated;
                        dgvModulos.BeginInvoke(new Action(AjustarAnchosModulosAdmin));
                    };

                    handlerVisible = (s, e) =>
                    {
                        if (dgvModulos.Visible)
                        {
                            dgvModulos.VisibleChanged -= handlerVisible;
                            dgvModulos.BeginInvoke(new Action(AjustarAnchosModulosAdmin));
                        }
                    };

                    dgvModulos.HandleCreated += handlerCreated;
                    dgvModulos.VisibleChanged += handlerVisible;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error en ConfigureModulesAdminGrid: {0}", ex.Message));
            }
        }

        private void AjustarAnchosModulosAdmin()
        {
            if (dgvModulos == null || !dgvModulos.IsHandleCreated) return;

            try
            {
                dgvModulos.SuspendLayout();

                if (dgvModulos.Columns["Name"] != null)
                {
                    dgvModulos.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dgvModulos.Columns["Name"].MinimumWidth = 150;
                }

                if (dgvModulos.Columns["RolesMinTypeAut"] != null)
                {
                    dgvModulos.Columns["RolesMinTypeAut"].AutoSizeMode =
                        DataGridViewAutoSizeColumnMode.None;
                    dgvModulos.Columns["RolesMinTypeAut"].Width = 100;
                }

                if (dgvModulos.Columns["Plant"] != null)
                {
                    dgvModulos.Columns["Plant"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    dgvModulos.Columns["Plant"].Width = 100;
                }

                dgvModulos.ResumeLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error ajustando anchos: {0}", ex.Message));
            }
        }

        private void BuildOverridesViewFor(string buttonName)
        {
            if (dgvOverrides == null || _dtUsers == null) return;

            try
            {
                _dtOverridesView = new DataTable();
                _dtOverridesView.Columns.Add("USU_UserLog", typeof(string));

                var usersToShow = _dtUsers.AsEnumerable()
                    .Select(r => Convert.ToString(r["USU_UserLog"]))
                    .Distinct()
                    .OrderBy(u => u)
                    .ToList();

                _dtOverridesView.Columns.Add(string.Format("chk_{0}", buttonName), typeof(bool));

                foreach (string empId in usersToShow)
                {
                    var row = _dtOverridesView.NewRow();
                    row["USU_UserLog"] = empId;

                    int? overrideValue = _overrides.Get(buttonName, empId);
                    row[string.Format("chk_{0}", buttonName)] = (overrideValue == 1);

                    _dtOverridesView.Rows.Add(row);
                }

                dgvOverrides.DataSource = _dtOverridesView;

                if (dgvOverrides.Columns["USU_UserLog"] != null)
                {
                    dgvOverrides.Columns["USU_UserLog"].HeaderText = "Usuario";
                    dgvOverrides.Columns["USU_UserLog"].ReadOnly = true;
                    dgvOverrides.Columns["USU_UserLog"].Width = 150;
                    dgvOverrides.Columns["USU_UserLog"].Frozen = true;
                }

                for (int i = 1; i < dgvOverrides.Columns.Count; i++)
                {
                    dgvOverrides.Columns[i].HeaderText = "Permitir";
                    dgvOverrides.Columns[i].Width = 80;
                }

                Debug.WriteLine(string.Format("Overrides construidos para: {0}", buttonName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Error en BuildOverridesViewFor: {0}", ex.Message));
            }
        }

        private void LoadPlantFilter()
        {
            if (cboPlantFilter != null)
            {
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

            if (cboPlantP != null)
            {
                cboPlantP.Items.Clear();
                cboPlantP.Items.Add(new { Value = 0, Text = "Todas las plantas" });
                cboPlantP.Items.Add(new { Value = 1, Text = "MTY" });
                cboPlantP.Items.Add(new { Value = 2, Text = "QRO" });
                cboPlantP.Items.Add(new { Value = 3, Text = "TIJ" });

                cboPlantP.DisplayMember = "Text";
                cboPlantP.ValueMember = "Value";
                cboPlantP.SelectedIndex = 0;
                cboPlantP.Enabled = chkPlantP != null && chkPlantP.Checked;
            }
        }

        private void chkFiltrarPorPlanta_CheckedChanged(object sender, EventArgs e)
        {
            if (cboPlantP != null && chkPlantP != null)
            {
                cboPlantP.Enabled = chkPlantP.Checked;
                LoadModules();
            }
        }

        void FilterUsersByPlant()
        {
            if (_dtUsers == null || dgvUsuarios == null) return;

            if (chkPlantFilter != null && !chkPlantFilter.Checked)
            {
                dgvUsuarios.DataSource = _dtUsers;
                return;
            }

            if (cboPlantFilter == null) return;

            var selectedPlant = cboPlantFilter.SelectedItem as dynamic;

            if (selectedPlant == null) return;

            if (selectedPlant.Value == 0)
            {
                dgvUsuarios.DataSource = _dtUsers;
            }
            else
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

                RefreshOverridesForCurrentModule();
            }
        }

        private void CboPlantFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Manejador adicional si es necesario
        }

        private void BtnAdminGuardar_Click(object sender, EventArgs e)
        {
            if (!_adminCanEdit)
            {
                MessageBox.Show(
                    "No tiene permisos para guardar cambios.",
                    "Acceso Denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;

                bool anyChanges = false;

                if (_dtUsers != null && _dtUsers.GetChanges() != null)
                {
                    _dataAccess.UpdateUsers(_dtUsers.GetChanges());
                    _dtUsers.AcceptChanges();
                    anyChanges = true;
                }

                if (_overrides != null)
                {
                    _dataAccess.ReplaceOverrides(_);
                    anyChanges = true;
                }

                if (anyChanges)
                {
                    if (btnAdminGuardar != null)
                    {
                        btnAdminGuardar.Enabled = false;
                        btnAdminGuardar.BackColor = SystemColors.Control;
                    }

                    UpdateStatus("Cambios guardados exitosamente");
                    MessageBox.Show(
                        "Cambios guardados correctamente.",
                        "Éxito",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                else
                {
                    MessageBox.Show(
                        "No hay cambios pendientes para guardar.",
                        "Información",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Error al guardar cambios: {0}", ex.Message),
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion

        #region Context Menu Operations

        private void OpenContextSelected(bool asAdmin)
        {
            if (cmuModulo != null && cmuModulo.SourceControl is Control ctrl &&
                ctrl.Tag is ModuleDef module)
            {
                _moduleService.LaunchModule(ctrl.Name, module, asAdmin, ALLOWED_ROOTS, UpdateStatus);
            }
        }

        private void CopyModulePathFromContext()
        {
            if (cmuModulo != null && cmuModulo.SourceControl is Control ctrl &&
                ctrl.Tag is ModuleDef module)
            {
                if (!string.IsNullOrEmpty(module.ExePath))
                {
                    Clipboard.SetText(module.ExePath);
                    UpdateStatus(string.Format("Ruta copiada: {0}", module.ExePath));
                }
            }
        }

        private void ShowModulePropertiesFromContext()
        {
            if (cmuModulo != null && cmuModulo.SourceControl is Control ctrl &&
                ctrl.Tag is ModuleDef module)
            {
                string props = string.Format(
                    "Nombre: {0}\nRuta: {1}\nCarpeta de trabajo: {2}\nRequiere Admin: {3}\nRol mínimo: {4}\nPlanta: {5}\nEs Test: {6}",
                    module.Name,
                    module.ExePath,
                    module.WorkingDir,
                    module.RequiresElevation ? "Sí" : "No",
                    module.RolesMinTypeAut,
                    module.Plant,
                    module.IsTest ? "Sí" : "No"
                );

                MessageBox.Show(props, "Propiedades del Módulo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Status & UI Updates

        private void UpdateStatusBar()
        {
            if (tsslUser != null)
                tsslUser.Text = string.Format("Usuario: {0}", Session.LogonName ?? "N/A");

            if (tsslRole != null)
            {
                string roleName = GetRoleName(Session.TypeAut);
                tsslRole.Text = string.Format("Rol: {0}", roleName);
            }

            if (tsslPlant != null)
            {
                string plantName = GetPlantName(Session.Sucursal);
                tsslPlant.Text = string.Format("Planta: {0}", plantName);
            }
        }

        private string GetRoleName(int typeAut)
        {
            switch (typeAut)
            {
                case 1: return "Viewer";
                case 2: return "Operator";
                case 3: return "Supervisor";
                case 4: return "AdminDept";
                case 5: return "SysAdmin";
                default: return "Desconocido";
            }
        }

        private string GetPlantName(int plant)
        {
            switch (plant)
            {
                case 1: return "MTY";
                case 2: return "QRO";
                case 3: return "TIJ";
                default: return "N/A";
            }
        }

        private void UpdateStatus(string message)
        {
            if (tsslEstado != null)
            {
                tsslEstado.Text = message;
            }
            Debug.WriteLine(string.Format("Status: {0}", message));
        }

        #endregion

        #region Public Methods

        public void Logout()
        {
            var result = MessageBox.Show(
                "¿Está seguro que desea cerrar sesión?",
                "Confirmar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
        }

        #endregion

        #region Loading Spinner

        private LoadingSpinner ShowLoadingSpinner(string message)
        {
            var spinner = new LoadingSpinner
            {
                LoadingText = message ?? "Cargando...",
                Location = new Point(
                    (this.ClientSize.Width - 150) / 2,
                    (this.ClientSize.Height - 150) / 2
                ),
                Size = new Size(150, 150)
            };

            this.Controls.Add(spinner);
            spinner.BringToFront();
            spinner.Start();

            return spinner;
        }

        private void HideLoadingSpinner(LoadingSpinner spinner)
        {
            if (spinner != null)
            {
                spinner.Stop();
                this.Controls.Remove(spinner);
                spinner.Dispose();
            }
        }

        #endregion

        #region Search in Admin Tab

        private void txtSearchAd_TextChanged(object sender, EventArgs e)
        {
            if (txtSearchAd == null || _dataAccess == null) return;

            string searchName = txtSearchAd.Text;

            _dtUsers = _dataAccess.GetUsers();
            _overrides = _dataAccess.GetOverrides();

            _bsUsers.DataSource = _dtUsers.DefaultView;
            _bsUsers.Filter = string.Format("USU_UserLog LIKE '%{0}%'", searchName);

            if (dgvUsuarios != null)
            {
                dgvUsuarios.DataSource = _bsUsers;
            }

            RefreshOverridesForCurrentModule();
        }

        #endregion
    }
}