using System;
using System.Configuration;
using System.Data.SqlClient;

namespace ElectroApp.Data
{
    public static class SqlConnectionFactory
    {
        public static SqlConnection Create()
        {
            var csSettings = ConfigurationManager.ConnectionStrings["ElectroDb"]; // coincide con App.config
            if (csSettings == null)
                throw new InvalidOperationException("No se encontró la cadena de conexión 'ElectroDB' en App.config.");
            return new SqlConnection(csSettings.ConnectionString);
        }
    }
}
