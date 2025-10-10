using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Content.Scripts.Integrations
{
    [System.Serializable]
    public class IntegrationModule
    {
        [SerializeField, ReadOnly] private bool isActive = false;
        private List<Action> callbacks = new List<Action>();

        public bool IsActive => isActive;

        public void AddCallback(Action callback)
        {
            callbacks.Add(callback);
        }


        protected virtual void ActiveModule()
        {
            if (isActive) return;
            for (int i = 0; i < callbacks.Count; i++)
            {
                callbacks[i]?.Invoke();
            }

            isActive = true;
        }
    }
}