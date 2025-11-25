using ModuleGPI.Domain;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ModuleGPI.Data
{
    public sealed class SqlDataAccess : IDataAccess
    {
        private SqlDataAdapter _daUsers; // Mantener adapter para usuarios

        private string GetConnString()
        {
            var cs = ConfigurationManager.ConnectionStrings["DBConnectionString"];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new InvalidOperationException("Falta DBConnectionString en App.config.");
            return cs.ConnectionString;
        }

        public DataTable GetModules(int? plant)
        {
            var dt = new DataTable();
            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand("dbo.ModGPI_Module_GetAll", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Plant", SqlDbType.Int).Value = (object)plant ?? DBNull.Value;
                cn.Open();
                using (var rd = cmd.ExecuteReader())
                    dt.Load(rd);
            }
            return dt;
        }

        public void UpsertModule(DataRow r)
        {
            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand("dbo.ModGPI_Module_Upsert", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ButtonName", SqlDbType.NVarChar, 80).Value = r["ButtonName"];
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = r["Name"];
                cmd.Parameters.Add("@ExePath", SqlDbType.NVarChar, 500).Value = r["ExePath"];
                cmd.Parameters.Add("@WorkingDir", SqlDbType.NVarChar, 500).Value = r["WorkingDir"] ?? DBNull.Value;

                // ✅ SOLO IconPath, sin Arguments
                cmd.Parameters.Add("@IconPath", SqlDbType.NVarChar, 500).Value =
                    r.Table.Columns.Contains("IconPath") && r["IconPath"] != DBNull.Value && !string.IsNullOrEmpty(r["IconPath"].ToString())
                    ? r["IconPath"]
                    : (object)DBNull.Value;

                cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 50).Value = r["Category"];
                cmd.Parameters.Add("@RequiresElevation", SqlDbType.Bit).Value = r["RequiresElevation"];
                cmd.Parameters.Add("@RolesMinTypeAut", SqlDbType.Int).Value = r["RolesMinTypeAut"];
                cmd.Parameters.Add("@Plant", SqlDbType.Int).Value = r["Plant"];
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteModule(string buttonName)
        {
            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand("dbo.ModGPI_Module_Delete", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ButtonName", SqlDbType.NVarChar, 80).Value = buttonName;
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetUsers()
        {
            var dt = new DataTable();

            // ✅ CAST explícito a BIT para asegurar tipo correcto
            string sql = @"
        SELECT 
            U.USU_EmpID, 
            U.USU_UserLog, 
            U.USU_TypeAut, 
            U.USU_Status, 
            U.USU_UserPLant,
            CAST(ISNULL(U.MTY_Access, 0) AS BIT) AS MTY_Access,
            CAST(ISNULL(U.QRO_Access, 0) AS BIT) AS QRO_Access,
            CAST(ISNULL(U.TIJ_Access, 0) AS BIT) AS TIJ_Access
        FROM dbo.ModGPI_User U 
        ORDER BY U.USU_EmpID;";

            using (var cn = new SqlConnection(GetConnString()))
            {
                _daUsers = new SqlDataAdapter(sql, cn);
                _daUsers.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                _daUsers.Fill(dt);
            }

            // ✅ Verificar tipos de columnas
            Debug.WriteLine("=== TIPOS DE COLUMNAS ===");
            Debug.WriteLine($"MTY_Access: {dt.Columns["MTY_Access"].DataType}");
            Debug.WriteLine($"QRO_Access: {dt.Columns["QRO_Access"].DataType}");
            Debug.WriteLine($"TIJ_Access: {dt.Columns["TIJ_Access"].DataType}");

            return dt;
        }

        public void UpdateUsers(DataTable users)
        {
            if (_daUsers == null)
            {
                string selectSql = @"
            SELECT 
                USU_EmpID, USU_UserLog, USU_TypeAut, USU_Status, USU_UserPLant,
                CAST(ISNULL(MTY_Access, 0) AS BIT) AS MTY_Access,
                CAST(ISNULL(QRO_Access, 0) AS BIT) AS QRO_Access,
                CAST(ISNULL(TIJ_Access, 0) AS BIT) AS TIJ_Access
            FROM dbo.ModGPI_User";

                _daUsers = new SqlDataAdapter(selectSql, GetConnString());
            }

            string updateSql = @"
        UPDATE dbo.ModGPI_User
        SET 
            USU_TypeAut = @USU_TypeAut, 
            USU_Status = @USU_Status, 
            USU_UserPLant = @USU_UserPLant,
            MTY_Access = @MTY_Access,
            QRO_Access = @QRO_Access,
            TIJ_Access = @TIJ_Access
        WHERE USU_EmpID = @USU_EmpID;";

            using (var cn = new SqlConnection(GetConnString()))
            {
                _daUsers.UpdateCommand = new SqlCommand(updateSql, cn);

                _daUsers.UpdateCommand.Parameters.Add("@USU_TypeAut", SqlDbType.Int).SourceColumn = "USU_TypeAut";
                _daUsers.UpdateCommand.Parameters.Add("@USU_Status", SqlDbType.Int).SourceColumn = "USU_Status";
                _daUsers.UpdateCommand.Parameters.Add("@USU_UserPLant", SqlDbType.Int).SourceColumn = "USU_UserPLant";

                // ✅ Parámetros BIT para checkboxes
                _daUsers.UpdateCommand.Parameters.Add("@MTY_Access", SqlDbType.Bit).SourceColumn = "MTY_Access";
                _daUsers.UpdateCommand.Parameters.Add("@QRO_Access", SqlDbType.Bit).SourceColumn = "QRO_Access";
                _daUsers.UpdateCommand.Parameters.Add("@TIJ_Access", SqlDbType.Bit).SourceColumn = "TIJ_Access";

                // Primary key
                var pk = _daUsers.UpdateCommand.Parameters.Add("@USU_EmpID", SqlDbType.NVarChar, 10);
                pk.SourceColumn = "USU_EmpID";
                pk.SourceVersion = DataRowVersion.Original;

                int rowsAffected = _daUsers.Update(users);

                Debug.WriteLine($"✅ Usuarios actualizados: {rowsAffected}");
            }
        }

        public OverridesStore GetOverrides()
        {
            var store = new OverridesStore();

            using (var cn = new SqlConnection(GetConnString()))
            using (var cmd = new SqlCommand("dbo.ModGPI_Override_GetAll", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cn.Open();

                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        store.Items.Add(new ModuleUserOverride
                        {
                            ButtonName = rd["ButtonName"].ToString(),
                            EmpId = rd["EmpId"].ToString(),
                            Override = Convert.ToInt32(rd["Override"])
                        });
                    }
                }
            }

            return store;
        }

        public void ReplaceOverrides(string buttonName, DataTable overridesView)
        {
            using (var cn = new SqlConnection(GetConnString()))
            {
                cn.Open();
                using (var tx = cn.BeginTransaction())
                {
                    try
                    {
                        // Primero eliminar todos los overrides existentes para este botón
                        using (var del = new SqlCommand(
                            "DELETE FROM dbo.ModGPI_UserModuleOverride WHERE ButtonName=@B", cn, tx))
                        {
                            del.Parameters.AddWithValue("@B", buttonName);
                            del.ExecuteNonQuery();
                        }

                        // Luego insertar los nuevos (solo los que no son heredados)
                        using (var ins = new SqlCommand(
                            "INSERT INTO dbo.ModGPI_UserModuleOverride (ButtonName, EmpId, Override) VALUES (@B,@E,@O)", cn, tx))
                        {
                            ins.Parameters.Add("@B", SqlDbType.NVarChar, 80);
                            ins.Parameters.Add("@E", SqlDbType.NVarChar, 10);
                            ins.Parameters.Add("@O", SqlDbType.Int);

                            foreach (DataRow r in overridesView.Rows)
                            {
                                int ov = Convert.ToInt32(r["Override"]);

                                // Solo guardar overrides explícitos (no heredados)
                                if (ov == 0) continue;

                                ins.Parameters["@B"].Value = buttonName;
                                ins.Parameters["@E"].Value = r["EmpId"];
                                ins.Parameters["@O"].Value = ov;
                                ins.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}