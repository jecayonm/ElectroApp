using System;
using System.Data;
using System.Windows.Forms;

namespace ElectroApp
{
    public class SimuladorCreditoForm : Form
    {
        private NumericUpDown _numMonto;
        private NumericUpDown _numMeses;
        private NumericUpDown _numInteres;
        private NumericUpDown _numInicial;
        private Button _btnSimular;
        private DataGridView _grid;
        private BindingSource _bs;
        private DataTable _dt;
        private Label _lblResumen;

        public SimuladorCreditoForm()
        {
            Text = "Simulador crédito"; Width = 900; Height = 600; StartPosition = FormStartPosition.CenterParent;
            BuildUi();
        }

        private void BuildUi()
        {
            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 60, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(6) };
            _numMonto = CrearNum(1000000, 100000, 10000000, "Monto");
            _numMeses = CrearNum(12, 1, 120, "Meses");
            _numInteres = CrearNum(2, 0, 100, "Interés % mensual");
            _numInicial = CrearNum(10, 0, 90, "Inicial %");
            _btnSimular = new Button { Text = "Simular", Width = 100, Height = 42 };
            _btnSimular.Click += (s, e) => Simular();
            top.Controls.AddRange(new Control[] { Etiqueta("Monto:"), _numMonto, Etiqueta("Meses:"), _numMeses, Etiqueta("Interés %:"), _numInteres, Etiqueta("Inicial %:"), _numInicial, _btnSimular });

            _bs = new BindingSource();
            _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            _grid.DataSource = _bs;

            _lblResumen = new Label { Dock = DockStyle.Bottom, Height = 36, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Padding = new Padding(6) };

            Controls.Add(_grid);
            Controls.Add(_lblResumen);
            Controls.Add(top);
        }

        private NumericUpDown CrearNum(decimal valor, decimal min, decimal max, string tag)
        {
            var nud = new NumericUpDown { DecimalPlaces = 2, Width = 90, Tag = tag, ThousandsSeparator = true };
            // Establecer primero el rango para evitar ArgumentOutOfRange cuando se asigna Value
            nud.Minimum = min;
            nud.Maximum = max;
            if (valor < nud.Minimum) valor = nud.Minimum;
            if (valor > nud.Maximum) valor = nud.Maximum;
            nud.Value = valor;
            return nud;
        }
        private Label Etiqueta(string txt) => new Label { Text = txt, AutoSize = true, Padding = new Padding(0, 12, 4, 0) };

        private void Simular()
        {
            decimal monto = _numMonto.Value;
            int meses = (int)_numMeses.Value;
            decimal interesMes = _numInteres.Value / 100m;
            decimal inicialPorc = _numInicial.Value / 100m;
            decimal inicial = Math.Round(monto * inicialPorc, 2);
            decimal saldoFinanciado = monto - inicial;

            // Modelo cuota fija (interés simple aproximado): cuota = saldo * (interésMes + 1/meses)
            // Alternativa amortización clásica: cuota = r*P / (1 - (1+r)^-n)
            decimal r = interesMes;
            decimal P = saldoFinanciado;
            decimal cuota = r <= 0 ? Math.Round(P / meses, 2) : Math.Round(r * P / (1 - (decimal)Math.Pow((double)(1 + r), -meses)), 2);

            _dt = new DataTable();
            _dt.Columns.Add("Nro", typeof(int));
            _dt.Columns.Add("Fecha", typeof(DateTime));
            _dt.Columns.Add("Cuota", typeof(decimal));
            _dt.Columns.Add("Interes", typeof(decimal));
            _dt.Columns.Add("Capital", typeof(decimal));
            _dt.Columns.Add("Saldo", typeof(decimal));

            decimal saldo = P;
            DateTime fecha = DateTime.Today;
            for (int i = 1; i <= meses; i++)
            {
                decimal interes = Math.Round(saldo * r, 2);
                decimal capital = Math.Round(cuota - interes, 2);
                if (capital > saldo) capital = saldo;
                saldo = Math.Round(saldo - capital, 2);
                _dt.Rows.Add(i, fecha.AddMonths(i), cuota, interes, capital, saldo);
            }

            _bs.DataSource = _dt;
            Formatear();
            decimal totalInteres = 0m; foreach (DataRow row in _dt.Rows) totalInteres += row.Field<decimal>("Interes");
            _lblResumen.Text = $"Monto: {monto:C2} | Inicial: {inicial:C2} | Financiado: {saldoFinanciado:C2} | Cuota: {cuota:C2} | Interés total: {totalInteres:C2}";
        }

        private void Formatear()
        {
            foreach (DataGridViewColumn c in _grid.Columns)
            {
                if (c.Name == "Cuota" || c.Name == "Interes" || c.Name == "Capital" || c.Name == "Saldo") c.DefaultCellStyle.Format = "C2";
                if (c.Name == "Fecha") c.DefaultCellStyle.Format = "dd/MM/yyyy";
            }
        }
    }
}
