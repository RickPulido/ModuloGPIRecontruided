using System.Windows.Forms;

namespace ModuleGPI.UI
{
    public interface IUIHelpers
    {
        /// <summary>
        /// Posiciona las cajas de búsqueda/refresh en los headers (Operación/Consultas).
        /// </summary>
        void PositionHeaderSearchBoxes(
            Panel pnlOpHeader, Button btnOpRefrescar, TextBox txtOpSearch,
            Panel pnlConsHeader, TextBox txtConsSearch);

        /// <summary>
        /// Activa doble buffer para suavizar el scroll/dibujo de DataGridView.
        /// </summary>
        void EnableDgvDoubleBuffer(DataGridView dgv);
    }
}