using System;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class CalculadoraForm : Form
    {
        private TextBox _display;
        private TableLayoutPanel _panel;
        private string _currentOp;
        private double? _valorAnterior;
        private bool _resetOnNext;

        public CalculadoraForm()
        {
            Text = "Calculadora"; Width = 300; Height = 420; StartPosition = FormStartPosition.CenterParent;
            BuildUi();
            this.Shown += (s, e) => Theme.Apply(this);
        }

        private void BuildUi()
        {
            _display = new TextBox { Dock = DockStyle.Top, ReadOnly = true, Font = new System.Drawing.Font("Consolas", 24f), TextAlign = HorizontalAlignment.Right, Height = 50 };
            _display.Text = "0";
            _panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 6 };
            for (int c = 0; c < 4; c++) _panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            for (int r = 0; r < 6; r++) _panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6));

            Controls.Add(_panel);
            Controls.Add(_display);

            string[,] layout = {
                {"CE","C","±","/"},
                {"7","8","9","*"},
                {"4","5","6","-"},
                {"1","2","3","+"},
                {"0","0",".","="},
                {"","","",""}
            };

            for (int r = 0; r < layout.GetLength(0); r++)
                for (int c = 0; c < layout.GetLength(1); c++)
                {
                    var txt = layout[r, c];
                    if (string.IsNullOrEmpty(txt)) continue;
                    var btn = new Button { Text = txt, Dock = DockStyle.Fill, Font = new System.Drawing.Font("Segoe UI", 14f) };
                    btn.Click += Btn_Click;
                    if (txt == "0" && r == 4 && c == 0) { _panel.Controls.Add(btn, c, r); } else { _panel.Controls.Add(btn, c, r); }
                }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            if (sender is Button b)
            {
                string t = b.Text;
                if (char.IsDigit(t, 0)) Digito(t);
                else
                {
                    switch (t)
                    {
                        case ".": Punto(); break;
                        case "+": Operador("+"); break;
                        case "-": Operador("-"); break;
                        case "*": Operador("*"); break;
                        case "/": Operador("/"); break;
                        case "CE": _display.Text = "0"; break;
                        case "C": _display.Text = "0"; _valorAnterior = null; _currentOp = null; break;
                        case "±": CambiarSigno(); break;
                        case "=": Calcular(); break;
                    }
                }
            }
        }

        private void Digito(string d)
        {
            if (_resetOnNext || _display.Text == "0") { _display.Text = d; _resetOnNext = false; }
            else _display.Text += d;
        }
        private void Punto()
        {
            if (_resetOnNext) { _display.Text = "0"; _resetOnNext = false; }
            if (!_display.Text.Contains(".")) _display.Text += ".";
        }
        private void Operador(string op)
        {
            Calcular();
            _valorAnterior = double.Parse(_display.Text, CultureInfo.InvariantCulture);
            _currentOp = op;
            _resetOnNext = true;
        }
        private void CambiarSigno()
        {
            if (_display.Text == "0") return;
            if (_display.Text.StartsWith("-")) _display.Text = _display.Text.Substring(1); else _display.Text = "-" + _display.Text;
        }
        private void Calcular()
        {
            if (_valorAnterior.HasValue && !string.IsNullOrEmpty(_currentOp) && !_resetOnNext)
            {
                double actual = double.Parse(_display.Text, CultureInfo.InvariantCulture);
                double res = _valorAnterior.Value;
                switch (_currentOp)
                {
                    case "+": res += actual; break;
                    case "-": res -= actual; break;
                    case "*": res *= actual; break;
                    case "/": res = actual == 0 ? 0 : res / actual; break;
                }
                _display.Text = res.ToString(CultureInfo.InvariantCulture);
                _valorAnterior = null; _currentOp = null; _resetOnNext = true;
            }
        }
    }
}
