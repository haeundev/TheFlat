using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Proto.UISystem
{
    public class TransText : UnityEngine.EventSystems.UIBehaviour
    {
        public delegate void ClickTranslate(bool isNative);

        private static readonly List<TransText> Instances = new();
        private static bool _translateState;
        [SerializeField] private TMP_Text textEng;
        [SerializeField] private TMP_Text textNative;
        private bool _localTranslateState;
        private bool _translatable;
        public string TextEng => textEng.text;
        public string TextNative => textNative.text;

        public TMP_Text TextEngGUI => textEng;
        public TMP_Text TextNativeGUI => textNative;

        private bool CheckedTranslateState
        {
            get
            {
                if (_localTranslateState == false && string.IsNullOrEmpty(textEng.text) == false)
                    return false;
                if (string.IsNullOrEmpty(textNative.text) == false)
                    return true;
                return false;
            }
        }

        protected override void Awake()
        {
            if (textEng == null)
                Collect();
            _translatable = !(textEng == null || textNative == null);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Instances.Add(this);
            _localTranslateState = _translateState;
            if (_translatable) SelectActiveText();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Instances.Remove(this);
        }

        public static event ClickTranslate OnTranslateAction;

        private void SelectActiveText()
        {
            if (!_translatable)
                return;

            if (CheckedTranslateState)
            {
                textEng.gameObject.SetActive(false);
                textNative.gameObject.SetActive(true);
            }
            else
            {
                textEng.gameObject.SetActive(true);
                textNative.gameObject.SetActive(false);
            }
        }

        public void Collect()
        {
            var texts = GetComponentsInChildren<TMP_Text>(true).ToList();
            if (texts.Count < 2)
            {
                if (texts.Count == 1)
                    SetEng(texts[0]);
                return;
            }

            textNative = FindText(texts, "Native", "native");
            if (textNative != null) texts.Remove(textNative);

            if (texts.Count == 1)
                SetEng(texts[0]);
            else
                SetEng(FindText(texts, "Eng", "eng"));
        }

        private TMP_Text FindText(IReadOnlyCollection<TMP_Text> texts, params string[] str)
        {
            TMP_Text result = null;
            foreach (var item in str)
            {
                result = texts.FirstOrDefault(p => p.name.Contains(item));
                if (result != null)
                    break;
            }

            return result;
        }

        private void SetEng(TMP_Text text)
        {
            textEng = text;
        }

        public void SetText(string eng, string native)
        {
            if (textEng != null)
                textEng.text = eng;
            if (textNative != null)
                textNative.text = native;
            SelectActiveText();
        }

        public void ChangeFont(TMP_FontAsset fontAsset)
        {
            if (textEng != null)
                textEng.font = fontAsset;
            if (textNative != null)
                textNative.font = fontAsset;
        }

        public void OnTranslate()
        {
            if (!_translatable)
                return;
            SelectActiveText();
            Play();
        }

        public static void Translate(bool toNative)
        {
            OnTranslateAction?.Invoke(toNative);
            _translateState = toNative;
            Instances.ForEach(p =>
            {
                p._localTranslateState = _translateState;
                p.OnTranslate();
            });
        }

        public static void ToggleTranslate()
        {
            Translate(!_translateState);
        }

        public void ToggleLocalTranslate()
        {
            _localTranslateState = !_localTranslateState;
            OnTranslate();
        }

        public void Play()
        {
            if (CheckedTranslateState)
                OnPlayNative();
            else
                OnPlayEng();
        }

        private void OnPlayEng()
        {
            ShowEng();
        }

        private void OnPlayNative()
        {
            ShowNative();
        }

        public void SetColor(Color color)
        {
            if (textEng != null)
                textEng.color = color;
            if (textNative != null)
                textNative.color = color;
        }

        public Color GetColor()
        {
            return textEng.color;
        }

        public void ShowEng()
        {
            textEng.gameObject.SetActive(true);
            textNative.gameObject.SetActive(false);
        }

        public void ShowNative()
        {
            textEng.gameObject.SetActive(false);
            textNative.gameObject.SetActive(true);
        }
    }
}