using Proto.SoundSystem;
using UnityEngine;

namespace Proto.Util
{
    public static class SoundServiceExtensions
    {
        public static bool IsNull(this object T)
        {
            return T == null;
        }

        public static void PlaySound(this Transform t, string name, bool loop = false)
        {
            SoundService.Play(name, t.position, loop);
        }

        public static void PlaySound(this string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
        
            SoundService.Play(path);
        }
    }
}