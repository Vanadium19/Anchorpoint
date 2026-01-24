using System;
using UnityEngine;

[Serializable]
public sealed class FriendlyNpcSettings
{
    [Min(0.5f)] public float interactionRadius = 2f;
    [Min(0.5f)] public float exitRadius = 4f;
    [Min(0.5f)] public float patrolRadius = 6f;

    [Min(0.1f)] public float moveSpeed = 2f;
    [Min(30f)] public float turnSpeed = 360f;

    [Min(0f)] public float minWait = 0.5f;
    [Min(0f)] public float maxWait = 2f;

    [TextArea] public string messageText;
}
