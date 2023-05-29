//This is Auto Generated Code, Don't modify this script.
using System;
using Proto.UISystem;

namespace UI
{
    //This is Auto Generated Code
    public partial class UI_Popup_NumberLock_Controller : UIController
    {

        public override System.Type UIWindowType => typeof(UI.UI_Popup_NumberLock);
        public UI.UI_Popup_NumberLock Window => window as UI.UI_Popup_NumberLock;
        public override void SetWindowOption(int id, Action<UIController> completeWindowSetting, WindowOption option)
        {
            base.SetWindowOption(id, completeWindowSetting, option);
           CreateWindow<UI.UI_Popup_NumberLock>();
        }
    }
}

