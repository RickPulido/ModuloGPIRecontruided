namespace ModuleGPI.Domain
{
   
    public sealed class ModuleDef
    {
        public string ButtonName { get; set; }
        public string Name { get; set; }
        public string ExePath { get; set; }
        public string Arguments { get; set; }
        public string WorkingDir { get; set; }
        public string Category { get; set; }
        public bool RequiresElevation { get; set; }
        public int RolesMinTypeAut { get; set; }
        public int Plant { get; set; }

        public string IconPath { get; set; }

        public ModuleDef()
        {
            ButtonName = string.Empty;
            Name = string.Empty;
            ExePath = string.Empty;
            Arguments = string.Empty;
            WorkingDir = string.Empty;
            IconPath = string.Empty;
            Category = "Operación";
            RequiresElevation = false;
            RolesMinTypeAut = 1;
            Plant = 1;
        }
    }
}