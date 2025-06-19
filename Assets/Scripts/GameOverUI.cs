using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public TextMeshProUGUI gameOverScoreText;
    private CanvasGroup _canvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false); // hide initially
    }

    public void Show()
    {
        gameOverScoreText.text = ScoreUI.Instance.GetScore().ToString();
        
        gameObject.SetActive(true);
        // send score to leaderboard api
        StartCoroutine(Leaderboard.SubmitScore(ScoreUI.Instance.GetScore()));
        StartCoroutine(FadeIn());
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void RestartGame()
    {
        ScoreUI.Instance.StartTimer(); // reset score timer
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}