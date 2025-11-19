using ModuleGPI.Domain;
using System;
using System.IO;
using System.Windows.Forms;

namespace ModuleGPI
{
    public partial class ModuleEditForm : Form
    {
        #region Fields
        private ModuleDef _module;
        private bool _isNewModule;

        // Controls
        private TextBox txtButtonName;
        private TextBox txtName;
        private TextBox txtExePath;
        private TextBox txtWorkingDir;
        private ComboBox cboCategory;
        private ComboBox cboRoleMin;
        private NumericUpDown nudPlant;
        private CheckBox chkRequiresElevation;
        private Button btnBrowseExe;
        private Button btnBrowseDir;
        private Button btnSave;
        private Button btnCancel;
        private Button btnTest;
        private GroupBox grpBasic;
        private GroupBox grpPaths;
        private GroupBox grpPermissions;
        #endregion

        #region Properties
        public ModuleDef Module => _module;
        #endregion

        #region Constructor
        public ModuleEditForm(ModuleDef module)
        {
            _isNewModule = (module == null);
            _module = module ?? new ModuleDef
            {
                ButtonName = $"btnMod_{DateTime.Now:yyyyMMddHHmmss}",
                Category = "Operación",
                RolesMinTypeAut = 2,
                Plant = 1
            };

            InitializeComponent();
            LoadModuleData();
            SetFormTitle();
        }
        #endregion

