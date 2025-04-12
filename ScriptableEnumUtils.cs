using System;
using Tauntastic.ScriptableEnums;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tauntastic
{
    public static class ScriptableEnumUtils
    {
        public static bool TrySetIfNullOrWrongName<T>(this object caller, ref T se, string name) where T : ScriptableEnum
        {
            var wasSet = ScriptableEnum.TrySetIfNullOrWrongName(ref se, name);

            if (!wasSet) return false;

            if (caller is not Object obj) return true;

            Type type = typeof(T);
            string print = $"{obj.GetType().Name.Color(Color.green)}";
            if (obj is Component component)
                print += $" in scene {component.gameObject.scene.name.Color(Color.cyan)}";

            Debug.LogWarning($"{type.Name.Color(Color.red)} was set in: {print}");
            return true;
        }
    }
}