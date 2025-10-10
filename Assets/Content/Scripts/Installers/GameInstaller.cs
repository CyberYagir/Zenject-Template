using Content.Scripts.BoatGame.Services;
using Content.Scripts.Game;

namespace Content.Scripts.Installers
{
    public class GameInstaller : MonoBinder
    {
        public override void InstallBindings()
        {
            BindService(new PrefabSpawnerFabric(Container));
        }
    }
}
