using EscapeShip.UI;
using MonoGameLibrary.Managers;
using System;

namespace EscapeShip.Misc;

public class EscapeShipGameManager : GameManager
{
    internal static EscapeShipGameManager s_instance;

    /// <summary>
    /// Gets a reference to the Core instance.
    /// </summary>
    public static EscapeShipGameManager Instance => s_instance;

    public int score = 0;

    public float time;

    public bool paused = false;

    public EscapeShipGameManager() : base()
    {
        // Ensure that multiple cores are not created.
        if (s_instance != null)
        {
            throw new InvalidOperationException($"Only a single GameManager instance can be created");
        }

        // Store reference to engine for global member access.
        s_instance = this;
    }

    public void PauseGame()
    {
        ((GameSceneUI)UIManager.Instance.currentUIEntity).PauseGame();
    }
}
