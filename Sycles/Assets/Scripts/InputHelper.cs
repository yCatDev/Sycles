using UnityEngine;

namespace SyclesInternals
{
    public static class InputHelper
    {
        public static bool IsAPressed() => Input.GetKeyDown(KeyCode.Z);
        public static bool IsBPressed() => Input.GetKeyDown(KeyCode.X);

        public static bool RegisteredKeyWasPressed() => IsAPressed() || IsBPressed();
    }
}