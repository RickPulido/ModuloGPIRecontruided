using System;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    public interface IUIHelpers
    {
        void PositionHeaderSearchBoxes(
            Panel pnlOpHeader, Button btnOpRefrescar, TextBox txtOpSearch,
            Panel pnlConsHeader, TextBox txtConsSearch);

        void EnableDgvDoubleBuffer(DataGridView dgv);

        DataGridView BuildOverridesGrid(Panel parent, int height);

        void ConfigureGridColumn(DataGridView dgv, string dataProperty, Action<DataGridViewColumn> apply);
    }
}