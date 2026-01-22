// Archivo nuevo: ModulesCache.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace ModuleGPI.Services
{
    /// <summary>
    /// Cache global de módulos precargados durante el login
    /// </summary>
    public static class ModulesCache
    {
        private static Task _loadTask;
        private static DataTable _allModules;
        private static Dictionary<string, object> _additionalData;
        private static Exception _loadError;
        private static readonly object _lock = new object();

        public static bool IsLoaded { get; private set; }
        public static bool IsLoading { get; private set; }

        /// <summary>
        /// Inicia la carga de todos los módulos en segundo plano
        /// </summary>
        public static void StartLoading()
        {
            lock (_lock)
            {
                if (_loadTask != null || IsLoading) return;

                IsLoading = true;
                _loadTask = Task.Run(() => LoadAllData());
            }
        }

        /// <summary>
        /// Carga TODOS los datos pesados
        /// </summary>
        private static void LoadAllData()
        {
            try
            {
                var dataAccess = new ModuleGPI.Data.SqlDataAccess();
                var moduleService = new ModuleGPI.Services.ModuleService(dataAccess);

                // ✅ CARGAR TODOS LOS MÓDULOS (sin filtro de planta)
                _allModules = moduleService.LoadModules(null);

                // ✅ Puedes precargar más cosas aquí si necesitas
                _additionalData = new Dictionary<string, object>();

                // Ejemplo: precargar overrides si también son pesados
                // _additionalData["Overrides"] = dataAccess.GetOverrides();

                IsLoaded = true;
                IsLoading = false;
            }
            catch (Exception ex)
            {
                _loadError = ex;
                IsLoaded = false;
                IsLoading = false;
                System.Diagnostics.Debug.WriteLine($"❌ Error precargando módulos: {ex.Message}");
            }
        }

        /// <summary>
        /// Espera a que termine la carga (async)
        /// </summary>
        public static async Task WaitForLoad()
        {
            if (_loadTask != null)
            {
                await _loadTask;
            }

            if (_loadError != null)
            {
                throw new Exception("Error al precargar módulos", _loadError);
            }
        }

        /// <summary>
        /// Obtiene la tabla de módulos precargada
        /// </summary>
        public static DataTable GetModules()
        {
            return _allModules?.Copy(); // Devuelve copia para evitar modificaciones
        }

        /// <summary>
        /// Verifica si los módulos están listos
        /// </summary>
        public static bool TryGetModules(out DataTable modules)
        {
            if (IsLoaded && _allModules != null)
            {
                modules = _allModules.Copy();
                return true;
            }

            modules = null;
            return false;
        }

        /// <summary>
        /// Limpia el cache (útil para logout/refresh)
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _allModules?.Dispose();
                _allModules = null;
                _additionalData?.Clear();
                _loadTask = null;
                _loadError = null;
                IsLoaded = false;
                IsLoading = false;
            }
        }

        /// <summary>
        /// Obtiene el progreso de carga (opcional para UI)
        /// </summary>
        public static string GetStatus()
        {
            if (IsLoaded) return "✓ Módulos cargados";
            if (IsLoading) return "⏳ Cargando módulos...";
            if (_loadError != null) return "❌ Error al cargar";
            return "⚪ Sin iniciar";
        }
    }
}