using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModuleGPI.Services
{
    /// <summary>
    /// Gestiona los módulos favoritos del usuario
    /// Guarda los favoritos en un archivo JSON en AppData
    /// </summary>
    public class FavoritesManager
    {
        private readonly string _favoritesPath;
        private HashSet<string> _favorites;
        private bool _isLoaded = false;

        public FavoritesManager()
        {
            try
            {
                // Crear directorio en AppData si no existe
                string appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ModuleGPI"
                );

                if (!Directory.Exists(appData))
                    Directory.CreateDirectory(appData);

                // Nombre del archivo basado en el usuario (si está disponible)
                string userName = !string.IsNullOrEmpty(Session.LogonName)
                    ? Session.LogonName
                    : Environment.UserName;

                // Sanitizar nombre de usuario para uso en nombre de archivo
                userName = SanitizeFileName(userName);

                _favoritesPath = Path.Combine(appData, $"favorites_{userName}.json");

                LoadFavorites();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inicializando FavoritesManager: {ex.Message}");
                _favorites = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Sanitiza un nombre para usarlo en un archivo
        /// </summary>
        private string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "default";

            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (char c in invalid)
            {
                name = name.Replace(c, '_');
            }
            return name;
        }

        /// <summary>
        /// Carga los favoritos desde el archivo
        /// </summary>
        private void LoadFavorites()
        {
            try
            {
                if (File.Exists(_favoritesPath))
                {
                    string json = File.ReadAllText(_favoritesPath);

                    // Parsear JSON manualmente (sin dependencias externas)
                    var list = ParseJsonArray(json);
                    _favorites = new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);

                    System.Diagnostics.Debug.WriteLine($"Favoritos cargados: {_favorites.Count}");
                }
                else
                {
                    _favorites = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    System.Diagnostics.Debug.WriteLine("Archivo de favoritos no existe, creando nuevo");
                }

                _isLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando favoritos: {ex.Message}");
                _favorites = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Guarda los favoritos al archivo
        /// </summary>
        private void SaveFavorites()
        {
            try
            {
                // Crear JSON manualmente
                string json = ToJsonArray(_favorites);
                File.WriteAllText(_favoritesPath, json);

                System.Diagnostics.Debug.WriteLine($"Favoritos guardados: {_favorites.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando favoritos: {ex.Message}");
            }
        }

        /// <summary>
        /// Parsea un array JSON simple
        /// </summary>
        private List<string> ParseJsonArray(string json)
        {
            var result = new List<string>();

            if (string.IsNullOrWhiteSpace(json))
                return result;

            // Remover corchetes
            json = json.Trim();
            if (json.StartsWith("[")) json = json.Substring(1);
            if (json.EndsWith("]")) json = json.Substring(0, json.Length - 1);

            // Separar elementos
            var items = json.Split(',');
            foreach (var item in items)
            {
                string cleaned = item.Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    result.Add(cleaned);
                }
            }

            return result;
        }

        /// <summary>
        /// Convierte a array JSON
        /// </summary>
        private string ToJsonArray(IEnumerable<string> items)
        {
            var quoted = items.Select(s => $"\"{s}\"");
            return "[\n  " + string.Join(",\n  ", quoted) + "\n]";
        }

        /// <summary>
        /// Verifica si un módulo es favorito
        /// </summary>
        public bool IsFavorite(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName))
                return false;

            return _favorites.Contains(buttonName);
        }

        /// <summary>
        /// Agrega un módulo a favoritos
        /// </summary>
        public void AddFavorite(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName))
                return;

            if (_favorites.Add(buttonName))
            {
                SaveFavorites();
                System.Diagnostics.Debug.WriteLine($"Agregado a favoritos: {buttonName}");
            }
        }

        /// <summary>
        /// Elimina un módulo de favoritos
        /// </summary>
        public void RemoveFavorite(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName))
                return;

            if (_favorites.Remove(buttonName))
            {
                SaveFavorites();
                System.Diagnostics.Debug.WriteLine($"Eliminado de favoritos: {buttonName}");
            }
        }

        /// <summary>
        /// Alterna el estado de favorito de un módulo
        /// </summary>
        public void ToggleFavorite(string buttonName)
        {
            if (string.IsNullOrEmpty(buttonName))
                return;

            if (IsFavorite(buttonName))
                RemoveFavorite(buttonName);
            else
                AddFavorite(buttonName);
        }

        /// <summary>
        /// Obtiene todos los favoritos
        /// </summary>
        public IEnumerable<string> GetFavorites()
        {
            return _favorites.ToList();
        }

        /// <summary>
        /// Obtiene la cantidad de favoritos
        /// </summary>
        public int Count => _favorites.Count;

        /// <summary>
        /// Limpia todos los favoritos
        /// </summary>
        public void ClearAll()
        {
            _favorites.Clear();
            SaveFavorites();
        }
    }
}
