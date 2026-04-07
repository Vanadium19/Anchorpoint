using System;
using System.IO;
using Sirenix.Serialization;
using UnityEngine;

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

            var bytes = File.ReadAllBytes(filePath);
            return SerializationUtility.DeserializeValue<TData>(bytes, DataFormat.JSON);
        }

        public void Save<TData>(TData data, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var directoryName = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            var bytes = SerializationUtility.SerializeValue(data, DataFormat.JSON);
            File.WriteAllBytes(filePath, bytes);
        }
    }
}
