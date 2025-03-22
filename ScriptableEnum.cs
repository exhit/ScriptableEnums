using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Tauntastic
{
    using ScriptableEnums;
    
    [Icon("Packages/com.tauntastic.scriptableenums/d_ScriptableEnum Icon.png")]
    abstract public class ScriptableEnum : ScriptableObject
    {
        [SerializeField]
        [ScriptableEnumsDisableAttribute]
        private string _displayText;

        public string DisplayText
        {
            get => _displayText;
            protected set => _displayText = value;
        }

        private static Dictionary<Type, ScriptableEnum[]> _allOptionsCache = new();

        private void Awake()
        {
            DisplayText = name;
        }

        protected virtual void OnValidate()
        {
            DisplayText = name;
        }

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
        
        public static T GetByName<T>(string textIdentifier)  where T : ScriptableEnum
        {
            textIdentifier = textIdentifier.Trim().ToLower();
            var allScriptableEnums = GetAll<T>();
            var matchingEnums = allScriptableEnums.Where(x => x.DisplayText.Trim().ToLower() == textIdentifier).ToArray();

            return matchingEnums.Length switch
            {
                0 => throw new Exception($"No scriptable enum found for {textIdentifier}"),
                > 1 => throw new Exception($"Multiple scriptable enums found for {textIdentifier}, please specify a more specific name"),
                _ => matchingEnums.FirstOrDefault()
            };
        }

        public static ScriptableEnum GetByName(Type type, string textIdentifier)
        {
            textIdentifier = textIdentifier.Trim().ToLower();
            var allScriptableEnums = GetAllOptions(type);
            var matchingEnums = allScriptableEnums.Where(x => x.DisplayText.Trim().ToLower() == textIdentifier).ToArray();

            return matchingEnums.Length switch
            {
                0 => throw new Exception($"No scriptable enum found for {textIdentifier}"),
                > 1 => throw new Exception($"Multiple scriptable enums found for {textIdentifier}, please specify a more specific name"),
                _ => matchingEnums.FirstOrDefault()
            };
        }

        public static IEnumerable<ScriptableEnum> GetAllInstances(Type type)
        {
            return GetAllOptions(type);
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ScriptableEnumAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ScriptableEnum), true)]
    public class ScriptableEnumPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return ScriptableEnumUtils.GetCorrespondingScriptableEnumElement(property, fieldInfo);
        }
    }

    [CustomPropertyDrawer(typeof(ScriptableEnumAttribute), true)]
    public class ScriptableEnumAttributePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return ScriptableEnumUtils.GetCorrespondingScriptableEnumElement(property, fieldInfo);
        }
    }

    public static class ScriptableEnumUtils
    {
        public static VisualElement GetCorrespondingScriptableEnumElement(SerializedProperty property, FieldInfo fieldInfo, int countThreshold = 10)
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
        
        public static bool IsExistingScriptableObjectCountAbove(this Type type, int threshold = 40)
        {
            var assets = GetAssets(type);
            return assets.Count > threshold;
        }
        
        public static List<ScriptableObject> GetAssets(Type type)
        {
            var assets =
                AssetDatabase
                    .FindAssets($"t:{type.FullName}")
                    .Select(guid => AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guid)))
                    .Where(obj => obj != null)
                    .ToList();
            
            return assets;
        }
    }

    public class ScriptableEnumField : BaseField<ScriptableObject>
    {
        private const string _POPUP_FIELD_NAME = "enum-field";
        private const string _PING_BUTTON_NAME = "ping-button";
        // private const string _SELECT_BUTTON_NAME = "select-button";
        private const string _OPEN_PROPERTY_EDITOR_BUTTON_NAME  = "open-property-editor-button";

        private FieldInfo _fieldInfo;
        private SerializedProperty _property;
        private SerializedProperty _displayNameProperty;

        private PopupField<string> _popupField;
        private Type _targetType;

        private readonly Dictionary<string, ScriptableObject> _nameToAssetMap = new();
        private readonly Dictionary<ScriptableObject, string> _assetToNameMap = new();

        public ScriptableEnumField(SerializedProperty property, FieldInfo fieldInfo)
            : this(property.displayName)
        {
            BindPropertyAndFieldInfo(property, fieldInfo);
        }

        public ScriptableEnumField(string label = nameof(ScriptableEnumField))
            : base(label, CreateVisualInputElement())
        {
            AddToClassList("unity-base-field__aligned");

            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                Undo.undoRedoPerformed += RefreshOptions;
                EditorApplication.projectChanged += RefreshOptions;
            });

            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                Undo.undoRedoPerformed -= RefreshOptions;
                EditorApplication.projectChanged -= RefreshOptions;
            });
        }

        private static VisualElement CreateVisualInputElement()
        {
            var root = new VisualElement()
            {
                style =
                {
                    flexDirection = FlexDirection.Row
                }
            };

            var popupField = new PopupField<string>("")
            {
                name = _POPUP_FIELD_NAME,
                style =
                {
                    flexGrow = 1f,
                    marginLeft = 0,
                    marginRight = 0,
                }
            };

            var pingButton = new Button()
            {
                name = _PING_BUTTON_NAME,
                text = "💡",
                style =
                {
                    paddingTop = 0,
                    paddingBottom = 0,
                }
            };
            
            var openPropertyEditorButton = new Button()
            {
                name = _OPEN_PROPERTY_EDITOR_BUTTON_NAME,
                text = "👉",
                style =
                {
                    marginLeft = 0,
                    paddingTop = 0,
                    paddingBottom = 4,
                }
            };

            root.Add(popupField);
            root.Add(pingButton);
            // root.Add(selectButton);
            root.Add(openPropertyEditorButton);
            
            return root;
        }

        private void BindPropertyAndFieldInfo(SerializedProperty property, FieldInfo fieldInfo)
        {
            _property = property;
            _fieldInfo = fieldInfo;
            
            if (_property == null)
            {
                throw new ArgumentNullException(nameof(property) + ": cannot bind to null");
            }

            if (_fieldInfo == null)
            {
                throw new ArgumentException("Cannot find field info for property: " + property.propertyPath);
            }

            _targetType = _fieldInfo.FieldType;

            if (_targetType == null)
            {
                throw new ArgumentException("ScriptableEnumField can only be used with ScriptableObject properties.");
            }
            
            if (_targetType.IsGenericType)
            {
                _targetType = _targetType.GetGenericArguments()[0];
            }

            _popupField = this.Q<PopupField<string>>(_POPUP_FIELD_NAME);
            var pingButton = this.Q<Button>(_PING_BUTTON_NAME);
            var openPropertyEditorButton = this.Q<Button>(_OPEN_PROPERTY_EDITOR_BUTTON_NAME);

            _popupField.TrackPropertyValue(_property, p =>
            {
                bool exists = _property.objectReferenceValue != null;
                pingButton.style.display = exists ? DisplayStyle.Flex : DisplayStyle.None;
                pingButton.SetEnabled(exists);
                openPropertyEditorButton.style.display = exists ? DisplayStyle.Flex : DisplayStyle.None;
                openPropertyEditorButton.SetEnabled(exists);
                _popupField.SetValueWithoutNotify(GetCurrentDisplayName(p));
            });

            _popupField.RegisterValueChangedCallback(evt => { OnSelectionChanged(_property, evt.newValue); });

            bool exists = _property.objectReferenceValue != null;

            pingButton.clickable = new Clickable(() =>
            {
                if (_property.objectReferenceValue != null)
                {
                    EditorGUIUtility.PingObject(_property.objectReferenceValue);
                }
            });

            pingButton.style.display = exists ? DisplayStyle.Flex : DisplayStyle.None;
            pingButton.SetEnabled(exists);

            openPropertyEditorButton.clickable = new Clickable(() =>
            {
                EditorUtility.OpenPropertyEditor(_property.objectReferenceValue);
            });
            
            openPropertyEditorButton.style.display = exists ? DisplayStyle.Flex : DisplayStyle.None;
            openPropertyEditorButton.SetEnabled(exists);

            RefreshOptions();
        }

        private void RefreshOptions()
        {
            var assets = GetAssetsOfType();

            _nameToAssetMap.Clear();
            _assetToNameMap.Clear();

            Dictionary<string, int> names = new();

            // Build asset lists with duplicate tracking
            foreach (var asset in assets)
            {
                string displayName = asset.name;

                if (!names.TryAdd(displayName, 1))
                {
                    names[displayName]++;
                }
            }

            // Assign unique names and track assets
            foreach (var asset in assets)
            {
                string displayName = asset.name;

                if (names[displayName] > 1)
                {
                    displayName += $" ({names[displayName]})";
                }

                _nameToAssetMap[displayName] = asset;
                _assetToNameMap[asset] = displayName;
            }

            List<string> choices = _assetToNameMap.Values.ToList();
            choices.Insert(0, "<null>");

            _popupField.choices = choices;
            _popupField.SetEnabled(_popupField.choices.Count > 1);
            _popupField.SetValueWithoutNotify(GetCurrentDisplayName(_property));
        }

        private List<ScriptableObject> GetAssetsOfType()
        {
            var assets =
                AssetDatabase
                    .FindAssets($"t:{_targetType.Name}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(path => (ScriptableObject) AssetDatabase.LoadAssetAtPath(path, typeof(ScriptableObject)))
                    .Where(obj => obj != null)
                    .ToList();
            
            return assets;
        }

        private string GetCurrentDisplayName(SerializedProperty property)
        {
            var currentValue = property.objectReferenceValue as ScriptableObject;

            if (currentValue == null)
            {
                return "<null>";
            }

            if (_assetToNameMap.TryGetValue(currentValue, out string nameValue))
            {
                return nameValue;
            }

            var assets = GetAssetsOfType();
            Debug.Log(assets.Count.ToString());
            var assetsToNameMap = assets.ToDictionary(x => x, y => y.name);
            if (assetsToNameMap.TryGetValue(currentValue, out nameValue))
            {
                return nameValue;
            }

            Debug.Log("Value was deleted or not found in the map.");
            return "<null>";
        }

        private void OnSelectionChanged(SerializedProperty property, string newValue)
        {
            if (_nameToAssetMap.TryGetValue(newValue, out ScriptableObject asset))
            {
                property.objectReferenceValue = asset;
            }
            else if (newValue == "<null>")
            {
                property.objectReferenceValue = null;
                // Debug.LogWarning("Are you sure you want to assign null?");
            }
            else
            {
                Debug.LogError("Error in selection change.");
            }

            property.serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}

namespace Tauntastic.ScriptableEnums
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ScriptableEnumsDisableAttribute : PropertyAttribute
    {
    }

    public interface IDisableable
    {
        void Disable();
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ScriptableEnumsDisableAttribute), true)]
    public class ScriptableEnumsDisableAttributePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var propertyField = new PropertyField(property);

            propertyField.RegisterCallbackOnce<GeometryChangedEvent>(_ =>
                {
                    var firstChild = propertyField.Q();
                    if (firstChild is IDisableable disableable)
                    {
                        disableable.Disable();
                    }
                    else
                    {
                        propertyField.SetEnabled(false);
                    }
                }
            );

            return propertyField;
        }
    }
#endif
}