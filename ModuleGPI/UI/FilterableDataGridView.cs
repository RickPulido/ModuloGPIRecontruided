using ModuleGPI.Controls;
using ModuleGPI.Services;
using ModuleGPI.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModuleGPI.UI
{
    /// <summary>
    /// DataGridView con fila de filtrado integrada
    /// </summary>
    public class FilterableDataGridView : UserControl
    {
        private DataGridView _grid;
        private Panel _filterPanel;
        private TableLayoutPanel _filterLayout;

        public DataGridView Grid => _grid;

        public FilterableDataGridView()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;

            _filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(5, 5, 5, 0)
            };

            _filterLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 0,
                RowCount = 1
            };

            _filterPanel.Controls.Add(_filterLayout);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.None
            };

            _grid.DataBindingComplete += Grid_DataBindingComplete;

            this.Controls.Add(_grid);
            this.Controls.Add(_filterPanel);
        }

        private void Grid_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            BuildFilterRow();
        }

        private void BuildFilterRow()
        {
            _filterLayout.Controls.Clear();
            _filterLayout.ColumnStyles.Clear();
            _filterLayout.ColumnCount = _grid.Columns.Count;

            for (int i = 0; i < _grid.Columns.Count; i++)
            {
                var col = _grid.Columns[i];

                if (!col.Visible)
                    continue;

                _filterLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, col.Width));

                var txtFilter = new TextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 8.5F),
                    Tag = col.DataPropertyName,
                    ForeColor = Color.Gray,
                    Text = $"Buscar {col.HeaderText}..."
                };

                // ✅ Usar helper para placeholder
                txtFilter.SetPlaceholder($"Buscar {col.HeaderText}...");
                txtFilter.TextChanged += (s, e) => ApplyFilters();

                _filterLayout.Controls.Add(txtFilter, i, 0);
            }
        }

        private void ApplyFilters()
        {
            if (_grid.DataSource == null) return;

            try
            {
                var bs = _grid.DataSource as BindingSource;
                if (bs == null) return;

                string filter = "";

                foreach (Control ctrl in _filterLayout.Controls)
                {
                    if (ctrl is TextBox txt)
                    {
                        string realText = txt.GetRealText(); // ✅ Usar helper

                        if (!string.IsNullOrWhiteSpace(realText))
                        {
                            string columnName = txt.Tag?.ToString();
                            if (!string.IsNullOrEmpty(columnName))
                            {
                                if (filter.Length > 0) filter += " AND ";
                                filter += $"Convert([{columnName}], 'System.String') LIKE '%{realText.Replace("'", "''")}%'";
                            }
                        }
                    }
                }

                bs.Filter = filter;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error aplicando filtro: {ex.Message}");
            }
        }

        public void ClearFilters()
        {
            foreach (Control ctrl in _filterLayout.Controls)
            {
                if (ctrl is TextBox txt)
                    txt.Clear();
            }
        }
    }
}
