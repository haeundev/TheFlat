using UnityEngine;

namespace Proto.Util
{
    public static class ColorExtensions
    {
        public static Color GetRandomColor()
        {
            return new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
    }
}