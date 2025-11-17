using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;
using Microsoft.Reporting.WinForms;
using ElectroApp.Data;

namespace ElectroApp
{
    public partial class FacturaFormView : Form
    {
        private readonly string _consecutivo;

        public FacturaFormView() : this(string.Empty)
        {
        }

        public FacturaFormView(string consecutivo)
        {
            InitializeComponent();      // lo genera el diseñador
            _consecutivo = consecutivo;
            this.Load += FacturaFormView_Load;
        }

        private void FacturaFormView_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_consecutivo))
            {
                // Mostrar interfaz para ingresar consecutivo o simplemente retornar
                return;
            }

            var dtEnc = new DataTable();
            var dtDet = new DataTable();

            using (var cn = SqlConnectionFactory.Create())
            {
                using (var da = new SqlDataAdapter(
                    "SELECT Consecutivo, Fecha, Nombres, Apellidos, Subtotal, IVA, TotalNeto " +
                    "FROM core.vw_Factura WHERE Consecutivo=@c", cn))
                {
                    da.SelectCommand.Parameters.Add("@c", SqlDbType.VarChar, 20).Value = _consecutivo;
                    da.Fill(dtEnc);
                }

                using (var da = new SqlDataAdapter(@"
SELECT p.Descripcion, c.Nombre AS Categoria
FROM core.DetalleVenta d
JOIN core.Venta v       ON v.IdVenta     = d.IdVenta
JOIN core.Producto p    ON p.IdProducto  = d.IdProducto
JOIN core.Categoria c   ON c.IdCategoria = p.IdCategoria
JOIN core.Factura f     ON f.IdVenta     = v.IdVenta
WHERE f.Consecutivo = @c
ORDER BY p.Descripcion", cn))
                {
                    da.SelectCommand.Parameters.Add("@c", SqlDbType.VarChar, 20).Value = _consecutivo;
                    da.Fill(dtDet);
                }
            }

            var rdlcRelative = Path.Combine("Reportes", "Factura.rdlc");
            var rdlcFull = Path.Combine(Application.StartupPath, rdlcRelative);
            reportViewer1.LocalReport.ReportPath = File.Exists(rdlcFull) ? rdlcFull : rdlcRelative;

            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DS_Factura", dtEnc));
            reportViewer1.LocalReport.DataSources.Add(new ReportDataSource("DS_FacturaDetalle", dtDet));
            reportViewer1.RefreshReport();
            //Boton exportar pdf
            var btnPdf = new Button { Text = "Exportar PDF", Dock = DockStyle.Top, Height = 36 };
            btnPdf.Click += (s, args) =>
            {
                try
                {
                    // por si el consecutivo tuviera caracteres no válidos
                    string safe = string.Concat(_consecutivo.Split(System.IO.Path.GetInvalidFileNameChars()));
                    var bytes = reportViewer1.LocalReport.Render("PDF");
                    var path = System.IO.Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        $"Factura_{safe}.pdf");
                    System.IO.File.WriteAllBytes(path, bytes);
                    MessageBox.Show($"PDF generado:\n{path}", "Factura", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exportar PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            Controls.Add(btnPdf);
            Controls.SetChildIndex(btnPdf, 0);

        }

    }
}
