
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NovelManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;

    [Header("Systems")]
    [SerializeField] private TypewriterEffect typewriter;
    [SerializeField] private NextIndicator nextIndicator;

    [Header("Scenario")]
    [SerializeField] private string startId = "start";

    private ScenarioLoader scenarioLoader;
    private ScenarioData currentScenario;

    private bool isChoiceShowing = false;

    private void Start()
    {
        scenarioLoader = new ScenarioLoader();

        if (!scenarioLoader.Load("scenario"))
            return;

        typewriter.OnTypingCompleted += HandleTypingCompleted;

        ShowScenario(startId);
    }

    private void OnDestroy()
    {
        if (typewriter != null)
            typewriter.OnTypingCompleted -= HandleTypingCompleted;
    }

    private void HandleTypingCompleted()
    {
        if (!isChoiceShowing)
            nextIndicator.Show();
    }

    public void OnAdvanceInput()
    {
        if (currentScenario == null || isChoiceShowing)
            return;

        if (typewriter.IsTyping)
        {
            typewriter.CompleteText();
            return;
        }

        nextIndicator.Hide();

        if (currentScenario.choices != null &&
            currentScenario.choices.Length > 0)
        {
            ShowChoices();
            return;
        }

        if (string.IsNullOrEmpty(currentScenario.nextId))
        {
            Debug.Log("エンディング終了");
            return;
        }

        ShowScenario(currentScenario.nextId);
    }

    private void ShowScenario(string id)
    {
        nextIndicator.Hide();

        currentScenario = scenarioLoader.GetScenario(id);

        if (currentScenario == null)
            return;

        nameText.text = currentScenario.name;

        typewriter.StartTyping(currentScenario.text);

        UpdateBackground(currentScenario.background);
        UpdateCharacter(currentScenario.character);
    }

    private void UpdateBackground(string imageName)
    {
        if (string.IsNullOrEmpty(imageName))
            return;

        Sprite sprite = Resources.Load<Sprite>(
            "Background/" + imageName
        );

        if (sprite != null)
            backgroundImage.sprite = sprite;
        else
            Debug.LogWarning("背景画像がありません: " + imageName);
    }

    private void UpdateCharacter(string imageName)
    {
        if (string.IsNullOrEmpty(imageName))
        {
            characterImage.enabled = false;
            return;
        }

        Sprite sprite = Resources.Load<Sprite>(
            "Characters/" + imageName
        );

        characterImage.sprite = sprite;
        characterImage.enabled = sprite != null;
    }

    private void ShowChoices()
    {
        isChoiceShowing = true;

        // 次回のステップで選択肢UIを接続する
        Debug.Log("選択肢を表示する場面です");
    }
}