        #region Form Designer
        private void InitializeComponent()
        {
            // Form settings
            this.Text = "Editar Módulo";
            this.Size = new System.Drawing.Size(650, 500);
            this.MinimumSize = new System.Drawing.Size(650, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // === GRUPO: Información Básica ===
            grpBasic = new GroupBox
            {
                Text = "Información Básica",
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(610, 120)
            };

            var lblButtonName = new Label
            {
                Text = "Nombre Botón:",
                Location = new System.Drawing.Point(15, 25),
                Size = new System.Drawing.Size(100, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            txtButtonName = new TextBox
            {
                Location = new System.Drawing.Point(120, 25),
                Size = new System.Drawing.Size(200, 23),
                MaxLength = 80
            };

            var lblName = new Label
            {
                Text = "Nombre Módulo:",
                Location = new System.Drawing.Point(15, 55),
                Size = new System.Drawing.Size(100, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            txtName = new TextBox
            {
                Location = new System.Drawing.Point(120, 55),
                Size = new System.Drawing.Size(300, 23),
                MaxLength = 100
            };

            var lblCategory = new Label
            {
                Text = "Categoría:",
                Location = new System.Drawing.Point(15, 85),
                Size = new System.Drawing.Size(100, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            cboCategory = new ComboBox
            {
                Location = new System.Drawing.Point(120, 85),
                Size = new System.Drawing.Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboCategory.Items.AddRange(new[] { "Operación", "Consultas" });

            grpBasic.Controls.AddRange(new Control[]
            {
                lblButtonName, txtButtonName,
                lblName, txtName,
                lblCategory, cboCategory
            });

            // === GRUPO: Rutas ===
            grpPaths = new GroupBox
            {
                Text = "Rutas y Directorios",
                Location = new System.Drawing.Point(12, 140),
                Size = new System.Drawing.Size(610, 120)
            };

            var lblExePath = new Label
            {
                Text = "Ejecutable:",
                Location = new System.Drawing.Point(15, 25),
                Size = new System.Drawing.Size(100, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            txtExePath = new TextBox
            {
                Location = new System.Drawing.Point(120, 25),
                Size = new System.Drawing.Size(420, 23),
                MaxLength = 500
            };

            btnBrowseExe = new Button
            {
                Text = "...",
                Location = new System.Drawing.Point(545, 24),
                Size = new System.Drawing.Size(40, 25)
            };
            btnBrowseExe.Click += BtnBrowseExe_Click;

            var lblWorkingDir = new Label
            {
                Text = "Dir. Trabajo:",
                Location = new System.Drawing.Point(15, 55),
                Size = new System.Drawing.Size(100, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            txtWorkingDir = new TextBox
            {
                Location = new System.Drawing.Point(120, 55),
                Size = new System.Drawing.Size(420, 23),
                MaxLength = 500
            };

            btnBrowseDir = new Button
            {
                Text = "...",
                Location = new System.Drawing.Point(545, 54),
                Size = new System.Drawing.Size(40, 25)
            };
            btnBrowseDir.Click += BtnBrowseDir_Click;

            btnTest = new Button
            {
                Text = "Probar Módulo",
                Location = new System.Drawing.Point(120, 85),
                Size = new System.Drawing.Size(120, 25)
            };
            btnTest.Click += BtnTest_Click;

            grpPaths.Controls.AddRange(new Control[]
            {
                lblExePath, txtExePath, btnBrowseExe,
                lblWorkingDir, txtWorkingDir, btnBrowseDir,
                btnTest
            });

            // === GRUPO: Permisos ===
            grpPermissions = new GroupBox
            {
                Text = "Permisos y Configuración",
                Location = new System.Drawing.Point(12, 270),
                Size = new System.Drawing.Size(610, 120)
            };

            var lblRoleMin = new Label
            {
                Text = "Rol Mínimo:",
                Location = new System.Drawing.Point(15, 25),
                Size = new System.Drawing.Size(100, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            cboRoleMin = new ComboBox
            {
                Location = new System.Drawing.Point(120, 25),
                Size = new System.Drawing.Size(150, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboRoleMin.Items.AddRange(new object[]
            {
                new { Value = 1, Text = "1 - Viewer" },
                new { Value = 2, Text = "2 - Operator" },
                new { Value = 3, Text = "3 - Supervisor" },
                new { Value = 4, Text = "4 - AdminDept" },
                new { Value = 5, Text = "5 - SysAdmin" }
            });
            cboRoleMin.DisplayMember = "Text";
            cboRoleMin.ValueMember = "Value";

            var lblPlant = new Label
            {
                Text = "Planta:",
                Location = new System.Drawing.Point(15, 55),
                Size = new System.Drawing.Size(100, 23),
                TextAlign = System.Drawing.ContentAlignment.MiddleRight
            };

            nudPlant = new NumericUpDown
            {
                Location = new System.Drawing.Point(120, 55),
                Size = new System.Drawing.Size(100, 23),
                Minimum = 1,
                Maximum = 99,
                Value = 1
            };

            chkRequiresElevation = new CheckBox
            {
                Text = "Requiere elevación de privilegios (Ejecutar como Administrador)",
                Location = new System.Drawing.Point(120, 85),
                Size = new System.Drawing.Size(400, 23),
                AutoSize = true
            };

            grpPermissions.Controls.AddRange(new Control[]
            {
                lblRoleMin, cboRoleMin,
                lblPlant, nudPlant,
                chkRequiresElevation
            });

            // === BOTONES ===
            btnSave = new Button
            {
                Text = _isNewModule ? "Crear" : "Guardar",
                Location = new System.Drawing.Point(466, 410),
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.OK
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancelar",
                Location = new System.Drawing.Point(547, 410),
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            // Agregar todos los controles al formulario
            this.Controls.AddRange(new Control[]
            {
                grpBasic, grpPaths, grpPermissions,
                btnSave, btnCancel
            });

            // Establecer botones de aceptar/cancelar
            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }
        #endregion

        #region Private Methods
        private void SetFormTitle()
        {
            this.Text = _isNewModule ? "Crear Nuevo Módulo" : $"Editar Módulo: {_module.Name}";

            // Si es nuevo, el campo ButtonName es editable
            txtButtonName.ReadOnly = !_isNewModule;

            if (!_isNewModule)
            {
                txtButtonName.BackColor = System.Drawing.SystemColors.Control;
            }
        }

        private void LoadModuleData()
        {
            txtButtonName.Text = _module.ButtonName;
            txtName.Text = _module.Name;
            txtExePath.Text = _module.ExePath;
            txtWorkingDir.Text = _module.WorkingDir;

            // Seleccionar categoría
            cboCategory.SelectedItem = _module.Category ?? "Operación";

            // Seleccionar rol mínimo
            for (int i = 0; i < cboRoleMin.Items.Count; i++)
            {
                dynamic item = cboRoleMin.Items[i];
                if (item.Value == _module.RolesMinTypeAut)
                {
                    cboRoleMin.SelectedIndex = i;
                    break;
                }
            }

            nudPlant.Value = _module.Plant > 0 ? _module.Plant : 1;
            chkRequiresElevation.Checked = _module.RequiresElevation;
        }

        private bool ValidateInput()
        {
            // Validar ButtonName
            if (string.IsNullOrWhiteSpace(txtButtonName.Text))
            {
                MessageBox.Show("El nombre del botón es obligatorio.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtButtonName.Focus();
                return false;
            }

            // Validar que empiece con btnMod_
            if (!txtButtonName.Text.StartsWith("btnMod_"))
            {
                MessageBox.Show("El nombre del botón debe comenzar con 'btnMod_'",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtButtonName.Focus();
                return false;
            }

            // Validar Name
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("El nombre del módulo es obligatorio.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }

            // Validar ExePath
            if (string.IsNullOrWhiteSpace(txtExePath.Text))
            {
                MessageBox.Show("La ruta del ejecutable es obligatoria.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtExePath.Focus();
                return false;
            }

            // Verificar si el archivo existe (advertencia, no error)
            if (!File.Exists(txtExePath.Text))
            {
                var result = MessageBox.Show(
                    $"El archivo no existe:\n{txtExePath.Text}\n\n" +
                    "¿Desea continuar de todas formas?",
                    "Advertencia",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                    return false;
            }

            // Validar Category
            if (cboCategory.SelectedItem == null)
            {
                MessageBox.Show("Seleccione una categoría.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboCategory.Focus();
                return false;
            }

            // Validar RoleMin
            if (cboRoleMin.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un rol mínimo.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cboRoleMin.Focus();
                return false;
            }

            return true;
        }
        #endregion

        #region Event Handlers
        private void BtnBrowseExe_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Seleccionar Ejecutable";
                dialog.Filter = "Ejecutables (*.exe)|*.exe|Todos los archivos (*.*)|*.*";
                dialog.CheckFileExists = false; // Permitir seleccionar archivos en red

                if (!string.IsNullOrEmpty(txtExePath.Text))
                {
                    dialog.FileName = Path.GetFileName(txtExePath.Text);

                    string dir = Path.GetDirectoryName(txtExePath.Text);
                    if (Directory.Exists(dir))
                        dialog.InitialDirectory = dir;
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtExePath.Text = dialog.FileName;

                    // Auto-llenar WorkingDir si está vacío
                    if (string.IsNullOrWhiteSpace(txtWorkingDir.Text))
                    {
                        txtWorkingDir.Text = Path.GetDirectoryName(dialog.FileName);
                    }

                    // Auto-llenar Name si está vacío
                    if (string.IsNullOrWhiteSpace(txtName.Text))
                    {
                        txtName.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                    }
                }
            }
        }

        private void BtnBrowseDir_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Seleccionar Directorio de Trabajo";
                dialog.ShowNewFolderButton = false;

                if (!string.IsNullOrEmpty(txtWorkingDir.Text) && Directory.Exists(txtWorkingDir.Text))
                {
                    dialog.SelectedPath = txtWorkingDir.Text;
                }
                else if (!string.IsNullOrEmpty(txtExePath.Text))
                {
                    string dir = Path.GetDirectoryName(txtExePath.Text);
                    if (Directory.Exists(dir))
                        dialog.SelectedPath = dir;
                }

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtWorkingDir.Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtExePath.Text))
            {
                MessageBox.Show("Ingrese la ruta del ejecutable primero.",
                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!File.Exists(txtExePath.Text))
            {
                MessageBox.Show($"El archivo no existe:\n{txtExePath.Text}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = txtExePath.Text,
                    WorkingDirectory = string.IsNullOrEmpty(txtWorkingDir.Text) ?
                                      Path.GetDirectoryName(txtExePath.Text) :
                                      txtWorkingDir.Text,
                    UseShellExecute = true
                };

                if (chkRequiresElevation.Checked)
                {
                    psi.Verb = "runas";
                }

                System.Diagnostics.Process.Start(psi);

                MessageBox.Show("Módulo lanzado exitosamente para prueba.",
                    "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al lanzar módulo:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                this.DialogResult = DialogResult.None;
                return;
            }

            // Actualizar el objeto módulo con los valores del formulario
            _module.ButtonName = txtButtonName.Text.Trim();
            _module.Name = txtName.Text.Trim();
            _module.ExePath = txtExePath.Text.Trim();
            _module.WorkingDir = txtWorkingDir.Text.Trim();
            _module.Category = cboCategory.SelectedItem?.ToString() ?? "Operación";

            dynamic selectedRole = cboRoleMin.SelectedItem;
            _module.RolesMinTypeAut = selectedRole?.Value ?? 1;

            _module.Plant = (int)nudPlant.Value;
            _module.RequiresElevation = chkRequiresElevation.Checked;

            this.DialogResult = DialogResult.OK;
        }
        #endregion
    }
}