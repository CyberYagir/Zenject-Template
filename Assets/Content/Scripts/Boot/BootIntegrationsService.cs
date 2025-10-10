using System.Collections.Generic;
using System.Linq;
using Content.Scripts.Integrations;
using Content.Scripts.Scriptable;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Boot
{
    public class BootIntegrationsService : MonoBehaviour
    {
        List<IntegrationModule> modules = new List<IntegrationModule>();
        
        private GameDataSo gameData;
        private SaveDataSo saveData;


        [Inject]
        private void Construct(GameDataSo gameData, SaveDataSo saveData)
        {
            this.saveData = saveData;
            this.gameData = gameData;

            modules = new List<IntegrationModule>
            {
                
            };
        }
        
        
        public bool IsAllModulesReady(out float percent)
        {
            var activeCount = modules.Count(x => x.IsActive);
            var isReady = activeCount >= modules.Count;
            percent = activeCount / (float)modules.Count;
            return isReady;
        }
    }
}
