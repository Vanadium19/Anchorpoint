using System;
using System.IO;
using Newtonsoft.Json;

namespace SaveModule
{
    public class GameRepository : IGameRepository
    {
        private const string TempSuffix = ".tmp";

        public TData Load<TData>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            if (!File.Exists(filePath))
                return default;

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<TData>(json);
            }
            catch (JsonException)
            {
                return default;
            }
            catch (IOException)
            {
                return default;
            }
            catch (UnauthorizedAccessException)
            {
                return default;
            }
        }

        public void Save<TData>(TData data, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var directoryName = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            var tempFilePath = filePath + TempSuffix;

            File.WriteAllText(tempFilePath, json);

            if (File.Exists(filePath))
                File.Replace(tempFilePath, filePath, null);
            else
                File.Move(tempFilePath, filePath);
        }

        public void Delete(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var tempFilePath = filePath + TempSuffix;

            if (File.Exists(filePath))
                File.Delete(filePath);

            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
}