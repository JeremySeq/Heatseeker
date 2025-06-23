using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public static ScoreUI Instance;
    private float _score = 0; // in seconds
    public bool started = false;
    private TextMeshProUGUI _scoreText;
    
    void Start()
    {
        // set static instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // get text component
        _scoreText = GetComponent<TextMeshProUGUI>();
        
        ScoreUI.Instance.StartTimer();
    }

    void Update()
    {

        if (started)
        {
            _score += Time.deltaTime;
        }
        
        // set score text, each second is worth 10 points
        _scoreText.text = GetScore().ToString();
    }

    public int GetScore()
    {
        return Mathf.FloorToInt(_score * 10);
    }

    public void StartTimer()
    {
        _score = 0;
        started = true;
    }
    
    public void StopTimer()
    {
        started = false;
    }
}
