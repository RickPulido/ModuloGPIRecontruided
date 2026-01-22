// Agregar este archivo nuevo: PreloadManager.cs
using ModuleGPI.Data;
using ModuleGPI.Services;
using System;
using System.Data;
using System.Threading.Tasks;

namespace ModuleGPI.Services
{
    public static class PreloadManager
    {
        private static Task _preloadTask;
        private static DataTable _cachedModules;
        private static Exception _preloadError;

        public static bool IsReady { get; private set; }

        /// <summary>
        /// Inicia la precarga de módulos en segundo plano
        /// </summary>
        public static void StartPreload()
        {
            if (_preloadTask != null) return; // Ya se inició

            _preloadTask = Task.Run(() =>
            {
                try
                {
                    var dataAccess = new SqlDataAccess();
                    var moduleService = new ModuleService(dataAccess);

                    // Cargar módulos (el método pesado)
                    _cachedModules = moduleService.LoadModules(null);

                    IsReady = true;
                }
                catch (Exception ex)
                {
                    _preloadError = ex;
                    IsReady = false;
                }
            });
        }

        /// <summary>
        /// Espera a que termine la precarga
        /// </summary>
        public static async Task WaitForPreload()
        {
            if (_preloadTask != null)
            {
                await _preloadTask;
            }

            if (_preloadError != null)
            {
                throw _preloadError;
            }
        }

        /// <summary>
        /// Obtiene los módulos precargados
        /// </summary>
        public static DataTable GetCachedModules()
        {
            return _cachedModules;
        }

        /// <summary>
        /// Invalida la cache para forzar recarga
        /// </summary>
        public static void InvalidateCache()
        {
            _cachedModules = null;
            _preloadTask = null;
            IsReady = false;
            _preloadError = null;
        }
    }
}