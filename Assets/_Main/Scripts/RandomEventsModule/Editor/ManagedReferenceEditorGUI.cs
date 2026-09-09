using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>
    /// Rect-based drawing shared by every editor in the module that renders a
    /// <c>[SerializeReference]</c> field as "type-picker button + the picked type's own fields":
    /// action steps, condition entries, trigger sources, chances and pickers. Keeps them in sync
    /// instead of duplicating the same rect math and dropdown wiring everywhere.
    /// </summary>
    public static class ManagedReferenceEditorGUI
    {
        /// <summary>Height of one type-picker button row.</summary>
        public static float HeaderHeight => EditorGUIUtility.singleLineHeight;

        /// <summary>Label for the reference's type-picker button.</summary>
        public static string GetLabel(SerializedProperty referenceProperty) =>
            referenceProperty.managedReferenceValue is IRandomEventAsset asset ? asset.EditorLabel : "<None>";

        /// <summary>Total height needed to draw the picked type's own fields.</summary>
        public static float GetBodyHeight(SerializedProperty referenceProperty)
        {
            if (referenceProperty.managedReferenceValue == null)
                return 0f;

            var height = 0f;
            var iterator = referenceProperty.Copy();
            var end = iterator.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                height += (iterator.propertyType == SerializedPropertyType.ManagedReference
                             ? GetReferenceFieldHeight(iterator)
                             : EditorGUI.GetPropertyHeight(iterator, true))
                          + EditorGUIUtility.standardVerticalSpacing;
                enterChildren = false;
            }

            return height;
        }

        /// <summary>Draws the picked type's own fields inside the given rect.</summary>
        public static void DrawBody(Rect rect, SerializedProperty referenceProperty)
        {
            if (referenceProperty.managedReferenceValue == null)
                return;

            var iterator = referenceProperty.Copy();
            var end = iterator.GetEndProperty();
            var enterChildren = true;
            var y = rect.y;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                if (iterator.propertyType == SerializedPropertyType.ManagedReference)
                {
                    var referenceProp = iterator.Copy();
                    var referenceHeight = GetReferenceFieldHeight(referenceProp);
                    DrawReferenceField(new Rect(rect.x, y, rect.width, referenceHeight), referenceProp, ResolveManagedReferenceFieldType(referenceProp), referenceProp.displayName);
                    y += referenceHeight + EditorGUIUtility.standardVerticalSpacing;
                }
                else
                {
                    var height = EditorGUI.GetPropertyHeight(iterator, true);
                    EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, height), iterator, true);
                    y += height + EditorGUIUtility.standardVerticalSpacing;
                }

                enterChildren = false;
            }
        }

        /// <summary>Total height of one labeled reference field, button row plus body.</summary>
        public static float GetReferenceFieldHeight(SerializedProperty referenceProperty)
        {
            var bodyHeight = GetBodyHeight(referenceProperty);

            return HeaderHeight + (bodyHeight > 0f ? EditorGUIUtility.standardVerticalSpacing + bodyHeight : 0f);
        }

        /// <summary>Draws a single <c>[SerializeReference]</c> field: a labeled type-picker button with the picked type's fields under it.</summary>
        public static void DrawReferenceField(Rect rect, SerializedProperty referenceProperty, Type baseType, string label)
        {
            var headerRect = new Rect(rect.x, rect.y, rect.width, HeaderHeight);
            var buttonRect = EditorGUI.PrefixLabel(headerRect, new GUIContent(label));

            if (GUI.Button(buttonRect, GetLabel(referenceProperty) + " ▾", EditorStyles.miniButton))
                ShowTypeDropdown(buttonRect, baseType, type => SetReference(referenceProperty, type));

            var bodyHeight = GetBodyHeight(referenceProperty);

            if (bodyHeight <= 0f)
                return;

            var bodyRect = new Rect(rect.x, headerRect.yMax + EditorGUIUtility.standardVerticalSpacing, rect.width, bodyHeight);

            EditorGUI.indentLevel++;
            DrawBody(bodyRect, referenceProperty);
            EditorGUI.indentLevel--;
        }

        /// <summary>Resolves the declared field type of a <c>[SerializeReference]</c> property so its type picker can be scoped.</summary>
        public static Type ResolveManagedReferenceFieldType(SerializedProperty referenceProperty)
        {
            var parts = referenceProperty.managedReferenceFieldTypename.Split(' ');

            return parts.Length == 2
                ? Type.GetType($"{parts[1]}, {parts[0]}") ?? typeof(object)
                : typeof(object);
        }

        /// <summary>Opens the searchable type picker for a <c>[SerializeReference]</c> field.</summary>
        public static void ShowTypeDropdown(Rect rect, Type baseType, Action<Type> onSelected) =>
            new ManagedReferenceTypeDropdown(new AdvancedDropdownState(), baseType, onSelected).Show(rect);

        /// <summary>Replaces a <c>[SerializeReference]</c> field's value with a new instance of the given type.</summary>
        public static void SetReference(SerializedProperty referenceProperty, Type type)
        {
            referenceProperty.serializedObject.Update();
            referenceProperty.managedReferenceValue = type == null ? null : Activator.CreateInstance(type);
            referenceProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>Finds a serialized property by name, or throws if a rename left the editor out of sync.</summary>
        public static SerializedProperty RequireProperty(SerializedObject serializedObjectRef, string name) =>
            serializedObjectRef.FindProperty(name)
                ?? throw new InvalidOperationException($"'{serializedObjectRef.targetObject.GetType().Name}' has no serialized field '{name}' — a rename left an editor out of sync.");

        /// <summary>Finds a relative serialized property by name, or throws if a rename left the editor out of sync.</summary>
        public static SerializedProperty RequireRelative(SerializedProperty property, string name) =>
            property.FindPropertyRelative(name)
                ?? throw new InvalidOperationException($"'{property.propertyPath}' has no relative field '{name}' — a rename left an editor out of sync.");
    }
}
