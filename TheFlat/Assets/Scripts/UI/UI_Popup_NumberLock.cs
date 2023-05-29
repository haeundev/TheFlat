using System;
using Proto.SoundSystem;
using Proto.UISystem;
using TMPro;
using UnityEngine;

namespace UI
{
    public partial class UI_Popup_NumberLock : UIWindow
    {
        public Action OnUnlockQuizSuccess;
        public string answerText = "1234";
        private FirstPersonController _firstPersonController;
        [SerializeField] private TMP_InputField inputField;

        protected override void Awake()
        {
            base.Awake();
            SubmitButton.onClick.AddListener(OnSubmitButtonClick);
            _firstPersonController = FindObjectOfType<FirstPersonController>();
            // _firstPersonController.cameraCanMove = false;
            _firstPersonController.playerCanMove = false;
            ButtonText.text = "Submit";
        }

        private void OnSubmitButtonClick()
        {
            StartCoroutine(SoundService.PlaySimple("Menu_Buttons_3", 
                _firstPersonController.gameObject.GetComponentInChildren<AudioSource>()));
            if (InputText.text.StartsWith(answerText))
                OnTextMatch();
        }

        private void OnTextMatch()
        {
            OnUnlockQuizSuccess?.Invoke();
        }

        protected override void OnDestroy()
        {
            // _firstPersonController.cameraCanMove = true;
            _firstPersonController.playerCanMove = true;
            base.OnDestroy();
        }
    }
}