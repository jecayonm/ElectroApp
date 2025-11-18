using System;
using System.Data;
using System.Data.SqlClient;
using ElectroApp.Data;

namespace ElectroApp.Utilities
{
    public static class DbMigrator
    {
        // Ejecuta migraciones mínimas necesarias para nuevas consultas
        public static void EnsureSchema()
        {
            try
            {
                using (var cn = SqlConnectionFactory.Create())
                {
                    cn.Open();

                    // core.Cliente.FechaNacimiento
                    EnsureColumn(cn, "core.Cliente", "FechaNacimiento", "DATE NULL");
                    // core.Cliente.Genero
                    EnsureColumn(cn, "core.Cliente", "Genero", "VARCHAR(10) NULL");

                    // core.Venta.Bruto / Iva / TotalNeto
                    EnsureColumn(cn, "core.Venta", "Bruto", "DECIMAL(18,2) NULL");
                    EnsureColumn(cn, "core.Venta", "Iva", "DECIMAL(18,2) NULL");
                    EnsureColumn(cn, "core.Venta", "TotalNeto", "DECIMAL(18,2) NULL");
                }
            }
            catch
            {
                // No bloquear la app si falla una migración; se puede registrar si se desea
            }
        }

        private static void EnsureColumn(SqlConnection cn, string table, string column, string definition)
        {
            using (var cmd = new SqlCommand($@"
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.objects o ON o.object_id = c.object_id
    WHERE o.type = 'U' AND o.name = PARSENAME('{table}', 1) AND SCHEMA_NAME(o.schema_id) = PARSENAME('{table}', 2) AND c.name = '{column}'
)
BEGIN
    ALTER TABLE {table} ADD {column} {definition};
END", cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }
    }
}
