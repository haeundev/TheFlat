using System.Collections.Generic;
using UnityEngine;

namespace Proto.UISystem
{
    public class UIComponentReferences : MonoBehaviour
    {
        [SerializeField] protected List<UIWindow.UIComponent> uiComponents;
        protected List<UIWindow.UIComponent> UIComponents => uiComponents;

        protected Component GetUI(int index)
        {
            if (UIComponents == null)
                return null;
            
            return UIComponents.Count <= index ? null : UIComponents[index].component;
        }

        public void SetUIComponents(List<UIWindow.UIComponent> components)
        {
            uiComponents = components;
        }
    }
}