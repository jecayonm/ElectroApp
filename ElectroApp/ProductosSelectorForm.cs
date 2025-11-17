using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using ElectroApp.Data;
using System.Data.SqlClient;

namespace ElectroApp
{
    public class ProductosSelectorForm : Form
    {
        private TextBox _txtBuscar;
        private Button _btnBuscar;
        private DataGridView _grid;
        private BindingSource _bs;
        private Button _btnAceptar;
        private Button _btnCancelar;
        private DataTable _dt;

        public DataRow SelectedProduct { get; private set; }

        public ProductosSelectorForm()
        {
            Text = "Seleccionar producto";
            Width = 800;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;

            _txtBuscar = new TextBox { Dock = DockStyle.Top }; // Placeholder no soportado en .NET 4.8
            _btnBuscar = new Button { Text = "Buscar", Dock = DockStyle.Top, Height = 32 };
            _btnBuscar.Click += (s, e) => Cargar();

            _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoGenerateColumns = true };
            _bs = new BindingSource();
            _grid.DataSource = _bs;
            _grid.DoubleClick += (s, e) => Aceptar();

            var panelBottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 45, FlowDirection = FlowDirection.RightToLeft };
            _btnAceptar = new Button { Text = "Aceptar", Width = 100 };
            _btnCancelar = new Button { Text = "Cancelar", Width = 100 };
            _btnAceptar.Click += (s, e) => Aceptar();
            _btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;
            panelBottom.Controls.Add(_btnAceptar);
            panelBottom.Controls.Add(_btnCancelar);

            Controls.Add(_grid);
            Controls.Add(panelBottom);
            Controls.Add(_btnBuscar);
            Controls.Add(_txtBuscar);

            Load += (s, e) => Cargar();
        }

        private void Cargar()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT p.IdProducto, p.Codigo, p.Descripcion, p.Costo, p.PrecioVenta, p.Stock, c.Iva, c.Utilidad
FROM core.Producto p
JOIN core.Categoria c ON c.IdCategoria = p.IdCategoria
WHERE (@f = '' OR p.Descripcion LIKE '%' + @f + '%' OR p.Codigo LIKE '%' + @f + '%')
ORDER BY p.Descripcion", cn))
            {
                da.SelectCommand.Parameters.AddWithValue("@f", _txtBuscar.Text?.Trim() ?? string.Empty);
                _dt = new DataTable();
                da.Fill(_dt);
                _bs.DataSource = _dt;
            }
            if (_grid.Columns.Contains("Codigo")) _grid.Columns["Codigo"].HeaderText = "Código";
            if (_grid.Columns.Contains("Descripcion")) _grid.Columns["Descripcion"].HeaderText = "Descripción";
            if (_grid.Columns.Contains("Costo")) _grid.Columns["Costo"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("PrecioVenta")) _grid.Columns["PrecioVenta"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("Stock")) _grid.Columns["Stock"].HeaderText = "Stock";
            if (_grid.Columns.Contains("Iva")) _grid.Columns["Iva"].HeaderText = "IVA";
            if (_grid.Columns.Contains("Utilidad")) _grid.Columns["Utilidad"].HeaderText = "Utilidad";
        }

        private void Aceptar()
        {
            if (_grid.CurrentRow == null) return;
            var drv = _grid.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;
            SelectedProduct = drv.Row;
            DialogResult = DialogResult.OK;
        }
    }
}
