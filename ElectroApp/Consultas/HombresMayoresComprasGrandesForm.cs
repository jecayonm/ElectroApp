using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities;

namespace ElectroApp
{
    public class HombresMayoresComprasGrandesForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripLabel _lblEdad = new ToolStripLabel("Edad >:");
        private readonly ToolStripTextBox _txtEdad = new ToolStripTextBox();
        private readonly ToolStripLabel _lblMinMonto = new ToolStripLabel("> Monto:");
        private readonly ToolStripTextBox _txtMinMonto = new ToolStripTextBox();
        private readonly ToolStripLabel _lblMinCompras = new ToolStripLabel("> Compras:");
        private readonly ToolStripTextBox _txtMinCompras = new ToolStripTextBox();
        private readonly ReportesDAO _dao = new ReportesDAO();
        public HombresMayoresComprasGrandesForm()
        {
            Text = "Hombres > edad con compras grandes"; Width = 1000; Height = 600; StartPosition = FormStartPosition.CenterParent;
            _txtEdad.AutoSize = false; _txtEdad.Width = 50; _txtEdad.Text = "50";
            _txtMinMonto.AutoSize = false; _txtMinMonto.Width = 80; _txtMinMonto.Text = "100000";
            _txtMinCompras.AutoSize = false; _txtMinCompras.Width = 60; _txtMinCompras.Text = "5";
            _btnRefrescar.Click += (s, e) => Cargar();
            _bar.Items.AddRange(new ToolStripItem[]{_lblEdad,_txtEdad,_lblMinMonto,_txtMinMonto,_lblMinCompras,_txtMinCompras,_btnRefrescar}); _bar.Dock = DockStyle.Top;
            Controls.Add(_grid); Controls.Add(_bar);
            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }
        private void Cargar()
        {
            int edad = 50; int.TryParse(_txtEdad.Text, out edad);
            decimal minMonto = 100000m; decimal.TryParse(_txtMinMonto.Text, out minMonto);
            int minCompras = 5; int.TryParse(_txtMinCompras.Text, out minCompras);
            DataTable dt = _dao.GetHombresMayoresConComprasGrandes(edad, minMonto, minCompras);
            _bs.DataSource = dt; _grid.DataSource = _bs;
        }
    }
}
