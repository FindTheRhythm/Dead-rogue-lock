using System;
using UnityEngine;

public static class GamePauseState
{
    public static bool IsPaused { get; private set; }
    public static bool ShouldDuckMusic { get; private set; }

    public static event Action<bool> OnPauseChanged;

    public static void Pause(bool duckMusic = true)
    {
        if (IsPaused)
        {
            if (ShouldDuckMusic != duckMusic)
            {
                ShouldDuckMusic = duckMusic;
                OnPauseChanged?.Invoke(true);
            }

            return;
        }

        IsPaused = true;
        ShouldDuckMusic = duckMusic;
        Time.timeScale = 0f;

        OnPauseChanged?.Invoke(true);
    }

    public static void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        ShouldDuckMusic = false;
        Time.timeScale = 1f;

        OnPauseChanged?.Invoke(false);
    }
}
