using UnityEngine;

namespace Content.Scripts.Game.Services
{
    public static class InputService
    {
        public static bool IsLMBDown => Input.GetMouseButtonDown(0);
        public static bool IsLMBUp => Input.GetMouseButtonUp(0);
        public static bool IsLMB => Input.GetMouseButton(0);
        
        public static Vector3 MousePosition => Input.mousePosition;
    }
}
