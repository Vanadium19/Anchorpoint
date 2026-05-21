using System;
using System.Collections.Generic;
using UnityEngine;

namespace NpcBaseModule
{
    [Serializable]
    public sealed class NpcDialogueLine
    {
        [SerializeField] private string text;
        [SerializeField] private int nextLineIndex = -1;
        [SerializeField] private List<NpcDialogueAnswer> answers = new();

        public string Text => text;
        public int NextLineIndex => nextLineIndex;
        public IReadOnlyList<NpcDialogueAnswer> Answers => answers;
        public bool HasAnswers => answers != null && answers.Count > 0;
    }
}