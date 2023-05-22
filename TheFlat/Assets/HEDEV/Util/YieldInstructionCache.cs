using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto.Util
{
    internal static class YieldInstructionCache 
    {
        private class FloatComparer : IEqualityComparer<float>
        {
            bool IEqualityComparer<float>.Equals(float x, float y)
            {
                return Mathf.Abs(x - y) < 0.001f;
            }

            int IEqualityComparer<float>.GetHashCode(float obj)
            {
                return obj.GetHashCode();
            }
        }
        
        public static readonly WaitForEndOfFrame WaitForEndOfFrame = new();
        public static readonly WaitForFixedUpdate WaitForFixedUpdate = new();

        private static readonly Dictionary<float, WaitForSeconds> TimeInterval = new(new FloatComparer());

        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            WaitForSeconds wfs;
            if (!TimeInterval.TryGetValue(seconds, out wfs))
                TimeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
            return wfs;
        }
        
        private static readonly Dictionary<float, WaitForSecondsRealtime> RealTimeInterval = new(new FloatComparer());

        public static WaitForSecondsRealtime WaitForSecondsRealtime(float seconds)
        {
            if (!RealTimeInterval.TryGetValue(seconds, out var wfsRt))
                RealTimeInterval.Add(seconds, wfsRt = new WaitForSecondsRealtime(seconds));
            return wfsRt;
        }
        
        private static readonly Dictionary<Func<bool>, WaitUntil> WaitUntilDic = new();

        public static WaitUntil WaitUntil(Func<bool> method)
        {
            if (!WaitUntilDic.TryGetValue(method, out var waitUntil))
                WaitUntilDic.Add(method, waitUntil = new WaitUntil(method));
            return waitUntil;
        }

        public static IEnumerator WaitForUnscaledSeconds(float seconds)
        {
            var currentTime = 0f;
            while (currentTime < seconds)
            {
                currentTime += Time.unscaledDeltaTime;
                yield return WaitForEndOfFrame;
            }
        }
    }
}