using UnityEditor;
using UnityEngine.UIElements;

namespace Tauntastic.ScriptableEnums.Editor
{
    [CustomPropertyDrawer(typeof(ScriptableEnumAttribute), true)]
    public class ScriptableEnumAttributePropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return ScriptableEnumUtils.GetCorrespondingScriptableEnumElement(property, fieldInfo);
        }
    }
}