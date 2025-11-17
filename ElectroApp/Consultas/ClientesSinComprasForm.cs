using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class ClientesSinComprasForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, ScrollBars = ScrollBars.Both };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripLabel _lblSem = new ToolStripLabel("Últimas N semanas:");
        private readonly ToolStripTextBox _txtSem = new ToolStripTextBox();

        private readonly ReportesDAO _dao = new ReportesDAO();

        public ClientesSinComprasForm()
        {
            Text = "Clientes sin compras (últimas N semanas)";
            Width = 900; Height = 550; StartPosition = FormStartPosition.CenterParent;

            _btnRefrescar.Click += (s, e) => Cargar();
            _txtSem.AutoSize = false; _txtSem.Width = 60; _txtSem.Text = "4";

            _bar.Items.AddRange(new ToolStripItem[] { _lblSem, _txtSem, _btnRefrescar });
            _bar.Dock = DockStyle.Top;

            _grid.DataSource = _bs;

            Controls.Add(_grid);
            Controls.Add(_bar);

            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }

        private void Cargar()
        {
            if (!int.TryParse(_txtSem.Text.Trim(), out var n) || n < 0) { MessageBox.Show("N inválido"); return; }
            DataTable dt = _dao.GetClientesSinCompras(n);
            _bs.DataSource = dt;
        }
    }
}
