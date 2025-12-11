using System;
using System.Data;
using System.Windows.Forms;
using ModuleGPI.Domain;  

namespace ModuleGPI.Services
{
    public interface IModuleService
    {
        DataTable LoadModules(int? plant);

        // ✅ ACTUALIZADO: Solo requiere un panel (el segundo puede ser null)
        void PaintButtons(
            DataTable dtModules,
            FlowLayoutPanel panel,         // ✅ Panel principal
            FlowLayoutPanel secondPanel,   // ✅ Puede ser null
            ContextMenuStrip context,
            ToolTip tips,
            Func<string, ModuleDef, bool> canSee);

        // ✅ ACTUALIZADO: Solo requiere un panel
        void RefreshVisibility(
            FlowLayoutPanel panel,         // ✅ Panel principal
            FlowLayoutPanel secondPanel,   // ✅ Puede ser null
            Func<string, ModuleDef, bool> canSee);

        void LaunchModule(
            string buttonName,
            ModuleDef module,
            bool asAdmin,
            string[] allowedRoots,
            Action<string> setStatus);

        // ✅ ACTUALIZADO: Solo requiere un panel
        void WireButtons(
            FlowLayoutPanel panel,         // ✅ Panel principal
            FlowLayoutPanel secondPanel,   // ✅ Puede ser null
            EventHandler clickHandler);
    }
}