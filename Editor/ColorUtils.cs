using UnityEngine;

namespace Tauntastic.ScriptableEnums.Editor
{
    public static class ColorUtils
    {
        public static string ToHexString(this Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}