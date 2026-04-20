using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using FlashCard.Models;

namespace FlashCard.Services
{

    public class JsonDataService
    {
        private readonly string _filePath;

        public JsonDataService()
        {
            // Path to store the JSON file in app data
            _filePath = Path.Combine(
                FileSystem.AppDataDirectory,
                "decks.json"
            );
        }

        public async Task<List<Deck>> LoadDecksAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<Deck>();
                }

                string json = await File.ReadAllTextAsync(_filePath);
                List<Deck>? decks = JsonSerializer.Deserialize<List<Deck>>(json);
                return decks ?? new List<Deck>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading: {ex.Message}");
                return new List<Deck>();
            }
        }

        public async Task SaveDecksAsync(IEnumerable<Deck> decks)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(decks, options);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving: {ex.Message}");
            }
        }

        public string GetFilePath()
        {
            return _filePath;
        }
    }
}
