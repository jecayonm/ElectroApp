using System;
using System.Data;
using System.Windows.Forms;
using ElectroApp.DAO;

namespace ElectroApp
{
    public class PlanesCreditoForm : Form
    {
        private readonly DataGridView _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = true };
        private readonly BindingSource _bs = new BindingSource();
        private readonly ToolStrip _bar = new ToolStrip();
        private readonly ToolStripButton _btnRefrescar = new ToolStripButton("Refrescar");
        private readonly ToolStripButton _btnGuardar = new ToolStripButton("Guardar");
        private readonly ToolStripButton _btnEliminar = new ToolStripButton("Eliminar fila");

        private DataTable _dt;
        private readonly PlanCreditoDAO _dao = new PlanCreditoDAO();

        public PlanesCreditoForm()
        {
            Text = "Planes de crédito";
            Width = 600; Height = 450;

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
                _dt = _dao.GetPlanes();
                _bs.DataSource = _dt;
                _grid.DataSource = _bs;

                if (_grid.Columns.Contains("IdPlan")) _grid.Columns["IdPlan"].ReadOnly = true; // identity
                if (_grid.Columns.Contains("Meses")) _grid.Columns["Meses"].HeaderText = "Meses";
                if (_grid.Columns.Contains("InteresPorc")) _grid.Columns["InteresPorc"].HeaderText = "% Interés";
                if (_grid.Columns.Contains("CuotaInicialPorc")) _grid.Columns["CuotaInicialPorc"].HeaderText = "% Cuota Inicial";

                _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                _grid.AllowUserToAddRows = true;
                _grid.AllowUserToDeleteRows = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar planes: {ex.Message}", "Planes", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"Cambios guardados: {cambios}", "Planes");
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

                if (!int.TryParse(row["Meses"]?.ToString(), out var meses) || (meses != 12 && meses != 18 && meses != 24))
                {
                    MessageBox.Show("Meses debe ser 12, 18 o 24.");
                    return false;
                }

                if (!decimal.TryParse(row["CuotaInicialPorc"]?.ToString(), out var cuotaIni) || cuotaIni != 0.30m)
                {
                    MessageBox.Show("% Cuota Inicial debe ser 0.30 (30%).");
                    return false;
                }

                if (!decimal.TryParse(row["InteresPorc"]?.ToString(), out var interes) || interes != 0.05m)
                {
                    MessageBox.Show("% Interés debe ser 0.05 (5%).");
                    return false;
                }
            }
            return true;
        }
    }
}
