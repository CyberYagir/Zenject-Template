using Content.Scripts.Boot;
using Content.Scripts.Game;

namespace Content.Scripts.Installers
{
    public class BootInstaller : MonoBinder
    {
        public override void InstallBindings()
        {
            BindService<BootIntegrationsService>();
            BindService<BootUIService>();
        }
    }
}
