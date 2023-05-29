//Generated file. Don't modify this script.
using UnityEngine;
using UnityEngine.UI;
using Proto.UISystem;
using System.Collections.Generic;
using TMPro;

namespace UI
{
    public partial class UI_HUD_MissionList : UIWindow
    {
        public enum UIComponents : int
        {
            Instruction,
            ListView,
        }

        public TextMeshProUGUI Instruction => GetUI(0) as TextMeshProUGUI;
        public UIGameObject ListView => GetUI(1) as UIGameObject;
    }

}

