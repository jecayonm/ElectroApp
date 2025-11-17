using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using ElectroApp.Data;
using ElectroApp.DAO;

namespace ElectroApp
{
    public partial class UsuariosForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Top, Height = 300, AutoGenerateColumns = false };
        private readonly BindingSource _bs = new BindingSource();
        private readonly Panel _panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };

        private readonly Label _lblLogin = new Label { Text = "Login", Left = 10, Top = 10, Width = 80 };
        private readonly TextBox _txtLogin = new TextBox { Left = 100, Top = 8, Width = 220 };

        private readonly Label _lblClave = new Label { Text = "Clave", Left = 10, Top = 40, Width = 80 };
        private readonly TextBox _txtClave = new TextBox { Left = 100, Top = 38, Width = 220, UseSystemPasswordChar = true };

        private readonly Label _lblConfirm = new Label { Text = "Confirmar", Left = 10, Top = 70, Width = 80 };
        private readonly TextBox _txtConfirm = new TextBox { Left = 100, Top = 68, Width = 220, UseSystemPasswordChar = true };

        private readonly Label _lblRol = new Label { Text = "Rol", Left = 350, Top = 10, Width = 80 };
        private readonly ComboBox _cmbRol = new ComboBox { Left = 420, Top = 8, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

        private readonly CheckBox _chkActivo = new CheckBox { Text = "Activo", Left = 420, Top = 40, Width = 100, Checked = true };

        private readonly Button _btnCrear = new Button { Text = "Crear", Left = 100, Top = 100, Width = 100 };
        private readonly Button _btnActualizar = new Button { Text = "Actualizar", Left = 210, Top = 100, Width = 100 };
        private readonly Button _btnCambiarClave = new Button { Text = "Cambiar Clave", Left = 320, Top = 100, Width = 120 };
        private readonly Button _btnEliminar = new Button { Text = "Eliminar", Left = 450, Top = 100, Width = 100 };
        private readonly Button _btnRefrescar = new Button { Text = "Refrescar", Left = 560, Top = 100, Width = 100 };

        private readonly UsuarioDAO _usuarioDao = new UsuarioDAO();

        private DataTable _dtUsuarios;

        public UsuariosForm()
        {
            Text = "Gestión de Usuarios";
            Width = 820;
            Height = 520;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;

            // Important: add panel first so grid docking is applied on top and panel fills remaining area
            Controls.Add(_panel);
            Controls.Add(_grid);

            _panel.Controls.AddRange(new Control[] {
                _lblLogin, _txtLogin,
                _lblClave, _txtClave,
                _lblConfirm, _txtConfirm,
                _lblRol, _cmbRol,
                _chkActivo,
                _btnCrear, _btnActualizar, _btnCambiarClave, _btnEliminar, _btnRefrescar
            });

            // Grid columns
            var colId = new DataGridViewTextBoxColumn { DataPropertyName = "IdUsuario", HeaderText = "Id", Width = 60, ReadOnly = true };
            var colLogin = new DataGridViewTextBoxColumn { DataPropertyName = "Login", HeaderText = "Login", Width = 200 };
            var colRol = new DataGridViewTextBoxColumn { DataPropertyName = "NombreRol", HeaderText = "Rol", Width = 160, ReadOnly = true };
            var colIdRol = new DataGridViewTextBoxColumn { DataPropertyName = "IdRol", HeaderText = "IdRol", Visible = false };
            var colActivo = new DataGridViewCheckBoxColumn { DataPropertyName = "Activo", HeaderText = "Activo", Width = 80 };

            _grid.Columns.AddRange(new DataGridViewColumn[] { colId, colLogin, colRol, colIdRol, colActivo });
            _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            _grid.MultiSelect = false;
            _grid.ReadOnly = false;
            _grid.AllowUserToAddRows = false;
            _grid.AllowUserToDeleteRows = false;
            _grid.RowHeadersVisible = false;

            // Events
            _btnCrear.Click += BtnCrear_Click;
            _btnActualizar.Click += BtnActualizar_Click;
            _btnCambiarClave.Click += BtnCambiarClave_Click;
            _btnEliminar.Click += BtnEliminar_Click;
            _btnRefrescar.Click += (s, e) => Cargar();
            _grid.SelectionChanged += Grid_SelectionChanged;

            Load += (s, e) => { CargarRoles(); Cargar(); };
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
                MessageBox.Show($"Error al cargar roles: {ex.Message}", "Roles", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Cargar()
        {
            try
            {
                _dtUsuarios = new DataTable();
                using (var cn = SqlConnectionFactory.Create())
                using (var cmd = new SqlCommand(@"
                    SELECT u.IdUsuario, u.Login, u.IdRol, r.Nombre AS NombreRol, u.Activo
                    FROM core.Usuario u
                    INNER JOIN core.Rol r ON u.IdRol = r.IdRol
                    ORDER BY u.IdUsuario", cn))
                {
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(_dtUsuarios);
                }

                _bs.DataSource = _dtUsuarios;
                _grid.DataSource = _bs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Grid_SelectionChanged(object sender, EventArgs e)
        {
            if (_grid.CurrentRow == null) return;
            try
            {
                var row = ((DataRowView)_grid.CurrentRow.DataBoundItem).Row;
                _txtLogin.Text = row["Login"].ToString();
                _chkActivo.Checked = Convert.ToBoolean(row["Activo"]);
                _cmbRol.SelectedValue = Convert.ToByte(row["IdRol"]);
                _txtClave.Text = "";
                _txtConfirm.Text = "";
            }
            catch
            {
                // Ignorar errores de selección
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
                    return;
                }

                if (string.IsNullOrEmpty(clave))
                {
                    MessageBox.Show("Ingrese una clave para el usuario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (clave != confirm)
                {
                    MessageBox.Show("Las claves no coinciden.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_cmbRol.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un rol.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                byte idRol = Convert.ToByte(_cmbRol.SelectedValue);
                bool activo = _chkActivo.Checked;

                _usuarioDao.CrearUsuario(login, clave, idRol, activo);

                MessageBox.Show("Usuario creado correctamente.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cargar();
                LimpiarCampos();
            }
            catch (SqlException sqlex) when (sqlex.Number == 2627)
            {
                MessageBox.Show("El login ya existe. Elige otro.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear usuario: {ex.Message}", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            if (_grid.CurrentRow == null) return;
            try
            {
                var row = ((DataRowView)_grid.CurrentRow.DataBoundItem).Row;
                int idUsuario = Convert.ToInt32(row["IdUsuario"]);
                string nuevoLogin = _txtLogin.Text.Trim();

                if (string.IsNullOrWhiteSpace(nuevoLogin))
                {
                    MessageBox.Show("El login no puede estar vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_cmbRol.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un rol.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                byte nuevoRol = Convert.ToByte(_cmbRol.SelectedValue);
                bool activo = _chkActivo.Checked;

                using (var cn = SqlConnectionFactory.Create())
                using (var cmd = new SqlCommand(@"
                    UPDATE core.Usuario
                    SET Login = @login, IdRol = @idRol, Activo = @activo
                    WHERE IdUsuario = @idUsuario", cn))
                {
                    cmd.Parameters.Add("@login", SqlDbType.VarChar, 50).Value = nuevoLogin;
                    cmd.Parameters.Add("@idRol", SqlDbType.TinyInt).Value = nuevoRol;
                    cmd.Parameters.Add("@activo", SqlDbType.Bit).Value = activo;
                    cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Usuario actualizado.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar usuario: {ex.Message}", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCambiarClave_Click(object sender, EventArgs e)
        {
            if (_grid.CurrentRow == null) return;
            try
            {
                var clave = _txtClave.Text;
                var confirm = _txtConfirm.Text;
                if (string.IsNullOrEmpty(clave))
                {
                    MessageBox.Show("Ingrese la nueva clave.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (clave != confirm)
                {
                    MessageBox.Show("Las claves no coinciden.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var row = ((DataRowView)_grid.CurrentRow.DataBoundItem).Row;
                int idUsuario = Convert.ToInt32(row["IdUsuario"]);

                _usuarioDao.ActualizarClaveUsuario(idUsuario, clave);

                MessageBox.Show("Clave actualizada correctamente.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _txtClave.Text = "";
                _txtConfirm.Text = "";
                Cargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar clave: {ex.Message}", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (_grid.CurrentRow == null) return;
            if (MessageBox.Show("¿Eliminar el usuario seleccionado?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                var row = ((DataRowView)_grid.CurrentRow.DataBoundItem).Row;
                int idUsuario = Convert.ToInt32(row["IdUsuario"]);
                using (var cn = SqlConnectionFactory.Create())
                using (var cmd = new SqlCommand("DELETE FROM core.Usuario WHERE IdUsuario = @id", cn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
                MessageBox.Show("Usuario eliminado.", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Cargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar usuario: {ex.Message}", "Usuarios", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            _txtLogin.Text = "";
            _txtClave.Text = "";
            _txtConfirm.Text = "";
            if (_cmbRol.Items.Count > 0) _cmbRol.SelectedIndex = 0;
            _chkActivo.Checked = true;
        }

        private void UsuariosForm_Load(object sender, EventArgs e)
        {
            //dejar vacio
        }

    }
}
