using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ModuleGPI.Services
{
    /// <summary>
    /// Rastrea procesos abiertos para evitar duplicados
    /// </summary>
    public static class ProcessTracker
    {
        private static readonly object _processLock = new object();
        private static readonly Dictionary<string, List<int>> _activeProcesses = new Dictionary<string, List<int>>();

        /// <summary>
        /// Verifica si el módulo ya está ejecutándose
        /// </summary>
        public static bool IsModuleRunning(string buttonName)
        {
            lock (_processLock)
            {
                if (!_activeProcesses.ContainsKey(buttonName))
                    return false;

                // Limpiar procesos que ya no existen
                CleanupDeadProcesses(buttonName);

                return _activeProcesses[buttonName].Count > 0;
            }
        }

        /// <summary>
        /// Registra un proceso como activo
        /// </summary>
        public static void RegisterProcess(string buttonName, int processId)
        {
            lock (_processLock)
            {
                if (!_activeProcesses.ContainsKey(buttonName))
                    _activeProcesses[buttonName] = new List<int>();

                _activeProcesses[buttonName].Add(processId);
            }
        }

        /// <summary>
        /// Limpia procesos que ya terminaron
        /// </summary>
        private static void CleanupDeadProcesses(string buttonName)
        {
            if (!_activeProcesses.ContainsKey(buttonName))
                return;

            var deadProcesses = new List<int>();

            foreach (int pid in _activeProcesses[buttonName])
            {
                try
                {
                    var process = Process.GetProcessById(pid);
                    if (process.HasExited)
                        deadProcesses.Add(pid);
                }
                catch
                {
                    // Proceso no existe
                    deadProcesses.Add(pid);
                }
            }

            foreach (int pid in deadProcesses)
            {
                _activeProcesses[buttonName].Remove(pid);
            }

            if (_activeProcesses[buttonName].Count == 0)
                _activeProcesses.Remove(buttonName);
        }

        /// <summary>
        /// Limpia todos los procesos rastreados
        /// </summary>
        public static void ClearAll()
        {
            lock (_processLock)
            {
                _activeProcesses.Clear();
            }
        }
    }
}