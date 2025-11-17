using System;
using System.Linq;
using System.Windows.Forms;
using ElectroApp.DAO;
using ElectroApp.Models;
using Microsoft.VisualBasic; // InputBox
using System; // para DateTime
using ElectroApp.Security; // UserSession
using ElectroApp.Utilities; // Theme

namespace ElectroApp
{
    public partial class MainForm : Form
    {
        private Usuario _usuario;
        private long _idBitacora;

        private readonly MenuStrip _menu = new MenuStrip();
        private readonly StatusStrip _status = new StatusStrip();
        private readonly ToolStripStatusLabel _lblUsuario = new ToolStripStatusLabel();

        private readonly ToolStripMenuItem entidadesMenu = new ToolStripMenuItem("Entidades");
        private readonly ToolStripMenuItem clientesMenuItem = new ToolStripMenuItem("Clientes");
        private readonly ToolStripMenuItem productosMenuItem = new ToolStripMenuItem("Productos");
        private readonly ToolStripMenuItem categoriasMenuItem = new ToolStripMenuItem("Categorías");
        private readonly ToolStripMenuItem planesMenuItem = new ToolStripMenuItem("Planes crédito");
        private readonly ToolStripMenuItem usuariosMenuItem = new ToolStripMenuItem("Usuarios");

        private readonly ToolStripMenuItem transaccionesMenu = new ToolStripMenuItem("Transacciones");
        private readonly ToolStripMenuItem ventasMenuItem = new ToolStripMenuItem("Ventas");
        private readonly ToolStripMenuItem cuotasMenuItem = new ToolStripMenuItem("Cuotas");

        private readonly ToolStripMenuItem ventanasMenu = new ToolStripMenuItem("Ventanas");
        private readonly ToolStripMenuItem cascadaMenuItem = new ToolStripMenuItem("Cascada");
        private readonly ToolStripMenuItem mosaicoHMenuItem = new ToolStripMenuItem("Mosaico Horizontal");
        private readonly ToolStripMenuItem mosaicoVMenuItem = new ToolStripMenuItem("Mosaico Vertical");
        private readonly ToolStripMenuItem organizarIconosMenuItem = new ToolStripMenuItem("Organizar iconos");

        private readonly ToolStripMenuItem reportesMenu = new ToolStripMenuItem("Reportes");
        private readonly ToolStripMenuItem reportFacturaMenuItem = new ToolStripMenuItem("Factura");
        private readonly ToolStripMenuItem reportEstadoCuentaMenuItem = new ToolStripMenuItem("Estado de cuenta");
        private readonly ToolStripMenuItem reportInventarioMenuItem = new ToolStripMenuItem("Inventario por categoría");
        private readonly ToolStripMenuItem reportMorososMenuItem = new ToolStripMenuItem("Clientes morosos");

        private readonly ToolStripMenuItem consultasMenu = new ToolStripMenuItem("Consultas");
        private readonly ToolStripMenuItem consultaProductosMargenMenuItem = new ToolStripMenuItem("Productos con margen y utilidad");
        private readonly ToolStripMenuItem consultaVentasClienteMenuItem = new ToolStripMenuItem("Ventas por cliente (rango)");
        private readonly ToolStripMenuItem consultaClientesSinComprasMenuItem = new ToolStripMenuItem("Clientes sin compras (N semanas)");
        private readonly ToolStripMenuItem consultaCreditosEstadoMenuItem = new ToolStripMenuItem("Créditos por estado");
        private readonly ToolStripMenuItem consultaStockBajoMenuItem = new ToolStripMenuItem("Stock bajo");

        private readonly ToolStripMenuItem utilidadesMenu = new ToolStripMenuItem("Utilidades");
        private readonly ToolStripMenuItem calcMenuItem = new ToolStripMenuItem("Calculadora");
        private readonly ToolStripMenuItem calendarioMenuItem = new ToolStripMenuItem("Calendario / Agenda");
        private readonly ToolStripMenuItem conversorMenuItem = new ToolStripMenuItem("Simulador crédito / Conversor");
        private readonly ToolStripMenuItem bitacoraMenuItem = new ToolStripMenuItem("Bitácora accesos");
        private readonly ToolStripMenuItem ayudaPdfMenuItem = new ToolStripMenuItem("Ayuda / About");

