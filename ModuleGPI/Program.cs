//using GPI.Launcher;
using ModuleGPI;
using ModuleGPI.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModuleGPI
{
    internal static class Program
    {
        private static Mutex _singleInstanceMutex;

        [STAThread]
        static void Main()
        {
            bool createdNew = false;
            _singleInstanceMutex = new Mutex(true, "ModuleGPI_SingleInstance", out createdNew);
            if (!createdNew)
            {
                MessageBox.Show("La aplicación ya está en ejecución.", "ModuleGPI",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.ThreadException += (s, e) =>
                MessageBox.Show("Error de aplicación: " + e.Exception.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                    MessageBox.Show("Error no controlado: " + ex.Message, "Error fatal",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ✅ INICIAR PRECARGA INMEDIATAMENTE (antes del login)
            ModulesCache.StartLoading();

            while (true)
            {
                // 1) Login
                using (var login = new FormLogin())
                {
                    var r = login.ShowDialog();
                    if (r != DialogResult.OK)
                    {
                        ModulesCache.Clear(); // Limpiar si cancela
                        break;
                    }
                }

                // 2) Shell principal
                using (var main = new MainForm())
                {
                    var r = main.ShowDialog();

                    if (r == DialogResult.Abort)
                    {
                        // Logout - limpiar cache para recargar con nuevo usuario
                        ModulesCache.Clear();
                        continue;
                    }

                    break;
                }
            }
        }
    }
}
