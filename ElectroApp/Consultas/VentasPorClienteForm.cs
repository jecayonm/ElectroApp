using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class VentasPorClienteForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ScrollBars = ScrollBars.Both };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripLabel _lblDesde = new ToolStripLabel("Desde:");
        private readonly ToolStripTextBox _txtDesde = new ToolStripTextBox();
        private readonly ToolStripLabel _lblHasta = new ToolStripLabel("Hasta:");
        private readonly ToolStripTextBox _txtHasta = new ToolStripTextBox();

        private readonly ReportesDAO _dao = new ReportesDAO();

        public VentasPorClienteForm()
        {
            Text = "Ventas por cliente (rango fechas)";
            Width = 1000; Height = 600; StartPosition = FormStartPosition.CenterParent;

            _btnRefrescar.Click += (s, e) => Cargar();
            _txtDesde.AutoSize = false; _txtDesde.Width = 100; _txtDesde.Text = DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd");
            _txtHasta.AutoSize = false; _txtHasta.Width = 100; _txtHasta.Text = DateTime.Today.ToString("yyyy-MM-dd");

            _bar.Items.AddRange(new ToolStripItem[] { _lblDesde, _txtDesde, _lblHasta, _txtHasta, _btnRefrescar });
            _bar.Dock = DockStyle.Top;

            _grid.DataSource = _bs;

            Controls.Add(_grid);
            Controls.Add(_bar);

            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }

        private void Cargar()
        {
            if (!DateTime.TryParse(_txtDesde.Text.Trim(), out var d)) { MessageBox.Show("Fecha 'Desde' inválida (yyyy-MM-dd)"); return; }
            if (!DateTime.TryParse(_txtHasta.Text.Trim(), out var h)) { MessageBox.Show("Fecha 'Hasta' inválida (yyyy-MM-dd)"); return; }
            DataTable dt = _dao.GetVentasPorCliente(d, h);
            _bs.DataSource = dt;
            Formatear();
        }

        private void Formatear()
        {
            if (_grid.Columns.Contains("Bruto")) _grid.Columns["Bruto"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("IVA")) _grid.Columns["IVA"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("Neto")) _grid.Columns["Neto"].DefaultCellStyle.Format = "C2";
        }
    }
}
