using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities;

namespace ElectroApp
{
    public class VentasInconsistentesForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripLabel _lblTol = new ToolStripLabel("Tolerancia:");
        private readonly ToolStripTextBox _txtTol = new ToolStripTextBox();
        private readonly ReportesDAO _dao = new ReportesDAO();
        public VentasInconsistentesForm()
        {
            Text = "Ventas inconsistentes"; Width = 900; Height = 550; StartPosition = FormStartPosition.CenterParent;
            _txtTol.AutoSize = false; _txtTol.Width = 70; _txtTol.Text = "0.01";
            _btnRefrescar.Click += (s, e) => Cargar();
            _bar.Items.AddRange(new ToolStripItem[]{_lblTol,_txtTol,_btnRefrescar}); _bar.Dock = DockStyle.Top;
            Controls.Add(_grid); Controls.Add(_bar);
            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }
        private void Cargar()
        {
            decimal tol = 0.01m; decimal.TryParse(_txtTol.Text, out tol);
            DataTable dt = _dao.GetVentasInconsistentes(tol);
            _bs.DataSource = dt; _grid.DataSource = _bs;
            if (_grid.Columns.Contains("ValorPersistido")) _grid.Columns["ValorPersistido"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("BrutoCalculado")) _grid.Columns["BrutoCalculado"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("Diferencia")) _grid.Columns["Diferencia"].DefaultCellStyle.Format = "C2";
        }
    }
}
