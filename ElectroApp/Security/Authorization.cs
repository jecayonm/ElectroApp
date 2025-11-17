using System;
using ElectroApp.Security;

namespace ElectroApp.Security
{
    public static class Authorization
    {
        // Roles: 1=Admin, 2=Paramétrico, 3=Esporádico
        public const byte Admin = 1;
        public const byte Parametrico = 2;
        public const byte Esporadico = 3;

        // Lanza UnauthorizedAccessException si no cumple
        public static void DemandAdmin()
        {
            UserSession.DemandRoles(Admin);
        }
        public static void DemandOperador()
        {
            UserSession.DemandRoles(Admin, Parametrico);
        }
        public static void DemandConsulta()
        {
            UserSession.DemandRoles(Admin, Parametrico, Esporadico);
        }
    }
}
