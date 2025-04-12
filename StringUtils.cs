namespace Tauntastic.ScriptableEnums
{
    public static class StringUtils
    {
        public static string Color(this object msg, UnityEngine.Color color)
        {
            return $"<color=#{color.ToHexString()}>{msg}</color>";
        }
    }
}