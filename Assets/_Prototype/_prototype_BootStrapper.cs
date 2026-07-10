using UnityEngine;

namespace TDG0407._prototype
{
    
    public class _prototype_BootStrapper : MonoBehaviour
    {
        
        public void Start()
        {
            
            _prototype_GridManager.Instance.Initialize();

        }

    }

}