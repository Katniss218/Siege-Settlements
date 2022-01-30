using UnityEngine;

namespace SS
{
    public static class PauseManager
    {
        public static bool isPaused { get; private set; }

        public static void Pause()
        {
            isPaused = true;
            Time.timeScale = 0.0f;
        }

        public static void Unpause()
        {
            isPaused = false;
            Time.timeScale = 1.0f;
        }
    }
}