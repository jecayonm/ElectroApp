using System.Linq;

namespace ElectroApp.Security
{
    public static class UserSession
    {
        public static ElectroApp.Models.Usuario CurrentUser { get; private set; }

        public static void SetUser(ElectroApp.Models.Usuario user)
        {
            CurrentUser = user;
        }

        public static bool IsInRole(byte roleId)
        {
            return CurrentUser != null && CurrentUser.IdRol == roleId;
        }

        public static void DemandRoles(params byte[] allowedRoles)
        {
            if (CurrentUser == null) throw new System.UnauthorizedAccessException("No ha iniciado sesión.");
            if (allowedRoles == null || allowedRoles.Length == 0) return;
            if (!allowedRoles.Contains(CurrentUser.IdRol))
                throw new System.UnauthorizedAccessException("No tiene permisos para esta operación.");
        }
    }
}
