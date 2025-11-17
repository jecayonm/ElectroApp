using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Data;

namespace ElectroApp
{
    public partial class LoginForm : Form
    {
        private readonly UsuarioDAO _usuarioDao = new UsuarioDAO();

        public ElectroApp.Models.Usuario UsuarioAutenticado { get; private set; }
        public long IdBitacoraActual { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Mostrar el link de registro siempre para permitir crear cuentas manualmente
            try
            {
                lnkCrearPrimerUsuario.Visible = true;
                lnkCrearPrimerUsuario.Text = "Registrar usuario";
            }
            catch
            {
                lnkCrearPrimerUsuario.Visible = true;
            }
        }

        private bool NoHayUsuarios()
        {
            try
            {
                using (var cn = SqlConnectionFactory.Create())
                using (var cmd = new SqlCommand("SELECT COUNT(1) FROM core.Usuario", cn))
                {
                    cn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                    return count == 0;
                }
            }
            catch
            {
                // En caso de error de conexión no mostramos el link
                return false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Botón Ingresar
            errorProvider1.Clear();

            var login = txtUsuario.Text.Trim();
            var clave = txtClave.Text;

            if (string.IsNullOrEmpty(login))
            {
                errorProvider1.SetError(txtUsuario, "Ingrese usuario");
                txtUsuario.Focus();
                return;
            }

            if (string.IsNullOrEmpty(clave))
            {
                errorProvider1.SetError(txtClave, "Ingrese clave");
                txtClave.Focus();
                return;
            }

            try
            {
                var usuario = _usuarioDao.GetUsuarioPorLogin(login);
                if (usuario == null)
                {
                    MessageBox.Show("Usuario no encontrado o inactivo.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!_usuarioDao.VerificarClave(usuario, clave))
                {
                    MessageBox.Show("Usuario o clave inválidos.", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Autenticación exitosa
                UsuarioAutenticado = new ElectroApp.Models.Usuario
                {
                    IdUsuario = usuario.IdUsuario,
                    Login = usuario.Login,
                    IdRol = usuario.IdRol,
                    NombreRol = usuario.NombreRol,
                    Activo = usuario.Activo,
                    PassHash = null,
                    Salt = null
                };

                // Registrar entrada
                IdBitacoraActual = _usuarioDao.RegistrarEntrada(usuario.IdUsuario, Environment.MachineName);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de autenticación: {ex.Message}", "Login", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Abrir RegisterForm para registro manual
            try
            {
                using (var rf = new RegisterForm())
                {
                    var dr = rf.ShowDialog(this);
                    if (dr == DialogResult.OK)
                    {
                        // Si el registro fue exitoso, rellenar el nombre de usuario para facilitar el login
                        txtUsuario.Text = rf.Controls.OfType<TextBox>().FirstOrDefault(t => t.Name == "_txtLogin")?.Text ?? string.Empty;
                        txtClave.Focus();
                    }
                }

                // Después de cerrar el formulario, colocar el foco en el campo de usuario
                txtUsuario.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al abrir el formulario de registro: {ex.Message}", "Registrar usuario", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // no-op
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Limpiar validación cuando el usuario escribe
            errorProvider1.SetError(txtClave, string.Empty);
            errorProvider1.SetError(txtUsuario, string.Empty);
        }
    }
}
