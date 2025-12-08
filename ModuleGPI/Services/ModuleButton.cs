using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace ModuleGPI.Controls
{
    public class ModuleButton : UserControl
    {
        private PictureBox picIcon;
        private Label lblText;
        private string _iconPath;
        private static readonly Icon DefaultIcon;
        private bool _isHovered = false;

        static ModuleButton()
        {
            DefaultIcon = SystemIcons.Application;
        }

        public ModuleButton()
        {
            // ESTILOS - SIN SupportsTransparentBackColor
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            // NO USAR: this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(120, 120);
            this.Cursor = Cursors.Hand;
            this.DoubleBuffered = true;
            this.Margin = new Padding(8);

            // COLOR SÓLIDO - NO TRANSPARENTE
            this.BackColor = Color.White;

            picIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point((this.Width - 48) / 2, 12),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White  // COLOR SÓLIDO
            };

            lblText = new Label
            {
                Size = new Size(this.Width - 8, 45),
                Location = new Point(4, 65),
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                BackColor = Color.White,  // COLOR SÓLIDO
                ForeColor = Color.FromArgb(50, 50, 50),
                AutoEllipsis = true,
                MaximumSize = new Size(this.Width - 8, 45)
            };

            this.Controls.Add(picIcon);
            this.Controls.Add(lblText);

            // Eventos hover
            this.MouseEnter += OnMouseEnterHandler;
            this.MouseLeave += OnMouseLeaveHandler;
            picIcon.MouseEnter += OnMouseEnterHandler;
            picIcon.MouseLeave += OnMouseLeaveHandler;
            lblText.MouseEnter += OnMouseEnterHandler;
            lblText.MouseLeave += OnMouseLeaveHandler;

            // Propagar click
            picIcon.Click += (s, e) => this.OnClick(e);
            lblText.Click += (s, e) => this.OnClick(e);
            picIcon.MouseDown += (s, e) => this.OnMouseDown(e);
            lblText.MouseDown += (s, e) => this.OnMouseDown(e);
        }

        public string ButtonText
        {
            get => lblText.Text;
            set => lblText.Text = value;
        }

        public string IconPath
        {
            get => _iconPath;
            set
            {
                _iconPath = value;
                LoadIcon();
            }
        }

        private void LoadIcon()
        {
            try
            {
                if (picIcon.Image != null)
                {
                    var oldImage = picIcon.Image;
                    picIcon.Image = null;
                    oldImage.Dispose();
                }

                // ⭐ VALIDACIÓN INICIAL
                if (string.IsNullOrEmpty(_iconPath))
                {
                    picIcon.Image = DefaultIcon.ToBitmap();
                    return;
                }

                // ⭐ CRÍTICO: Si es ruta de red y no existe rápido, usar icono por defecto
                if (_iconPath.StartsWith(@"\\"))
                {
                    // Timeout de 2 segundos para rutas de red
                    bool exists = CheckFileExistsWithTimeout(_iconPath, 2000);

                    if (!exists)
                    {
                        Debug.WriteLine($"⚠️ Timeout o archivo no existe: {_iconPath}");
                        picIcon.Image = DefaultIcon.ToBitmap();
                        return;
                    }
                }
                else if (!File.Exists(_iconPath))
                {
                    picIcon.Image = DefaultIcon.ToBitmap();
                    return;
                }

                string extension = Path.GetExtension(_iconPath).ToLower();

                if (extension == ".ico")
                {
                    using (var icon = new Icon(_iconPath, 48, 48))
                    {
                        picIcon.Image = icon.ToBitmap();
                    }
                }
                else if (extension == ".exe" || extension == ".dll")
                {
                    var icon = Icon.ExtractAssociatedIcon(_iconPath);
                    if (icon != null)
                    {
                        picIcon.Image = icon.ToBitmap();
                        icon.Dispose();
                    }
                    else
                    {
                        picIcon.Image = DefaultIcon.ToBitmap();
                    }
                }
                else
                {
                    picIcon.Image = DefaultIcon.ToBitmap();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error cargando icono: {ex.Message}");
                picIcon.Image = DefaultIcon.ToBitmap();
            }
        }

        // ⭐ MÉTODO HELPER: Verificar existencia con timeout
        private bool CheckFileExistsWithTimeout(string path, int timeoutMs)
        {
            try
            {
                var task = System.Threading.Tasks.Task.Run(() => File.Exists(path));

                if (task.Wait(timeoutMs))
                {
                    return task.Result;
                }
                else
                {
                    Debug.WriteLine($"⏱️ Timeout verificando: {path}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error verificando archivo: {ex.Message}");
                return false;
            }
        }
        private void OnMouseEnterHandler(object sender, EventArgs e)
        {
            if (!_isHovered)
            {
                _isHovered = true;
                this.BackColor = Color.FromArgb(229, 243, 255);
                picIcon.BackColor = Color.FromArgb(229, 243, 255);
                lblText.BackColor = Color.FromArgb(229, 243, 255);
                this.Invalidate();
            }
        }

        private void OnMouseLeaveHandler(object sender, EventArgs e)
        {
            Point mousePos = this.PointToClient(Control.MousePosition);
            if (!this.ClientRectangle.Contains(mousePos))
            {
                _isHovered = false;
                this.BackColor = Color.White;
                picIcon.BackColor = Color.White;
                lblText.BackColor = Color.White;
                this.Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = CreateRoundedRectangle(rect, 8))
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                Color borderColor = _isHovered ? Color.FromArgb(0, 120, 215) : Color.FromArgb(220, 220, 220);
                float borderWidth = _isHovered ? 2f : 1f;

                using (Pen pen = new Pen(borderColor, borderWidth))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // No pintar fondo predeterminado
        }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && picIcon?.Image != null)
            {
                picIcon.Image.Dispose();
                picIcon.Image = null;
            }
            base.Dispose(disposing);
        }
    }
}