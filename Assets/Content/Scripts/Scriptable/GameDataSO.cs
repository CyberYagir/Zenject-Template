using UnityEngine;
using Zenject;

namespace Content.Scripts.Scriptable
{
    [CreateAssetMenu(menuName = "SO/GameDataSO", fileName = "GameDataSO", order = 0)]
    public class GameDataSo : ScriptableObjectInstaller
    {
        
        public override void InstallBindings()
        {
            Container.Bind<GameDataSo>().FromInstance(this).AsSingle().NonLazy();
        }
    }
}
