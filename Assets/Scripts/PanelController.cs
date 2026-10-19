using UnityEngine;

public class PanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject playingPanel;
    [SerializeField] private GameObject pausedPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameCompletedPanel;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("PanelController requires an active GameManager.", this);
            return;
        }

        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        HandleGameStateChanged();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged()
    {
        if (GameManager.Instance == null)
            return;

        GameState state = GameManager.Instance.State;
        SetPanel(mainMenuPanel, state == GameState.Ready);
        SetPanel(playingPanel, state == GameState.Playing);
        SetPanel(pausedPanel, state == GameState.Paused);
        SetPanel(gameOverPanel, state == GameState.GameOver);
        SetPanel(gameCompletedPanel, state == GameState.GameCompleted);
    }

    private static void SetPanel(GameObject panel, bool visible)
    {
        if (panel != null && panel.activeSelf != visible)
            panel.SetActive(visible);
    }
}
