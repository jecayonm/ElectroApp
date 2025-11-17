using System;
using System.Data;
using System.Data.SqlClient;
using ElectroApp.Data;

namespace ElectroApp.DAO
{
    public class ReportesDAO
    {
        // Total ventas por mes
        public DataTable GetTotalVentasPorMes(int anio, int mes)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT v.IdVenta, v.Fecha, SUM(d.Cantidad*d.PrecioUnit) AS Bruto
FROM core.Venta v
JOIN core.DetalleVenta d ON d.IdVenta = v.IdVenta
WHERE YEAR(v.Fecha)=@y AND MONTH(v.Fecha)=@m
GROUP BY v.IdVenta, v.Fecha
ORDER BY v.Fecha", cn))
            {
                da.SelectCommand.Parameters.AddWithValue("@y", anio);
                da.SelectCommand.Parameters.AddWithValue("@m", mes);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Total IVA por trimestre
        public DataTable GetTotalIvaPorTrimestre(int anio, int trimestre)
        {
            int mesIni = (trimestre - 1) * 3 + 1;
            int mesFin = mesIni + 2;
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT SUM(d.Cantidad*d.PrecioUnit*c.Iva) AS TotalIva
FROM core.Venta v
JOIN core.DetalleVenta d ON d.IdVenta=v.IdVenta
JOIN core.Producto p ON p.IdProducto=d.IdProducto
JOIN core.Categoria c ON c.IdCategoria=p.IdCategoria
WHERE YEAR(v.Fecha)=@y AND MONTH(v.Fecha) BETWEEN @mi AND @mf", cn))
            {
                da.SelectCommand.Parameters.AddWithValue("@y", anio);
                da.SelectCommand.Parameters.AddWithValue("@mi", mesIni);
                da.SelectCommand.Parameters.AddWithValue("@mf", mesFin);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Ventas por tipo (crédito vs contado) en período
        public DataTable GetVentasPorTipo(DateTime desde, DateTime hasta)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT v.TipoPago, COUNT(*) AS Cantidad, SUM(d.Cantidad*d.PrecioUnit) AS Bruto
FROM core.Venta v
JOIN core.DetalleVenta d ON d.IdVenta=v.IdVenta
WHERE v.Fecha BETWEEN @d AND @h
GROUP BY v.TipoPago", cn))
            {
                da.SelectCommand.Parameters.AddWithValue("@d", desde);
                da.SelectCommand.Parameters.AddWithValue("@h", hasta);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Inventario por categoría con costo asociado
        public DataTable GetInventarioPorCategoria()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT c.Nombre AS Categoria, SUM(p.Stock) AS Unidades, SUM(p.Stock*p.Costo) AS CostoTotal
FROM core.Producto p
JOIN core.Categoria c ON c.IdCategoria=p.IdCategoria
GROUP BY c.Nombre", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Clientes morosos (cuotas vencidas no pagadas)
        public DataTable GetClientesMorosos()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT DISTINCT cl.IdCliente, cl.Nombres, cl.Apellidos, v.IdVenta
FROM core.Cliente cl
JOIN core.Venta v ON v.IdCliente=cl.IdCliente
JOIN core.Credito cr ON cr.IdVenta=v.IdVenta
JOIN core.CuotaCredito q ON q.IdCredito=cr.IdCredito
WHERE q.Pagada=0 AND q.FechaVence < GETDATE()", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}
