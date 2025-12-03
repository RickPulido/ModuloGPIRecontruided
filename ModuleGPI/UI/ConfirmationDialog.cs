using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    /// <summary>
    /// Diálogo de confirmación moderno con animación
    /// </summary>
    public class ConfirmationDialog : Form
    {
        private Label lblMessage;
        private Button btnYes;
        private Button btnNo;
        private PictureBox picIcon;

        public ConfirmationDialog(string message, string title = "Confirmar", MessageBoxIcon icon = MessageBoxIcon.Question)
        {
            InitializeComponent();
            this.Text = title;
            lblMessage.Text = message;
            SetIcon(icon);
        }

        private void InitializeComponent()
        {
            this.Size = new Size(450, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            // Icono
            picIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(20, 20),
                SizeMode = PictureBoxSizeMode.StretchImage
            };
            this.Controls.Add(picIcon);

            // Mensaje
            lblMessage = new Label
            {
                Location = new Point(80, 20),
                Size = new Size(350, 60),
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(51, 51, 51)
            };
            this.Controls.Add(lblMessage);

            // Botón Sí
            btnYes = new Button
            {
                Text = "Sí",
                Size = new Size(100, 35),
                Location = new Point(240, 95),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                DialogResult = DialogResult.Yes,
                Cursor = Cursors.Hand
            };
            btnYes.FlatAppearance.BorderSize = 0;
            this.Controls.Add(btnYes);

            // Botón No
            btnNo = new Button
            {
                Text = "No",
                Size = new Size(100, 35),
                Location = new Point(350, 95),
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F),
                DialogResult = DialogResult.No,
                Cursor = Cursors.Hand
            };
            btnNo.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
            this.Controls.Add(btnNo);

            this.AcceptButton = btnYes;
            this.CancelButton = btnNo;
        }

        private void SetIcon(MessageBoxIcon icon)
        {
            switch (icon)
            {
                case MessageBoxIcon.Question:
                    picIcon.Image = SystemIcons.Question.ToBitmap();
                    break;
                case MessageBoxIcon.Warning:
                    picIcon.Image = SystemIcons.Warning.ToBitmap();
                    break;
                case MessageBoxIcon.Error:
                    picIcon.Image = SystemIcons.Error.ToBitmap();
                    break;
                case MessageBoxIcon.Information:
                    picIcon.Image = SystemIcons.Information.ToBitmap();
                    break;
            }
        }

        /// <summary>
        /// Muestra diálogo de confirmación moderno
        /// </summary>
        public static bool Show(string message, string title = "Confirmar", MessageBoxIcon icon = MessageBoxIcon.Question)
        {
            using (var dialog = new ConfirmationDialog(message, title, icon))
            {
                return dialog.ShowDialog() == DialogResult.Yes;
            }
        }
    }
}