using System;

public class EventStorrage
{
    public static bool playerHealthChanged;
    public static Action OnPlayerHealthChanged;

    public static void Update()
    {

        if (playerHealthChanged)
        {
            playerHealthChanged = false;
            OnPlayerHealthChanged?.Invoke();
        }
    }
}
