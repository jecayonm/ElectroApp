using System.Data;
using System.Data.SqlClient;
using ElectroApp.Data;

namespace ElectroApp.DAO
{
    public class CategoriaDAO
    {
        private const string SelectSql = @"SELECT IdCategoria, Nombre, Iva, Utilidad
FROM core.Categoria
ORDER BY IdCategoria";

        public DataTable GetCategorias()
        {
            var dt = new DataTable { TableName = "Categoria" };
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(SelectSql, cn))
            {
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                var cb = new SqlCommandBuilder(da) { ConflictOption = ConflictOption.OverwriteChanges };
                da.InsertCommand = cb.GetInsertCommand(true);
                da.UpdateCommand = cb.GetUpdateCommand();
                da.DeleteCommand = cb.GetDeleteCommand();
                da.Fill(dt);
            }
            return dt;
        }

        public int SaveChanges(DataTable dt)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var da = new SqlDataAdapter(SelectSql, cn))
            {
                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                var cb = new SqlCommandBuilder(da) { ConflictOption = ConflictOption.OverwriteChanges };
                da.InsertCommand = cb.GetInsertCommand(true);
                da.UpdateCommand = cb.GetUpdateCommand();
                da.DeleteCommand = cb.GetDeleteCommand();
                return da.Update(dt);
            }
        }
    }
}
