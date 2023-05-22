using System;
using UnityEngine;

namespace Proto.CameraSystem
{
    public class CameraFadeService : MonoBehaviour
    {
        public enum State
        {
            FadeIn,
            FadeOut,
            FadingIn,
            FadingOut
        }

        private static bool _holdFadeIn;
        private static bool _reserveFadeIn;
        private static float _reserveFadeInDuration;
        private State _state = State.FadeIn;

        private static CameraFadeService Instance { get; set; }

        public static Color Color
        {
            get => CameraFadeSystem.Color;
            set
            {
                if (Instance == null) return;
                if (Instance._state == State.FadeIn)
                    CameraFadeSystem.Color = value;
            }
        }

        public static State CurrentState => Instance == null ? State.FadeIn : Instance._state;

        private void Awake()
        {
            Instance = this;
            gameObject.AddComponent<CameraFadeSystem>();
        }

        private static event Action OnFadeIn;
        private static event Action OnFadeOut;

        public static void HoldFadeIn(float duration = 0.3f)
        {
            Debug.Log($"[camera fade log] Hold ({CurrentState})");
            _holdFadeIn = true;
            if (CurrentState == State.FadingIn)
                Out(duration);
        }

        public static void ReleaseHoldingFadeIn()
        {
            Debug.Log($"[camera fade log] Release ({CurrentState})");
            if (Instance == null) return;

            _holdFadeIn = false;
        }

        private static void FadeInReserved()
        {
            _reserveFadeIn = false;
            In(_reserveFadeInDuration);
        }

        private static void ReserveFadeIn(float duration, Action callback)
        {
            Debug.Log($"[camera fade log] Reserve ({duration}, {CurrentState})");
            _reserveFadeIn = true;
            _reserveFadeInDuration = duration;
            AddOnFadeIn(callback);
        }

        internal static void OnIn()
        {
            Instance._state = State.FadeIn;
            Color = Color.black;
            Instance.ExecuteFadeInInvoke();
        }

        internal static void OnOut()
        {
            Instance._state = State.FadeOut;
            Instance.ExecuteFadeOutInvoke();
            if (_reserveFadeIn)
            {
                if (!_holdFadeIn)
                    FadeInReserved();
                _reserveFadeIn = false;
            }
        }

        private void ExecuteFadeInInvoke()
        {
            OnFadeIn?.Invoke();
            OnFadeIn = null;
        }

        private void ExecuteFadeOutInvoke()
        {
            OnFadeOut?.Invoke();
            OnFadeOut = null;
        }

        private static void AddOnFadeIn(Action callback)
        {
            if (callback != null)
                OnFadeIn += callback;
        }

        private static void AddOnFadeOut(Action callback)
        {
            if (callback != null)
                OnFadeOut += callback;
        }

        public static void In(float duration, Action callback = null)
        {
            Debug.Log($"[camera fade log] In ({duration}, {CurrentState})");
            if (_holdFadeIn) return;
            if (Instance == null) return;

            switch (Instance._state)
            {
                case State.FadeIn:
                    callback?.Invoke();
                    break;
                case State.FadeOut:
                    AddOnFadeIn(callback);
                    CameraFadeSystem.In(duration);
                    Instance._state = State.FadingIn;
                    break;
                case State.FadingIn:
                    AddOnFadeIn(callback);
                    break;
                case State.FadingOut: // 페이드 아웃이 끝날 때 까지 기다린 후 페이드 인
                    ReserveFadeIn(duration, callback);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void Out(float duration, Action callback = null)
        {
            Debug.Log($"[camera fade log] Out ({duration}), {CurrentState}");
            if (Instance == null) return;

            switch (Instance._state)
            {
                case State.FadeIn:
                    AddOnFadeOut(callback);
                    CameraFadeSystem.Out(duration);
                    Instance._state = State.FadingOut;
                    break;
                case State.FadeOut:
                    callback?.Invoke();
                    break;
                case State.FadingIn: //페이드 인을 중단하고 페이드 아웃
                    AddOnFadeOut(callback);
                    CameraFadeSystem.Out(duration, false, true);
                    Instance._state = State.FadingOut;
                    break;
                case State.FadingOut:
                    AddOnFadeOut(callback);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}