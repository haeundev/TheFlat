using System;

namespace UI
{
    public partial class UI_Popup_NumberLock_Controller : UIController
    {

        // UiWindow Create Completed Call Awake
        protected override void Awake()
        {
            Show();
            _completeWindowSetting?.Invoke(this);
        }
    }
}

