using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Tooltip("UI Text element to display the score")]
    public Text scoreText;

    [Tooltip("Base points awarded per hit")]
    public int basePoints = 100;

    [Tooltip("Extra points per meter of distance from the player")]
    public float distanceMultiplier = 20f;

    public int CurrentScore { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddScore(Vector3 targetPosition)
    {
        float distance = Vector3.Distance(Camera.main.transform.position, targetPosition);
        int points = basePoints + Mathf.RoundToInt(distance * distanceMultiplier);
        CurrentScore += points;
        UpdateDisplay();

        Debug.Log($"Score: +{points} pts (dist: {distance:F1}m) | Total: {CurrentScore}");
    }

    void UpdateDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {CurrentScore}";
        }
    }
}
