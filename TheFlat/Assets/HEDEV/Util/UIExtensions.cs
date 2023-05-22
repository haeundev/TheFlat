using System;
using System.Collections.Generic;
using Proto.UISystem;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Proto.Util
{
    public static class UIExtensions
    {
        private static readonly List<Vector3> bezierPoints = new();

        public static Vector3 BezierCurvePoint(this IEnumerable<Vector3> list, float rate)
        {
            bezierPoints.Clear();
            bezierPoints.AddRange(list);
            var points = bezierPoints;
            var count = points.Count - 1;

            while (count > 0)
            {
                for (var i = 0; i < count; i++)
                {
                    var result = Vector3.Lerp(points[i], points[i + 1], rate);
                    points[i] = result;
                }

                count--;
            }

            return points[0];
        }

        public static Vector2 GetRandomPointInRect(RectTransform rectTransform, bool useLocal = false)
        {
            var rect = rectTransform.rect;
            var position = useLocal ? rectTransform.localPosition : rectTransform.position;
            var randomX = Random.Range(position.x - rect.width / 2, position.x + rect.width / 2);
            var randomY = Random.Range(position.y - rect.height / 2, position.y + rect.height / 2);
            var result = new Vector2(randomX, randomY);
            return result;
        }

        public static bool GetMousePositionOnCanvas(Canvas canvas, out Vector2 localPoint)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.GetComponent<RectTransform>(),
                    new Vector2(Input.mousePosition.x, Input.mousePosition.y), canvas.worldCamera,
                    out var mousePos))
            {
                localPoint = mousePos;
                return true;
            }

            localPoint = Vector2.zero;
            return false;
        }
        
          public static void SetGray(this Image image, bool isSet) { image.material = isSet ? UIWindowManager.GrayscaleMaterial : null; }
        
        [Flags]
        public enum GrayscaleApplyingOption
        {
            None = 0,
            IncludeInactive = 1 << 0,
            UseKeptTargets = 1 << 1,
        }

        [Flags]
        public enum GrayscaleUnapplyingOption
        {
            None = 0,
            KeepTargets = 1 << 0,
        }

        public static void ApplyGrayscaleIncludingChildren(this GameObject gameObject, GrayscaleApplyingOption option = GrayscaleApplyingOption.IncludeInactive)
        {
            GrayscaleSetter grayscaleSetter;
            
            if (option.HasFlag(GrayscaleApplyingOption.UseKeptTargets))
            {
                grayscaleSetter = gameObject.GetComponent<GrayscaleSetter>();
                if (!grayscaleSetter)
                {
                    grayscaleSetter = gameObject.AddComponent<GrayscaleSetter>();
                    grayscaleSetter.ClearAllLists();
                    grayscaleSetter.CollectLayoutElementsAndApplyGrayscaleIncludingChildren(option);
                    return;
                }
                                
                grayscaleSetter.ApplyGrayscaleToLayoutElementsListedPreviously(option);
                return;
            }

            grayscaleSetter = gameObject.GetComponent<GrayscaleSetter>();

            if (!grayscaleSetter)
            {
                grayscaleSetter = gameObject.AddComponent<GrayscaleSetter>();
                grayscaleSetter.ClearAllLists();
            }

            grayscaleSetter.CollectLayoutElementsAndApplyGrayscaleIncludingChildren(option);
        }

        public static void UnapplyGrayscaleIncludingChildren(this GameObject gameObject, GrayscaleUnapplyingOption option = GrayscaleUnapplyingOption.None)
        {
            var grayscaleSetter = gameObject.GetComponent<GrayscaleSetter>();

            if (!grayscaleSetter)
                return;
                
            grayscaleSetter.UnapplyGrayscaleIncludingChildren(option);
        }
    }
}