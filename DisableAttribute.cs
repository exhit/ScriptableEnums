using System;
using UnityEngine;
using UnityEngine.Scripting;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Tauntastic.ScriptableEnums
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [Preserve]
    public class DisableAttribute : PropertyAttribute
    {
    }

    public interface IDisableable
    {
        void Disable();
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DisableAttribute), true)]
    public class DisableAttributePropertyDrawer : PropertyDrawer
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