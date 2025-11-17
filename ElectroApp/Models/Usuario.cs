namespace ElectroApp.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Login { get; set; }
        public byte[] PassHash { get; set; }
        public byte[] Salt { get; set; }
        public byte IdRol { get; set; }
        public string NombreRol { get; set; }
        public bool Activo { get; set; }
    }
}
