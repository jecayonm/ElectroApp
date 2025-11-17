using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class MorososForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ScrollBars = ScrollBars.Both };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripButton _btnVerCuotas = new ToolStripButton("Ver cuotas");

        private readonly ReportesDAO _dao = new ReportesDAO();

        public MorososForm()
        {
            Text = "Clientes morosos";
            Width = 900; Height = 550; StartPosition = FormStartPosition.CenterParent;

            _btnRefrescar.Click += (s, e) => Cargar();
            _btnVerCuotas.Click += (s, e) => AbrirCuotas();

            _bar.Items.AddRange(new ToolStripItem[] { _btnRefrescar, new ToolStripSeparator(), _btnVerCuotas });
            _bar.Dock = DockStyle.Top;

            _grid.DataSource = _bs;

            Controls.Add(_grid);
            Controls.Add(_bar);

            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }

        private void Cargar()
        {
            var dt = _dao.GetClientesMorososResumen();
            _bs.DataSource = dt;
            if (_grid.Columns.Contains("TotalVencido")) _grid.Columns["TotalVencido"].DefaultCellStyle.Format = "C2";
        }

        private void AbrirCuotas()
        {
            if (_grid.CurrentRow?.DataBoundItem is DataRowView drv)
            {
                int idVenta = 0;
                int.TryParse(drv.Row["IdVenta"].ToString(), out idVenta);
                if (idVenta > 0)
                {
                    var f = new CuotasForm(idVenta) { MdiParent = this.MdiParent as MainForm };
                    f.Show();
                }
            }
        }
    }
}
