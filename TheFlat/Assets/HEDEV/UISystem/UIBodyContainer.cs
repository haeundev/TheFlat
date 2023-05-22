using System.Collections.Generic;

namespace Proto.UISystem
{
    public class UIBodyContainer
    {
        private readonly Dictionary<int, UIWindow> _windows;

        public UIBodyContainer()
        {
            _windows = new Dictionary<int, UIWindow>();
        }

        public bool IsHasWindow(int id)
        {
            return _windows.ContainsKey(id);
        }

        public UIWindow GetWindow(int id)
        {
            return _windows.ContainsKey(id) ? _windows[id] : null;
        }

        public void RegisterWindow(int id, UIWindow window)
        {
            if (!_windows.ContainsKey(id)) _windows[id] = window;
        }

        public int RemoveWindow(int id)
        {
            _windows.Remove(id);
            return _windows.Count;
        }
    }
}