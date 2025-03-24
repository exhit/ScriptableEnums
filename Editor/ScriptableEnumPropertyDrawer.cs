using UnityEditor;
using UnityEngine.UIElements;

namespace Tauntastic.ScriptableEnums.Editor
{
    [CustomPropertyDrawer(typeof(ScriptableEnum), true)]
    public class ScriptableEnumPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return ScriptableEnumUtils.GetCorrespondingScriptableEnumElement(property, fieldInfo);
        }
    }
}