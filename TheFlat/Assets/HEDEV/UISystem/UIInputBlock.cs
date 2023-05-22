using UnityEngine;

namespace Proto.UISystem
{
    public class UIInputBlock : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private bool _previousBlocksRaycasts;
        private bool _alreadyCanvasGroup;

        private void Awake()
        {
            if (!gameObject.TryGetComponent(out _canvasGroup))
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            else
            {
                _alreadyCanvasGroup = true;
                _previousBlocksRaycasts = _canvasGroup.blocksRaycasts;
            }

            InputBlockService.RegisterEnableEvent(EnableInteraction);
            InputBlockService.RegisterDisableEvent(DisableInteraction);

            if (InputBlockService.IsBlock) DisableInteraction();
        }

        private void OnDestroy()
        {
            InputBlockService.RemoveEnableEvent(EnableInteraction);
            InputBlockService.RemoveDisableEvent(DisableInteraction);
        }

        public void OnOpenAndCheckedDisable()
        {
            if (InputBlockService.IsBlock) DisableInteraction();
        }

        private void DisableInteraction()
        {
            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = false;
        }

        private void EnableInteraction()
        {
            if (_canvasGroup != null)
            {
                if (_alreadyCanvasGroup)
                    _canvasGroup.blocksRaycasts = _previousBlocksRaycasts;
                else
                    _canvasGroup.blocksRaycasts = true;
            }
        }
    }
}