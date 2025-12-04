using System;
using System.Collections.Generic;
using System.Diagnostics;

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

                // ✅ CRÍTICO: Verificar nuevamente si la clave existe después de limpiar
                // CleanupDeadProcesses puede haber eliminado la clave si todos los procesos terminaron
                if (!_activeProcesses.ContainsKey(buttonName))
                    return false;

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

                Debug.WriteLine($"✅ Proceso registrado: {buttonName} (PID: {processId})");
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
                    {
                        deadProcesses.Add(pid);
                        Debug.WriteLine($"🗑️ Proceso terminado: {buttonName} (PID: {pid})");
                    }
                }
                catch (ArgumentException)
                {
                    // Proceso no existe
                    deadProcesses.Add(pid);
                    Debug.WriteLine($"❌ Proceso no existe: {buttonName} (PID: {pid})");
                }
                catch (Exception ex)
                {
                    // Cualquier otro error = proceso no accesible
                    deadProcesses.Add(pid);
                    Debug.WriteLine($"⚠️ Error accediendo proceso: {buttonName} (PID: {pid}) - {ex.Message}");
                }
            }

            // Eliminar procesos muertos de la lista
            foreach (int pid in deadProcesses)
            {
                _activeProcesses[buttonName].Remove(pid);
            }

            // Si no quedan procesos, eliminar la entrada del diccionario
            if (_activeProcesses[buttonName].Count == 0)
            {
                _activeProcesses.Remove(buttonName);
                Debug.WriteLine($"🧹 Limpiada entrada: {buttonName}");
            }
        }

        /// <summary>
        /// Limpia todos los procesos rastreados
        /// </summary>
        public static void ClearAll()
        {
            lock (_processLock)
            {
                int count = _activeProcesses.Count;
                _activeProcesses.Clear();
                Debug.WriteLine($"🧹 Limpiados {count} registros de procesos");
            }
        }

        /// <summary>
        /// Obtiene información de diagnóstico
        /// </summary>
        public static string GetDiagnostics()
        {
            lock (_processLock)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("=== PROCESS TRACKER DIAGNOSTICS ===");
                sb.AppendLine($"Módulos rastreados: {_activeProcesses.Count}");
                sb.AppendLine();

                foreach (var kvp in _activeProcesses)
                {
                    sb.AppendLine($"Módulo: {kvp.Key}");
                    sb.AppendLine($"  Procesos activos: {kvp.Value.Count}");

                    foreach (int pid in kvp.Value)
                    {
                        try
                        {
                            var p = Process.GetProcessById(pid);
                            sb.AppendLine($"    - PID {pid}: {p.ProcessName} (Running: {!p.HasExited})");
                        }
                        catch
                        {
                            sb.AppendLine($"    - PID {pid}: (No accesible)");
                        }
                    }
                }

                return sb.ToString();
            }
        }
    }
}