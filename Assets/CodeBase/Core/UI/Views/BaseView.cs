using CodeBase.Core.UI.Views.Animations;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CodeBase.Core.UI.Views
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Canvas))]
    public abstract class BaseView : MonoBehaviour, IView
    {
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        
        [field: SerializeField] public BaseAnimationElement AnimationElement { get; private set; }
        public bool IsActive { get; private set; } = true;
        public bool IsInteractable { get; private set; }

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponent<Canvas>();
        }

        public virtual async UniTask Show()
        {
            SetActive(true);
            if (IsActive && AnimationElement)
                await AnimationElement.Show();

            IsInteractable = true;
        }

        public virtual async UniTask Hide()
        {
            IsInteractable = false;

            if (IsActive && AnimationElement)
                await AnimationElement.Hide();
            SetActive(false);
        }
        
        protected void SetActive(bool isActive)
        {
            if (IsActive == isActive) return;
            IsActive = isActive;
            if (_canvas != null) _canvas.enabled = isActive;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = isActive ? 1 : 0;
                _canvasGroup.blocksRaycasts = isActive;
                _canvasGroup.interactable = isActive;
            }

            gameObject.SetActive(isActive);
        }
        
        public virtual void HideInstantly() => SetActive(false);
        
        public virtual void Dispose()
        {
            // if (this) Destroy(gameObject);
        } 
    }
}