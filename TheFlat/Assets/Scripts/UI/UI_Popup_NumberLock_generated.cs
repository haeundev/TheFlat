//Generated file. Don't modify this script.
using UnityEngine;
using UnityEngine.UI;
using Proto.UISystem;
using System.Collections.Generic;
using TMPro;

namespace UI
{
    public partial class UI_Popup_NumberLock : UIWindow
    {
        public enum UIComponents : int
        {
            PlaceholderText,
            InputText,
            SubmitButton,
            ButtonText,
        }

        public TextMeshProUGUI PlaceholderText => GetUI(0) as TextMeshProUGUI;
        public TextMeshProUGUI InputText => GetUI(1) as TextMeshProUGUI;
        public Button SubmitButton => GetUI(2) as Button;
        public TextMeshProUGUI ButtonText => GetUI(3) as TextMeshProUGUI;
    }

}

