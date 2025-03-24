using System;
using UnityEngine;

namespace Tauntastic
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ScriptableEnumAttribute : PropertyAttribute { }
}