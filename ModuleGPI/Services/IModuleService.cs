using System;
using System.Data;
using System.Windows.Forms;
using ModuleGPI.Domain;  // ModuleDef public

namespace ModuleGPI.Services
{
    public interface IModuleService
    {
        DataTable LoadModules(int? plant);
        void PaintButtons(DataTable dtModules, FlowLayoutPanel opPanel, FlowLayoutPanel consPanel,
                          ContextMenuStrip context, ToolTip tips, Func<string, ModuleDef, bool> canSee);
        void RefreshVisibility(FlowLayoutPanel opPanel, FlowLayoutPanel consPanel, Func<string, ModuleDef, bool> canSee);
        void LaunchModule(string buttonName, ModuleDef module, bool asAdmin,
                          string[] allowedRoots, Action<string> setStatus);
        void WireButtons(FlowLayoutPanel op, FlowLayoutPanel cons, EventHandler clickHandler);
    }
}