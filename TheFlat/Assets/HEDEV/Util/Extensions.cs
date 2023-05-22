using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Proto.Util
{
    public static class Extensions
    {
        private static readonly Random Random = new();

        public static void Shuffle<T>(this IList<T> list, Random rnd = null)
        {
            rnd ??= Random;
            for (var i = list.Count; i > 0; i--)
                list.Swap(i - 1, rnd.Next(0, i));
        }

        public static List<T> ToShuffleList<T>(this IEnumerable<T> enumerable, Random rnd = null)
        {
            if (rnd == null)
                rnd = Random;
            var list = enumerable.ToList();
            for (var i = list.Count; i > 0; i--)
                list.Swap(i - 1, rnd.Next(0, i));
            return list;
        }

        public static int PeekRandomIndex<T>(this IList<T> list, Random rnd = null)
        {
            if (list == null || list.Count == 0)
                return default;
            rnd ??= Random;
            var index = rnd.Next(0, list.Count);
            return index;
        }

        public static T PeekRandom<T>(this IList<T> list, Random rnd = null)
        {
            if (list == null || list.Count == 0)
                return default;
            rnd ??= Random;
            var index = rnd.Next(0, list.Count);
            return list[index];
        }

        public static T PeekRandom<T>(this IEnumerable<T> enumerable, Random rnd = null) where T : class
        {
            return enumerable == null ? default : PeekRandom(enumerable.ToList(), rnd);
        }

        public static IEnumerable<T> PeekRandoms<T>(this IList<T> list, int count, Random rnd = null)
        {
            if (list == null || list.Count == 0 || count < 1)
                return default;
            if (rnd == null)
                rnd = Random;

            return list.OrderBy(_ => rnd.Next(int.MinValue, int.MaxValue)).Take(count);
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            if (i == j)
                return;
            (list[i], list[j]) = (list[j], list[i]);
        }

        public static T PopFirstOrDefault<T>(this IList<T> list)
        {
            if (list.Count == 0)
                return default;
            var temp = list[0];
            list.RemoveAt(0);
            return temp;
        }
        
        private static string EnumerableToString(this IEnumerable list)
        {
            var str = "(";
            foreach (var item in list)
                str = str + item.ToStringWithComma() + ", ";
            str = str.TrimEnd(',', ' ') + ")";
            return str;
        }

        public static string ToStringWithComma<T>(this T obj)
        {
            if (obj is IEnumerable enumerable) return enumerable.EnumerableToString();
            return obj.ToString();
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null)
                return null;
            
            var forEach = enumerable.ToList();
            using var it = forEach.GetEnumerator();
            while (it.MoveNext())
                action(it.Current);
            
            return forEach;
        }

        public static Transform GetNearest(this IEnumerable<Transform> enumerable, Transform from,
            float maxDistance = -1f)
        {
            if (enumerable == null)
                return null;

            using var it = enumerable.GetEnumerator();
            var distance = maxDistance;
            Transform nearest = null;
            while (it.MoveNext())
            {
                var newDistance = Vector3.Distance(from.position, it.Current.position);
                if (distance < 0 || newDistance < distance)
                {
                    distance = newDistance;
                    nearest = it.Current;
                }
            }

            return nearest;
        }

        public static Transform GetNearest<T>(this IEnumerable<T> enumerable, Transform from, float maxDistance = -1f)
            where T : MonoBehaviour
        {
            if (enumerable == null)
                return null;

            using var it = enumerable.GetEnumerator();
            var distance = maxDistance;
            Transform nearest = null;
            while (it.MoveNext())
            {
                var newDistance = Vector3.Distance(from.position, it.Current.transform.position);
                if (distance < 0 || newDistance < distance)
                {
                    distance = newDistance;
                    nearest = it.Current.transform;
                }
            }

            return nearest;
        }

        public static void ForEachChild(this Transform transform, Action<Transform> action)
        {
            if (transform == null)
                return;

            var count = transform.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = transform.GetChild(i);
                action?.Invoke(child);
            }
        }

        public static Transform ToTransform(this GameObject gameObject)
        {
            return gameObject == null ? null : gameObject.transform;
        }

        public static Transform ToTransform(this Component component)
        {
            return component == null ? null : component.transform;
        }

        public static GameObject ToGameObject(this Component component)
        {
            return component == null ? null : component.gameObject;
        }

        public static bool IsOneOf<T>(this T source, params T[] targets) where T : class
        {
            return targets.Any(target => target == source);
        }

        public static int IndexOfMax<T>(this IEnumerable<T> source) where T : IComparable<T>
        {
            var maxIndex = -1;
            var index = -1;
            T maxValue = default;

            using var iterator = source.GetEnumerator();
            while (iterator.MoveNext())
            {
                ++index;
                var value = iterator.Current;
                if (value != null && (maxIndex == -1 || value.CompareTo(maxValue) > 0))
                {
                    maxIndex = index;
                    maxValue = value;
                }
            }

            return maxIndex;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : new()
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;

            value = new TValue();
            dictionary.Add(key, value);
            return value;
        }

        public static void AddOrCreate<TKey, TValue>(this IDictionary<TKey, List<TValue>> dictionary, TKey key,
            TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                if (dictionary[key].Contains(value) == false)
                    dictionary[key].Add(value);
            }
            else
            {
                dictionary.Add(key, new List<TValue> { value });
            }
        }

        public static void EnqueueOrCreate<TKey, TValue>(this IDictionary<TKey, Queue<TValue>> dictionary, TKey key,
            TValue value, bool ignoreDuplicateValue = false)
        {
            if (dictionary.ContainsKey(key))
            {
                if (ignoreDuplicateValue)
                {
                    if (dictionary[key].Contains(value) == false)
                        dictionary[key].Enqueue(value);
                }
                else
                {
                    dictionary[key].Enqueue(value);
                }
            }
            else
            {
                dictionary.Add(key, new Queue<TValue>());
                dictionary[key].Enqueue(value);
            }
        }

        public static void AddOrSet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        public static string ConvertSpaceToUnderScore(this string str)
        {
            var temp = str.Replace('-', '_');
            temp = temp.Replace(' ', '_');
            return temp;
        }

        public static bool IsInCullingLayer(this Camera camera, int layer)
        {
            var mask = camera.cullingMask;
            var position = 1 << layer;
            mask &= position;
            return mask != 0;
        }

        public static void AddCullingLayer(this Camera camera, int layer)
        {
            var mask = camera.cullingMask;
            var position = 1 << layer;
            mask |= position;
            camera.cullingMask = mask;
        }

        public static void RemoveCullingLayer(this Camera camera, int layer)
        {
            var mask = camera.cullingMask;
            var position = 1 << layer;
            mask &= ~position;
            camera.cullingMask = mask;
        }

        public static void AddCullingLayers(this Camera camera, int cullingMaskForLayers)
        {
            var newCullingMask = camera.cullingMask;
            newCullingMask |= cullingMaskForLayers;
            camera.cullingMask = newCullingMask;
        }

        public static void RemoveCullingLayers(this Camera camera, int cullingMaskForLayers)
        {
            var newCullingMask = camera.cullingMask;
            newCullingMask &= ~cullingMaskForLayers;
            camera.cullingMask = newCullingMask;
        }

        public static void AddCullingLayer(this Camera camera, string layerName)
        {
            camera.AddCullingLayer(LayerMask.NameToLayer(layerName));
        }

        public static void RemoveCullingLayer(this Camera camera, string layerName)
        {
            camera.RemoveCullingLayer(LayerMask.NameToLayer(layerName));
        }

        public static bool IsPositionInScreen(this Camera camera, Vector3 worldPosition)
        {
            var viewPortPosition = camera.WorldToViewportPoint(worldPosition);
            if (viewPortPosition.x is < 0 or > 1) return false;
            if (viewPortPosition.y is < 0 or > 1) return false;
            if (viewPortPosition.z < 0) return false;

            return true;
        }

        public static void DoEach(this int count, Action action)
        {
            for (var i = 0; i < count; i++) action.Invoke();
        }

        public static void DoWait(this MonoBehaviour mono, Action action, int frame = 1)
        {
            mono.StartCoroutine(DoWaitFrame(frame, action));
        }

        private static IEnumerator DoWaitFrame(int frame, Action action)
        {
            while (frame-- > 0)
                yield return null;
            action.Invoke();
        }

        public static void DoWaitForWhile(this MonoBehaviour mono, Action action, Func<bool> condition,
            float checkTime = 0.1f)
        {
            if (mono)
                mono.StartCoroutine(DoWaitWhile(action, condition, checkTime));
        }

        private static IEnumerator DoWaitWhile(Action action, Func<bool> condition, float checkTime)
        {
            while (condition.Invoke())
                yield return new WaitForSeconds(checkTime);
            action.Invoke();
        }

        public static IEnumerator DoWhileForSeconds(this MonoBehaviour mono, float time, Action action)
        {
            var enumerator = DoWhileTime(time, action);
            mono.StartCoroutine(enumerator);
            return enumerator;
        }

        private static IEnumerator DoWhileTime(float time, Action action)
        {
            var startTime = Time.time;
            while (startTime + time > Time.time)
            {
                yield return null;
                action.Invoke();
            }
        }

        public static IEnumerator DoWaitForSeconds(this MonoBehaviour mono, float time, Action action)
        {
            var enumerator = DoWaitTime(time, action);
            mono.StartCoroutine(enumerator);
            return enumerator;
        }

        private static IEnumerator DoWaitTime(float time, Action action)
        {
            if (Mathf.Approximately(time, 0))
                yield return null;
            else
                yield return new WaitForSeconds(time);
            action.Invoke();
        }

        public static void SetPositionAndRotation(this Transform transform, Transform other)
        {
            transform.SetPositionAndRotation(other.position, other.rotation);
        }

        public static void Enqueue<T>(this Queue<T> queue, IEnumerable<T> enumerable)
        {
            foreach (var obj in enumerable)
                queue.Enqueue(obj);
        }

        public static Vector3 WorldScale(this Transform transform)
        {
            var localToWorldMatrix = transform.localToWorldMatrix;
            var scaleX = localToWorldMatrix.GetColumn(0).magnitude;
            var scaleY = localToWorldMatrix.GetColumn(1).magnitude;
            var scaleZ = localToWorldMatrix.GetColumn(2).magnitude;
            return new Vector3(scaleX, scaleY, scaleZ);
        }

        public static void SetWorldScale(this Transform transform, Vector3 worldScale)
        {
            var currentWorldScale = transform.lossyScale;
            transform.localScale = new Vector3(
                worldScale.x / currentWorldScale.x,
                worldScale.y / currentWorldScale.y,
                worldScale.z / currentWorldScale.z);
        }

        public static float GetMaximumWorldScaleValue(this Transform transform)
        {
            var localToWorldMatrix = transform.localToWorldMatrix;
            var scaleX = localToWorldMatrix.GetColumn(0).magnitude;
            var scaleY = localToWorldMatrix.GetColumn(1).magnitude;
            var scaleZ = localToWorldMatrix.GetColumn(2).magnitude;
            return Mathf.Max(scaleX, scaleY, scaleZ);
        }

        public static int GetDigit(this int source, int digit)
        {
            return source / (int)Math.Pow(10, digit - 1) % 10;
        }

        public static bool HasBatchimEnd(this string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;
            return HasBatchim(word.Last());
        }

        public static bool HasBatchim(this char character)
        {
            if (character < 0xAC00 || character > 0xD7A3)
                return false;
            return (character - 0xAC00) % 28 > 0;
        }

        public static T GetOrAddComponent<T>(this Component component)
            where T : Component
        {
            if (!component.TryGetComponent<T>(out var newComponent))
                newComponent = component.gameObject.AddComponent<T>();

            return newComponent;
        }

        public static bool Is(this float source, float value)
        {
            return Mathf.Approximately(source, value);
        }

        public static int GetRandomWeightedIndex(this IReadOnlyList<int> weights)
        {
            if (weights == null || weights.Count == 0)
                return -1;

            var totalWeight = 0;

            for (var index = 0; index < weights.Count; ++index)
            {
                var weight = weights[index];
                totalWeight += weight;
            }

            if (totalWeight == 0)
                return -1;

            var randomValue = Random.NextDouble() * totalWeight;
            var upperLimitForCurrentIndex = 0d;

            for (var i = 0; i < weights.Count; ++i)
            {
                if (weights[i] == 0)
                    continue;

                upperLimitForCurrentIndex += weights[i];

                if (upperLimitForCurrentIndex >= randomValue)
                    return i;
            }

            return -1;
        }

        public static Vector3 GetVectorBetween(Vector3 v1, Vector3 v2, float percentage = 0.5f)
        {
            return (v2 - v1) * percentage + v1;
        }

        public static void DisposeAndNull(ref IDisposable disposable)
        {
            disposable?.Dispose();
            disposable = null;
        }
    }
}