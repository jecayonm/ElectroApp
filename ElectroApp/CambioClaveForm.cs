using System;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Security;

namespace ElectroApp
{
    public class CambioClaveForm : Form
    {
        private readonly Label _lblActual = new Label { Text = "Clave actual", Left = 10, Top = 15, Width = 120 };
        private readonly TextBox _txtActual = new TextBox { Left = 140, Top = 12, Width = 200, UseSystemPasswordChar = true };

        private readonly Label _lblNueva = new Label { Text = "Nueva clave", Left = 10, Top = 50, Width = 120 };
        private readonly TextBox _txtNueva = new TextBox { Left = 140, Top = 47, Width = 200, UseSystemPasswordChar = true };

        private readonly Label _lblConfirm = new Label { Text = "Confirmar", Left = 10, Top = 85, Width = 120 };
        private readonly TextBox _txtConfirm = new TextBox { Left = 140, Top = 82, Width = 200, UseSystemPasswordChar = true };

        private readonly Button _btnCambiar = new Button { Text = "Cambiar", Left = 140, Top = 120, Width = 90 };
        private readonly Button _btnCancelar = new Button { Text = "Cancelar", Left = 250, Top = 120, Width = 90 };

        private readonly UsuarioDAO _dao = new UsuarioDAO();

        public CambioClaveForm()
        {
            Text = "Cambiar contraseña";
            Width = 370; Height = 210; StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
            Controls.AddRange(new Control[] { _lblActual, _txtActual, _lblNueva, _txtNueva, _lblConfirm, _txtConfirm, _btnCambiar, _btnCancelar });
            _btnCambiar.Click += BtnCambiar_Click;
            _btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
        }

        private void BtnCambiar_Click(object sender, EventArgs e)
        {
            try
            {
                var u = UserSession.CurrentUser;
                if (u == null) { MessageBox.Show("No hay sesión activa."); return; }

                var actual = _txtActual.Text;
                var nueva = _txtNueva.Text;
                var confirm = _txtConfirm.Text;

                if (string.IsNullOrEmpty(actual) || string.IsNullOrEmpty(nueva))
                { MessageBox.Show("Complete todos los campos."); return; }
                if (nueva != confirm)
                { MessageBox.Show("La confirmación no coincide."); return; }

                // Validar clave actual
                var full = _dao.GetUsuarioPorLogin(u.Login);
                if (!_dao.VerificarClave(full, actual))
                {
                    MessageBox.Show("La clave actual es incorrecta.", "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _dao.ActualizarClaveUsuario(u.IdUsuario, nueva);
                MessageBox.Show("Contraseña actualizada.", "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Cambiar contraseña", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
