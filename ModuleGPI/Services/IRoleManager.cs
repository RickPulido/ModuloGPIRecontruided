using ModuleGPI.Domain;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ModuleGPI.Services
{
    public interface IRoleManager
    {
        string GetRoleName(int typeAut);
        void ApplyVisibility(TabControl tabMain, TabPage tabAdmin, TabPage tabConfig, int typeAut, string empId, OverridesStore store, IEnumerable<ModuleDef> modules);
        bool CanSeeModule(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store);
        string DiagnoseModuleAccess(string buttonName, ModuleDef module, int userRole, string empId, OverridesStore store);
        bool HasAccessToAnyTestModule(int userRole, string empId, OverridesStore store, IEnumerable<ModuleDef> modules);


    }
}