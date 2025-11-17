using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using ElectroApp.Data;

namespace ElectroApp
{
    public partial class ProductosForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false }; // manual columns
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripButton _btnGuardar = new ToolStripButton("Guardar");
        private readonly ToolStripButton _btnEliminar = new ToolStripButton("Eliminar fila");

        private DataTable _dt;            // Productos
        private DataTable _dtCategorias;  // Categorías (Id + Nombre + Iva + Utilidad)

        public ProductosForm()
        {
            Text = "Productos (CRUD ADO.NET + cálculo)";
            Width = 1100; Height = 600;

            _bar.Items.AddRange(new ToolStripItem[] { _btnRefrescar, _btnGuardar, _btnEliminar });
            _btnRefrescar.Click += (s, e) => Cargar();
            _btnGuardar.Click += (s, e) => Guardar();
            _btnEliminar.Click += (s, e) => EliminarFilaSeleccionada();

            Controls.Add(_grid);
            Controls.Add(_bar);
            _bar.Dock = DockStyle.Top;

            Load += (s, e) => Cargar();
        }

        private void Cargar()
        {
            // Cargar categorías (Audio, Video, Tecnología, Cocina)
            using (var cn = SqlConnectionFactory.Create())
            {
                _dtCategorias = new DataTable();
                using (var daCat = new SqlDataAdapter("SELECT IdCategoria, Nombre, Iva, Utilidad FROM core.Categoria ORDER BY Nombre", cn))
                {
                    daCat.Fill(_dtCategorias);
                }

                _dt = new DataTable { TableName = "Producto" };
                using (var da = new SqlDataAdapter(@"SELECT p.IdProducto, p.Codigo, p.Descripcion, p.IdCategoria, p.Costo, p.PrecioVenta, p.Stock,
       c.Iva, c.Utilidad
FROM core.Producto p
LEFT JOIN core.Categoria c ON c.IdCategoria = p.IdCategoria
ORDER BY p.IdProducto", cn))
                {
                    da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    da.Fill(_dt);
                }
                // Asegurar columnas IVA / Utilidad (por si no regresan valores)
                if (!_dt.Columns.Contains("Iva")) _dt.Columns.Add("Iva", typeof(decimal));
                if (!_dt.Columns.Contains("Utilidad")) _dt.Columns.Add("Utilidad", typeof(decimal));
            }

            _bs.DataSource = _dt;
            _grid.DataSource = _bs;

            ConfigurarColumnas();

            _grid.AllowUserToAddRows = true;
            _grid.AllowUserToDeleteRows = true;
            _grid.CellEndEdit += _grid_CellEndEdit;
        }

        private void ConfigurarColumnas()
        {
            _grid.Columns.Clear();

            // IdProducto (readonly)
            var colId = new DataGridViewTextBoxColumn { DataPropertyName = "IdProducto", Name = "IdProducto", HeaderText = "IdProducto", ReadOnly = true };
            _grid.Columns.Add(colId);

            // Código
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", Name = "Codigo", HeaderText = "Código" });
            // Descripción
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Descripcion", Name = "Descripcion", HeaderText = "Descripción" });

            // Categoría (Combo) muestra Nombre pero guarda IdCategoria
            var colCat = new DataGridViewComboBoxColumn
            {
                Name = "Categoria",
                HeaderText = "Categoría",
                DataPropertyName = "IdCategoria", // columna real en la tabla de productos
                DataSource = _dtCategorias,
                DisplayMember = "Nombre",
                ValueMember = "IdCategoria",
                FlatStyle = FlatStyle.Flat,
                AutoComplete = true
            };
            _grid.Columns.Add(colCat);

            // Costo
            var colCosto = new DataGridViewTextBoxColumn { DataPropertyName = "Costo", Name = "Costo", HeaderText = "Costo", DefaultCellStyle = { Format = "C2" } };
            _grid.Columns.Add(colCosto);
            // PrecioVenta
            var colPrecio = new DataGridViewTextBoxColumn { DataPropertyName = "PrecioVenta", Name = "PrecioVenta", HeaderText = "PrecioVenta", DefaultCellStyle = { Format = "C2" } };
            _grid.Columns.Add(colPrecio);
            // Stock
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Stock", Name = "Stock", HeaderText = "Stock" });
            // IVA (solo lectura calculado de categoría)
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Iva", Name = "Iva", HeaderText = "IVA", ReadOnly = true, DefaultCellStyle = { Format = "P" } });
            // Utilidad (solo lectura calculada de categoría)
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Utilidad", Name = "Utilidad", HeaderText = "Utilidad", ReadOnly = true, DefaultCellStyle = { Format = "P" } });

            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void _grid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var rowView = _grid.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (rowView == null) return;
            var row = rowView.Row;

            string col = _grid.Columns[e.ColumnIndex].Name;
            if (col == "Categoria") // cambio de categoría
            {
                if (row["IdCategoria"] != DBNull.Value)
                {
                    using (var cn = SqlConnectionFactory.Create())
                    using (var cmd = new SqlCommand("SELECT Iva, Utilidad FROM core.Categoria WHERE IdCategoria=@id", cn))
                    {
                        cn.Open();
                        cmd.Parameters.AddWithValue("@id", row["IdCategoria"]);
                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                row["Iva"] = rd.GetDecimal(0);
                                row["Utilidad"] = rd.GetDecimal(1);
                            }
                        }
                    }
                }
                RecalcularPrecio(row);
            }
            else if (col == "Costo")
            {
                RecalcularPrecio(row);
            }
            else if (col == "Stock")
            {
                if (!int.TryParse(row["Stock"]?.ToString(), out var st) || st < 0)
                {
                    MessageBox.Show("Stock debe ser entero >= 0");
                    row["Stock"] = 0;
                }
            }
        }

        private void RecalcularPrecio(DataRow row)
        {
            if (row == null) return;
            decimal costo = 0m; decimal utilidad = 0m;
            decimal.TryParse(row["Costo"]?.ToString(), out costo);
            decimal.TryParse(row["Utilidad"]?.ToString(), out utilidad);
            if (costo >= 0 && utilidad >= 0)
            {
                var precio = System.Math.Round(costo + (costo * utilidad), 2);
                row["PrecioVenta"] = precio;
            }
        }

        private void Guardar()
        {
            try
            {
                Validate();
                _bs.EndEdit();

                using (var cn = SqlConnectionFactory.Create())
                using (var da = new SqlDataAdapter(@"SELECT IdProducto, Codigo, Descripcion, IdCategoria, Costo, PrecioVenta, Stock FROM core.Producto ORDER BY IdProducto", cn))
                {
                    da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    var cb = new SqlCommandBuilder(da) { ConflictOption = ConflictOption.OverwriteChanges };
                    da.InsertCommand = cb.GetInsertCommand(true);
                    da.UpdateCommand = cb.GetUpdateCommand();
                    da.DeleteCommand = cb.GetDeleteCommand();
                    int cambios = da.Update(_dt);
                    MessageBox.Show($"Cambios guardados: {cambios}", "Productos");
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EliminarFilaSeleccionada()
        {
            if (_grid.CurrentRow == null) return;
            if (MessageBox.Show("¿Eliminar fila seleccionada?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _grid.Rows.Remove(_grid.CurrentRow);
                Guardar();
            }
        }
        private void ProductosForm_Load(object sender, System.EventArgs e)
        {

        }

    }
}
