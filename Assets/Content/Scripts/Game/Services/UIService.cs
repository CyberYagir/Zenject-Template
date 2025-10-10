using Content.Scripts.UI;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Game.Services
{
    public class UIService : MonoBehaviour
    {
        
        private UIOverlayClick uiOverlayClick;



        [Inject]
        private void Construct()
        {
            uiOverlayClick = new UIOverlayClick(transform);
            
            
        }
    }
}
