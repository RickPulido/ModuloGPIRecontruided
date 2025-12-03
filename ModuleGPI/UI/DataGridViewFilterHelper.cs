using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    /// <summary>
    /// Helper para agregar filtrado en tiempo real a DataGridViews
    /// </summary>
    public class DataGridViewFilterHelper
    {
        private DataGridView _grid;
        private Panel _filterPanel;
        private DataTable _originalData;
        private BindingSource _bindingSource;

        public DataGridViewFilterHelper(DataGridView grid)
        {
            _grid = grid;
            _bindingSource = new BindingSource();
        }

        /// <summary>
        /// Agrega una fila de filtros encima del DataGridView
        /// </summary>
        public void AddFilterRow()
        {
            if (_grid.Parent == null) return;

            // Guardar datos originales
            if (_grid.DataSource is DataTable dt)
            {
                _originalData = dt.Copy();
            }
            else if (_grid.DataSource is BindingSource bs && bs.DataSource is DataTable dt2)
            {
                _originalData = dt2.Copy();
            }

            // Crear panel de filtros
            _filterPanel = new Panel
            {
                Height = 35,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(2)
            };

            // Agregar label de instrucción
            var lblInstruction = new Label
            {
                Text = "🔍 Filtrar:",
                AutoSize = true,
                Location = new Point(5, 10),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _filterPanel.Controls.Add(lblInstruction);

            // Crear textbox de filtro global
            var txtFilter = new TextBox
            {
                Width = 250,
                Location = new Point(70, 7),
                PlaceholderText = "Buscar en todas las columnas..."
            };

            txtFilter.TextChanged += (s, e) => ApplyFilter(txtFilter.Text);
            _filterPanel.Controls.Add(txtFilter);

            // Botón para limpiar filtro
            var btnClear = new Button
            {
                Text = "✖",
                Width = 30,
                Height = 23,
                Location = new Point(325, 6),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnClear.FlatAppearance.BorderColor = Color.Gray;
            btnClear.Click += (s, e) =>
            {
                txtFilter.Clear();
                ClearFilter();
            };
            _filterPanel.Controls.Add(btnClear);

            // Insertar panel ANTES del grid
            var parent = _grid.Parent;
            var gridIndex = parent.Controls.GetChildIndex(_grid);
            parent.Controls.Add(_filterPanel);
            parent.Controls.SetChildIndex(_filterPanel, gridIndex);
        }

        /// <summary>
        /// Aplica filtro global a todas las columnas visibles
        /// </summary>
        private void ApplyFilter(string filterText)
        {
            if (_originalData == null || string.IsNullOrWhiteSpace(filterText))
            {
                ClearFilter();
                return;
            }

            try
            {
                // Construir expresión de filtro para todas las columnas de texto
                var filters = _originalData.Columns.Cast<DataColumn>()
                    .Where(col => col.DataType == typeof(string))
                    .Select(col => $"CONVERT([{col.ColumnName}], 'System.String') LIKE '%{filterText.Replace("'", "''")}%'");

                string filterExpression = string.Join(" OR ", filters);

                if (!string.IsNullOrEmpty(filterExpression))
                {
                    var filteredView = _originalData.DefaultView;
                    filteredView.RowFilter = filterExpression;

                    _grid.DataSource = filteredView.ToTable();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error aplicando filtro: {ex.Message}");
            }
        }

        /// <summary>
        /// Limpia el filtro y restaura datos originales
        /// </summary>
        public void ClearFilter()
        {
            if (_originalData != null)
            {
                _grid.DataSource = _originalData.Copy();
            }
        }

        /// <summary>
        /// Actualiza los datos originales (llamar después de guardar cambios)
        /// </summary>
        public void RefreshData(DataTable newData)
        {
            _originalData = newData.Copy();
            _grid.DataSource = newData;
        }
    }
}