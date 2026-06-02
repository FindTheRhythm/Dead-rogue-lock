using UnityEngine;
using UnityEngine.InputSystem;

public static class InputModeManager
{
    private static GameInput input;

    public static void Init(GameInput gameInput)
    {
        input = gameInput;
    }

    public static void SetPaused(bool isPaused)
    {
        if (input == null)
            return;

        if (isPaused)
        {
            input.Player.Disable();
            input.UI.Enable();
        }
        else
        {
            input.UI.Disable();
            input.Player.Enable();
        }
    }
}