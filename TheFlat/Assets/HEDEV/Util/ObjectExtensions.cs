using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Proto.Util
{
    public static class ObjectExtensions
    {
        public static T Nullable<T>(this T obj) where T : UnityEngine.Object
        {
            return obj == null ? null : obj;
        }
    
        public static T Do<T>(this T obj, Action<T> action) where T : class
        {
            if (obj == null)
                return null;
            action?.Invoke(obj);
            return obj;
        }

        public static bool IsSame(this string source, string target, bool ignoreCase = true)
        {
            if (source == null || target == null)
                return source == target;

            return string.Compare(source, target, ignoreCase) == 0;
        }
        
        public static void StopCoroutineAndNull(this MonoBehaviour mono, ref Coroutine handle)
        {
            if (handle == default)
                return;
            mono.StopCoroutine(handle);
            handle = default;
        }

        public static Transform FindNode(this Transform node, string nodeName)
        {
            if (node.name == nodeName)
                return node;

            for (var index = 0; index < node.childCount; ++index)
            {
                var child = node.GetChild(index);
                if (child == default)
                    continue;
                var found = child.FindNode(nodeName);
                if (found != default)
                    return found;
            }

            return default;
        }

        public static T[] FindNodes<T>(this Transform node) where T : Component
        {
            List<T1> FindNodeImpl<T1>(Transform child, List<T1> list) where T1 : Component
            {
                var component = child.GetComponent<T1>();
                if (component != default)
                    list.Add(component);

                for (var index = 0; index < child.childCount; ++index)
                {
                    var childNode = child.GetChild(index);
                    if (childNode == default)
                        continue;
                    FindNodeImpl(child.GetChild(index), list);
                }

                return list;
            }

            return FindNodeImpl(node, new List<T>()).ToArray();
        }

        public static Transform FindNodeInPath(this Transform node, string path)
        {
            var paths = path.Split('.');
            if (paths.IsEmpty())
                return default;

            if (node.name != paths.First())
                return default;

            var parent = node;
            for (var index = 1; index < paths.Length; ++index)
            {
                var child = parent.Find(paths[index]);
                if (child == default)
                    return default;

                parent = child;
            }

            return parent;
        }
        
        public static string[] Parse(this string argStr, char[] delimiters = default)
        {
            var bQuote = false;
            var substr = "";
            var args = new List<string>();

            if (delimiters == default)
                delimiters = new[] { ' ', '\t' };

            Action<bool> ChangeParseState = quote =>
            {
                if (bQuote || !substr.IsEmpty())
                {
                    args.Add(substr);
                    substr = "";
                }

                bQuote = quote;
            };
            argStr.ForEach(ch =>
            {
                if (ch == '\"')
                    ChangeParseState(!bQuote);
                else if (delimiters.FirstOrDefault(data => data == ch) != default && !bQuote)
                    ChangeParseState(bQuote);
                else
                    substr += ch;
            });
            ChangeParseState(false);

            return args.ToArray();
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static Color GetColorFromString(string colorStr)
        {
            ColorUtility.TryParseHtmlString(colorStr, out var color);
            return color;
        }

        public static TComponentType GetComponent<TComponentType>(this MonoBehaviour obj, ref TComponentType component,
            Transform child = default) where TComponentType : Component
        {
            if (component != default)
                return component;
            component = obj.GetComponent<TComponentType>();
            if (component == default && child != default)
                component = child.GetComponentInChildren<TComponentType>();
            return component;
        }
    }
}