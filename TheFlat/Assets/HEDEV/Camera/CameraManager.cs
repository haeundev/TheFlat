using System;
using UnityEngine;

namespace Proto.CameraSystem
{
    public static class CameraManager
    {
        public enum CameraType
        {
            Main,
            UI
        }

        private static Camera _main;
        private static Camera _ui;

        public static Camera Main
        {
            get
            {
                _main ??= Camera.main;
                _main ??= GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
                return _main;
            }
        }

        public static Camera UI
        {
            get
            {
                _ui ??= GameObject.FindWithTag("UICamera")?.GetComponent<Camera>();
                return _ui;
            }
        }

        public static void Reset()
        {
            _main = null;
            _ui = null;
        }

        public static Camera Get(CameraType type)
        {
            return type switch
            {
                CameraType.Main => Main,
                CameraType.UI => UI,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}