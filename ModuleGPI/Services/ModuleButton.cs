using System;
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
            // ✅ CRÍTICO: Activar soporte para BackColor transparente ANTES de usarlo
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(120, 120);
            this.Cursor = Cursors.Hand;

            // ✅ AHORA SÍ podemos usar Color.Transparent
            this.BackColor = Color.Transparent;
            this.DoubleBuffered = true;

            // PictureBox para el icono
            picIcon = new PictureBox
            {
                Size = new Size(64, 64),
                Location = new Point((this.Width - 64) / 2, 15),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };

            // Label para el texto
            lblText = new Label
            {
                Size = new Size(this.Width - 10, 35),
                Location = new Point(5, 85),
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                MaximumSize = new Size(this.Width - 10, 35)
            };

            this.Controls.Add(picIcon);
            this.Controls.Add(lblText);

            // Eventos hover
            this.MouseEnter += ModuleButton_MouseEnter;
            this.MouseLeave += ModuleButton_MouseLeave;
            picIcon.MouseEnter += (s, e) => ModuleButton_MouseEnter(this, e);
            picIcon.MouseLeave += (s, e) => ModuleButton_MouseLeave(this, e);
            lblText.MouseEnter += (s, e) => ModuleButton_MouseEnter(this, e);
            lblText.MouseLeave += (s, e) => ModuleButton_MouseLeave(this, e);

            // Propagar click
            picIcon.Click += (s, e) => this.OnClick(e);
            lblText.Click += (s, e) => this.OnClick(e);
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
                if (string.IsNullOrEmpty(_iconPath) || !File.Exists(_iconPath))
                {
                    picIcon.Image = DefaultIcon.ToBitmap();
                    return;
                }

                string extension = Path.GetExtension(_iconPath).ToLower();

                if (extension == ".ico")
                {
                    using (var icon = new Icon(_iconPath, 64, 64))
                    {
                        picIcon.Image = icon.ToBitmap();
                    }
                }
                else if (extension == ".exe" || extension == ".dll")
                {
                    var icon = Icon.ExtractAssociatedIcon(_iconPath);
                    if (icon != null)
                    {
                        picIcon.Image = new Icon(icon, 64, 64).ToBitmap();
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
            catch
            {
                picIcon.Image = DefaultIcon.ToBitmap();
            }
        }

        private void ModuleButton_MouseEnter(object sender, EventArgs e)
        {
            _isHovered = true;
            this.BackColor = Color.FromArgb(229, 243, 255);  // Azul claro hover
            this.Invalidate();
        }

        private void ModuleButton_MouseLeave(object sender, EventArgs e)
        {
            _isHovered = false;
            this.BackColor = Color.Transparent;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_isHovered)
            {
                using (Pen pen = new Pen(Color.FromArgb(0, 120, 215), 2))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                    // Borde redondeado
                    using (GraphicsPath path = GetRoundedRectangle(new Rectangle(1, 1, this.Width - 3, this.Height - 3), 8))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        private GraphicsPath GetRoundedRectangle(Rectangle rect, int radius)
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
            if (disposing)
            {
                picIcon?.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}