        private readonly ToolStripMenuItem ayudaMenu = new ToolStripMenuItem("Ayuda");
        private readonly ToolStripMenuItem cambiarClaveMenuItem = new ToolStripMenuItem("Cambiar contraseña");
        private readonly ToolStripMenuItem cerrarSesionMenuItem = new ToolStripMenuItem("Cerrar sesión");

        public MainForm()
        {
            // Asegurar inicialización del diseñador (aunque mínima) y configuración MDI
            InitializeComponent();
            IsMdiContainer = true;

            // Aplicar tema a este formulario una vez construido su menú y status
            this.Shown += (s, e) => Theme.Apply(this);
            this.MdiChildActivate += (s, e) =>
            {
                if (ActiveMdiChild != null) Theme.Apply(ActiveMdiChild);
            };

            Text = "ElectroApp - Principal";
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            // Construir menú
            clientesMenuItem.Click += ClientesMenuItem_Click;
            productosMenuItem.Click += ProductosMenuItem_Click;
            categoriasMenuItem.Click += (s, e) => OpenOrActivateMdiChild<CategoriasForm>();
            planesMenuItem.Click += (s, e) => OpenOrActivateMdiChild<PlanesCreditoForm>();
            usuariosMenuItem.Click += UsuariosMenuItem_Click;
            ventasMenuItem.Click += VentasMenuItem_Click;
            cuotasMenuItem.Click += CuotasMenuItem_Click;
            reportFacturaMenuItem.Click += ReportFacturaMenuItem_Click;
            reportEstadoCuentaMenuItem.Click += ReportEstadoCuentaMenuItem_Click;
            reportInventarioMenuItem.Click += (s, e) => OpenOrActivateMdiChild<InventarioCategoriaForm>();
            reportMorososMenuItem.Click += (s, e) => OpenOrActivateMdiChild<MorososForm>();

            // Eventos Consultas
            consultaProductosMargenMenuItem.Click += (s, e) => OpenOrActivateMdiChild<ProductosMargenForm>();
            consultaVentasClienteMenuItem.Click += (s, e) => OpenOrActivateMdiChild<VentasPorClienteForm>();
            consultaClientesSinComprasMenuItem.Click += (s, e) => OpenOrActivateMdiChild<ClientesSinComprasForm>();
            consultaCreditosEstadoMenuItem.Click += (s, e) => OpenOrActivateMdiChild<CreditosPorEstadoForm>();
            consultaStockBajoMenuItem.Click += (s, e) => OpenOrActivateMdiChild<StockBajoForm>();

            cascadaMenuItem.Click += (s, e) => LayoutMdi(MdiLayout.Cascade);
            mosaicoHMenuItem.Click += (s, e) => LayoutMdi(MdiLayout.TileHorizontal);
            mosaicoVMenuItem.Click += (s, e) => LayoutMdi(MdiLayout.TileVertical);
            organizarIconosMenuItem.Click += (s, e) => LayoutMdi(MdiLayout.ArrangeIcons);

            // Utilidades mini apps
            calcMenuItem.Click += (s, e) => OpenOrActivateMdiChild<CalculadoraForm>();
            calendarioMenuItem.Click += (s, e) => OpenOrActivateMdiChild<CalendarioAgendaForm>();
            conversorMenuItem.Click += (s, e) => OpenOrActivateMdiChild<SimuladorCreditoForm>();
            bitacoraMenuItem.Click += (s, e) => OpenOrActivateMdiChild<BitacoraAccesosForm>();
            ayudaPdfMenuItem.Click += (s, e) => OpenOrActivateMdiChild<AboutAyudaForm>();

            entidadesMenu.DropDownItems.AddRange(new ToolStripItem[] { clientesMenuItem, productosMenuItem, categoriasMenuItem, planesMenuItem, usuariosMenuItem });
            transaccionesMenu.DropDownItems.AddRange(new ToolStripItem[] { ventasMenuItem, cuotasMenuItem });
            ventanasMenu.DropDownItems.AddRange(new ToolStripItem[] { cascadaMenuItem, mosaicoHMenuItem, mosaicoVMenuItem, organizarIconosMenuItem });
            reportesMenu.DropDownItems.AddRange(new ToolStripItem[] { reportFacturaMenuItem, reportEstadoCuentaMenuItem, new ToolStripSeparator(), reportInventarioMenuItem, reportMorososMenuItem });
            consultasMenu.DropDownItems.AddRange(new ToolStripItem[] { consultaProductosMargenMenuItem, consultaVentasClienteMenuItem, consultaClientesSinComprasMenuItem, consultaCreditosEstadoMenuItem, consultaStockBajoMenuItem });
            utilidadesMenu.DropDownItems.AddRange(new ToolStripItem[] { calcMenuItem, calendarioMenuItem, conversorMenuItem, bitacoraMenuItem, ayudaPdfMenuItem });

            _menu.Items.AddRange(new ToolStripItem[] { entidadesMenu, transaccionesMenu, reportesMenu, consultasMenu, utilidadesMenu, ventanasMenu, ayudaMenu });
            _menu.Dock = DockStyle.Top;
            MainMenuStrip = _menu;
            Controls.Add(_menu);

            // Status strip
            _status.Items.Add(_lblUsuario);
            _status.Dock = DockStyle.Bottom;
            Controls.Add(_status);

            FormClosing += MainForm_FormClosing;

            // Agregar item de cambio de contraseña en Ayuda
            cambiarClaveMenuItem.Click += (s, e) => OpenOrActivateMdiChild<CambioClaveForm>();
            cerrarSesionMenuItem.Click += CerrarSesionMenuItem_Click;

            // Asegurar que el menú Ayuda tenga las opciones
            if (!ayudaMenu.DropDownItems.Contains(cambiarClaveMenuItem)) ayudaMenu.DropDownItems.Add(cambiarClaveMenuItem);
            if (!ayudaMenu.DropDownItems.Contains(cerrarSesionMenuItem)) ayudaMenu.DropDownItems.Add(new ToolStripSeparator());
            if (!ayudaMenu.DropDownItems.Contains(cerrarSesionMenuItem)) ayudaMenu.DropDownItems.Add(cerrarSesionMenuItem);
        }

