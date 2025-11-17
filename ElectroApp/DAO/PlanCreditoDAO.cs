using System.Data;
using System.Data.SqlClient;
using ElectroApp.Data;

namespace ElectroApp.DAO
{
    public class PlanCreditoDAO
    {
        private const string SelectSql = @"SELECT IdPlan, Meses, InteresPorc, CuotaInicialPorc
FROM core.PlanCredito
ORDER BY Meses";

        public DataTable GetPlanes()
        {
            var dt = new DataTable { TableName = "PlanCredito" };
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
