using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    /// <summary>
    /// Overlay con spinner de carga que bloquea la UI
    /// </summary>
    public class LoadingOverlay : Panel
    {
        private Timer _timer;
        private int _angle = 0;
        private Label _lblMessage;
        private const int SPINNER_SIZE = 60;

        public LoadingOverlay()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(180, 255, 255, 255); // Semi-transparente
            this.Visible = false;
            this.BringToFront();

            // Label para mensaje
            _lblMessage = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(_lblMessage);

            // Timer para animación
            _timer = new Timer { Interval = 50 };
            _timer.Tick += (s, e) =>
            {
                _angle = (_angle + 15) % 360;
                this.Invalidate();
            };

            this.Paint += LoadingOverlay_Paint;
            this.Resize += (s, e) => CenterControls();
        }

        private void LoadingOverlay_Paint(object sender, PaintEventArgs e)
        {
            if (!this.Visible) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Dibujar spinner circular
            int centerX = this.Width / 2;
            int centerY = (this.Height / 2) - 30;

            using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 4))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;

                e.Graphics.DrawArc(pen,
                    centerX - SPINNER_SIZE / 2,
                    centerY - SPINNER_SIZE / 2,
                    SPINNER_SIZE,
                    SPINNER_SIZE,
                    _angle,
                    270);
            }
        }

        private void CenterControls()
        {
            if (_lblMessage != null)
            {
                _lblMessage.Location = new Point(
                    (this.Width - _lblMessage.Width) / 2,
                    (this.Height / 2) + 40);
            }
        }

        /// <summary>
        /// Muestra el loading overlay con un mensaje
        /// </summary>
        public void Show(string message = "Cargando...")
        {
            _lblMessage.Text = message;
            CenterControls();
            this.Visible = true;
            this.BringToFront();
            _timer.Start();
            Application.DoEvents();
        }

        /// <summary>
        /// Oculta el loading overlay
        /// </summary>
        public new void Hide()
        {
            _timer.Stop();
            this.Visible = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Ejecuta una acción con loading overlay
        /// </summary>
        public static void Execute(Control parent, Action action, string message = "Procesando...")
        {
            var overlay = new LoadingOverlay();
            parent.Controls.Add(overlay);
            overlay.Show(message);

            try
            {
                action?.Invoke();
            }
            finally
            {
                overlay.Hide();
                parent.Controls.Remove(overlay);
                overlay.Dispose();
            }
        }
    }
}