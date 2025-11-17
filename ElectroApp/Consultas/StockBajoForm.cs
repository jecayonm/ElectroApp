using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class StockBajoForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ScrollBars = ScrollBars.Both };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripLabel _lblMin = new ToolStripLabel("Mínimo:");
        private readonly ToolStripTextBox _txtMin = new ToolStripTextBox();

        private readonly ReportesDAO _dao = new ReportesDAO();

        public StockBajoForm()
        {
            Text = "Stock bajo";
            Width = 800; Height = 500; StartPosition = FormStartPosition.CenterParent;

            _btnRefrescar.Click += (s, e) => Cargar();
            _txtMin.AutoSize = false; _txtMin.Width = 60; _txtMin.Text = "5";

            _bar.Items.AddRange(new ToolStripItem[] { _lblMin, _txtMin, _btnRefrescar });
            _bar.Dock = DockStyle.Top;

            _grid.DataSource = _bs;

            Controls.Add(_grid);
            Controls.Add(_bar);

            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }

        private void Cargar()
        {
            if (!int.TryParse(_txtMin.Text.Trim(), out var min) || min < 0) { MessageBox.Show("Mínimo inválido"); return; }
            DataTable dt = _dao.GetProductosStockBajo(min);
            _bs.DataSource = dt;
            if (_grid.Columns.Contains("Costo")) _grid.Columns["Costo"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("PrecioVenta")) _grid.Columns["PrecioVenta"].DefaultCellStyle.Format = "C2";
        }
    }
}
