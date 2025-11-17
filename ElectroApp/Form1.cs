using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using ElectroApp.Data;
using ElectroApp.Services;

namespace ElectroApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // Botón: Venta contado + factura
            var btnContado = new Button
            {
                Text = "Venta demo + Factura",
                Dock = DockStyle.Top,
                Height = 40
            };
            btnContado.Click += (s, e) => VentaDemo();
            Controls.Add(btnContado);

            // Botón: Venta a CRÉDITO
            var btnCredito = new Button
            {
                Text = "Venta demo a CRÉDITO",
                Dock = DockStyle.Top,
                Height = 40
            };
            btnCredito.Click += (s, e) => VentaCreditoDemo();
            Controls.Add(btnCredito);

            AgregarBotonEstadoCuenta();

            // Productos
            var btnProd = new Button { Text = "CRUD Productos", Dock = DockStyle.Top, Height = 40 };
            btnProd.Click += (s, e) => new ProductosForm().ShowDialog();
            Controls.Add(btnProd);
            Controls.SetChildIndex(btnProd, 0);

            // Clientes
            var btnCli = new Button { Text = "CRUD Clientes", Dock = DockStyle.Top, Height = 40 };
            btnCli.Click += (s, e) => new ClientesForm().ShowDialog();
            Controls.Add(btnCli);
            Controls.SetChildIndex(btnCli, 0);

        }

        // ===== CONTADO + FACTURA =====
        private void VentaDemo()
        {
            try
            {
                int idCliente = GetId("SELECT IdCliente FROM core.Cliente WHERE Documento=@x", "CCTEST");
                int idProd1 = GetId("SELECT IdProducto FROM core.Producto WHERE Codigo=@x", "PRB-001");
                int idProd2 = GetId("SELECT IdProducto FROM core.Producto WHERE Codigo=@x", "PRB-002");

                var svc = new VentasService();
                var r = svc.RegistrarVentaContado(idCliente, new[]
                {
                    (idProd1, 1, 150000m),
                    (idProd2, 1, 320000m)
                });

                string consecutivo = svc.GenerarFactura(r.IdVenta);
                new FacturaFormView(consecutivo).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ===== CRÉDITO (plan 1) =====
        private void VentaCreditoDemo()
        {
            try
            {
                int idCliente = GetId("SELECT IdCliente FROM core.Cliente WHERE Documento=@x", "CCTEST2");
                int idProd1 = GetId("SELECT IdProducto FROM core.Producto WHERE Codigo=@x", "PRB-001");

                // 1) ¿El cliente ya tiene un crédito ACTIVO?
                var activo = GetCreditoActivoPorCliente(idCliente);
                if (activo.HasValue)
                {
                    MessageBox.Show("El cliente tiene crédito pendiente. Abriré sus cuotas para gestionarlo.");
                    new CuotasForm(activo.Value.IdVenta).ShowDialog();
                    return;
                }

                // 2) Registrar venta a crédito
                var svc = new VentasService();
                var venta = svc.RegistrarVentaCredito(idCliente, new[]
                {
            (idProd1, 2, 150000m)
        });

                // 3) Crear crédito (Plan 1 = 12 meses, 5%)
                var _ = svc.CrearCredito(venta.IdVenta, 1);

                // 4) Resumen + abrir cuotas
                var cuotas = svc.ObtenerCuotasPorVenta(venta.IdVenta);
                string resumen = "Crédito creado correctamente.\n" +
                                 $"IdVenta: {venta.IdVenta}\n" +
                                 $"Cuotas generadas: {cuotas.Rows.Count}";
                if (cuotas.Rows.Count > 0)
                {
                    var primera = cuotas.Rows[0];
                    var fv = Convert.ToDateTime(primera["FechaVence"]).ToShortDateString();
                    var val = Convert.ToDecimal(primera["ValorCuota"]);
                    resumen += $"\n1ª cuota: {val:C2} (vence {fv})";
                }

                MessageBox.Show(resumen, "Venta a CRÉDITO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                new CuotasForm(venta.IdVenta).ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AgregarBotonEstadoCuenta()
        {
            var btn = new Button { Text = "Estado de cuenta (último)", Dock = DockStyle.Top, Height = 40 };
            btn.Click += (s, e) =>
            {
                int idVenta = GetId(
                    @"SELECT TOP 1 v.IdVenta
              FROM core.Credito c JOIN core.Venta v ON v.IdVenta=c.IdVenta
              WHERE c.Estado='ACTIVO' ORDER BY c.IdCredito DESC", "");
                new EstadoCuentaForm(idVenta).ShowDialog();
            };
            Controls.Add(btn);
            Controls.SetChildIndex(btn, 0);
        }



        // ===== Helpers =====
        private int GetId(string sql, string val)
        {
            using (var cn = SqlConnectionFactory.Create())
            {
                cn.Open();
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@x", val);
                    return (int)cmd.ExecuteScalar();
                }
            }
        }

        private (int IdVenta, int IdCredito)? GetCreditoActivoPorCliente(int idCliente)
        {
            using (var cn = SqlConnectionFactory.Create())
            {
                cn.Open();
                using (var cmd = new SqlCommand(@"
            SELECT TOP 1 v.IdVenta, c.IdCredito
            FROM core.Credito c
            JOIN core.Venta v ON v.IdVenta = c.IdVenta
            WHERE v.IdCliente = @cli AND c.Estado = 'ACTIVO'
            ORDER BY c.IdCredito DESC;", cn))
                {
                    cmd.Parameters.AddWithValue("@cli", idCliente);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                            return (rd.GetInt32(0), rd.GetInt32(1));
                        return null;
                    }
                }
            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // opcional: inicializaciones de inicio
        }
    }
}
