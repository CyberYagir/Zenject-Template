using System.Collections.Generic;
using UnityEngine;

namespace Content.Scripts.Pool
{
    [System.Serializable]
    public class Pool<T> where T : Object, IPoolItem
    {
        [SerializeField] private T prefab;
        [SerializeField] private int startCount;

        private List<T> pool = new List<T>(50);
        private List<T> outOfPool = new List<T>(50);
        private Transform holder;

        public virtual void Init(Transform holder = null)
        {
            this.holder = holder;
            if (pool.Count == 0 && outOfPool.Count == 0)
            {
                for (int i = 0; i < startCount; i++)
                {
                    CreateItem();
                }
            }
        }

        public virtual T Pop()
        {
            if (pool.Count == 0)
            {
                CreateItem();
            }

            var item = pool[0];
            pool.RemoveAt(0);
            outOfPool.Add(item);
            item.Activate();
            return item;
        }

        protected virtual void CreateItem()
        {
            T obj = Object.Instantiate(prefab, holder);
            obj.Sleep();
            pool.Add(obj);
        }


        public virtual void CollectAll()
        {
            outOfPool.RemoveAll(x => x == null);
            pool.RemoveAll(x => x == null);

            while (outOfPool.Count != 0)
            {
                Push(outOfPool[0]);
            }
        }
        
        public virtual void Push(T obj)
        {
            outOfPool.Remove(obj);
            pool.Add(obj);
            obj.Sleep();
        }
    }
}