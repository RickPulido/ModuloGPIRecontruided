using System;

namespace ModuleGPI
{
    /// <summary>
    /// Información de la sesión actual del usuario
    /// </summary>
    public static class Session
    {
        public static string LogonName { get; set; }
        public static string EmpId { get; set; }
        public static int TypeAut { get; set; } = 1;  // 1=Viewer, 2=Operator, 3=Supervisor, 4=AdminDept, 5=SysAdmin
        public static int Sucursal { get; set; }      // Planta principal: 1=MTY, 2=QRO, 3=TIJ
        public static int Status { get; set; } = 1;

        // ✅ NUEVO: Acceso multi-planta
        public static bool MTY_Access { get; set; }
        public static bool QRO_Access { get; set; }
        public static bool TIJ_Access { get; set; }

        /// <summary>
        /// Verifica si el usuario tiene acceso a una planta específica
        /// </summary>
        /// <param name="plant">1=MTY, 2=QRO, 3=TIJ, 0=Todas las plantas</param>
        /// <returns>True si tiene acceso, False si no</returns>
        public static bool HasAccessToPlant(int plant)
        {
            // Plant = 0 significa "disponible para todas las plantas"
            if (plant == 0)
                return true;

            // Si es su planta principal, siempre tiene acceso
            if (plant == Sucursal)
                return true;

            // Verificar acceso multi-planta específico
            switch (plant)
            {
                case 1: return MTY_Access;
                case 2: return QRO_Access;
                case 3: return TIJ_Access;
                default: return false;
            }
        }

        /// <summary>
        /// Obtiene el nombre legible de la planta
        /// </summary>
        public static string GetPlantName(int plant)
        {
            switch (plant)
            {
                case 0: return "TODAS";
                case 1: return "MTY";
                case 2: return "QRO";
                case 3: return "TIJ";
                default: return "DESCONOCIDA";
            }
        }

        /// <summary>
        /// Limpia la sesión actual
        /// </summary>
        public static void Clear()
        {
            LogonName = null;
            EmpId = null;
            TypeAut = 1;
            Sucursal = 0;
            Status = 1;
            MTY_Access = false;
            QRO_Access = false;
            TIJ_Access = false;
        }
    }
}