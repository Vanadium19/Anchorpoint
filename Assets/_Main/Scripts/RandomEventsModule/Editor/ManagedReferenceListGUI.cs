using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>
    /// Builds a <see cref="ReorderableList"/> over a <c>[SerializeReference]</c> array/list whose
    /// elements ARE the polymorphic reference (e.g. <see cref="RandomEventConditionSet.conditions"/>).
    /// Gives drag-to-reorder and a searchable "+" type picker instead of Unity's default array
    /// drawer, which only lets you resize the array and leaves every new slot null.
    /// </summary>
    public static class ManagedReferenceListGUI
    {
        /// <summary>Builds a reorderable, type-picking list drawer over a <c>[SerializeReference]</c> array.</summary>
        public static ReorderableList Create(SerializedProperty arrayProperty, Type baseType, string headerLabel) =>
            new(arrayProperty.serializedObject, arrayProperty, true, true, true, false)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, headerLabel, EditorStyles.boldLabel),
                elementHeightCallback = index => GetElementHeight(arrayProperty, index),
                drawElementCallback = (rect, index, isActive, isFocused) => DrawElement(rect, arrayProperty, baseType, index),
                onAddDropdownCallback = (buttonRect, _) =>
                    ManagedReferenceEditorGUI.ShowTypeDropdown(buttonRect, baseType, type => Add(arrayProperty, type)),
            };

        private static float GetElementHeight(SerializedProperty arrayProperty, int index)
        {
            var element = arrayProperty.GetArrayElementAtIndex(index);
            return ManagedReferenceEditorGUI.HeaderHeight + ManagedReferenceEditorGUI.GetBodyHeight(element) + RandomEventEditorStyles.ListBottomPadding;
        }

        private static void DrawElement(Rect rect, SerializedProperty arrayProperty, Type baseType, int index)
        {
            var element = arrayProperty.GetArrayElementAtIndex(index);

            var headerRect = new Rect(rect.x, rect.y, rect.width - RandomEventEditorStyles.ListRemoveButtonWidth - RandomEventEditorStyles.ListElementSpacing, ManagedReferenceEditorGUI.HeaderHeight);
            var removeRect = new Rect(headerRect.xMax + RandomEventEditorStyles.ListElementSpacing, rect.y, RandomEventEditorStyles.ListRemoveButtonWidth, ManagedReferenceEditorGUI.HeaderHeight);

            if (GUI.Button(headerRect, ManagedReferenceEditorGUI.GetLabel(element) + " ▾", EditorStyles.miniButton))
                ManagedReferenceEditorGUI.ShowTypeDropdown(headerRect, baseType, type => ManagedReferenceEditorGUI.SetReference(element, type));

            if (GUI.Button(removeRect, "✕", EditorStyles.miniButton))
            {
                RemoveAt(arrayProperty, index);
                GUIUtility.ExitGUI();
            }

            var bodyY = headerRect.yMax + RandomEventEditorStyles.ListElementSpacing;
            var bodyRect = new Rect(rect.x, bodyY, rect.width, rect.yMax - bodyY);

            EditorGUI.indentLevel++;
            ManagedReferenceEditorGUI.DrawBody(bodyRect, element);
            EditorGUI.indentLevel--;
        }

        private static void Add(SerializedProperty arrayProperty, Type type)
        {
            arrayProperty.serializedObject.Update();
            arrayProperty.arraySize++;
            arrayProperty.GetArrayElementAtIndex(arrayProperty.arraySize - 1).managedReferenceValue =
                type == null ? null : Activator.CreateInstance(type);
            arrayProperty.serializedObject.ApplyModifiedProperties();
        }

        private static void RemoveAt(SerializedProperty arrayProperty, int index)
        {
            arrayProperty.serializedObject.Update();
            arrayProperty.DeleteArrayElementAtIndex(index);
            arrayProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
