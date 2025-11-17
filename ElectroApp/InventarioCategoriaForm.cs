using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class InventarioCategoriaForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ScrollBars = ScrollBars.Both };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripDropDownButton _btnVista = new ToolStripDropDownButton("Vista");
        private readonly ToolStripMenuItem _vistaBasica = new ToolStripMenuItem("Básica (Unidades, Costo Total)") { Checked = false };
        private readonly ToolStripMenuItem _vistaDetallada = new ToolStripMenuItem("Detallada (+ Potencial, Margen)") { Checked = true };

        private readonly ReportesDAO _dao = new ReportesDAO();
        private bool _detallada = true;

        public InventarioCategoriaForm()
        {
            Text = "Inventario por categoría";
            Width = 800; Height = 500; StartPosition = FormStartPosition.CenterParent;

            _btnRefrescar.Click += (s, e) => Cargar();
            _vistaBasica.Click += (s, e) => { _detallada = false; _vistaBasica.Checked = true; _vistaDetallada.Checked = false; Cargar(); };
            _vistaDetallada.Click += (s, e) => { _detallada = true; _vistaBasica.Checked = false; _vistaDetallada.Checked = true; Cargar(); };
            _btnVista.DropDownItems.AddRange(new ToolStripItem[] { _vistaBasica, _vistaDetallada });

            _bar.Items.AddRange(new ToolStripItem[] { _btnRefrescar, new ToolStripSeparator(), _btnVista });
            _bar.Dock = DockStyle.Top;

            _grid.DataSource = _bs;

            Controls.Add(_grid);
            Controls.Add(_bar);

            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }

        private void Cargar()
        {
            DataTable dt = _detallada ? _dao.GetInventarioPorCategoriaDetalle() : _dao.GetInventarioPorCategoria();
            _bs.DataSource = dt;
            Formatear();
        }

        private void Formatear()
        {
            if (_grid.Columns.Contains("CostoTotal")) _grid.Columns["CostoTotal"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("PotencialVenta")) _grid.Columns["PotencialVenta"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("MargenEstimado")) _grid.Columns["MargenEstimado"].DefaultCellStyle.Format = "C2";
        }
    }
}
