using System;
using UnityEngine;

namespace TDG0407._prototype
{
    
    public class _prototype_CardView : MonoBehaviour
    {
        
        private _prototype_CardData _cardData;

        public void Initialize(_prototype_CardData cardData)
        {
            _cardData = cardData ?? throw new Exception("Card Data is not assigned.");
        }

    }

}