using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Renders a <see cref="RandomEventActionSequence"/> as a drag-reorderable, type-picking step list.</summary>
    /// <remarks>
    /// Applies wherever a sequence is embedded — an event definition or a nested sequence inside a
    /// composite action — so nested sequences are edited exactly like top-level ones. Consecutive
    /// steps marked parallel share one numbered, collapsible card.
    /// </remarks>
    [CustomPropertyDrawer(typeof(RandomEventActionSequence))]
    public class RandomEventActionSequenceDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, ReorderableList> _lists = new();
        private readonly HashSet<string> _collapsedGroups = new();

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var list = GetOrCreateList(property, label.text);

            list.DoList(new Rect(position.x, position.y, position.width, list.GetHeight()));
        }

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            GetOrCreateList(property, label.text).GetHeight();

        private ReorderableList GetOrCreateList(SerializedProperty property, string headerLabel)
        {
            var stepsProperty = ManagedReferenceEditorGUI.RequireRelative(property, "steps");

            if (_lists.TryGetValue(property.propertyPath, out var cachedList))
            {
                cachedList.serializedProperty = stepsProperty;
                return cachedList;
            }

            var list = new ReorderableList(stepsProperty.serializedObject, stepsProperty, true, true, true, false)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, headerLabel, EditorStyles.boldLabel),
                elementHeightCallback = index => GetElementHeight(stepsProperty, index),
                drawElementCallback = (rect, index, _, _) => DrawElement(rect, stepsProperty, index),
                onAddDropdownCallback = (buttonRect, _) =>
                    ManagedReferenceEditorGUI.ShowTypeDropdown(buttonRect, typeof(IRandomEventActionAsset), type => AddStep(stepsProperty, type)),
            };

            _lists[property.propertyPath] = list;

            return list;
        }

        private float GetElementHeight(SerializedProperty stepsProperty, int index)
        {
            var element = stepsProperty.GetArrayElementAtIndex(index);
            var actionProperty = ManagedReferenceEditorGUI.RequireRelative(element, "action");
            var isCollapsed = IsGroupCollapsed(stepsProperty, GetGroupNumber(stepsProperty, index));

            var bodyHeight = isCollapsed ? 0f : ManagedReferenceEditorGUI.GetBodyHeight(actionProperty);
            var contentHeight = ManagedReferenceEditorGUI.HeaderHeight + (bodyHeight > 0f ? RandomEventEditorStyles.ButtonSpacing + bodyHeight : 0f);
            var height = contentHeight + RandomEventEditorStyles.CardInnerPadding * 2f + RandomEventEditorStyles.ElementBottomPadding;

            if (index > 0 && IsGroupStart(stepsProperty, index))
                height += RandomEventEditorStyles.GroupGapHeight;

            return height;
        }

        private void DrawElement(Rect rect, SerializedProperty stepsProperty, int index)
        {
            var element = stepsProperty.GetArrayElementAtIndex(index);
            var runInParallelProperty = ManagedReferenceEditorGUI.RequireRelative(element, "runInParallel");
            var actionProperty = ManagedReferenceEditorGUI.RequireRelative(element, "action");
            var groupNumber = GetGroupNumber(stepsProperty, index);
            var isCollapsed = IsGroupCollapsed(stepsProperty, groupNumber);

            var y = rect.y;

            if (index > 0 && IsGroupStart(stepsProperty, index))
            {
                EditorGUI.LabelField(new Rect(rect.x, y, rect.width, RandomEventEditorStyles.GroupGapHeight), "↓", RandomEventEditorStyles.ArrowStyle);
                y += RandomEventEditorStyles.GroupGapHeight;
            }

            var cardBottom = rect.yMax - RandomEventEditorStyles.ElementBottomPadding;
            var cardRect = new Rect(rect.x, y, rect.width, cardBottom - y);
            GUI.Box(cardRect, GUIContent.none, EditorStyles.helpBox);

            var accentRect = new Rect(cardRect.x + 1f, cardRect.y + 1f, RandomEventEditorStyles.AccentWidth, cardRect.height - 2f);
            EditorGUI.DrawRect(accentRect, GetGroupColor(groupNumber));

            var contentX = cardRect.x + RandomEventEditorStyles.AccentWidth + RandomEventEditorStyles.ContentPadding;
            var contentWidth = cardRect.width - RandomEventEditorStyles.AccentWidth - RandomEventEditorStyles.ContentPadding * 2f;

            var headerRect = new Rect(contentX, cardRect.y + RandomEventEditorStyles.CardInnerPadding, contentWidth, ManagedReferenceEditorGUI.HeaderHeight);
            DrawStepHeader(headerRect, stepsProperty, index, groupNumber, isCollapsed, runInParallelProperty, actionProperty);

            if (isCollapsed || actionProperty.managedReferenceValue == null)
                return;

            var bodyY = headerRect.yMax + RandomEventEditorStyles.ButtonSpacing;
            var bodyRect = new Rect(contentX + RandomEventEditorStyles.NumberWidth, bodyY, contentWidth - RandomEventEditorStyles.NumberWidth, ManagedReferenceEditorGUI.GetBodyHeight(actionProperty));

            EditorGUI.indentLevel++;
            ManagedReferenceEditorGUI.DrawBody(bodyRect, actionProperty);
            EditorGUI.indentLevel--;
        }

        private void DrawStepHeader(
            Rect rect,
            SerializedProperty stepsProperty,
            int index,
            int groupNumber,
            bool isCollapsed,
            SerializedProperty runInParallelProperty,
            SerializedProperty actionProperty)
        {
            var numberRect = new Rect(rect.x, rect.y, RandomEventEditorStyles.NumberWidth, rect.height);

            if (IsGroupStart(stepsProperty, index))
            {
                var isExpanded = EditorGUI.Foldout(numberRect, !isCollapsed, groupNumber.ToString(), true, RandomEventEditorStyles.GroupFoldoutStyle);

                if (isExpanded == isCollapsed)
                    ToggleGroupCollapsed(stepsProperty, groupNumber);
            }
            else
            {
                EditorGUI.LabelField(numberRect, "‖", RandomEventEditorStyles.NumberStyle);
            }

            var removeRect = new Rect(rect.xMax - RandomEventEditorStyles.ButtonWidth, rect.y, RandomEventEditorStyles.ButtonWidth, rect.height);
            var parallelRect = new Rect(removeRect.x - RandomEventEditorStyles.ButtonWidth - RandomEventEditorStyles.ButtonSpacing, rect.y, RandomEventEditorStyles.ButtonWidth, rect.height);
            var typeRect = new Rect(numberRect.xMax + RandomEventEditorStyles.ButtonSpacing, rect.y, parallelRect.x - numberRect.xMax - RandomEventEditorStyles.ButtonSpacing * 2f, rect.height);

            if (GUI.Button(typeRect, ManagedReferenceEditorGUI.GetLabel(actionProperty) + " ▾", EditorStyles.miniButton))
                ManagedReferenceEditorGUI.ShowTypeDropdown(typeRect, typeof(IRandomEventActionAsset), type => ManagedReferenceEditorGUI.SetReference(actionProperty, type));

            using (new EditorGUI.DisabledScope(index == 0))
            {
                var wasParallel = runInParallelProperty.boolValue;
                var isParallel = GUI.Toggle(parallelRect, wasParallel, new GUIContent("‖", "Run in parallel with previous step"), EditorStyles.miniButton);

                if (isParallel != wasParallel)
                {
                    runInParallelProperty.boolValue = isParallel;
                    runInParallelProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            if (GUI.Button(removeRect, "✕", EditorStyles.miniButton))
            {
                RemoveStep(stepsProperty, index);
                GUIUtility.ExitGUI();
            }
        }

        private static void AddStep(SerializedProperty stepsProperty, Type type)
        {
            stepsProperty.serializedObject.Update();
            stepsProperty.arraySize++;

            var element = stepsProperty.GetArrayElementAtIndex(stepsProperty.arraySize - 1);
            ManagedReferenceEditorGUI.RequireRelative(element, "runInParallel").boolValue = false;
            ManagedReferenceEditorGUI.RequireRelative(element, "action").managedReferenceValue = type == null ? null : Activator.CreateInstance(type);

            stepsProperty.serializedObject.ApplyModifiedProperties();
        }

        private static void RemoveStep(SerializedProperty stepsProperty, int index)
        {
            stepsProperty.serializedObject.Update();
            stepsProperty.DeleteArrayElementAtIndex(index);
            stepsProperty.serializedObject.ApplyModifiedProperties();
        }

        private bool IsGroupCollapsed(SerializedProperty stepsProperty, int groupNumber) =>
            _collapsedGroups.Contains(GetGroupId(stepsProperty, groupNumber));

        private void ToggleGroupCollapsed(SerializedProperty stepsProperty, int groupNumber)
        {
            var groupId = GetGroupId(stepsProperty, groupNumber);

            if (!_collapsedGroups.Remove(groupId))
                _collapsedGroups.Add(groupId);
        }

        private static string GetGroupId(SerializedProperty stepsProperty, int groupNumber) => $"{stepsProperty.propertyPath}:{groupNumber}";

        private static bool IsGroupStart(SerializedProperty stepsProperty, int index) =>
            index == 0 || !ManagedReferenceEditorGUI.RequireRelative(stepsProperty.GetArrayElementAtIndex(index), "runInParallel").boolValue;

        private static int GetGroupNumber(SerializedProperty stepsProperty, int index)
        {
            var groupNumber = 1;

            for (var i = 1; i <= index; i++)
                if (IsGroupStart(stepsProperty, i))
                    groupNumber++;

            return groupNumber;
        }

        private static Color GetGroupColor(int groupNumber) =>
            groupNumber % 2 == 0 ? RandomEventEditorStyles.EvenGroupColor : RandomEventEditorStyles.OddGroupColor;
    }
}
