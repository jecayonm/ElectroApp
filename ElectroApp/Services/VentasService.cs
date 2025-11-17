using System;
using System.Data;
using System.Data.SqlClient;
using ElectroApp.Data;

namespace ElectroApp.Services
{
    public class VentasService
    {
        // ----------- VENTAS -----------

        public (int IdVenta, decimal Bruto, decimal Iva, decimal Neto) RegistrarVentaContado(
            int idCliente,
            (int IdProducto, int Cantidad, decimal PrecioUnit)[] items)
        {
            using (var cn = SqlConnectionFactory.Create())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        var venta = RegistrarVentaInterno(idCliente, "CONTADO", items, cn, tx);
                        DescontarStock(items, cn, tx);
                        tx.Commit();
                        return venta;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public (int IdVenta, decimal Bruto, decimal Iva, decimal Neto) RegistrarVentaCredito(
            int idCliente,
            (int IdProducto, int Cantidad, decimal PrecioUnit)[] items)
        {
            // Restricción: un cliente solo puede tener un crédito activo
            if (TieneCreditoActivo(idCliente))
                throw new InvalidOperationException("El cliente ya tiene un crédito activo. Debe pagarlo antes de solicitar otro.");

            using (var cn = SqlConnectionFactory.Create())
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        var venta = RegistrarVentaInterno(idCliente, "CREDITO", items, cn, tx);
                        DescontarStock(items, cn, tx);
                        tx.Commit();
                        return venta;
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        private (int IdVenta, decimal Bruto, decimal Iva, decimal Neto) RegistrarVentaInterno(int idCliente, string tipoPago, (int IdProducto, int Cantidad, decimal PrecioUnit)[] items, SqlConnection cn, SqlTransaction tx)
        {
            using (var cmd = new SqlCommand("core.sp_RegistrarVenta", cn, tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = idCliente;
                cmd.Parameters.Add("@TipoPago", SqlDbType.VarChar, 10).Value = tipoPago;

                var tvp = CrearTvpDetalle(items);
                var pItems = cmd.Parameters.Add("@Items", SqlDbType.Structured);
                pItems.TypeName = "core.ReadonlyDetalleVenta";
                pItems.Value = tvp;

                using (var rd = cmd.ExecuteReader())
                {
                    rd.Read();
                    return (
                        rd.GetInt32(0),
                        rd.GetDecimal(1),
                        rd.GetDecimal(2),
                        rd.GetDecimal(3)
                    );
                }
            }
        }

        private void DescontarStock((int IdProducto, int Cantidad, decimal PrecioUnit)[] items, SqlConnection cn, SqlTransaction tx)
        {
            foreach (var it in items)
            {
                using (var cmd = new SqlCommand("UPDATE core.Producto SET Stock = Stock - @c WHERE IdProducto=@p", cn, tx))
                {
                    cmd.Parameters.Add("@c", SqlDbType.Int).Value = it.Cantidad;
                    cmd.Parameters.Add("@p", SqlDbType.Int).Value = it.IdProducto;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool TieneCreditoActivo(int idCliente)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                SELECT COUNT(1)
                FROM core.Credito cr
                JOIN core.Venta v ON v.IdVenta = cr.IdVenta
                WHERE v.IdCliente = @c AND EXISTS (
                    SELECT 1 FROM core.CuotaCredito q
                    WHERE q.IdCredito = cr.IdCredito AND q.Pagada = 0
                )", cn))
            {
                cn.Open();
                cmd.Parameters.Add("@c", SqlDbType.Int).Value = idCliente;
                var count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        // ----------- FACTURA -----------

        public string GenerarFactura(int idVenta)
        {
            using (var cn = SqlConnectionFactory.Create())
            {
                cn.Open();
                using (var cmd = new SqlCommand("core.sp_GenerarFactura", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdVenta", SqlDbType.Int).Value = idVenta;
                    object consec = cmd.ExecuteScalar();
                    return consec != null ? consec.ToString() : null;
                }
            }
        }

        // ----------- CRÉDITO -----------

        /// <summary>
        /// Crea el crédito para una venta a crédito. 
        /// Devuelve un DataTable con columnas: IdCredito, ValorCuota, Meses, CuotaInicial, MontoAFin (según SP).
        /// </summary>
        public DataTable CrearCredito(int idVenta, byte idPlan)
        {
            using (var cn = SqlConnectionFactory.Create())
            {
                cn.Open();
                using (var cmd = new SqlCommand("core.sp_CrearCredito", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdVenta", SqlDbType.Int).Value = idVenta;
                    cmd.Parameters.Add("@IdPlan", SqlDbType.TinyInt).Value = idPlan;

                    var dt = new DataTable();
                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                    return dt;
                }
            }
        }

        /// <summary>
        /// Devuelve las cuotas del crédito correspondiente a la venta indicada.
        /// </summary>
        public DataTable ObtenerCuotasPorVenta(int idVenta)
        {
            using (var cn = SqlConnectionFactory.Create())
            {
                cn.Open();
                using (var da = new SqlDataAdapter(@"
                    SELECT c.IdCredito, q.IdCuota, q.NroCuota, q.ValorCuota, q.FechaVence, q.Pagada
                    FROM core.Credito c
                    JOIN core.CuotaCredito q ON q.IdCredito = c.IdCredito
                    WHERE c.IdVenta = @v
                    ORDER BY q.NroCuota", cn))
                {
                    da.SelectCommand.Parameters.Add("@v", SqlDbType.Int).Value = idVenta;
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        /// <summary>
        /// Marca como pagada una cuota del crédito con el valor indicado (usa sp_PagarCuota).
        /// </summary>
        public void PagarCuota(int idCuota, decimal valor)
        {
            using (var cn = SqlConnectionFactory.Create())
            {
                cn.Open();
                using (var cmd = new SqlCommand("core.sp_PagarCuota", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@IdCuota", SqlDbType.Int).Value = idCuota;
                    cmd.Parameters.Add("@Valor", SqlDbType.Decimal).Value = valor;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ----------- Helpers -----------

        private static DataTable CrearTvpDetalle((int IdProducto, int Cantidad, decimal PrecioUnit)[] items)
        {
            var tvp = new DataTable();
            tvp.Columns.Add("IdProducto", typeof(int));
            tvp.Columns.Add("Cantidad", typeof(int));
            tvp.Columns.Add("PrecioUnit", typeof(decimal));
            foreach (var it in items)
                tvp.Rows.Add(it.IdProducto, it.Cantidad, it.PrecioUnit);
            return tvp;
        }
    }
}
