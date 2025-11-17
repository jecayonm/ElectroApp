using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using ElectroApp.Data;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class BitacoraAccesosForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripTextBox _txtFiltroUsuario = new ToolStripTextBox();
        private readonly ToolStripLabel _lblFiltro = new ToolStripLabel("Usuario:");

        public BitacoraAccesosForm()
        {
            Text = "Bitácora de accesos"; Width = 900; Height = 550; StartPosition = FormStartPosition.CenterParent;
            _txtFiltroUsuario.AutoSize = false; _txtFiltroUsuario.Width = 140;
            _btnRefrescar.Click += (s, e) => Cargar();
            _bar.Items.AddRange(new ToolStripItem[] { _lblFiltro, _txtFiltroUsuario, _btnRefrescar });
            _bar.Dock = DockStyle.Top;
            _grid.DataSource = _bs;
            Controls.Add(_grid);
            Controls.Add(_bar);
            Load += (s, e) => { Theme.Apply(this); Cargar(); };
        }

        private void Cargar()
        {
            string f = _txtFiltroUsuario.Text.Trim();
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT TOP 500 b.IdBitacora, u.Login, b.FechaIngreso, b.FechaSalida, b.Origen
FROM core.BitacoraAcceso b
JOIN core.Usuario u ON u.IdUsuario = b.IdUsuario
WHERE (@f='' OR u.Login LIKE '%' + @f + '%')
ORDER BY b.FechaIngreso DESC", cn))
            {
                da.SelectCommand.Parameters.AddWithValue("@f", f);
                var dt = new DataTable();
                da.Fill(dt);
                _bs.DataSource = dt;
                if (_grid.Columns.Contains("FechaIngreso")) _grid.Columns["FechaIngreso"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
                if (_grid.Columns.Contains("FechaSalida")) _grid.Columns["FechaSalida"].DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
            }
        }
    }
}
