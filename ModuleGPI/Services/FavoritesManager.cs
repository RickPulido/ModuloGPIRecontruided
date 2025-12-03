using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ModuleGPI.Services
{
    /// <summary>
    /// Gestiona los módulos favoritos del usuario
    /// </summary>
    public class FavoritesManager
    {
        private readonly string _favoritesPath;
        private HashSet<string> _favorites;

        public FavoritesManager()
        {
            string appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ModuleGPI"
            );

            if (!Directory.Exists(appData))
                Directory.CreateDirectory(appData);

            _favoritesPath = Path.Combine(appData, $"favorites_{Session.LogonName}.json");
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            try
            {
                if (File.Exists(_favoritesPath))
                {
                    string json = File.ReadAllText(_favoritesPath);
                    _favorites = JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
                }
                else
                {
                    _favorites = new HashSet<string>();
                }
            }
            catch
            {
                _favorites = new HashSet<string>();
            }
        }

        private void SaveFavorites()
        {
            try
            {
                string json = JsonSerializer.Serialize(_favorites, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_favoritesPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando favoritos: {ex.Message}");
            }
        }

        public bool IsFavorite(string buttonName)
        {
            return _favorites.Contains(buttonName);
        }

        public void AddFavorite(string buttonName)
        {
            if (_favorites.Add(buttonName))
                SaveFavorites();
        }

        public void RemoveFavorite(string buttonName)
        {
            if (_favorites.Remove(buttonName))
                SaveFavorites();
        }

        public void ToggleFavorite(string buttonName)
        {
            if (IsFavorite(buttonName))
                RemoveFavorite(buttonName);
            else
                AddFavorite(buttonName);
        }

        public IEnumerable<string> GetFavorites()
        {
            return _favorites.ToList();
        }
    }
}