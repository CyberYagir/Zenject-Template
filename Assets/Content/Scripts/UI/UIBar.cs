using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Content.Scripts.UI
{
    public class UIBar : MonoBehaviour
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform value;
        [SerializeField] private RectTransform secondValue;
        [SerializeField] private TMP_Text text;

        public RectTransform Root => root;

        public void DrawBar(float percent, string text, bool animated = true)
        {
            if (this.text != null)
            {
                this.text.text = text;
            }

            value
                .DOSizeDelta(new Vector2(root.sizeDelta.x * percent, value.sizeDelta.y), animated ? 0.25f : 0f)
                .SetLink(value.gameObject);

            if (secondValue != null)
            {
                secondValue
                    .DOSizeDelta(new Vector2(root.sizeDelta.x * percent, value.sizeDelta.y), animated ? 0.25f : 0)
                    .SetDelay(0.25f)
                    .SetLink(secondValue.gameObject);
            }
        }
    }
}
