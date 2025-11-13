using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    public class UIHelpers : IUIHelpers
    {
        public void PositionHeaderSearchBoxes(
            Panel pnlOpHeader, Button btnOpRefrescar, TextBox txtOpSearch,
            Panel pnlConsHeader, TextBox txtConsSearch)
        {
            if (pnlOpHeader == null || pnlConsHeader == null) return;

            txtOpSearch.Location = new Point(pnlOpHeader.Width - txtOpSearch.Width - 10, 5);
            btnOpRefrescar.Location = new Point(txtOpSearch.Left - btnOpRefrescar.Width - 5, 5);

            txtConsSearch.Location = new Point(pnlConsHeader.Width - txtConsSearch.Width - 10, 5);
        }

        public void EnableDgvDoubleBuffer(DataGridView dgv)
        {
            try
            {
                var pi = typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
                pi?.SetValue(dgv, true, null);
            }
            catch { /* ignore */ }
        }

        public DataGridView BuildOverridesGrid(Panel parent, int height)
        {
            var dgv = new DataGridView
            {
                Dock = DockStyle.Bottom,
                Height = height
            };
            parent.Controls.Add(dgv);
            return dgv;
        }

        public void ConfigureGridColumn(DataGridView dgv, string dataProperty, Action<DataGridViewColumn> apply)
        {
            var col = dgv.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.DataPropertyName == dataProperty);
            if (col != null) apply(col);
        }
    }
}