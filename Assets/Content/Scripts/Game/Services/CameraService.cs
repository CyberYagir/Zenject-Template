using Content.Scripts.Services;
using UnityEngine;

namespace Content.Scripts.Game.Services
{
    public class CameraService : MonoBehaviour, ICameraService
    {
        [SerializeField] private Camera camera;
        public Camera Camera => camera;
    }
}
