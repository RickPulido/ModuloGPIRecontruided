using System.Data;
using ModuleGPI.Domain;

namespace ModuleGPI.Data
{
    public interface IDataAccess
    {
        DataTable GetModules(int? plant);                               // SP: ModGPI_Module_GetAll
        void UpsertModule(DataRow row);                                 // SP: ModGPI_Module_Upsert
        void DeleteModule(string buttonName);                           // SP: ModGPI_Module_Delete

        DataTable GetUsers();                                           // SELECT ModGPI_User
        void UpdateUsers(DataTable users);                             // UPDATE ModGPI_User

        OverridesStore GetOverrides();                                 // SP: ModGPI_Override_GetAll
        void ReplaceOverrides(string buttonName, DataTable overridesView); // tx: DELETE+INSERT


       
    }
    //public static class AppCache
    //{
    //    public static DataTable Modules { get; set; }
    //}

}