using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class CalendarioAgendaForm : Form
    {
        private MonthCalendar _cal;
        private ListBox _lista;
        private TextBox _txtNota;
        private Button _btnAgregar;
        private DataTable _dtNotas; // Columns: Fecha(Date), Nota(string)

        public CalendarioAgendaForm()
        {
            Text = "Calendario / Agenda"; Width = 640; Height = 480; StartPosition = FormStartPosition.CenterParent;
            BuildUi();
            this.Shown += (s, e) => Theme.Apply(this);
        }

        private void BuildUi()
        {
            _cal = new MonthCalendar { Dock = DockStyle.Left, MaxSelectionCount = 1 }; _cal.DateSelected += (s, e) => RefrescarLista();
            _lista = new ListBox { Dock = DockStyle.Fill };
            var rightPanel = new Panel { Dock = DockStyle.Right, Width = 260, Padding = new Padding(6) };
            _txtNota = new TextBox { Dock = DockStyle.Top, Multiline = true, Height = 100 };
            _btnAgregar = new Button { Text = "Agregar nota", Dock = DockStyle.Top, Height = 32 };
            _btnAgregar.Click += (s, e) => AgregarNota();
            rightPanel.Controls.Add(_btnAgregar);
            rightPanel.Controls.Add(_txtNota);

            Controls.Add(_lista);
            Controls.Add(rightPanel);
            Controls.Add(_cal);

            _dtNotas = new DataTable();
            _dtNotas.Columns.Add("Fecha", typeof(DateTime));
            _dtNotas.Columns.Add("Nota", typeof(string));
        }

        private void AgregarNota()
        {
            string nota = _txtNota.Text.Trim();
            if (string.IsNullOrEmpty(nota)) return;
            _dtNotas.Rows.Add(_cal.SelectionStart.Date, nota);
            _txtNota.Clear();
            RefrescarLista();
        }

        private void RefrescarLista()
        {
            var fecha = _cal.SelectionStart.Date;
            var rows = _dtNotas.Select($"Fecha = '#{fecha:MM/dd/yyyy}#'");
            _lista.Items.Clear();
            foreach (var r in rows)
            {
                _lista.Items.Add(r["Nota"].ToString());
            }
            if (rows.Length == 0) _lista.Items.Add("(sin notas)");
        }
    }
}
