using System;

namespace ModuleGPI.Domain
{
    // Override de visibilidad/ejecución por usuario y módulo.
    // -1 = Denegar, 0 = Heredado (usa rol mínimo), 1 = Permitir
    
    public sealed class ModuleUserOverride
    {
        public string ButtonName { get; set; }   // nombre del botón (clave del módulo)
        public string EmpId { get; set; }   // empleado (USU_EmpID)
        public int Override { get; set; }   // -1, 0, 1

        public override string ToString()
            => $"{ButtonName} :: {EmpId} -> {Override}";
    }
}
