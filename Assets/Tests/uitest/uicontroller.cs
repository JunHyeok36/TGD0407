using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

[RequireComponent(typeof(PanelRenderer))]
public class uicontroller : MonoBehaviour
{
    private VisualElement _bottomContainer;
    private VisualElement _scrim;
    private VisualElement _bottomSheet;
    private VisualElement _boy;
    private VisualElement _warning;
    private Label _message;
    private Tweener _message_tweener;
    private Button _openButton;
    private Button _closeButton;

    void Awake()
    {
        GetComponent<PanelRenderer>().RegisterUIReloadCallback(OnUIReload);
    }

    void OnDestroy()
    {
        GetComponent<PanelRenderer>().UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer panelRenderer, VisualElement root)
    {
        _bottomContainer = root.Q<VisualElement>("container_bottom");
        _openButton = root.Q<Button>("open_btn");
        _closeButton = root.Q<Button>("close_btn");
        
        _scrim = root.Q<VisualElement>("scrim");
        _bottomSheet = root.Q<VisualElement>("bottomsheet");
        _boy = root.Q<VisualElement>("image_boy");
        _warning = root.Q<VisualElement>("image_warning");
        _message = root.Q<Label>("message");

        _bottomSheet.RegisterCallback<TransitionEndEvent>(evt =>
        {
            if (evt.target == _bottomSheet && !_bottomSheet.ClassListContains("bottomsheet--up"))
            {
                _bottomContainer.style.display = DisplayStyle.None;
                if(_message_tweener != null && _message_tweener.IsActive()) _message_tweener.Kill();
            }
        });

        _openButton.RegisterCallback<ClickEvent>(evt =>
        {
            _bottomContainer.style.display = DisplayStyle.Flex;
            _bottomSheet.AddToClassList("bottomsheet--up");
            _scrim.AddToClassList("scrim--fadein");

            _warning.ToggleInClassList("image-warning--up");
            _warning.RegisterCallback<TransitionEndEvent>(OnWarningTransitionEnd);

            void OnWarningTransitionEnd(TransitionEndEvent e)
            {
                _warning.UnregisterCallback<TransitionEndEvent>(OnWarningTransitionEnd);
                _warning.ToggleInClassList("image-warning--up");
            }
            
            _message.text = string.Empty; 
            string str = "\"Sed in rebus apertissimis nimiym longi sumus.\"";
            _message_tweener = DOTween.To(() => _message.text, x => _message.text = x, str, 3f).SetEase(Ease.Linear);
        });
        _closeButton.RegisterCallback<ClickEvent>(evt =>
        {
            _bottomSheet.RemoveFromClassList("bottomsheet--up");
            _scrim.RemoveFromClassList("scrim--fadein");
        });

        BeginFirstAnimation().Forget();

        async UniTaskVoid BeginFirstAnimation()
        {
            await UniTask.Yield();
            _boy.RemoveFromClassList("image-boy--inair");
        }
    }

}
