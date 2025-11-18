using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities;

namespace ElectroApp
{
    public class ProductoMasCostosoPorClienteForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ReportesDAO _dao = new ReportesDAO();
        public ProductoMasCostosoPorClienteForm()
        {
            Text = "Producto más costoso por cliente"; Width = 900; Height = 550; StartPosition = FormStartPosition.CenterParent;
            _btnRefrescar.Click += (s, e) => Cargar();
            _bar.Items.Add(_btnRefrescar); _bar.Dock = DockStyle.Top;
            Controls.Add(_grid); Controls.Add(_bar);
            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }
        private void Cargar()
        {
            DataTable dt = _dao.GetProductoMasCostosoPorCliente();
            _bs.DataSource = dt; _grid.DataSource = _bs;
        }
    }
}
