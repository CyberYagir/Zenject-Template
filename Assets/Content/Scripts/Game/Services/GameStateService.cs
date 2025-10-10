using System;
using UnityEngine;

namespace Content.Scripts.Game.Services
{
    public class GameStateService : MonoBehaviour
    {
        public enum EState
        {
            Game,
            Pause
        }

        [SerializeField] private EState gameState;

        public event Action<EState> OnGameModeChanged;


        public EState State => gameState;


        public void ChangeState(EState state)
        {
            if (gameState != state)
            {
                gameState = state;
                OnGameModeChanged?.Invoke(gameState);
            }
        }
    }
}
