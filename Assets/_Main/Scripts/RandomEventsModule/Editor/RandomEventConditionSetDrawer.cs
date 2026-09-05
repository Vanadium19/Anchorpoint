using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Renders <see cref="RandomEventConditionSet"/> as a match-mode popup plus a drag-reorderable, type-picking condition list.</summary>
    /// <remarks>Applies automatically wherever a <see cref="RandomEventConditionSet"/> field is embedded, no per-asset drawing code needed.</remarks>
    [CustomPropertyDrawer(typeof(RandomEventConditionSet))]
    public class RandomEventConditionSetDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> _lists = new();

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var matchProperty = ManagedReferenceEditorGUI.RequireRelative(property, "conditionMatch");
            var list = GetOrCreateList(property);

            var matchRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(matchRect, matchProperty, new GUIContent("Match"));

            var listRect = new Rect(position.x, matchRect.yMax + EditorGUIUtility.standardVerticalSpacing, position.width, list.GetHeight());
            list.DoList(listRect);
        }

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var list = GetOrCreateList(property);
            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + list.GetHeight();
        }

        private ReorderableList GetOrCreateList(SerializedProperty property)
        {
            var conditionsProperty = ManagedReferenceEditorGUI.RequireRelative(property, "conditions");

            if (_lists.TryGetValue(property.propertyPath, out var list))
            {
                list.serializedProperty = conditionsProperty;
                return list;
            }

            list = ManagedReferenceListGUI.Create(conditionsProperty, typeof(IRandomEventConditionAsset), "Conditions");
            _lists[property.propertyPath] = list;

            return list;
        }
    }
}
