//This is Auto Generated Code, Don't modify this script.
using System;
using Proto.UISystem;

namespace UI
{
    //This is Auto Generated Code
    public partial class UI_HUD_MissionList_Controller : UIController
    {

        public override System.Type UIWindowType => typeof(UI.UI_HUD_MissionList);
        public UI.UI_HUD_MissionList Window => window as UI.UI_HUD_MissionList;
        public override void SetWindowOption(int id, Action<UIController> completeWindowSetting, WindowOption option)
        {
            base.SetWindowOption(id, completeWindowSetting, option);
           CreateWindow<UI.UI_HUD_MissionList>();
        }
    }
}

