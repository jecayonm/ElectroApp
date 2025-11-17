using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.Services;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public partial class CuotasForm : Form
    {
        private readonly int _idVenta;
        private readonly VentasService _svc = new VentasService();
        private DataGridView _grid;
        private ToolStrip _bar;
        private ToolStripButton _btnPagarTs;
        private ToolStripButton _btnImprimirTs;
        // Mantener campo antiguo para evitar error ENC0020 en Hot Reload
        private Button _btnPagar; // no usado

        public CuotasForm(int idVenta)
        {
            _idVenta = idVenta;

            InitializeComponent();
            SetupUi();

            Load += (s, e) => { Theme.Apply(this); CargarCuotas(); };
        }

        private void SetupUi()
        {
            Text = "Cuotas del crédito";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            // Barra superior
            _bar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Stretch = true, RenderMode = ToolStripRenderMode.System, Dock = DockStyle.Top };
            _btnPagarTs = new ToolStripButton("Pagar cuota seleccionada");
            _btnImprimirTs = new ToolStripButton("Imprimir estado de cuenta");
            _btnPagarTs.Enabled = false;
            _btnPagarTs.Click += BtnPagar_Click;
            _btnImprimirTs.Click += (s, e) => new EstadoCuentaForm(_idVenta).ShowDialog(this);
            _bar.Items.AddRange(new ToolStripItem[] { _btnPagarTs, new ToolStripSeparator(), _btnImprimirTs });

            // Grilla
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ScrollBars = ScrollBars.Both
            };
            _grid.SelectionChanged += (s, e) => _btnPagarTs.Enabled = _grid.CurrentRow != null;

            Controls.Add(_grid);
            Controls.Add(_bar);
        }

        private void CargarCuotas()
        {
            var dt = _svc.ObtenerCuotasPorVenta(_idVenta);
            _grid.DataSource = dt;

            if (_grid.Columns.Contains("IdCredito")) _grid.Columns["IdCredito"].Visible = false;
            if (_grid.Columns.Contains("IdCuota")) _grid.Columns["IdCuota"].Visible = false;

            if (_grid.Columns.Contains("ValorCuota")) _grid.Columns["ValorCuota"].DefaultCellStyle.Format = "C2";
            if (_grid.Columns.Contains("FechaVence")) _grid.Columns["FechaVence"].DefaultCellStyle.Format = "dd/MM/yyyy";
        }

        private void BtnPagar_Click(object sender, EventArgs e)
        {
            if (_grid.CurrentRow == null) { MessageBox.Show("Selecciona una cuota."); return; }
            var rowView = _grid.CurrentRow.DataBoundItem as DataRowView;
            if (rowView == null) return;

            var row = rowView.Row;
            if (row.Field<bool>("Pagada")) { MessageBox.Show("Esa cuota ya está pagada."); return; }

            if (MessageBox.Show("¿Confirmar pago de la cuota seleccionada?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

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
