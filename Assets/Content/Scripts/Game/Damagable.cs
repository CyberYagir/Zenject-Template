using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace Content.Scripts.Game.Units
{
    public abstract class Damagable : MonoBehaviour
    {
        [SerializeField, FoldoutGroup("Damagable")] private float health;
        [SerializeField, ReadOnly, FoldoutGroup("Damagable")] private float currentHealth;
        [SerializeField, FoldoutGroup("Damagable")] private List<Transform> bones = new List<Transform>();
        
        public float MaxHealth => health;

        public float Health => currentHealth;

        public event Action<float, float> OnHealthChanged;
        public event Action<Damagable> OnDeath;
        public event Action OnTakeDamage;
        public event Action OnHealing;


        public bool IsDead => currentHealth <= 0;
        
        public virtual void InitHealth()
        {
            currentHealth = health;
        }


        public virtual void TakeDamage(float damage)
        {
            if (IsDead) return;
            currentHealth -= damage;
            
            OnHealthChanged?.Invoke(Health, MaxHealth);
            if (damage > 0)
            {
                OnTakeDamage?.Invoke();
            }
            
            if (currentHealth <= 0)
            {
                OnDeath?.Invoke(this);
            }
        }

        public virtual void Heal(float heal)
        {
            currentHealth += heal;
            currentHealth = Mathf.Clamp(currentHealth, 0, MaxHealth);
            OnHealthChanged?.Invoke(Health, MaxHealth);
            OnHealing?.Invoke();
        }

        public virtual void SetHealth(float health)
        {
            this.health = health;
            currentHealth = health;
            OnHealthChanged?.Invoke(Health, MaxHealth);
        }

        #if UNITY_EDITOR
        [Button, FoldoutGroup("Damagable")]
        public void CollectBones()
        {
            bones.Clear();
            var renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            
            for (var i = 0; i < renderer.bones.Length; i++)
            {
                bones.Add(renderer.bones[i]);
            }
        }
        
        #endif
        
        public Transform GetRandomBone()
        {
            if (bones.Count != 0)
            {
                return bones.GetRandomItem();
            }

            return null;
        }
    }
}
