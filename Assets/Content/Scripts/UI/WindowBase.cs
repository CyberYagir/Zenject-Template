using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Content.Scripts.UI
{
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField, FoldoutGroup("Window")] private Canvas canvas;
        [SerializeField, FoldoutGroup("Window")] private CanvasGroup background;
        [SerializeField, FoldoutGroup("Window")] private Transform window;
        [SerializeField, FoldoutGroup("Window")] private bool isUnscaled;

        private bool isModalOpening;
        private bool isAnimated;

        public bool IsAnimated => isAnimated;

        public bool IsOpened => canvas.enabled;

        public event Action OnShow;
        public event Action OnShowEnd;
        public event Action OnHide;
        public event Action OnHideEnd;

        protected virtual void InitWindow()
        {
            canvas.enabled = false;
            var button = background.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(Close);
            }
        }

        protected virtual void Open()
        {
            isAnimated = true;
            canvas.enabled = true;
            var scale = window.ScaleFromZero(0.25f);
            var alpha = background.AlphaFromZero(0.25f);
            alpha.onComplete += delegate
            {
                isAnimated = false;
                OnShowEnd?.Invoke();
            };

            if (isUnscaled)
            {
                scale.SetUpdate(true);
                alpha.SetUpdate(true);
            }

            scale.Play();
            alpha.Play();
            
            OnShow?.Invoke();
        }

        protected virtual void Close()
        {
            if (isModalOpening) return;

            isAnimated = true;
            var scale = background.AlphaToZero(0.25f);
            var alpha = window.ScaleToZero(0.25f);

            alpha.onComplete += delegate
            {
                isAnimated = false;
                canvas.enabled = false;
                OnHideEnd?.Invoke();
            };
            
            if (isUnscaled)
            {
                scale.SetUpdate(true);
                alpha.SetUpdate(true);
            }

            scale.Play();
            alpha.Play();
            
            OnHide?.Invoke();
        }

        public void SetModalOpening(bool isModalOpening)
        {
            this.isModalOpening = isModalOpening;
        }
    }
}