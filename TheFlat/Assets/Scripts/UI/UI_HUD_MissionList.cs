using UnityEngine;
using Proto.UISystem;

namespace UI
{
    public partial class UI_HUD_MissionList : UIWindow
    {
        private bool _isShowList;

        protected override void Awake()
        {
            base.Awake();
            ShowList(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                ShowList(!_isShowList);
            }
        }

        private void ShowList(bool isShow)
        {
            ListView.gameObject.SetActive(isShow);
            Instruction.gameObject.SetActive(!isShow);
            _isShowList = isShow;
        }
    }
}