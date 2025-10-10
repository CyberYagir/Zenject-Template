using Content.Scripts.Game;
using Content.Scripts.Services;

namespace Content.Scripts.Installers
{
    public class ProjectInstaller : MonoBinder
    {
        public override void InstallBindings()
        {
            BindService<ScenesService>();
        }
    }
}
