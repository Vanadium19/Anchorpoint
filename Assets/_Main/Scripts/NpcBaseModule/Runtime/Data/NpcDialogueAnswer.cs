using System;
using UnityEngine;

namespace NpcBaseModule
{
    [Serializable]
    public sealed class NpcDialogueAnswer
    {
        [SerializeField] private string text;
        [SerializeField] private int relationshipDelta;
        [SerializeField] private int nextLineIndex = -1;

        public string Text => text;
        public int RelationshipDelta => relationshipDelta;
        public int NextLineIndex => nextLineIndex;
    }
}