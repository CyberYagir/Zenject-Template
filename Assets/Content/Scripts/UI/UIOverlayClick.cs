using System.Collections.Generic;
using System.Linq;
using Content.Scripts.Game.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Content.Scripts.UI
{
    public class UIOverlayClick
    {
        private List<RaycastResult> results = new List<RaycastResult>(10);
        private List<GraphicRaycaster> graphicRaycasters = new List<GraphicRaycaster>();
        public UIOverlayClick(Transform transform)
        {
            graphicRaycasters = transform.GetComponentsInChildren<GraphicRaycaster>(true).ToList();
        }

        public bool IsCanClick()
        {
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = InputService.MousePosition
            };
            for (int i = 0; i < graphicRaycasters.Count; i++)
            {
                results.Clear();
                graphicRaycasters[i].Raycast(pointerEventData, results);
                if (results.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}