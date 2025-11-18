using System.Data;
using System.Data.SqlClient;
using ElectroApp.Data;

namespace ElectroApp.DAO
{
    public class ClienteDAO
    {
        private const string SelectSql = @"SELECT IdCliente, Nombres, Apellidos, Documento, Telefono, Email, FechaNacimiento, Genero
                  FROM core.Cliente
                  ORDER BY IdCliente";

        // Carga el DataTable y configura el SqlDataAdapter + CommandBuilder para CRUD (sin retener estado)
        public DataTable GetClientes()
        {
            var dt = new DataTable { TableName = "Cliente" };

            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(SelectSql, cn))
            {
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                var cb = new SqlCommandBuilder(da)
                {
                    ConflictOption = ConflictOption.OverwriteChanges
                };

                da.InsertCommand = cb.GetInsertCommand(true);
                da.UpdateCommand = cb.GetUpdateCommand();
                da.DeleteCommand = cb.GetDeleteCommand();

                da.Fill(dt);
            }
            return dt;
        }

        // Persiste los cambios creando un nuevo DataAdapter/CommandBuilder para esta operación
        public int SaveChanges(DataTable dt)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(SelectSql, cn))
            {
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;

                var cb = new SqlCommandBuilder(da)
                {
                    ConflictOption = ConflictOption.OverwriteChanges
                };

                da.InsertCommand = cb.GetInsertCommand(true);
                da.UpdateCommand = cb.GetUpdateCommand();
                da.DeleteCommand = cb.GetDeleteCommand();

                return da.Update(dt);
            }
        }

        // Eliminación directa por Id (útil si necesitas borrar fuera del DataGridView)
        public void DeleteById(int idCliente)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var cmd = new SqlCommand("DELETE FROM core.Cliente WHERE IdCliente = @id", cn))
            {
                cn.Open();
                cmd.Parameters.AddWithValue("@id", idCliente);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
