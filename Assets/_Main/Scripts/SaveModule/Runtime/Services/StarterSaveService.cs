using System.IO;
using UnityEngine;

namespace SaveModule
{
    public class StarterSaveService
    {
        private const string TemplateDirectory = "Saves";
        private const string DefaultTemplateFileName = "GameSave.json";

        private readonly string _targetFilePath;

        public StarterSaveService(string targetFilePath)
        {
            _targetFilePath = targetFilePath;
        }

        public bool TryCreateStarterSave()
        {
            if (File.Exists(_targetFilePath))
                return false;

            var templatePath = Path.Combine(
                Application.streamingAssetsPath,
                TemplateDirectory,
                DefaultTemplateFileName);

            if (!File.Exists(templatePath))
                return false;

            var directoryName = Path.GetDirectoryName(_targetFilePath);

            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);

            File.Copy(templatePath, _targetFilePath);
            return true;
        }
    }
}