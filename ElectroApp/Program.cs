using System;
using System.Windows.Forms;
using ElectroApp.Data;
using System.Data.SqlClient;
using ElectroApp.Utilities;

namespace ElectroApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Manejo global de excepciones
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                Logger.Log(e.Exception, "ThreadException");
                MessageBox.Show($"Ocurrió un error inesperado:\n{e.Exception.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null)
                {
                    Logger.Log(ex, "UnhandledException");
                }
            };

            // Aplicar tema global (renderer para ToolStrip/MenuStrip/StatusStrip)
            Theme.ApplyGlobals();

            ProbarConexion();

            using (var login = new LoginForm())
            {
                var dr = login.ShowDialog();
                if (dr != DialogResult.OK)
                {
                    return; // salir si no se autentica
                }

                var main = new MainForm();
                // Aplicar tema al formulario principal (propaga a controles)
                Theme.Apply(main);
                main.SetUsuario(login.UsuarioAutenticado, login.IdBitacoraActual);
                Application.Run(main);
            }
        }

        private static void ProbarConexion()
        {
            try
            {
                using (var cn = SqlConnectionFactory.Create())
                {
                    // Muestra la cadena usada (útil para detectar si apunta a la BD correcta)
                    MessageBox.Show("ConnectionString usada:\n" + cn.ConnectionString, "Prueba conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Intentar abrir la conexión
                    cn.Open();
                    MessageBox.Show("Conexión abierta correctamente.", "Prueba conexión", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cn.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex, "ProbarConexion");
                MessageBox.Show("Error de conexión: " + ex.Message, "Prueba conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
