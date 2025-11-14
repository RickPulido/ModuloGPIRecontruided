using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    public sealed class UIHelpers : IUIHelpers
    {
        public void PositionHeaderSearchBoxes(
            Panel pnlOpHeader, Button btnOpRefrescar, TextBox txtOpSearch,
            Panel pnlConsHeader, TextBox txtConsSearch)
        {
            if (pnlOpHeader != null && pnlOpHeader.ClientSize.Width > 0 && btnOpRefrescar != null && txtOpSearch != null)
            {
                int r = pnlOpHeader.ClientSize.Width - 8;
                btnOpRefrescar.Width = 90;
                btnOpRefrescar.Location = new Point(r - btnOpRefrescar.Width, 10);
                txtOpSearch.Width = 220;
                txtOpSearch.Location = new Point(btnOpRefrescar.Left - txtOpSearch.Width - 8, 12);
            }

            if (pnlConsHeader != null && pnlConsHeader.ClientSize.Width > 0 && txtConsSearch != null)
            {
                int r2 = pnlConsHeader.ClientSize.Width - 8;
                txtConsSearch.Width = 220;
                txtConsSearch.Location = new Point(r2 - txtConsSearch.Width, 12);
            }
        }

        public void EnableDgvDoubleBuffer(DataGridView dgv)
        {
            if (dgv == null) return;
            try
            {
                var pi = typeof(DataGridView).GetProperty("DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                pi?.SetValue(dgv, true, null);
            }
            catch { /* ignore */ }
        }
    }
}