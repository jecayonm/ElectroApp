using System;
using System.IO;
using System.Windows.Forms;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public class AboutAyudaForm : Form
    {
        private RichTextBox _rtf;
        private Button _btnAbrirPdf;
        private OpenFileDialog _open;

        public AboutAyudaForm()
        {
            Text = "Ayuda / About"; Width = 700; Height = 500; StartPosition = FormStartPosition.CenterParent;
            BuildUi();
            this.Shown += (s, e) => Theme.Apply(this);
        }

        private void BuildUi()
        {
            _rtf = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, DetectUrls = true };
            _rtf.Text = "ElectroApp\n\nAplicación de ejemplo para gestión de ventas, créditos e inventario.\n\nVersion 1.0";
            _btnAbrirPdf = new Button { Text = "Abrir ayuda PDF...", Dock = DockStyle.Bottom, Height = 40 };
            _btnAbrirPdf.Click += (s, e) => AbrirPdf();
            _open = new OpenFileDialog { Filter = "PDF (*.pdf)|*.pdf", Title = "Seleccionar archivo de ayuda" };
            Controls.Add(_rtf);
            Controls.Add(_btnAbrirPdf);
        }

        private void AbrirPdf()
        {
            if (_open.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = _open.FileName, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Abrir PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
