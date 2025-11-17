using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using ElectroApp.Data;
using System.Runtime.InteropServices;

namespace ElectroApp
{
    public class FacturaBuscarForm : Form
    {
        private TextBox _txtFiltro;
        private Button _btnBuscar;
        private Button _btnSeleccionar;
        private DataGridView _grid;
        private BindingSource _bs;

        public string ConsecutivoSeleccionado { get; private set; }

        // Win32: establecer cue banner (placeholder) en TextBox para .NET Framework
        private const int EM_SETCUEBANNER = 0x1501;
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
        private static void SetCueBanner(TextBox textBox, string text, bool showWhenFocused = false)
        {
            if (textBox == null || textBox.IsDisposed) return;
            // wParam = 1 para mostrar cuando tiene foco, 0 para cuando no
            if (textBox.IsHandleCreated)
            {
                SendMessage(textBox.Handle, EM_SETCUEBANNER, showWhenFocused ? 1 : 0, text ?? string.Empty);
            }
            else
            {
                textBox.HandleCreated += (s, e) => SendMessage(textBox.Handle, EM_SETCUEBANNER, showWhenFocused ? 1 : 0, text ?? string.Empty);
            }
        }

        public FacturaBuscarForm()
        {
            Text = "Buscar factura";
            Width = 800;
            Height = 500;
            StartPosition = FormStartPosition.CenterParent;

            var pnlTop = new Panel { Dock = DockStyle.Top, Height = 36, Padding = new Padding(6) };
            _txtFiltro = new TextBox { Dock = DockStyle.Fill };
            _btnBuscar = new Button { Text = "Buscar", Dock = DockStyle.Right, Width = 100 };
            pnlTop.Controls.Add(_txtFiltro);
            pnlTop.Controls.Add(_btnBuscar);

            _bs = new BindingSource();
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _grid.DataSource = _bs;

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 42, Padding = new Padding(6) };
            _btnSeleccionar = new Button { Text = "Seleccionar", Dock = DockStyle.Right, Width = 120 };
            pnlBottom.Controls.Add(_btnSeleccionar);

            Controls.Add(_grid);
            Controls.Add(pnlTop);
            Controls.Add(pnlBottom);

            Load += OnLoad;
            _btnBuscar.Click += (s, e) => Cargar();
            _btnSeleccionar.Click += (s, e) => SeleccionarActual();
            _grid.DoubleClick += (s, e) => SeleccionarActual();

            // Establecer placeholder compatible con .NET Framework
            SetCueBanner(_txtFiltro, "Consecutivo o nombre cliente");
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Cargar();
        }

        private void Cargar()
        {
            string f = _txtFiltro?.Text?.Trim() ?? string.Empty;
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT TOP 200 Consecutivo, Fecha, (Nombres + ' ' + Apellidos) AS Cliente, Subtotal, IVA, TotalNeto
FROM core.vw_Factura
WHERE (@f = '' OR Consecutivo LIKE '%' + @f + '%' OR Nombres LIKE '%' + @f + '%' OR Apellidos LIKE '%' + @f + '%')
ORDER BY Fecha DESC", cn))
            {
                da.SelectCommand.Parameters.Add("@f", SqlDbType.VarChar, 60).Value = f;
                var dt = new DataTable();
                da.Fill(dt);
                _bs.DataSource = dt;
            }
        }

        private void SeleccionarActual()
        {
            if (_grid.CurrentRow == null) return;
            var drv = _grid.CurrentRow.DataBoundItem as DataRowView;
            if (drv == null) return;
            ConsecutivoSeleccionado = Convert.ToString(drv["Consecutivo"]);
            if (!string.IsNullOrEmpty(ConsecutivoSeleccionado))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
