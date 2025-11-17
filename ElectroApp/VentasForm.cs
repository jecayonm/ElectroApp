using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using ElectroApp.Services;
using ElectroApp.Data;
using System.Data.SqlClient;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class VentasForm : Form
    {
        private readonly VentasService _svc = new VentasService();

        private ComboBox _cbClientes; // ahora dentro del ToolStrip
        private DataGridView _gridItems;
        private BindingSource _bsItems;
        private ToolStrip _bar;
        private ToolStripButton _btnAgregar;
        private ToolStripButton _btnQuitar;
        private ToolStripSeparator _sep;
        private ToolStripButton _btnContado;
        private ToolStripButton _btnCredito;
        private Label _lblPlan;
        private StatusStrip _status;
        private ToolStripStatusLabel _lblTotales;
        private ToolStripButton _btnRefrescarClientes;
        private ToolStripButton _btnNuevoCliente;
        private ComboBox _cbPlan;

        private SplitContainer _split;
        private DataGridView _gridProductos;
        private BindingSource _bsProductos;
        private TextBox _txtBuscarProd;
        private Button _btnBuscarProd;
        private Panel _panelBuscarProd;

        private DataTable _dtItems;
        private DataTable _dtProductos;

        public VentasForm()
        {
            Text = "Ventas";
            Width = 1200;
            Height = 720;
            StartPosition = FormStartPosition.CenterScreen;

            BuildUi();
            Load += (s, e) => {
                Theme.Apply(this);
                CargarClientes();
                CargarPlanes();
                PrepararTablaItems();
                CargarProductos();
            };
        }

        private void BuildUi()
        {
            // ToolStrip
            _bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Stretch = true, RenderMode = ToolStripRenderMode.System };

            _btnAgregar = new ToolStripButton("Agregar producto (selector)");
            _btnQuitar = new ToolStripButton("Quitar seleccionado");
            _sep = new ToolStripSeparator();
            _btnContado = new ToolStripButton("Registrar CONTADO");
            _btnCredito = new ToolStripButton("Registrar CRÉDITO");
            _btnRefrescarClientes = new ToolStripButton("Refrescar clientes");
            _btnNuevoCliente = new ToolStripButton("Nuevo cliente");

            // Cliente en ToolStrip
            _cbClientes = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            var lblCliente = new Label { Text = "Cliente:", AutoSize = true };

            // Plan en ToolStrip
            _cbPlan = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 240 };
            _lblPlan = new Label { Text = "Plan:", AutoSize = true };

            _btnAgregar.Click += (s, e) => MostrarSelectorProductos();
            _btnQuitar.Click += (s, e) => QuitarItem();
            _btnContado.Click += (s, e) => RegistrarVenta(false);
            _btnCredito.Click += (s, e) => RegistrarVenta(true);
            _btnRefrescarClientes.Click += (s, e) => CargarClientes();
            _btnNuevoCliente.Click += (s, e) => {
                using (var f = new ClientesForm())
                {
                    f.ShowDialog(this);
                    CargarClientes();
                }
            };

            _bar.Items.Add(_btnAgregar);
            _bar.Items.Add(_btnQuitar);
            _bar.Items.Add(new ToolStripSeparator());
            _bar.Items.Add(new ToolStripControlHost(lblCliente));
            _bar.Items.Add(new ToolStripControlHost(_cbClientes));
            _bar.Items.Add(new ToolStripSeparator());
            _bar.Items.Add(new ToolStripControlHost(_lblPlan));
            _bar.Items.Add(new ToolStripControlHost(_cbPlan));
            _bar.Items.Add(new ToolStripSeparator());
            _bar.Items.Add(_btnContado);
            _bar.Items.Add(_btnCredito);
            _bar.Items.Add(new ToolStripSeparator());
            _bar.Items.Add(_btnRefrescarClientes);
            _bar.Items.Add(_btnNuevoCliente);
            _bar.Dock = DockStyle.Top;
            Controls.Add(_bar);

            // Items (detalle de venta)
            _bsItems = new BindingSource();
            _gridItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Vertical
            };
            _gridItems.DataSource = _bsItems;
            _gridItems.UserDeletedRow += (s, e) => RecalcularTotales();
            _gridItems.CellEndEdit += (s, e) => {
                if (_gridItems.Columns[e.ColumnIndex].Name == "Cantidad")
                {
                    ValidarStockFila(_gridItems.Rows[e.RowIndex]);
                    RecalcularTotales();
                }
            };
            _gridItems.DataBindingComplete += (s, e) => ConfigurarColumnasItems();

            // Panel izquierdo: catálogo de productos
            _split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterWidth = 6 };

            _panelBuscarProd = new Panel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(4, 4, 4, 4) };
            _txtBuscarProd = new TextBox { Dock = DockStyle.Fill };
            _btnBuscarProd = new Button { Text = "Buscar", Dock = DockStyle.Right, Width = 90 };
            _btnBuscarProd.Click += (s, e) => CargarProductos();
            _panelBuscarProd.Controls.Add(_txtBuscarProd);
            _panelBuscarProd.Controls.Add(_btnBuscarProd);

            _bsProductos = new BindingSource();
            _gridProductos = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Vertical
            };
            _gridProductos.DataSource = _bsProductos;
            _gridProductos.DoubleClick += (s, e) => AgregarDesdeGridProductos();

            var leftPanel = new Panel { Dock = DockStyle.Fill };
            leftPanel.Controls.Add(_gridProductos);
            leftPanel.Controls.Add(_panelBuscarProd);

            _split.Panel1.Controls.Add(leftPanel);
            _split.Panel2.Controls.Add(_gridItems);

            Controls.Add(_split);

            // Ajustar SplitterDistance y tamaños mínimos cuando el control ya tiene tamaño
            this.Load += (s, e) => AjustarSplitterDistanceInicial();
            this.Resize += (s, e) => AjustarSplitterDistanceInicial();

            // Status bar
            _status = new StatusStrip();
            _lblTotales = new ToolStripStatusLabel();
            _status.Items.Add(_lblTotales);
            Controls.Add(_status);
        }

        private void AjustarSplitterDistanceInicial()
        {
            if (_split == null) return;

            // Deseados
            int desiredMin1 = 320;
            int desiredMin2 = 400;

            int width = _split.Width;
            int splitter = _split.SplitterWidth;

            // Asegurar que las min sizes quepan en el ancho actual
            int min1 = desiredMin1;
            int min2 = desiredMin2;
            if (width < min1 + splitter + min2)
            {
                // Reducir Panel2MinSize para que quepa al menos Panel1MinSize y el splitter
                min2 = Math.Max(0, width - min1 - splitter);
            }

            _split.Panel1MinSize = min1;
            _split.Panel2MinSize = min2;

            // Calcular y ajustar SplitterDistance de forma segura
            int max = width - min2;
            if (max < min1)
            {
                // Como último recurso, bajar aún más min2 para permitir un rango válido
                min2 = Math.Max(0, width - min1);
                _split.Panel2MinSize = min2;
                max = width - min2;
            }

            if (max >= min1)
            {
                int desired = 450;
                int clamped = Math.Max(min1, Math.Min(desired, max));
                if (_split.SplitterDistance != clamped)
                {
                    _split.SplitterDistance = clamped;
                }
            }
        }

        private void ConfigurarColumnasItems()
        {
            if (_gridItems.Columns.Contains("IdProducto")) _gridItems.Columns["IdProducto"].HeaderText = "Id";
            if (_gridItems.Columns.Contains("Descripcion")) _gridItems.Columns["Descripcion"].HeaderText = "Producto";
            if (_gridItems.Columns.Contains("Cantidad")) _gridItems.Columns["Cantidad"].HeaderText = "Cant.";
            if (_gridItems.Columns.Contains("Costo")) { _gridItems.Columns["Costo"].HeaderText = "Costo"; _gridItems.Columns["Costo"].DefaultCellStyle.Format = "C2"; }
            if (_gridItems.Columns.Contains("PrecioVenta")) { _gridItems.Columns["PrecioVenta"].HeaderText = "Precio Venta"; _gridItems.Columns["PrecioVenta"].DefaultCellStyle.Format = "C2"; }
            if (_gridItems.Columns.Contains("Iva")) { _gridItems.Columns["Iva"].HeaderText = "IVA"; _gridItems.Columns["Iva"].DefaultCellStyle.Format = "P"; }
            if (_gridItems.Columns.Contains("Utilidad")) { _gridItems.Columns["Utilidad"].HeaderText = "Utilidad"; _gridItems.Columns["Utilidad"].DefaultCellStyle.Format = "P"; }
            if (_gridItems.Columns.Contains("Subtotal")) { _gridItems.Columns["Subtotal"].HeaderText = "Subtotal"; _gridItems.Columns["Subtotal"].DefaultCellStyle.Format = "C2"; }
        }

        private void CargarClientes()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter("SELECT IdCliente, (Nombres+' '+Apellidos) AS Nombre FROM core.Cliente ORDER BY Nombres, Apellidos", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                _cbClientes.DataSource = dt;
                _cbClientes.DisplayMember = "Nombre";
                _cbClientes.ValueMember = "IdCliente";
            }
        }

        private void CargarPlanes()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter("SELECT IdPlan, Meses, InteresPorc, CuotaInicialPorc FROM core.PlanCredito ORDER BY Meses", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                // Agregar columna de texto amigable
                if (!dt.Columns.Contains("Texto")) dt.Columns.Add("Texto", typeof(string));
                foreach (DataRow r in dt.Rows)
                {
                    var meses = r.Field<short>("Meses");
                    var interes = r.Field<decimal>("InteresPorc");
                    var ini = r.Field<decimal>("CuotaInicialPorc");
                    r["Texto"] = string.Format("{0} meses | Inicial {1:P0} | Interés {2:P0}", meses, ini, interes);
                }
                _cbPlan.DataSource = dt;
                _cbPlan.DisplayMember = "Texto";
                _cbPlan.ValueMember = "IdPlan";
            }
        }

        private void PrepararTablaItems()
        {
            _dtItems = new DataTable();
            _dtItems.Columns.Add("IdProducto", typeof(int));
            _dtItems.Columns.Add("Descripcion", typeof(string));
            _dtItems.Columns.Add("Cantidad", typeof(int));
            _dtItems.Columns.Add("Costo", typeof(decimal));
            _dtItems.Columns.Add("Utilidad", typeof(decimal));
            _dtItems.Columns.Add("Iva", typeof(decimal));
            _dtItems.Columns.Add("PrecioVenta", typeof(decimal));
            _dtItems.Columns.Add("Stock", typeof(int));
            _dtItems.Columns.Add("Subtotal", typeof(decimal), "Cantidad * PrecioVenta");
            _bsItems.DataSource = _dtItems;
            RecalcularTotales();
        }

        private void CargarProductos()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT p.IdProducto, p.Descripcion, p.Costo, p.PrecioVenta, p.Stock, c.Iva, c.Utilidad
