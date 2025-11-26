using System.Windows.Forms;
using ModuleGPI.Domain;  // ModuleDef es public

namespace ModuleGPI.Services
{
    public interface IRoleManager
    {
        string GetRoleName(int typeAut);
        void ApplyVisibility(TabControl tabMain, TabPage tabAdmin, TabPage tabConfig, int typeAut);
        bool CanSeeModule(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store);
        string DiagnoseModuleAccess(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store);


    }
}