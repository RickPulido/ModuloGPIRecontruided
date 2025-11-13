using System.Data;
using ModuleGPI.Domain;

namespace ModuleGPI.Data
{
    public interface IDataAccess
    {
        DataTable GetModules(int? plant);
        void UpsertModule(DataRow row);
        void DeleteModule(string buttonName);

        DataTable GetUsers();
        void UpdateUsers(DataTable users);

        OverridesStore GetOverrides();
        void ReplaceOverrides(string buttonName, DataTable overridesView);
    }
}