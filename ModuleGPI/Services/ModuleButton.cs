using System;
using System.Drawing;
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

        static ModuleButton()
        {
            // Icono por defecto (puedes usar un icono embebido o System.Drawing)
            DefaultIcon = SystemIcons.Application;
        }

        public ModuleButton()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(100, 100);
            this.Cursor = Cursors.Hand;
            this.BackColor = Color.Transparent;

            // PictureBox para el icono
            picIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point((this.Width - 48) / 2, 10),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };

            // Label para el texto
            lblText = new Label
            {
                Size = new Size(this.Width, 30),
                Location = new Point(0, 62),
                TextAlign = ContentAlignment.TopCenter,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                BackColor = Color.Transparent,
                AutoEllipsis = true
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

        // Propiedades públicas
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
                    // Usar icono por defecto
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
                        picIcon.Image = new Icon(icon, 48, 48).ToBitmap();
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
            this.BackColor = Color.FromArgb(229, 243, 255); // Azul claro
        }

        private void ModuleButton_MouseLeave(object sender, EventArgs e)
        {
            this.BackColor = Color.Transparent;
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