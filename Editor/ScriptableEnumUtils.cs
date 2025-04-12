using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Tauntastic.ScriptableEnums.Editor
{
    public static class ScriptableEnumUtils
    {
        private const int _PREDEFINED_COUNT = 25;

        public static VisualElement GetCorrespondingScriptableEnumElement(SerializedProperty property, FieldInfo fieldInfo, int countThreshold = _PREDEFINED_COUNT)
        {
            if (property.isArray)
            {
                return new PropertyField(property);
            }
            
            if (fieldInfo.FieldType.IsExistingScriptableObjectCountAbove(countThreshold))
            {
                return new PropertyField(property);
            }

            return new ScriptableEnumField(property, fieldInfo);
        }
        
        public static bool IsExistingScriptableObjectCountAbove(this Type type, int threshold = _PREDEFINED_COUNT)
        {
            var assets = GetAssets(type);
            return assets.Count > threshold;
        }
        
        public static List<ScriptableObject> GetAssets(Type type)
        {
            var assets =
                AssetDatabase
                    .FindAssets($"t:{type.FullName}")
                    .Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath((string)guid)))
                    .Where(obj => obj != null)
                    .ToList();
            
            return assets;
        }
        
        public static bool TrySetIfNullOrWrongName<T>(this object caller, ref T se, string name)
            where T : ScriptableEnum
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