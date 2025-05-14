using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tauntastic
{
    using ScriptableEnums;

    [Icon(_PATH_PREFIX + "/com.tauntastic.scriptableenums/Images/d_ScriptableEnum Icon.png")]
    abstract public class ScriptableEnum : ScriptableObject
    {
        private const string _PATH_PREFIX =
#if TAUNTASTIC_ASSETS_PACKAGE
            "Assets/Tauntastic";
#else
            "Packages";
#endif

        [SerializeField]
        [ScriptableEnumsDisable]
        private string _displayText;

        public string DisplayText
        {
            get => _displayText;
            protected set => _displayText = value;
        }


        private void Awake()
        {
            DisplayText = name;
        }

        protected virtual void OnValidate()
        {
            DisplayText = name;
        }

        #region STATIC

        private static readonly Dictionary<Type, ScriptableEnum[]> _allOptionsCache = new();

        public static implicit operator ScriptableEnum(string textIdentifier)
        {
            var allScriptableEnums = Resources.LoadAll<ScriptableEnum>("");
            var matchingEnums = allScriptableEnums.Where(x => x.DisplayText.Contains(textIdentifier)).ToArray();

            return matchingEnums.Length switch
            {
                0 => throw new Exception($"No scriptable enum found for {textIdentifier}"),
                > 1 => throw new Exception(
                    $"Multiple scriptable enums found for {textIdentifier}, please specify a more specific name"),
                _ => matchingEnums.FirstOrDefault()
            };
        }

        public static T[] GetAll<T>() where T : ScriptableEnum
        {
            if (_allOptionsCache.TryGetValue(typeof(T), out ScriptableEnum[] cachedOptions))
            {
                return cachedOptions.Cast<T>().ToArray();
            }

            T[] assets;
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            string[] paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            assets = paths.Select(AssetDatabase.LoadAssetAtPath<T>).ToArray();
#else
            assets = Resources.LoadAll<T>("");
#endif
            _allOptionsCache[typeof(T)] = assets;
            return assets;
        }

        public static ScriptableEnum[] GetAllOptions(Type type)
        {
            if (_allOptionsCache.TryGetValue(type, out ScriptableEnum[] cachedOptions))
            {
                return cachedOptions;
            }

            ScriptableEnum[] assets;
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets("t:" + type.Name);
            string[] paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToArray();
            assets = paths.Select(path => AssetDatabase.LoadAssetAtPath(path, type)).Cast<ScriptableEnum>().ToArray();
#else
            assets = Resources.LoadAll("", type).Cast<ScriptableEnum>().ToArray();
#endif
            _allOptionsCache[type] = assets;
            return assets;
        }

        public static T GetByName<T>(string textIdentifier) where T : ScriptableEnum
        {
            return GetByName(typeof(T), textIdentifier) as T;
        }

        public static ScriptableEnum GetByName(Type type, string textIdentifier)
        {
            textIdentifier = textIdentifier.Trim().ToLower();
            var allScriptableEnums = GetAllOptions(type);
            var matchingEnums = allScriptableEnums.Where(x => x.DisplayText.Trim().ToLower() == textIdentifier)
                .ToArray();

            return matchingEnums.Length switch
            {
                0 => throw new Exception($"No scriptable enum found for {textIdentifier}"),
                > 1 => throw new Exception(
                    $"Multiple scriptable enums found for {textIdentifier}, please specify a more specific name"),
                _ => matchingEnums.FirstOrDefault()
            };
        }

        public static IEnumerable<ScriptableEnum> GetAllInstances(Type type)
        {
            return GetAllOptions(type);
        }

        public static void SetByName<T>(ref T se, string name) where T : ScriptableEnum
        {
            var getByName = GetByName(typeof(T), name);
            se = getByName as T;
        }

        public static bool TrySetIfNullOrWrongName<T>(ref T se, string name) where T : ScriptableEnum
        {
            if (se != null && se.name == name) return false;
            SetByName(ref se, name);
            return se != null;
        }

        public static bool TrySetIfNullOrWrongName<T>(ref T se) where T : ScriptableEnum
        {
            if (se != null) return false;
            se = ScriptableEnum.GetAll<T>().FirstOrDefault();
            return se != null;
        }
        
        public static bool TrySetIfNullOrWrongName<T>(ICollection<T> collection, string name) where T : ScriptableEnum
        {
            if (collection.Any(x => x.name == name)) return false;
            T se = null;
            SetByName(ref se, name);
            if (se == null) return false;
            collection.Add(se);
            return true;
        }

        public static bool TrySetIfNullOrWrongName<T>(Dictionary<string, T> dictionary, string name) where T : ScriptableEnum
        {
            if (dictionary == null || dictionary.TryGetValue(name, out T se)) return false;
            SetByName(ref se, name);
            if (se == null) return false;
            if (dictionary.TryAdd(name, se)) return true;
            dictionary[name] = se;
            return true;
        }

        #endregion
    }
}