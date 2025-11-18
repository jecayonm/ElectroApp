using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities;

namespace ElectroApp
{
    public class ClientesMasDeNVentasForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripLabel _lblMin = new ToolStripLabel("Mín. ventas:");
        private readonly ToolStripTextBox _txtMin = new ToolStripTextBox();
        private readonly ReportesDAO _dao = new ReportesDAO();
        public ClientesMasDeNVentasForm()
        {
            Text = "Clientes con más de N ventas"; Width = 900; Height = 550; StartPosition = FormStartPosition.CenterParent;
            _txtMin.AutoSize = false; _txtMin.Width = 50; _txtMin.Text = "10";
            _btnRefrescar.Click += (s, e) => Cargar();
            _bar.Items.AddRange(new ToolStripItem[]{_lblMin,_txtMin,_btnRefrescar}); _bar.Dock = DockStyle.Top;
            Controls.Add(_grid); Controls.Add(_bar);
            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }
        private void Cargar()
        {
            int min = 10; int.TryParse(_txtMin.Text, out min);
            DataTable dt = _dao.GetClientesConMasDeNVentas(min);
            _bs.DataSource = dt; _grid.DataSource = _bs;
        }
    }
}
