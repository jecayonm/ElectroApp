using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.Services;

namespace ElectroApp
{
    public partial class CuotasForm : Form
    {
        private readonly int _idVenta;
        private readonly VentasService _svc = new VentasService();
        private DataGridView _grid;
        private Button _btnPagar;

        public CuotasForm(int idVenta)
        {
            _idVenta = idVenta;

            // Llama al InitializeComponent generado por el diseñador
            InitializeComponent();

            // Construye la UI por código (antes la llamabas InitializeComponent)
            SetupUi();

            // Carga datos al abrir
            Load += (s, e) => CargarCuotas();

            _btnPagar.Enabled = false;
            _grid.SelectionChanged += (s, e) => _btnPagar.Enabled = _grid.CurrentRow != null;

            var btnPrint = new Button { Text = "Imprimir estado de cuenta", Dock = DockStyle.Top, Height = 40 };
            btnPrint.Click += (s, e) => new EstadoCuentaForm(_idVenta).ShowDialog();
            Controls.Add(btnPrint);
            Controls.SetChildIndex(btnPrint, 0); // que quede arriba

        }

        private void SetupUi()
        {
            Text = "Cuotas del crédito";
            Width = 800;
            Height = 500;

            _btnPagar = new Button
            {
                Text = "Pagar cuota seleccionada",
                Dock = DockStyle.Top,
                Height = 40
            };
            _btnPagar.Click += BtnPagar_Click;

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.Add(_grid);
            Controls.Add(_btnPagar);
        }

        private void CargarCuotas()
        {
            var dt = _svc.ObtenerCuotasPorVenta(_idVenta);
            _grid.DataSource = dt;

            if (_grid.Columns.Contains("IdCredito")) _grid.Columns["IdCredito"].Visible = false;
            if (_grid.Columns.Contains("IdCuota")) _grid.Columns["IdCuota"].Visible = false;
        }



        private void BtnPagar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Confirmar pago de la cuota seleccionada?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            if (_grid.CurrentRow == null) { MessageBox.Show("Selecciona una cuota."); return; }
            var rowView = _grid.CurrentRow.DataBoundItem as DataRowView;
            if (rowView == null) return;

            var row = rowView.Row;
            if (row.Field<bool>("Pagada")) { MessageBox.Show("Esa cuota ya está pagada."); return; }

            int idCuota = Convert.ToInt32(row["IdCuota"]);
            decimal valor = Convert.ToDecimal(row["ValorCuota"]);

            try
            {
                _svc.PagarCuota(idCuota, valor);
                MessageBox.Show("Pago registrado.");
                CargarCuotas();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CuotasForm_Load(object sender, EventArgs e)
        {

        }
    }
}
