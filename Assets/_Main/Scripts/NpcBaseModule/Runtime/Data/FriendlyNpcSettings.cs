using System;
using UnityEngine;

[Serializable]
public sealed class FriendlyNpcSettings
{
    [Min(0.5f)] public float InteractionRadius = 2f;
    [Min(0.5f)] public float ExitRadius = 4f;
    [Min(0.5f)] public float PatrolRadius = 6f;

    [Min(0.1f)] public float MoveSpeed = 2f;
    [Min(30f)] public float TurnSpeed = 360f;

    [Min(0f)] public float MinWait = 0.5f;
    [Min(0f)] public float MaxWait = 2f;

    [TextArea] public string MessageText;
}