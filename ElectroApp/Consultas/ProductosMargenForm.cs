using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class ProductosMargenForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ScrollBars = ScrollBars.Both };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");

        private readonly ReportesDAO _dao = new ReportesDAO();

        public ProductosMargenForm()
        {
            Text = "Productos - Margen y Utilidad";
            Width = 1000; Height = 600; StartPosition = FormStartPosition.CenterParent;

            _btnRefrescar.Click += (s, e) => Cargar();
            _bar.Items.AddRange(new ToolStripItem[] { _btnRefrescar });
            _bar.Dock = DockStyle.Top;

            _grid.DataSource = _bs;

            Controls.Add(_grid);
            Controls.Add(_bar);

            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }

        private void Cargar()
        {
            DataTable dt = _dao.GetProductosMargenUtilidad();
            _bs.DataSource = dt;
            Formatear();
        }

        private void Formatear()
        {
            void fmtC(string name) { if (_grid.Columns.Contains(name)) _grid.Columns[name].DefaultCellStyle.Format = "C2"; }
            void fmtP(string name) { if (_grid.Columns.Contains(name)) _grid.Columns[name].DefaultCellStyle.Format = "P2"; }

            fmtC("Costo");
            fmtC("PrecioVenta");
            fmtC("MargenUnitario");
            fmtP("MargenUnitarioPorc");
            fmtC("CostoInventario");
            fmtC("PotencialVenta");
            fmtC("MargenInventario");
            fmtP("MargenInventarioPorc");
        }
    }
}
