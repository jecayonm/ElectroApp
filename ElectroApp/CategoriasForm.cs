using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;

namespace ElectroApp
{
    public class CategoriasForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripButton _btnGuardar = new ToolStripButton("Guardar");
        private readonly ToolStripButton _btnEliminar = new ToolStripButton("Eliminar fila");

        private DataTable _dt;
        private readonly CategoriaDAO _dao = new CategoriaDAO();

        public CategoriasForm()
        {
            Text = "Categorías";
            Width = 700; Height = 500;

            _bar.Items.AddRange(new ToolStripItem[] { _btnRefrescar, _btnGuardar, _btnEliminar });
            _btnRefrescar.Click += (s, e) => Cargar();
            _btnGuardar.Click += (s, e) => Guardar();
            _btnEliminar.Click += (s, e) => EliminarFilaSeleccionada();

            Controls.Add(_grid);
            Controls.Add(_bar);
            _bar.Dock = DockStyle.Top;

            Load += (s, e) => Cargar();
        }

        private void Cargar()
        {
            try
            {
                _dt = _dao.GetCategorias();
                _bs.DataSource = _dt;
                _grid.DataSource = _bs;

                if (_grid.Columns.Contains("IdCategoria")) _grid.Columns["IdCategoria"].ReadOnly = true; // identity
                if (_grid.Columns.Contains("Nombre")) _grid.Columns["Nombre"].HeaderText = "Nombre";
                if (_grid.Columns.Contains("Iva")) _grid.Columns["Iva"].HeaderText = "IVA";
                if (_grid.Columns.Contains("Utilidad")) _grid.Columns["Utilidad"].HeaderText = "% Utilidad";

                _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                _grid.AllowUserToAddRows = true;
                _grid.AllowUserToDeleteRows = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", "Categorías", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"Cambios guardados: {cambios}", "Categorías");
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

        private bool ValidarTabla(DataTable dt)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState != DataRowState.Added && row.RowState != DataRowState.Modified)
                    continue;

                string nombre = row["Nombre"]?.ToString()?.Trim() ?? "";
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    MessageBox.Show("El nombre no puede estar vacío.");
                    return false;
                }

                if (!decimal.TryParse(row["Iva"]?.ToString(), out var iva) || iva < 0 || iva > 1)
                {
                    MessageBox.Show("IVA debe estar entre 0 y 1 (ej: 0.19).");
                    return false;
                }
                if (!decimal.TryParse(row["Utilidad"]?.ToString(), out var util) || util < 0 || util > 1)
                {
                    MessageBox.Show("% Utilidad debe estar entre 0 y 1 (ej: 0.35).");
                    return false;
                }
            }
            return true;
        }
    }
}
