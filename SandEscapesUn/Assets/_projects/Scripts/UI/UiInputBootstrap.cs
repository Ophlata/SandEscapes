using UnityEngine;
using UnityEngine.EventSystems;

namespace SandEscapes.UI
{
    /// <summary>
    /// Ensures an <see cref="EventSystem"/> exists for UI clicks, and temporarily unlocks the cursor for FPS-style controls.
    /// </summary>
    public static class UiInputBootstrap
    {
        static int s_UiOpenCount;

        public static void EnsureEventSystemExists()
        {
            if (EventSystem.current != null)
                return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            Object.DontDestroyOnLoad(go);
        }

        public static void PushUiCursor()
        {
            EnsureEventSystemExists();
            s_UiOpenCount++;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static void PopUiCursor()
        {
            s_UiOpenCount = Mathf.Max(0, s_UiOpenCount - 1);
            if (s_UiOpenCount > 0)
                return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