        private void CerrarSesionMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Desea cerrar la sesión actual?", "Cerrar sesión", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                // Registrar salida en bitácora si corresponde
                if (_idBitacora > 0)
                {
                    var dao = new UsuarioDAO();
                    dao.RegistrarSalida(_idBitacora);
                }
            }
            catch { /* no bloquear por errores de bitácora */ }

            // Limpiar sesión y UI
            _idBitacora = 0;
            _usuario = null;
            UserSession.SetUser(null);
            Text = "ElectroApp - Principal";
            _lblUsuario.Text = string.Empty;

            // Cerrar ventanas MDI abiertas
            foreach (var child in MdiChildren)
            {
                try { child.Close(); } catch { }
            }

            // Pedir nuevo login; si cancela, cerrar app
            using (var login = new LoginForm())
            {
                var dr = login.ShowDialog(this);
                if (dr == DialogResult.OK)
                {
                    SetUsuario(login.UsuarioAutenticado, login.IdBitacoraActual);
                }
                else
                {
                    Close();
                }
            }
        }

        // Se llama después de login exitoso para establecer usuario y bitácora
        public void SetUsuario(Usuario usuario, long idBitacora)
        {
            _usuario = usuario ?? throw new ArgumentNullException(nameof(usuario));
            _idBitacora = idBitacora;
            Text = $"ElectroApp - Usuario: {_usuario.Login} - Rol: {_usuario.NombreRol}";
            _lblUsuario.Text = $"Usuario: {_usuario.Login} | Rol: {_usuario.NombreRol}";
            // Setear usuario en contexto global
            UserSession.SetUser(_usuario);
            AplicarPermisos();
        }

        private void AplicarPermisos()
        {
            if (_usuario == null)
            {
                // Deshabilitar todo por seguridad
                entidadesMenu.Enabled = transaccionesMenu.Enabled = reportesMenu.Enabled = consultasMenu.Enabled = utilidadesMenu.Enabled = false;
                return;
            }

            // Asumo: IdRol valores: 1 = Administrador, 2 = Paramétrico, 3 = Esporádico
            switch (_usuario.IdRol)
            {
                case 1: // Administrador
                    entidadesMenu.Enabled = true;
                    transaccionesMenu.Enabled = true;
                    reportesMenu.Enabled = true;
                    consultasMenu.Enabled = true;
                    utilidadesMenu.Enabled = true;
                    usuariosMenuItem.Enabled = true;
                    break;

                case 2: // Paramétrico (no usuarios ni bitácora)
                    entidadesMenu.Enabled = true;
                    transaccionesMenu.Enabled = true;
                    reportesMenu.Enabled = true;
                    consultasMenu.Enabled = true;
                    utilidadesMenu.Enabled = true;
                    usuariosMenuItem.Enabled = false;
                    break;

                case 3: // Esporádico (solo consultas/reportes)
                    entidadesMenu.Enabled = false;
                    transaccionesMenu.Enabled = false;
                    reportesMenu.Enabled = true;
                    consultasMenu.Enabled = true;
                    utilidadesMenu.Enabled = false;
                    usuariosMenuItem.Enabled = false;
                    break;

                default:
                    entidadesMenu.Enabled = transaccionesMenu.Enabled = reportesMenu.Enabled = consultasMenu.Enabled = utilidadesMenu.Enabled = false;
                    usuariosMenuItem.Enabled = false;
                    break;
            }
        }

        // Helper: abrir o activar una instancia única de un hijo MDI por tipo
        private void OpenOrActivateMdiChild<T>() where T : Form, new()
        {
            var existing = MdiChildren.FirstOrDefault(c => c is T);
            if (existing != null)
            {
                existing.Activate();
                if (existing.WindowState == FormWindowState.Minimized)
                    existing.WindowState = FormWindowState.Normal;
                return;
            }

            var f = new T { MdiParent = this };
            f.Show();
        }

        // Eventos de menú: abren formularios MDI
        private void ClientesMenuItem_Click(object sender, EventArgs e)
        {
            OpenOrActivateMdiChild<ClientesForm>();
        }

        private void ProductosMenuItem_Click(object sender, EventArgs e)
        {
            OpenOrActivateMdiChild<ProductosForm>();
        }

        private void UsuariosMenuItem_Click(object sender, EventArgs e)
        {
            OpenOrActivateMdiChild<UsuariosForm>();
        }

        private void VentasMenuItem_Click(object sender, EventArgs e)
        {
            // Reemplaza por el formulario de ventas que implementes
            OpenOrActivateMdiChild<VentasForm>();
        }

        private void CuotasMenuItem_Click(object sender, EventArgs e)
        {
            // Solicitar IdVenta y abrir CuotasForm como MDI
            string input = Interaction.InputBox("Ingrese IdVenta para ver/pagar cuotas:", "Cuotas", "");
            if (string.IsNullOrWhiteSpace(input)) return;
            if (!int.TryParse(input.Trim(), out var idVenta))
            {
                MessageBox.Show("IdVenta inválido.");
                return;
            }

            var f = new CuotasForm(idVenta) { MdiParent = this };
            f.Show();
        }

        private void ReportFacturaMenuItem_Click(object sender, EventArgs e)
        {
            // Permitir buscar el consecutivo si el usuario no lo sabe
            using (var buscar = new FacturaBuscarForm())
            {
                if (buscar.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(buscar.ConsecutivoSeleccionado))
                {
                    var f = new FacturaFormView(buscar.ConsecutivoSeleccionado) { MdiParent = this };
                    f.Show();
                    return;
                }
            }

            // Fallback: pedir manualmente
            string consecutivo = Interaction.InputBox("Ingrese el consecutivo de la factura:", "Reporte Factura", "");
            if (string.IsNullOrWhiteSpace(consecutivo)) return;

            var f2 = new FacturaFormView(consecutivo) { MdiParent = this };
            f2.Show();
        }

        private void ReportEstadoCuentaMenuItem_Click(object sender, EventArgs e)
        {
            OpenOrActivateMdiChild<EstadoCuentaForm>();
        }

        // Al cerrar la aplicación registrar salida en bitácora
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_idBitacora > 0)
                {
                    var dao = new UsuarioDAO();
                    dao.RegistrarSalida(_idBitacora);
                }
                else if (_usuario != null)
                {
                    var dao = new UsuarioDAO();
                    dao.RegistrarSalidaPorUsuario(_usuario.IdUsuario);
                }
            }
            catch
            {
                // No bloquear cierre por errores de bitácora
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //dejar vacio
        }

    }
}
