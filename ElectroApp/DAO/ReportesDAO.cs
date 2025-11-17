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

        // Inventario por categoría con costo asociado (básico)
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

        // Inventario detallado: agrega potencial de venta y margen estimado
        public DataTable GetInventarioPorCategoriaDetalle()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT c.Nombre AS Categoria,
       SUM(p.Stock) AS Unidades,
       SUM(p.Stock*p.Costo) AS CostoTotal,
       SUM(p.Stock*p.PrecioVenta) AS PotencialVenta,
       CASE WHEN SUM(p.Stock*p.PrecioVenta)=0 THEN 0
            ELSE (SUM(p.Stock*p.PrecioVenta) - SUM(p.Stock*p.Costo)) END AS MargenEstimado
FROM core.Producto p
JOIN core.Categoria c ON c.IdCategoria=p.IdCategoria
GROUP BY c.Nombre
ORDER BY c.Nombre", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // Clientes morosos (cuotas vencidas no pagadas) básico
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

        // Clientes morosos resumen: número de cuotas vencidas, total vencido, días de mayor mora
        public DataTable GetClientesMorososResumen()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT cl.IdCliente,
       cl.Nombres,
       cl.Apellidos,
       v.IdVenta,
       COUNT(*) AS CuotasVencidas,
       SUM(q.ValorCuota) AS TotalVencido,
       MAX(DATEDIFF(DAY, q.FechaVence, GETDATE())) AS MaxDiasMora,
       MIN(q.FechaVence) AS PrimeraFechaVencida
FROM core.Cliente cl
JOIN core.Venta v      ON v.IdCliente=cl.IdCliente
JOIN core.Credito cr   ON cr.IdVenta=v.IdVenta
JOIN core.CuotaCredito q ON q.IdCredito=cr.IdCredito
WHERE q.Pagada=0 AND q.FechaVence < GETDATE()
GROUP BY cl.IdCliente, cl.Nombres, cl.Apellidos, v.IdVenta
ORDER BY TotalVencido DESC", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // ================= NUEVAS CONSULTAS (solo visualización) =================

        // 1. Listado de productos con margen y utilidad
        public DataTable GetProductosMargenUtilidad()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT p.IdProducto, p.Codigo, p.Descripcion,
       p.Costo, p.PrecioVenta, p.Stock,
       c.Iva, c.Utilidad,
       (p.PrecioVenta - p.Costo) AS MargenUnitario,
       CASE WHEN p.Costo = 0 THEN 0 ELSE (p.PrecioVenta - p.Costo)/p.Costo END AS MargenUnitarioPorc,
       (p.Stock * p.Costo) AS CostoInventario,
       (p.Stock * p.PrecioVenta) AS PotencialVenta,
       (p.Stock * (p.PrecioVenta - p.Costo)) AS MargenInventario,
       CASE WHEN (p.Stock * p.Costo)=0 THEN 0 ELSE (p.Stock * (p.PrecioVenta - p.Costo)) / (p.Stock * p.Costo) END AS MargenInventarioPorc
FROM core.Producto p
LEFT JOIN core.Categoria c ON c.IdCategoria = p.IdCategoria
ORDER BY p.Descripcion", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // 2. Ventas por cliente en rango de fechas
        public DataTable GetVentasPorCliente(DateTime desde, DateTime hasta)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT v.IdCliente, (cl.Nombres+' '+cl.Apellidos) AS Cliente,
       COUNT(DISTINCT v.IdVenta) AS NumVentas,
       SUM(d.Cantidad*d.PrecioUnit) AS Bruto,
       SUM(d.Cantidad*d.PrecioUnit*c.Iva) AS IVA,
       SUM(d.Cantidad*d.PrecioUnit*(1+c.Iva)) AS Neto
FROM core.Venta v
JOIN core.Cliente cl ON cl.IdCliente = v.IdCliente
JOIN core.DetalleVenta d ON d.IdVenta = v.IdVenta
JOIN core.Producto p ON p.IdProducto = d.IdProducto
JOIN core.Categoria c ON c.IdCategoria = p.IdCategoria
WHERE v.Fecha BETWEEN @d AND @h
GROUP BY v.IdCliente, cl.Nombres, cl.Apellidos
ORDER BY Neto DESC", cn))
            {
                da.SelectCommand.Parameters.AddWithValue("@d", desde);
                da.SelectCommand.Parameters.AddWithValue("@h", hasta);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // 3. Clientes sin compras en las últimas N semanas
        public DataTable GetClientesSinCompras(int semanas)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT cl.IdCliente, cl.Nombres, cl.Apellidos, cl.Documento, cl.Telefono, cl.Email
FROM core.Cliente cl
WHERE NOT EXISTS (
  SELECT 1 FROM core.Venta v
  WHERE v.IdCliente = cl.IdCliente AND v.Fecha >= DATEADD(WEEK, -@sem, GETDATE())
)
ORDER BY cl.Nombres, cl.Apellidos", cn))
            {
                da.SelectCommand.Parameters.AddWithValue("@sem", semanas);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // 4. Créditos activos por estado (pagado / con mora / activo al día)
        public DataTable GetCreditosPorEstado()
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT cr.IdCredito, v.IdVenta, v.Fecha AS FechaVenta,
       cl.IdCliente, (cl.Nombres+' '+cl.Apellidos) AS Cliente,
       CASE 
         WHEN NOT EXISTS (SELECT 1 FROM core.CuotaCredito q WHERE q.IdCredito=cr.IdCredito AND q.Pagada=0) THEN 'PAGADO'
         WHEN EXISTS (SELECT 1 FROM core.CuotaCredito q WHERE q.IdCredito=cr.IdCredito AND q.Pagada=0 AND q.FechaVence < GETDATE()) THEN 'CON_MORA'
         ELSE 'ACTIVO'
       END AS Estado,
       (SELECT COUNT(1) FROM core.CuotaCredito q WHERE q.IdCredito=cr.IdCredito) AS TotalCuotas,
       (SELECT COUNT(1) FROM core.CuotaCredito q WHERE q.IdCredito=cr.IdCredito AND q.Pagada=1) AS CuotasPagadas,
       (SELECT COUNT(1) FROM core.CuotaCredito q WHERE q.IdCredito=cr.IdCredito AND q.Pagada=0 AND q.FechaVence < GETDATE()) AS CuotasVencidas
FROM core.Credito cr
JOIN core.Venta v ON v.IdVenta = cr.IdVenta
JOIN core.Cliente cl ON cl.IdCliente = v.IdCliente
ORDER BY Estado, FechaVenta DESC", cn))
            {
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // 5. Stock bajo (productos con stock < mínimo global indicado)
        public DataTable GetProductosStockBajo(int minimo)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(@"SELECT p.IdProducto, p.Codigo, p.Descripcion, p.Stock, @min AS Minimo,
       p.Costo, p.PrecioVenta
FROM core.Producto p
WHERE p.Stock < @min
ORDER BY p.Stock ASC", cn))
            {
                da.SelectCommand.Parameters.AddWithValue("@min", minimo);
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}
