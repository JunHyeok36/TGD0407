using UnityEngine;

namespace TDG0407._prototype
{
    
    public class _prototype_BootStrapper : MonoBehaviour
    {
        
        public void Start()
        {
            _prototype_TickManager.Initialize();
            _prototype_GridManager.Instance.Initialize();

            if (_prototype_PlayerController.Instance != null)
            {
                _prototype_PlayerController.Instance.InitializeRuntime();
            }
            if (_prototype_PlayerUIView.Instance != null)
            {
                _prototype_PlayerUIView.Instance.UpdatePlayerInfo();
                _prototype_PlayerUIView.Instance.UpdatePlayerCardDeck();
            }
        }

    }

}