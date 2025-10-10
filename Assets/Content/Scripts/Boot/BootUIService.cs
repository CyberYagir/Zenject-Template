using System.Collections.Generic;
using Content.Scripts.Services;
using Content.Scripts.UI;
using MEC;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Boot
{
    public class BootUIService : MonoBehaviour
    {
        [SerializeField] private UIBar bar;
        private BootIntegrationsService bootIntegrationsService;
        private ScenesService scenesService;

        [Inject]
        private void Construct(BootIntegrationsService bootIntegrationsService, ScenesService scenesService)
        {
            this.scenesService = scenesService;
            this.bootIntegrationsService = bootIntegrationsService;

            Timing.RunCoroutine(WaitForIntegrations());
        }


        IEnumerator<float> WaitForIntegrations()
        {
            bar.DrawBar(0, "0%", false);
            var lastPercentage = 0f;
            yield return Timing.WaitForOneFrame;
            while (!bootIntegrationsService.IsAllModulesReady(out float percentage))
            {
                yield return Timing.WaitForOneFrame;
                if (lastPercentage < percentage)
                {
                    bar.DrawBar(percentage, (percentage / 100f).ToString("F0"), true);

                    lastPercentage = percentage;
                }
            }
            bar.DrawBar(1f, "100%", true);

            yield return Timing.WaitForSeconds(1f);

            scenesService.FadeScene(ESceneName.Game);

        }
    }
}
