using System;

namespace ModuleGPI.Domain
{
    public class ModuleDef
    {
        public string Name { get; set; }
        public string ExePath { get; set; }
        public string WorkingDir { get; set; }
        public string Category { get; set; }
        public bool RequiresElevation { get; set; }
        public int RolesMinTypeAut { get; set; }
        public string Arguments { get; set; } = "";
    }
}