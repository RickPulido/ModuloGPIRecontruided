using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using System.Drawing;

namespace ModuleGPI
{
    public partial class FormLogin : MaterialForm
    {
        readonly string _cs = ConfigurationManager
            .ConnectionStrings["DBConnectionString"].ConnectionString;
        private bool _passVisible = false;


        public FormLogin()
        {
            InitializeComponent();
            txtPass.UseSystemPasswordChar = true;

            var manager = MaterialSkinManager.Instance;
            manager.EnforceBackcolorOnAllComponents = true;   // asegura que respete el backcolor
            manager.AddFormToManage(this);
            manager.Theme = MaterialSkinManager.Themes.LIGHT;

            // Tu verde exacto:
            var primary = Color.FromArgb(119, 189, 27);              // #77BD1B
            // tonos derivados para app bar y estados
            var darkPrimary = ControlPaint.Dark(primary, 0.15f);    // ~15% más oscuro
            var lightPrimary = ControlPaint.Light(primary, 0.35f);   // ~35% más claro
            var accent = primary; // puedes usar el mismo como acento

            // Si tu versión de MaterialSkin.2 soporta ColorScheme con Color:
            manager.ColorScheme = new ColorScheme(
                primary,      // Primary
                darkPrimary,  // DarkPrimary (barra superior)
                lightPrimary, // LightPrimary
                accent,       // Accent
                TextShade.WHITE);


            this.StartPosition = FormStartPosition.CenterScreen;
            this.Sizable = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Text = "Modulos GPI";
            // -------------------------------------------

            // Ajustes visuales de tus controles actuales
            txtPass.UseSystemPasswordChar = true;

            // Texto blanco en labels
            label1.ForeColor = Color.Black;
            label2.ForeColor = Color.Black;

            // Fondo general ligeramente más oscuro para contraste
            this.BackColor = darkPrimary; // app bar + fondo en la misma gama

            // Estado inicial
            _passVisible = false;
            ApplyPasswordMask();
            UpdateLoginUI();

            // Validación en vivo para habilitar Enter/Intro
            txtUser.TextChanged += (s, e) => UpdateLoginUI();
            txtPass.TextChanged += (s, e) => UpdateLoginUI();

            // Permite Enter al estar en los textbox (AcceptButton se maneja en UpdateLoginUI)
            this.KeyPreview = true; // por si quieres capturar otras teclas luego


        }

        private void UpdateLoginUI()
        {
            bool filled = !string.IsNullOrWhiteSpace(txtUser.Text) &&
                          !string.IsNullOrWhiteSpace(txtPass.Text);

            btnLogin.Enabled = filled;
            // Permite Enter/Intro solo si ambos campos están llenos
            this.AcceptButton = filled ? btnLogin : null;
        }

        private void ApplyPasswordMask()
        {
            txtPass.UseSystemPasswordChar = !_passVisible;
            // Cambia icono/emoji según estado
            btnTogglePass.Text = _passVisible ? "🙈" : "👁";
            // Si usas recursos:
            // btnTogglePass.Image = _passVisible ? Properties.Resources.eye_off16 : Properties.Resources.eye16;
        }

        private void btnTogglePass_Click(object sender, EventArgs e)
        {
            _passVisible = !_passVisible;
            ApplyPasswordMask();
            txtPass.Focus();
            txtPass.SelectionStart = txtPass.TextLength;
        }


        private void btnLogin_Click(object sender, EventArgs e)
        {
            var user = txtUser.Text.Trim();
            var pass = txtPass.Text;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Ingresa usuario y contraseña.");
                return;
            }
            //ACTIVAR EN CUANTO SE ACABE EL DESARROLLO    



            //1) Validar con Active Directory
            //if (!ValidateAdLogin(user, pass))
            //{
            //    MessageBox.Show("Usuario o contraseña incorrectos.");
            //    return;
            //}






            try
            {
                // 2) Llamar SP para verificar permisos y traer datos
                string empId = null;
                using (var con = new SqlConnection(_cs))
                using (var cmd = new SqlCommand("dbo.ModGPI_Login", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Logon", SqlDbType.NVarChar, 25).Value = user;
                    cmd.Parameters.Add("@pass", SqlDbType.NVarChar, 25).Value = pass; // e  

                    con.Open();
                    var o = cmd.ExecuteScalar();
                    empId = o?.ToString();
                }

                if (string.IsNullOrEmpty(empId))
                {
                    MessageBox.Show("No tienes permisos para este módulo o el usuario está inactivo.");
                    return;
                }

                // 3) Traer más info del usuario
                DataRow info;
                using (var con = new SqlConnection(_cs))
                using (var da = new SqlDataAdapter(@"
                    SELECT USU_EmpID, USU_UserLog, USU_TypeAut, USU_Status, USU_UserPLant
                    FROM ModGPI_User
                    WHERE USU_EmpID = @empId AND USU_Status = 1;", con))
                {
                    da.SelectCommand.Parameters.Add("@empId", SqlDbType.NVarChar, 10).Value = empId;
                    var dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows.Count == 0)
                    {
                        MessageBox.Show("No se encontró información del usuario.");
                        return;
                    }
                    info = dt.Rows[0];


                }

                // 4) Guardar sesión
                Session.LogonName = user;
                Session.Sucursal = info["USU_UserPLant"].ToString();
                Session.TypeAut = Convert.ToInt32(info["USU_TypeAut"]);
                Session.EmpId = empId;
                // 5) Indicar éxito al Program.cs
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de login: " + ex.Message);
            }
        }

        private bool ValidateAdLogin(string username, string password)
        {
            var domain = ConfigurationManager.AppSettings["AD_Domain"];
            var container = ConfigurationManager.AppSettings["AD_Container"]; // puede ser null

            try
            {
                using (var context = string.IsNullOrWhiteSpace(container)
                    ? new PrincipalContext(ContextType.Domain, domain)
                    : new PrincipalContext(ContextType.Domain, domain, container))
                {
                    return context.ValidateCredentials(username, password, ContextOptions.Negotiate);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error validando en Active Directory: " + ex.Message);
                return false;
            }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
