using System;

namespace AudioModule
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter)]
    public sealed class UISoundKeyAttribute : Attribute
    {
    }
}