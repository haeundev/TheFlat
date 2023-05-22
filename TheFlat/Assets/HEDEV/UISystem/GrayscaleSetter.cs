using System.Collections.Generic;
using System.Linq;
using Proto.Util;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Proto.UISystem
{
    public class GrayscaleSetter : MonoBehaviour
    {
        private List<Image> _targetImages;
        private List<KeyValuePair<TextMeshProUGUI, Color>> _targetTextOriginalColorPairs = new();

        private void OnDestroy()
        {
            ClearAllLists();
        }

        public void ClearAllLists()
        {
            _targetImages = new List<Image>();
            _targetTextOriginalColorPairs = new List<KeyValuePair<TextMeshProUGUI, Color>>();
        }

        public void CollectLayoutElementsAndApplyGrayscaleIncludingChildren(UIExtensions.GrayscaleApplyingOption option)
        {
            var layoutElements =
                gameObject.GetComponentsInChildren<ILayoutElement>(
                    option.HasFlag(UIExtensions.GrayscaleApplyingOption.IncludeInactive));
            foreach (var layoutElement in layoutElements)
                switch (layoutElement)
                {
                    case TextMeshProUGUI textMeshProUGUI:
                        _targetTextOriginalColorPairs.Add(
                            new KeyValuePair<TextMeshProUGUI, Color>(textMeshProUGUI, textMeshProUGUI.color));
                        textMeshProUGUI.color = GetGrayscaleColor(textMeshProUGUI.color);
                        break;
                    case Image image:
                        _targetImages.Add(image);
                        image.SetGray(true);
                        break;
                    default:
                        continue;
                }
        }

        public void ApplyGrayscaleToLayoutElementsListedPreviously(UIExtensions.GrayscaleApplyingOption option)
        {
            var includeInactive = option.HasFlag(UIExtensions.GrayscaleApplyingOption.IncludeInactive);

            var targetTextOriginalColorPairsFiltered =
                includeInactive
                    ? _targetTextOriginalColorPairs.Where(pair => pair.Key)
                    : _targetTextOriginalColorPairs.Where(pair => pair.Key && pair.Key.gameObject.activeSelf);

            foreach (var targetTextOriginalColorPair in targetTextOriginalColorPairsFiltered)
            {
                var textMeshProUGUI = targetTextOriginalColorPair.Key;
                textMeshProUGUI.color = GetGrayscaleColor(textMeshProUGUI.color);
            }

            var targetImagesFiltered =
                includeInactive
                    ? _targetImages.Where(targetImage => targetImage)
                    : _targetImages.Where(targetImage => targetImage && targetImage.gameObject.activeSelf);

            foreach (var targetImage in targetImagesFiltered)
                targetImage.SetGray(true);
        }

        public void UnapplyGrayscaleIncludingChildren(UIExtensions.GrayscaleUnapplyingOption option)
        {
            foreach (var targetTextOriginalColorPair in _targetTextOriginalColorPairs.Where(pair => pair.Key))
                targetTextOriginalColorPair.Key.color = targetTextOriginalColorPair.Value;

            foreach (var targetImage in _targetImages.Where(targetImage => targetImage))
                targetImage.SetGray(false);

            if (!option.HasFlag(UIExtensions.GrayscaleUnapplyingOption.KeepTargets))
                ClearAllLists();
        }

        private Color GetGrayscaleColor(Color originalColor)
        {
            var grayColor = 0.2126f * originalColor.r + 0.7152f * originalColor.g + 0.0722f * originalColor.b;
            return new Color(grayColor, grayColor, grayColor, originalColor.a);
        }
    }
}