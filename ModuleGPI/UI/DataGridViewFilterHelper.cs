using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    /// <summary>
    /// Helper para agregar fila de filtro a DataGridViews
    /// </summary>
    public static class DataGridViewFilterHelper
    {
        /// <summary>
        /// Agrega una fila de filtro encima del DataGridView
        /// </summary>
        public static Panel AddFilterRow(DataGridView dgv)
        {
            if (dgv == null || dgv.Parent == null)
                return null;

            // Crear panel de filtros
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(245, 245, 245),
                Padding = new Padding(3)
            };

            // Crear TableLayoutPanel para alinear textboxes con columnas
            var filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = false,
                BackColor = Color.FromArgb(245, 245, 245),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single
            };

            // Esperar a que el grid esté listo
            if (dgv.Columns.Count > 0)
            {
                BuildFilterControls(dgv, filterLayout);
            }
            else
            {
                dgv.DataBindingComplete += (s, e) => BuildFilterControls(dgv, filterLayout);
            }

            filterPanel.Controls.Add(filterLayout);

            // Insertar el panel antes del DataGridView
            var parent = dgv.Parent;
            int index = parent.Controls.GetChildIndex(dgv);
            parent.Controls.Add(filterPanel);
            parent.Controls.SetChildIndex(filterPanel, index);

            return filterPanel;
        }

        private static void BuildFilterControls(DataGridView dgv, TableLayoutPanel layout)
        {
            layout.SuspendLayout();
            layout.Controls.Clear();
            layout.ColumnStyles.Clear();

            int visibleColumns = 0;
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Visible) visibleColumns++;
            }

            layout.ColumnCount = visibleColumns;

            int colIndex = 0;
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (!col.Visible) continue;

                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, col.Width));

                var txtFilter = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 8.5F),
                    Margin = new Padding(2),
                    Tag = col.DataPropertyName,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Placeholder
                txtFilter.SetPlaceholder($"🔍 {col.HeaderText}");

                // Evento de filtrado
                txtFilter.TextChanged += (s, e) => ApplyFilter(dgv, layout);

                layout.Controls.Add(txtFilter, colIndex, 0);
                colIndex++;
            }

            layout.ResumeLayout();
        }

        private static void ApplyFilter(DataGridView dgv, TableLayoutPanel layout)
        {
            if (dgv.DataSource == null) return;

            var bs = dgv.DataSource as BindingSource;
            if (bs == null) return;

            string filter = "";

            foreach (Control ctrl in layout.Controls)
            {
                if (ctrl is TextBox txt)
                {
                    string realText = txt.GetRealText();

                    if (!string.IsNullOrWhiteSpace(realText))
                    {
                        string columnName = txt.Tag?.ToString();
                        if (!string.IsNullOrEmpty(columnName))
                        {
                            if (filter.Length > 0) filter += " AND ";

                            // Escapar comillas simples
                            string escapedText = realText.Replace("'", "''");

                            filter += $"Convert([{columnName}], 'System.String') LIKE '%{escapedText}%'";
                        }
                    }
                }
            }

            try
            {
                bs.Filter = filter;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error aplicando filtro: {ex.Message}");
            }
        }
    }
}