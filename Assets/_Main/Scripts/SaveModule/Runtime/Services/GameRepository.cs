using System;
using System.IO;
using Newtonsoft.Json;

namespace SaveModule
{
    public class GameRepository : IGameRepository
    {
        public TData Load<TData>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                return default;

            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<TData>(json);
        }

        public void Save<TData>(TData data, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var directoryName = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
