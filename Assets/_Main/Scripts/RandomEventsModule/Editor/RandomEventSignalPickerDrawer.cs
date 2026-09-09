using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Renders a <see cref="RandomEventSignalPickerAttribute"/> field as a dropdown of every <see cref="RandomEventSignalKeyAttribute"/> constant.</summary>
    /// <remarks>Discovery is by attribute via <see cref="TypeCache"/>, so the drawer never hardcodes a domain's signal names.</remarks>
    [CustomPropertyDrawer(typeof(RandomEventSignalPickerAttribute))]
    public class RandomEventSignalPickerDrawer : PropertyDrawer
    {
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var keys = GetKeys();
            var labels = keys.Select(key => new GUIContent($"{key.Owner}/{key.Name}")).Prepend(new GUIContent("<None>")).ToArray();
            var values = keys.Select(key => key.Value).Prepend(string.Empty).ToArray();
            var currentIndex = Array.IndexOf(values, property.stringValue ?? string.Empty);

            EditorGUI.BeginProperty(position, label, property);

            if (currentIndex < 0)
            {
                var missingLabel = new GUIContent($"{label.text} (не найден: \"{property.stringValue}\")");
                EditorGUI.PropertyField(position, property, missingLabel);
            }
            else
            {
                var selected = EditorGUI.Popup(position, label, currentIndex, labels);

                if (selected != currentIndex)
                    property.stringValue = values[selected];
            }

            EditorGUI.EndProperty();
        }

        private static SignalKeyInfo[] GetKeys() =>
            TypeCache.GetFieldsWithAttribute<RandomEventSignalKeyAttribute>()
                .Where(field => field.IsLiteral && field.FieldType == typeof(string))
                .Select(field => new SignalKeyInfo(field.DeclaringType?.Name ?? string.Empty, field.Name, (string)field.GetRawConstantValue()))
                .OrderBy(key => key.Owner)
                .ThenBy(key => key.Name)
                .ToArray();

        private readonly struct SignalKeyInfo
        {
            public SignalKeyInfo(string owner, string name, string value)
            {
                Owner = owner;
                Name = name;
                Value = value;
            }

            public string Owner { get; }
            public string Name { get; }
            public string Value { get; }
        }
    }
}
