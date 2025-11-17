using ElectroApp.Data;
using ElectroApp.Models;
using ElectroApp.Security;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace ElectroApp.DAO
{
    public class UsuarioDAO
    {
        // Devuelve Usuario completo (incluye PassHash y Salt) o null si no existe/está inactivo
        public Usuario GetUsuarioPorLogin(string login)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                SELECT u.IdUsuario, u.Login, u.PassHash, u.Salt, u.IdRol, r.Nombre AS NombreRol, u.Activo
                FROM core.Usuario u
                INNER JOIN core.Rol r ON u.IdRol = r.IdRol
                WHERE u.Login = @login", cn))
            {
                cmd.Parameters.Add("@login", SqlDbType.VarChar, 50).Value = login;
                cn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    return new Usuario
                    {
                        IdUsuario = Convert.ToInt32(rdr["IdUsuario"]),
                        Login = rdr["Login"].ToString(),
                        PassHash = (byte[])rdr["PassHash"],
                        Salt = (byte[])rdr["Salt"],
                        IdRol = Convert.ToByte(rdr["IdRol"]),
                        NombreRol = rdr["NombreRol"].ToString(),
                        Activo = Convert.ToBoolean(rdr["Activo"])
                    };
                }
            }
        }

        // Verifica la contraseña usando PBKDF2 (Rfc2898DeriveBytes)
        public bool VerificarClave(Usuario u, string claveIngresada)
        {
            if (u == null || !u.Activo) return false;
            if (u.Salt == null || u.PassHash == null) return false;

            // Use same PRF (HMACSHA256) and iterations as when generating the hash
            using (var pbkdf2 = new Rfc2898DeriveBytes(claveIngresada, u.Salt, 100_000, HashAlgorithmName.SHA256))
            {
                var hash = pbkdf2.GetBytes(u.PassHash.Length);
                return FixedTimeEquals(hash, u.PassHash);
            }
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
            {
                diff |= a[i] ^ b[i];
            }
            return diff == 0;
        }

        // Registrar entrada en bitácora y devolver IdBitacora (bigint)
        public long RegistrarEntrada(int idUsuario, string origen = null)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                INSERT INTO core.BitacoraAcceso (IdUsuario, FechaIngreso, Origen)
                VALUES (@idUsuario, @fecha, @origen);
                SELECT CAST(SCOPE_IDENTITY() AS bigint);", cn))
            {
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@fecha", SqlDbType.DateTime2).Value = DateTime.Now;
                cmd.Parameters.Add("@origen", SqlDbType.NVarChar, 100).Value = (object)origen ?? DBNull.Value;
                cn.Open();
                var res = cmd.ExecuteScalar();
                return res == null ? 0 : Convert.ToInt64(res);
            }
        }

        // Registrar salida por IdBitacora
        public void RegistrarSalida(long idBitacora)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE core.BitacoraAcceso
                SET FechaSalida = @fecha
                WHERE IdBitacora = @idBitacora", cn))
            {
                cmd.Parameters.Add("@fecha", SqlDbType.DateTime2).Value = DateTime.Now;
                cmd.Parameters.Add("@idBitacora", SqlDbType.BigInt).Value = idBitacora;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Alternativa: actualizar la última entrada abierta para un usuario (si no guardaste IdBitacora)
        public void RegistrarSalidaPorUsuario(int idUsuario)
        {
            using (var cn = SqlConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE core.BitacoraAcceso
                SET FechaSalida = @fecha
                WHERE IdUsuario = @idUsuario AND FechaSalida IS NULL", cn))
            {
                cmd.Parameters.Add("@fecha", SqlDbType.DateTime2).Value = DateTime.Now;
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Crea un usuario nuevo con salt+hash. Devuelve el Id insertado.
        public int CrearUsuario(string login, string password, byte idRol, bool activo = true)
        {
            var salt = SecurityHelper.GenerateSalt(16);          // 16 bytes
            var hash = SecurityHelper.GenerateHash(password, salt, 100_000, 64); // 64 bytes

            using (var cn = SqlConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                INSERT INTO core.Usuario (Login, PassHash, Salt, IdRol, Activo)
                VALUES (@login, @passHash, @salt, @idRol, @activo);
                SELECT CAST(SCOPE_IDENTITY() AS int);", cn))
            {
                cmd.Parameters.Add("@login", SqlDbType.VarChar, 50).Value = login;
                cmd.Parameters.Add("@passHash", SqlDbType.VarBinary, 64).Value = hash;
                cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = salt;
                cmd.Parameters.Add("@idRol", SqlDbType.TinyInt).Value = idRol;
                cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo;
                cn.Open();
                var res = cmd.ExecuteScalar();
                return res == null ? 0 : Convert.ToInt32(res);
            }
        }

        // Actualiza la contraseña de un usuario (genera nuevo salt y hash)
        public void ActualizarClaveUsuario(int idUsuario, string nuevaPassword)
        {
            var salt = SecurityHelper.GenerateSalt(16);
            var hash = SecurityHelper.GenerateHash(nuevaPassword, salt, 100_000, 64);

            using (var cn = SqlConnectionFactory.Create())
            using (var cmd = new SqlCommand(@"
                UPDATE core.Usuario
                SET PassHash = @passHash, Salt = @salt
                WHERE IdUsuario = @idUsuario;", cn))
            {
                cmd.Parameters.Add("@passHash", SqlDbType.VarBinary, 64).Value = hash;
                cmd.Parameters.Add("@salt", SqlDbType.VarBinary, 16).Value = salt;
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

    }
}
