using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using ElectroApp.Data;
using Microsoft.VisualBasic; // para Interaction.InputBox (añadir referencia a Microsoft.VisualBasic si hace falta)

namespace ElectroApp
{
    public partial class EstadoCuentaForm : Form
    {
        private readonly int? _idVenta;
        private ReportViewer reportViewer1;

        // Constructor que recibe idVenta
        public EstadoCuentaForm(int idVenta)
        {
            _idVenta = idVenta;
            Init();
            Load += OnLoad;
        }

        // Constructor vacío: solicitará el idVenta al usuario al abrir
        public EstadoCuentaForm()
        {
            _idVenta = null;
            Init();
            Load += OnLoad;
        }

        void Init()
        {
            reportViewer1 = new ReportViewer { Dock = DockStyle.Fill, ProcessingMode = ProcessingMode.Local };
            Text = "Estado de cuenta del crédito";
            Width = 900;
            Height = 600;
            Controls.Add(reportViewer1);
        }

        void OnLoad(object s, EventArgs e)
        {
            int idVentaReal;

            if (_idVenta.HasValue)
            {
                idVentaReal = _idVenta.Value;
            }
            else
            {
                // Pedir idVenta al usuario
                string input = Interaction.InputBox("Ingrese IdVenta para generar el Estado de Cuenta:", "Buscar Estado de Cuenta", "");
                if (string.IsNullOrWhiteSpace(input) || !int.TryParse(input.Trim(), out idVentaReal))
                {
                    MessageBox.Show("IdVenta inválido. Se canceló la operación.", "Estado de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Close();
                    return;
                }
            }

            try
            {
                var dtEnc = new DataTable();
                var dtDet = new DataTable();
                using (var cn = SqlConnectionFactory.Create())
                {
                    using (var da = new SqlDataAdapter(
                        @"SELECT c.IdCredito, v.IdVenta, v.Fecha, cl.Nombres, cl.Apellidos, c.MontoAFin AS TotalCredito
                          FROM core.Credito c
                          JOIN core.Venta v ON v.IdVenta=c.IdVenta
                          JOIN core.Cliente cl ON cl.IdCliente=v.IdCliente
                          WHERE v.IdVenta=@v;", cn))
                    {
                        da.SelectCommand.Parameters.Add("@v", SqlDbType.Int).Value = idVentaReal;
                        da.Fill(dtEnc);
                    }

                    using (var da = new SqlDataAdapter(
                        @"SELECT q.NroCuota, q.ValorCuota, q.FechaVence, q.Pagada
                          FROM core.CuotaCredito q
                          JOIN core.Credito c ON c.IdCredito=q.IdCredito
                          WHERE c.IdVenta=@v ORDER BY q.NroCuota;", cn))
                    {
                        da.SelectCommand.Parameters.Add("@v", SqlDbType.Int).Value = idVentaReal;
                        da.Fill(dtDet);
                    }
                }

                if (dtEnc.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontró crédito/venta con el Id proporcionado.", "Estado de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Close();
                    return;
                }

                // Resolver ruta del RDLC de forma robusta
                var rdlcRelative = Path.Combine("Reportes", "EstadoCuenta.rdlc");
                var rdlcFull = Path.Combine(Application.StartupPath, rdlcRelative);
                reportViewer1.LocalReport.ReportPath = File.Exists(rdlcFull) ? rdlcFull : rdlcRelative;

                reportViewer1.LocalReport.DataSources.Clear();
                reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DS_Encabezado", dtEnc));
                reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DS_EstadoCuenta", dtDet));
                reportViewer1.RefreshReport();

                var btnPdf = new Button { Text = "Exportar PDF", Dock = DockStyle.Top, Height = 36 };
                btnPdf.Click += (sender, args) =>
                {
                    var bytes = reportViewer1.LocalReport.Render("PDF");
                    var path = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        $"EstadoCuenta_{idVentaReal}.pdf");
                    System.IO.File.WriteAllBytes(path, bytes);
                    MessageBox.Show($"PDF generado:\n{path}", "Estado de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };
                Controls.Add(btnPdf);
                Controls.SetChildIndex(btnPdf, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar estado de cuenta: {ex.Message}", "Estado de cuenta", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void EstadoCuentaForm_Load(object sender, EventArgs e)
        {
            // no usado, la carga se hace en OnLoad
        }
    }
}