FROM core.Producto p
JOIN core.Categoria c ON c.IdCategoria = p.IdCategoria
WHERE (@f = '' OR p.Descripcion LIKE '%' + @f + '%' OR p.Codigo LIKE '%' + @f + '%')
ORDER BY p.Descripcion", cn))
            {
                da.SelectCommand.Parameters.AddWithValue("@f", _txtBuscarProd?.Text?.Trim() ?? string.Empty);
                _dtProductos = new DataTable();
                da.Fill(_dtProductos);
                _bsProductos.DataSource = _dtProductos;
            }

            if (_gridProductos.Columns.Contains("Descripcion")) _gridProductos.Columns["Descripcion"].HeaderText = "Producto";
            if (_gridProductos.Columns.Contains("Costo")) _gridProductos.Columns["Costo"].DefaultCellStyle.Format = "C2";
            if (_gridProductos.Columns.Contains("PrecioVenta")) _gridProductos.Columns["PrecioVenta"].HeaderText = "Precio Venta";
            if (_gridProductos.Columns.Contains("PrecioVenta")) _gridProductos.Columns["PrecioVenta"].DefaultCellStyle.Format = "C2";
            if (_gridProductos.Columns.Contains("Iva")) _gridProductos.Columns["Iva"].DefaultCellStyle.Format = "P";
            if (_gridProductos.Columns.Contains("Utilidad")) _gridProductos.Columns["Utilidad"].DefaultCellStyle.Format = "P";
        }

        private void MostrarSelectorProductos()
        {
            using (var dlg = new ProductosSelectorForm())
            {
                if (dlg.ShowDialog() == DialogResult.OK && dlg.SelectedProduct != null)
                {
                    var p = dlg.SelectedProduct;
                    var row = _dtItems.NewRow();
                    row["IdProducto"] = p.Field<int>("IdProducto");
                    row["Descripcion"] = p.Field<string>("Descripcion");
                    row["Cantidad"] = 1;
                    row["Costo"] = p.Field<decimal>("Costo");
                    row["Utilidad"] = p.Field<decimal>("Utilidad");
                    row["Iva"] = p.Field<decimal>("Iva");
                    row["PrecioVenta"] = p.Field<decimal>("PrecioVenta");
                    row["Stock"] = p.Field<int>("Stock");
                    _dtItems.Rows.Add(row);
                    RecalcularTotales();
                }
            }
        }

        private void AgregarDesdeGridProductos()
        {
            if (_gridProductos?.CurrentRow == null) return;
            var drv = _gridProductos.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;
            var p = drv.Row;
            var row = _dtItems.NewRow();
            row["IdProducto"] = p.Field<int>("IdProducto");
            row["Descripcion"] = p.Field<string>("Descripcion");
            row["Cantidad"] = 1;
            row["Costo"] = p.Field<decimal>("Costo");
            row["Utilidad"] = p.Field<decimal>("Utilidad");
            row["Iva"] = p.Field<decimal>("Iva");
            row["PrecioVenta"] = p.Field<decimal>("PrecioVenta");
            row["Stock"] = p.Field<int>("Stock");
            _dtItems.Rows.Add(row);
            RecalcularTotales();
        }

        private void QuitarItem()
        {
            if (_gridItems.CurrentRow == null) return;
            _gridItems.Rows.Remove(_gridItems.CurrentRow);
            RecalcularTotales();
        }

        private void ValidarStockFila(DataGridViewRow gridRow)
        {
            if (gridRow?.DataBoundItem is DataRowView drv)
            {
                var r = drv.Row;
                int cant = r.Field<int>("Cantidad");
                int stock = r.Field<int>("Stock");
                if (cant > stock)
                {
                    MessageBox.Show(string.Format("Cantidad ({0}) excede stock disponible ({1}).", cant, stock), "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    r["Cantidad"] = stock; // ajustar
                    gridRow.DefaultCellStyle.BackColor = System.Drawing.Color.MistyRose;
                }
                else
                {
                    gridRow.DefaultCellStyle.BackColor = System.Drawing.Color.White;
                }
            }
        }

        private void RecalcularTotales()
        {
            decimal bruto = 0m;
            decimal ivaTotal = 0m;
            foreach (DataRow r in _dtItems.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                var cant = r.Field<int>("Cantidad");
                var precio = r.Field<decimal>("PrecioVenta");
                var ivaRate = r.Field<decimal>("Iva");
                var lineBruto = cant * precio;
                bruto += lineBruto;
                ivaTotal += System.Decimal.Round(lineBruto * ivaRate, 2);
            }
            var neto = bruto + ivaTotal;
            _lblTotales.Text = string.Format("Bruto: {0:C} | IVA: {1:C} | Neto: {2:C}", bruto, ivaTotal, neto);
        }

        private void RegistrarVenta(bool esCredito)
        {
            if (_cbClientes.SelectedValue == null)
            {
                MessageBox.Show("Seleccione un cliente.");
                return;
            }
            if (_dtItems.Rows.Count == 0)
            {
                MessageBox.Show("Agregue al menos un producto.");
                return;
            }
            // Validar stock nuevamente
            foreach (DataRow r in _dtItems.Rows)
            {
                if (r.RowState == DataRowState.Deleted) continue;
                if (r.Field<int>("Cantidad") > r.Field<int>("Stock"))
                {
                    MessageBox.Show("Hay líneas con cantidad mayor al stock. Corrija antes de continuar.");
                    return;
                }
            }

            var idCliente = (int)_cbClientes.SelectedValue;
            var items = _dtItems.Rows.Cast<DataRow>()
                .Where(r => r.RowState != DataRowState.Deleted)
                .Select(r => (
                    IdProducto: r.Field<int>("IdProducto"),
                    Cantidad: r.Field<int>("Cantidad"),
                    PrecioUnit: r.Field<decimal>("PrecioVenta")
                ))
                .ToArray();

            try
            {
                var resultado = esCredito
                    ? _svc.RegistrarVentaCredito(idCliente, items)
                    : _svc.RegistrarVentaContado(idCliente, items);

                var idVenta = resultado.IdVenta;
                var consec = _svc.GenerarFactura(idVenta);

                if (esCredito)
                {
                    if (_cbPlan.SelectedValue == null)
                    {
                        MessageBox.Show("Seleccione un plan de crédito.");
                        return;
                    }
                    byte idPlan = System.Convert.ToByte(_cbPlan.SelectedValue);
                    var dt = _svc.CrearCredito(idVenta, idPlan);
                    MessageBox.Show(string.Format("Venta CRÉDITO registrada. IdVenta={0}. Cuotas generadas: {1}", idVenta, dt.Rows.Count));
                    var estado = new EstadoCuentaForm(idVenta) { MdiParent = this.MdiParent as MainForm };
                    estado.Show();
                }
                else
                {
                    MessageBox.Show(string.Format("Venta CONTADO registrada. IdVenta={0}. Consecutivo factura: {1}", idVenta, consec));
                }

                // Abrir reporte de factura inmediatamente
                try
                {
                    var rep = new FacturaFormView(consec) { MdiParent = this.MdiParent as MainForm };
                    rep.Show();
                }
                catch { /* no bloquear flujo si hay error al abrir reporte */ }

                _dtItems.Clear();
                RecalcularTotales();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Registrar venta", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
