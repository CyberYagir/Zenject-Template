using Zenject;

namespace Content.Scripts.Game
{
    public class MonoBinder : MonoInstaller
    {
        protected T BindService<T>()
        {
            var service = GetComponentInChildren<T>();
            print(typeof(T).Name + " service <b>binded</b>");
            Container.Bind<T>().FromInstance(service).AsSingle();

            return service;
        }
        
        protected void BindService<T>(T instance)
        {
            print(typeof(T).Name + " service <b>binded</b>");
            Container.Bind<T>().FromInstance(instance).AsSingle();
        }
    }
}