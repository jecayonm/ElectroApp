using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class CreditosPorEstadoForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ScrollBars = ScrollBars.Both };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripButton _btnVerCuotas = new ToolStripButton("Ver cuotas");

        private readonly ReportesDAO _dao = new ReportesDAO();

        public CreditosPorEstadoForm()
        {
            Text = "Créditos por estado";
            Width = 1000; Height = 600; StartPosition = FormStartPosition.CenterParent;

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
            DataTable dt = _dao.GetCreditosPorEstado();
            _bs.DataSource = dt;
        }

        private void AbrirCuotas()
        {
            if (_grid.CurrentRow?.DataBoundItem is DataRowView drv)
            {
                if (int.TryParse(drv.Row["IdVenta"].ToString(), out var idVenta) && idVenta > 0)
                {
                    var f = new CuotasForm(idVenta) { MdiParent = this.MdiParent as MainForm };
                    f.Show();
                }
            }
        }
    }
}
