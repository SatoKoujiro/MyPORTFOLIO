using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private int scorePerBlock = 10;

    public int Score { get; private set; }

    private void Start() { UpdateDisplay(); }

    public void AddScore(int blockCount)
    {
        Score += blockCount * scorePerBlock;
        UpdateDisplay();
    }

    public void ResetScore()
    {
        Score = 0;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (scoreText != null) scoreText.text = "SCORE : " + Score;
    }
}
