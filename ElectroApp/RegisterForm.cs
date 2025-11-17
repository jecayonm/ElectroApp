using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Data;

namespace ElectroApp
{
    public class RegisterForm : Form
    {
        private readonly Label _lblLogin = new Label { Text = "Login", Left = 10, Top = 15, Width = 70 };
        private readonly TextBox _txtLogin = new TextBox { Left = 90, Top = 12, Width = 200 };

        private readonly Label _lblClave = new Label { Text = "Clave", Left = 10, Top = 50, Width = 70 };
        private readonly TextBox _txtClave = new TextBox { Left = 90, Top = 47, Width = 200, UseSystemPasswordChar = true };

        private readonly Label _lblConfirm = new Label { Text = "Confirmar", Left = 10, Top = 85, Width = 70 };
        private readonly TextBox _txtConfirm = new TextBox { Left = 90, Top = 82, Width = 200, UseSystemPasswordChar = true };

        private readonly Label _lblRol = new Label { Text = "Rol", Left = 10, Top = 120, Width = 70 };
        private readonly ComboBox _cmbRol = new ComboBox { Left = 90, Top = 117, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

        private readonly CheckBox _chkActivo = new CheckBox { Text = "Activo", Left = 90, Top = 147, Width = 80, Checked = true };

        private readonly Button _btnCrear = new Button { Text = "Crear", Left = 90, Top = 185, Width = 90 };
        private readonly Button _btnCancelar = new Button { Text = "Cancelar", Left = 200, Top = 185, Width = 90 };

        private readonly UsuarioDAO _usuarioDao = new UsuarioDAO();

        public RegisterForm()
        {
            Text = "Registrar usuario";
            Width = 340;
            Height = 270;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            Controls.AddRange(new Control[] {
                _lblLogin, _txtLogin,
                _lblClave, _txtClave,
                _lblConfirm, _txtConfirm,
                _lblRol, _cmbRol,
                _chkActivo,
                _btnCrear, _btnCancelar
            });

            _btnCrear.Click += BtnCrear_Click;
            _btnCancelar.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Load += (s, e) => { CargarRoles(); _txtLogin.Focus(); };
        }

        private void CargarRoles()
        {
            try
            {
                var dt = new DataTable();
                using (var cn = SqlConnectionFactory.Create())
                using (var cmd = new SqlCommand("SELECT IdRol, Nombre FROM core.Rol ORDER BY IdRol", cn))
                {
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(dt);
                }

                _cmbRol.DataSource = dt;
                _cmbRol.DisplayMember = "Nombre";
                _cmbRol.ValueMember = "IdRol";
                if (_cmbRol.Items.Count > 0) _cmbRol.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar roles: {ex.Message}", "Registrar usuario", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCrear_Click(object sender, EventArgs e)
        {
            try
            {
                var login = _txtLogin.Text.Trim();
                var clave = _txtClave.Text;
                var confirm = _txtConfirm.Text;

                if (string.IsNullOrWhiteSpace(login))
                {
                    MessageBox.Show("El login no puede estar vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtLogin.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(clave))
                {
                    MessageBox.Show("Ingrese una clave.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtClave.Focus();
                    return;
                }

                if (clave != confirm)
                {
                    MessageBox.Show("Las claves no coinciden.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _txtConfirm.Focus();
                    return;
                }

                if (_cmbRol.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un rol.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                byte idRol = Convert.ToByte(_cmbRol.SelectedValue);
                bool activo = _chkActivo.Checked;

                int newId = _usuarioDao.CrearUsuario(login, clave, idRol, activo);
                if (newId > 0)
                {
                    MessageBox.Show("Usuario creado correctamente.", "Registrar usuario", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("No se pudo crear el usuario.", "Registrar usuario", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (SqlException sqlex) when (sqlex.Number == 2627)
            {
                MessageBox.Show("El login ya existe. Elige otro.", "Registrar usuario", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear usuario: {ex.Message}", "Registrar usuario", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
