using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Custom inspector for <see cref="RandomEventScope"/>: scene pacing above the list of triggers.</summary>
    /// <remarks>Each trigger is drawn by <see cref="RandomEventTriggerDrawer"/> as a self-contained rule, so adding a second way to start events is adding a list element.</remarks>
    [CustomEditor(typeof(RandomEventScope))]
    public class RandomEventScopeEditor : Editor
    {
        private SerializedProperty _triggersProperty;
        private ReorderableList _triggersList;

        private void OnEnable()
        {
            _triggersProperty = ManagedReferenceEditorGUI.RequireProperty(serializedObject, "triggers");
            _triggersList = new ReorderableList(serializedObject, _triggersProperty, true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Triggers", EditorStyles.boldLabel),
                elementHeightCallback = GetElementHeight,
                drawElementCallback = DrawElement,
            };
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "minIntervalBetweenEventsSeconds"));

            EditorGUILayout.Space();
            var listRect = EditorGUILayout.GetControlRect(false, _triggersList.GetHeight());
            _triggersList.DoList(listRect);

            serializedObject.ApplyModifiedProperties();
        }

        private float GetElementHeight(int index) =>
            EditorGUI.GetPropertyHeight(_triggersProperty.GetArrayElementAtIndex(index), true)
            + RandomEventEditorStyles.CardInnerPadding * 2f
            + RandomEventEditorStyles.ElementBottomPadding;

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = _triggersProperty.GetArrayElementAtIndex(index);
            var cardRect = new Rect(rect.x, rect.y, rect.width, rect.height - RandomEventEditorStyles.ElementBottomPadding);
            GUI.Box(cardRect, GUIContent.none, EditorStyles.helpBox);

            var accentRect = new Rect(cardRect.x + 1f, cardRect.y + 1f, RandomEventEditorStyles.AccentWidth, cardRect.height - 2f);
            EditorGUI.DrawRect(accentRect, index % 2 == 0 ? RandomEventEditorStyles.OddGroupColor : RandomEventEditorStyles.EvenGroupColor);

            var contentX = cardRect.x + RandomEventEditorStyles.AccentWidth + RandomEventEditorStyles.CardInnerPadding;
            var contentRect = new Rect(
                contentX,
                cardRect.y + RandomEventEditorStyles.CardInnerPadding,
                cardRect.width - RandomEventEditorStyles.AccentWidth - RandomEventEditorStyles.CardInnerPadding * 2f,
                cardRect.height - RandomEventEditorStyles.CardInnerPadding * 2f);

            EditorGUI.PropertyField(contentRect, element, GUIContent.none, true);
        }
    }
}
