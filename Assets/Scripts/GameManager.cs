using System;
using UnityEngine;

public enum GameState
{
    Init,
    Ready,
    Playing,
    Paused,
    GameCompleted,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Init;
    public bool IsPlaying => State == GameState.Playing;
    public bool IsPaused => State == GameState.Paused;

    // Notification only. Subscribers read State when notified.
    public event Action OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (Instance == this)
            SetState(GameState.Ready);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void SetState(GameState newState)
    {
        if (State == newState)
            return;

        State = newState;
        OnGameStateChanged?.Invoke();
    }

    public void Play()
    {
        if (State == GameState.Ready)
            SetState(GameState.Playing);
    }

    public void Pause()
    {
        if (State == GameState.Playing)
            SetState(GameState.Paused);
    }

    public void Resume()
    {
        if (State == GameState.Paused)
            SetState(GameState.Playing);
    }

    public void CompleteGame()
    {
        if (State == GameState.Playing)
            SetState(GameState.GameCompleted);
    }

    public void GameOver()
    {
        if (State == GameState.Playing)
            SetState(GameState.GameOver);
    }

    public void Restart()
    {
        if (State == GameState.Paused ||
            State == GameState.GameCompleted ||
            State == GameState.GameOver)
        {
            SetState(GameState.Ready);
        }
    }
}
