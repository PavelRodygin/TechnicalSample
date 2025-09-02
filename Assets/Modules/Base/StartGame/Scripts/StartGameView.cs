using System.Threading;
using CodeBase.Core.UI.Views;
using CodeBase.Core.UI.Views.Animations;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.StartGame.Scripts
{
    public class StartGameView : BaseView
    {
        [Header("UI Interaction Components")]
        [SerializeField] private Button continueButton;

        [Header("Progress UI Components")]
        [SerializeField] private CanvasGroup progressBarCanvasGroup;
        [SerializeField] private TMP_Text progressValueText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image progressBar;

        [Header("Dynamic UI Visuals")]
        [SerializeField] private CanvasGroup lightingCanvasGroup;
        [SerializeField] private Image stuffImage;
        [SerializeField] private Image overlay;

        [Header("Splash Screen Components")]
        [SerializeField] private TMP_Text splashTooltipsText;
        [SerializeField] private TMP_Text versionText;

        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private FlickerAnimation _flickerAnimation;
        private Sequence _sequence;

        private const string TapToContinueText = "Tap to continue";
        private const float ProgressBarAnimDuration = 0.5f;

        
        private void Start()
        {
            splashTooltipsText.transform.parent.gameObject.SetActive(true);
            _flickerAnimation = new FlickerAnimation(lightingCanvasGroup, overlay);
        }

        public void SetupEventListeners(ReactiveCommand<Unit> startCommand,
            ReadOnlyReactiveProperty<string> progressStatus,
            ReadOnlyReactiveProperty<float> exponentialProgress)
        {
            continueButton.OnClickAsObservable()
                .Subscribe(_ => startCommand.Execute(default))
                .AddTo(_disposables);

            Observable.CombineLatest(exponentialProgress, progressStatus,
                    (progress, status) => new { progress, status })
                .Subscribe(data => ReportProgress(data.progress, data.status).Forget())
                .AddTo(_disposables);
        }
        
        public void SetVersionText(string version) => versionText.text = version;

        private UniTask ReportProgress(float expProgress, string progressStatus)
        {
            progressText.text = progressStatus;
    
            return DOTween.To(() => progressBar.fillAmount, x =>
            {
                progressBar.fillAmount = x;
                progressValueText.text = $"{(int)(x * 100)}%";
                overlay.color = new Color(0, 0, 0, 1 - expProgress);
                lightingCanvasGroup.alpha = expProgress;
                stuffImage.color = new Color(1, 1, 1, Mathf.Max(.1f, expProgress));
            }, expProgress, 1f).ToUniTask();
        }

        public void SetTooltipText(string text) => splashTooltipsText.text = text;

        public void ShowAnimations(CancellationToken cancellationToken)
        {
            progressText.text = TapToContinueText;

            _sequence = DOTween.Sequence();
            _sequence.Append(progressText.transform.DOScale(1.2f, ProgressBarAnimDuration))
                     .SetLoops(-1, LoopType.Yoyo);

            progressBarCanvasGroup.DOFade(0, ProgressBarAnimDuration);

            _flickerAnimation.StartFlickering(cancellationToken).Forget();
        }

        public override UniTask Show()
        {
            base.Show().Forget();
            SetActive(true);
            progressBar.fillAmount = 0;
            return UniTask.CompletedTask;
        }

        public override void Dispose()
        {
            _disposables.Dispose();
            StopAnimation();
            base.Dispose();
        }

        private void StopAnimation()
        {
            if (_sequence != null && _sequence.IsActive())
            {
                _sequence.Kill();
                _sequence = null;
            }
        }
    }
}
