using System;
using UnityEngine;

namespace Tauntastic.ScriptableEnums
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ScriptableEnumsDisableAttribute : PropertyAttribute { }
}