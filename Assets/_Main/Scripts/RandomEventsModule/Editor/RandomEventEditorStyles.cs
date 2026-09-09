using UnityEditor;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Layout constants and <see cref="GUIStyle"/>s shared by the module's custom editors.</summary>
    public static class RandomEventEditorStyles
    {
        public const float AccentWidth = 3f;
        public const float ContentPadding = 14f;
        public const float CardInnerPadding = 4f;
        public const float NumberWidth = 28f;
        public const float ButtonWidth = 22f;
        public const float ButtonSpacing = 2f;
        public const float GroupGapHeight = 16f;
        public const float ElementBottomPadding = 8f;

        public const float ListRemoveButtonWidth = 20f;
        public const float ListElementSpacing = 4f;
        public const float ListBottomPadding = 6f;

        public static readonly Color EvenGroupColor = new(0.36f, 0.56f, 0.95f);
        public static readonly Color OddGroupColor = new(0.68f, 0.48f, 0.95f);

        private static GUIStyle _numberStyle;
        private static GUIStyle _arrowStyle;
        private static GUIStyle _groupFoldoutStyle;

        public static GUIStyle NumberStyle => _numberStyle ??= new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter,
        };

        public static GUIStyle ArrowStyle => _arrowStyle ??= new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
        };

        public static GUIStyle GroupFoldoutStyle => _groupFoldoutStyle ??= new GUIStyle(EditorStyles.foldout)
        {
            fontStyle = FontStyle.Bold,
        };
    }
}
