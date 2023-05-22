using UnityEngine;

namespace Proto.UISystem
{
    public interface IUIWindowManager
    {
        void InitSubWindow(Transform window);
        void ReleaseSubWindow(Transform window);
        void Close(UIWindow window);
        void DestroyWindow(UIWindow window);
    }
}