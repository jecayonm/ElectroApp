using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public partial class ClientesForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripButton _btnGuardar = new ToolStripButton("Guardar");
        private readonly ToolStripButton _btnEliminar = new ToolStripButton("Eliminar fila");
        private TableLayoutPanel _layout;

        private DataTable _dt;
        private readonly ClienteDAO _dao = new ClienteDAO();

        public ClientesForm()
        {
            InitializeComponent();
            Text = "Clientes";
            Width = 900; Height = 550;
            StartPosition = FormStartPosition.CenterParent;
            SetupUi();
            Load += (s, e) => Cargar();
            Shown += (s, e) => Theme.Apply(this);
        }

        private void SetupUi()
        {
            _btnRefrescar.Click += (s, e) => Cargar();
            _btnGuardar.Click += (s, e) => Guardar();
            _btnEliminar.Click += (s, e) => EliminarFilaSeleccionada();
            _bar.Items.AddRange(new ToolStripItem[] { _btnRefrescar, _btnGuardar, _btnEliminar });

            _layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // ToolStrip altura auto
            _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // Grilla ocupa el resto

            _layout.Controls.Add(_bar, 0, 0);
            _layout.Controls.Add(_grid, 0, 1);

            Controls.Add(_layout);
        }

        private void Cargar()
        {
            try
            {
                _dt = _dao.GetClientes();
                _bs.DataSource = _dt;
                _grid.DataSource = _bs;

                if (_grid.Columns.Contains("IdCliente"))
                    _grid.Columns["IdCliente"].ReadOnly = true; // identity

                // Configurar columnas nuevas si existen
                if (_grid.Columns.Contains("FechaNacimiento"))
                {
                    _grid.Columns["FechaNacimiento"].HeaderText = "Fecha Nac.";
                    _grid.Columns["FechaNacimiento"].DefaultCellStyle.Format = "dd/MM/yyyy";
                }
                if (_grid.Columns.Contains("Genero"))
                {
                    _grid.Columns["Genero"].HeaderText = "Género";
                }

                _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                _grid.AllowUserToAddRows = true;
                _grid.AllowUserToDeleteRows = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Guardar()
        {
            try
            {
                if (!ValidarTabla(_dt)) return;

                Validate();
                _bs.EndEdit();

                int cambios = _dao.SaveChanges(_dt);
                MessageBox.Show($"Cambios guardados: {cambios}", "Clientes");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Guardar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EliminarFilaSeleccionada()
        {
            if (_grid.CurrentRow == null) return;

            if (MessageBox.Show("¿Eliminar fila seleccionada?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _grid.Rows.Remove(_grid.CurrentRow);
                    Guardar();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se pudo eliminar: {ex.Message}", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Validaciones básicas antes de guardar
        private bool ValidarTabla(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState != DataRowState.Added && row.RowState != DataRowState.Modified)
                    continue;

                string documento = row["Documento"]?.ToString()?.Trim() ?? "";
                string nombres = row["Nombres"]?.ToString()?.Trim() ?? "";
                string apellidos = row["Apellidos"]?.ToString()?.Trim() ?? "";
                string email = row["Email"]?.ToString()?.Trim() ?? "";

                if (string.IsNullOrWhiteSpace(documento))
                {
                    MessageBox.Show("El Documento no puede estar vacío.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (string.IsNullOrWhiteSpace(nombres))
                {
                    MessageBox.Show("Los Nombres no pueden estar vacíos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (string.IsNullOrWhiteSpace(apellidos))
                {
                    MessageBox.Show("Los Apellidos no pueden estar vacíos.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                if (!string.IsNullOrEmpty(email) && !EsEmailValido(email))
                {
                    MessageBox.Show("El Email no tiene un formato válido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Validación opcional de género
                if (dt.Columns.Contains("Genero"))
                {
                    var g = row["Genero"]?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(g) && !(g.Equals("M", StringComparison.OrdinalIgnoreCase) || g.Equals("F", StringComparison.OrdinalIgnoreCase) || g.Equals("Hombre", StringComparison.OrdinalIgnoreCase) || g.Equals("Mujer", StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("Género permitido: M/F/Hombre/Mujer", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }
            }
            return true;
        }

        private bool EsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void ClientesForm_Load(object sender, EventArgs e)
        {
            // No usar: la carga ya se hace en el constructor mediante el evento Load
        }
    }
}
