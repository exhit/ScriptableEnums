using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tauntastic.ScriptableEnums.Editor
{
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
                    flexDirection = FlexDirection.Row,
                    maxHeight = 21
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
                    marginLeft = 1,
                    marginRight = 0,
                }
            };
            
            var openPropertyEditorButton = new Button()
            {
                name = _OPEN_PROPERTY_EDITOR_BUTTON_NAME,
                text = "👉",
                style =
                {
                    marginLeft = 1,
                    marginRight = 0,
                    paddingTop = 0,
                    paddingBottom = 5,
                    paddingLeft = 4,
                    paddingRight = 4,
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
}