using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Custom inspector for <see cref="RandomEventDefinition"/>: a drag-reorderable, type-picking action sequence.</summary>
    /// <remarks>Steps are grouped visually by run of consecutive parallel steps; each group can be collapsed independently.</remarks>
    [CustomEditor(typeof(RandomEventDefinition))]
    public class RandomEventDefinitionEditor : Editor
    {
        private readonly HashSet<int> _collapsedGroups = new();

        private SerializedProperty _stepsProperty;
        private ReorderableList _stepsList;

        private void OnEnable()
        {
            _stepsProperty = ManagedReferenceEditorGUI.RequireProperty(serializedObject, "actionSteps");
            _stepsList = new ReorderableList(serializedObject, _stepsProperty, true, true, true, false)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Action Sequence", EditorStyles.boldLabel),
                elementHeightCallback = GetElementHeight,
                drawElementCallback = DrawElement,
                onAddDropdownCallback = (buttonRect, _) =>
                    ManagedReferenceEditorGUI.ShowTypeDropdown(buttonRect, typeof(IRandomEventActionAsset), AddStep),
            };
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawEventSettings();

            EditorGUILayout.Space();
            var stepsRect = EditorGUILayout.GetControlRect(false, _stepsList.GetHeight());
            _stepsList.DoList(stepsRect);

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "displayNameKey"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEventSettings()
        {
            EditorGUILayout.LabelField("Event Settings", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "weight"));

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "isEnabled"));
                EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "isExclusive"));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "cooldownSeconds"));
            }
        }

        private float GetElementHeight(int index)
        {
            var element = _stepsProperty.GetArrayElementAtIndex(index);
            var actionProp = ManagedReferenceEditorGUI.RequireRelative(element, "action");
            var collapsed = IsGroupCollapsed(GetGroupNumber(index));

            var bodyHeight = collapsed ? 0f : ManagedReferenceEditorGUI.GetBodyHeight(actionProp);
            var contentHeight = ManagedReferenceEditorGUI.HeaderHeight + (bodyHeight > 0f ? RandomEventEditorStyles.ButtonSpacing + bodyHeight : 0f);
            var height = contentHeight + RandomEventEditorStyles.CardInnerPadding * 2f + RandomEventEditorStyles.ElementBottomPadding;

            if (index > 0 && IsGroupStart(index))
                height += RandomEventEditorStyles.GroupGapHeight;

            return height;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _stepsProperty.GetArrayElementAtIndex(index);
            var runInParallelProp = ManagedReferenceEditorGUI.RequireRelative(element, "runInParallel");
            var actionProp = ManagedReferenceEditorGUI.RequireRelative(element, "action");
            var groupNumber = GetGroupNumber(index);
            var collapsed = IsGroupCollapsed(groupNumber);

            var y = rect.y;

            if (index > 0 && IsGroupStart(index))
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
            DrawStepHeader(headerRect, index, groupNumber, collapsed, runInParallelProp, actionProp);

            if (!collapsed && actionProp.managedReferenceValue != null)
            {
                var bodyY = headerRect.yMax + RandomEventEditorStyles.ButtonSpacing;
                var bodyRect = new Rect(contentX + RandomEventEditorStyles.NumberWidth, bodyY, contentWidth - RandomEventEditorStyles.NumberWidth, ManagedReferenceEditorGUI.GetBodyHeight(actionProp));
                EditorGUI.indentLevel++;
                ManagedReferenceEditorGUI.DrawBody(bodyRect, actionProp);
                EditorGUI.indentLevel--;
            }
        }

        private void DrawStepHeader(Rect rect, int index, int groupNumber, bool collapsed, SerializedProperty runInParallelProp, SerializedProperty actionProp)
        {
            var numberRect = new Rect(rect.x, rect.y, RandomEventEditorStyles.NumberWidth, rect.height);

            if (IsGroupStart(index))
            {
                var expanded = EditorGUI.Foldout(numberRect, !collapsed, groupNumber.ToString(), true, RandomEventEditorStyles.GroupFoldoutStyle);
                if (expanded == collapsed)
                    ToggleGroupCollapsed(groupNumber);
            }
            else
            {
                EditorGUI.LabelField(numberRect, "‖", RandomEventEditorStyles.NumberStyle);
            }

            var removeRect = new Rect(rect.xMax - RandomEventEditorStyles.ButtonWidth, rect.y, RandomEventEditorStyles.ButtonWidth, rect.height);
            var parallelRect = new Rect(removeRect.x - RandomEventEditorStyles.ButtonWidth - RandomEventEditorStyles.ButtonSpacing, rect.y, RandomEventEditorStyles.ButtonWidth, rect.height);
            var typeRect = new Rect(numberRect.xMax + RandomEventEditorStyles.ButtonSpacing, rect.y, parallelRect.x - numberRect.xMax - RandomEventEditorStyles.ButtonSpacing * 2f, rect.height);

            if (GUI.Button(typeRect, ManagedReferenceEditorGUI.GetLabel(actionProp) + " ▾", EditorStyles.miniButton))
                ManagedReferenceEditorGUI.ShowTypeDropdown(typeRect, typeof(IRandomEventActionAsset), type => ManagedReferenceEditorGUI.SetReference(actionProp, type));

            using (new EditorGUI.DisabledScope(index == 0))
            {
                var wasParallel = runInParallelProp.boolValue;
                var isParallel = GUI.Toggle(parallelRect, wasParallel, new GUIContent("‖", "Run in parallel with previous step"), EditorStyles.miniButton);

                if (isParallel != wasParallel)
                {
                    runInParallelProp.boolValue = isParallel;
                    runInParallelProp.serializedObject.ApplyModifiedProperties();
                }
            }

            if (GUI.Button(removeRect, "✕", EditorStyles.miniButton))
            {
                RemoveStep(index);
                GUIUtility.ExitGUI();
            }
        }

        private void AddStep(Type type)
        {
            serializedObject.Update();

            _stepsProperty.arraySize++;
            var element = _stepsProperty.GetArrayElementAtIndex(_stepsProperty.arraySize - 1);
            ManagedReferenceEditorGUI.RequireRelative(element, "runInParallel").boolValue = false;
            ManagedReferenceEditorGUI.RequireRelative(element, "action").managedReferenceValue = type == null ? null : Activator.CreateInstance(type);

            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveStep(int index)
        {
            serializedObject.Update();
            _stepsProperty.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }

        private bool IsGroupCollapsed(int groupNumber) => _collapsedGroups.Contains(groupNumber);

        private void ToggleGroupCollapsed(int groupNumber)
        {
            if (!_collapsedGroups.Remove(groupNumber))
                _collapsedGroups.Add(groupNumber);
        }

        private bool IsGroupStart(int index) =>
            index == 0 || !ManagedReferenceEditorGUI.RequireRelative(_stepsProperty.GetArrayElementAtIndex(index), "runInParallel").boolValue;

        private int GetGroupNumber(int index)
        {
            var groupNumber = 1;

            for (var i = 1; i <= index; i++)
                if (IsGroupStart(i))
                    groupNumber++;

            return groupNumber;
        }

        private static Color GetGroupColor(int groupNumber) =>
            groupNumber % 2 == 0 ? RandomEventEditorStyles.EvenGroupColor : RandomEventEditorStyles.OddGroupColor;
    }
}
