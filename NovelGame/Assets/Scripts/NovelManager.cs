
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class NovelManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image characterImage;
    [SerializeField] private GameObject choicePanel;

    [Header("Systems")]
    [SerializeField] private TypewriterEffect typewriter;
    [SerializeField] private NextIndicator nextIndicator;

    [Header("Scenario")]
    [SerializeField] private string startId = "start";

    private ScenarioLoader scenarioLoader;
    private ScenarioData currentScenario;

    private Button[] choiceButtons;
    private TMP_Text[] choiceLabels;
    private bool isChoiceShowing = false;

    private void Start()
    {
        scenarioLoader = new ScenarioLoader();

        if (!scenarioLoader.Load("scenario"))
            return;

        InitializeChoiceUI();
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
        isChoiceShowing = false;

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
        if (choicePanel == null || choiceButtons == null || choiceLabels == null)
        {
            Debug.LogError("Choice UI is not initialized.");
            return;
        }

        if (currentScenario.choices.Length > choiceButtons.Length)
        {
            Debug.LogError(
                "Choice count exceeds the number of Canvas buttons: " +
                currentScenario.choices.Length
            );
            return;
        }

        isChoiceShowing = true;

        float firstButtonY = (currentScenario.choices.Length - 1) * 44f;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool hasChoice = i < currentScenario.choices.Length;
            Button button = choiceButtons[i];
            button.gameObject.SetActive(hasChoice);
            button.interactable = hasChoice;

            if (!hasChoice)
                continue;

            choiceLabels[i].text = currentScenario.choices[i].text;

            RectTransform buttonRect = button.transform as RectTransform;
            if (buttonRect != null)
            {
                buttonRect.anchoredPosition =
                    new Vector2(0f, firstButtonY - i * 88f);
                buttonRect.sizeDelta = new Vector2(560f, 72f);
            }
        }

        choicePanel.SetActive(true);

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(choiceButtons[0].gameObject);
    }

    private void InitializeChoiceUI()
    {
        if (choicePanel == null)
        {
            Debug.LogError("ChoicePanel is not assigned on NovelManager.");
            return;
        }

        choicePanel.SetActive(false);
        choiceButtons = choicePanel.GetComponentsInChildren<Button>(true);

        if (choiceButtons.Length == 0)
        {
            Debug.LogError("No Canvas buttons were found in ChoicePanel.");
            return;
        }

        choiceLabels = new TMP_Text[choiceButtons.Length];

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int choiceIndex = i;
            choiceLabels[i] =
                choiceButtons[i].GetComponentInChildren<TMP_Text>(true);

            if (choiceLabels[i] == null)
            {
                Debug.LogError(
                    "Choice button has no TextMeshPro label: " +
                    choiceButtons[i].name
                );
                choiceButtons = null;
                choiceLabels = null;
                return;
            }

            choiceButtons[i].onClick.AddListener(
                () => SelectChoice(choiceIndex)
            );
        }
    }

    private void SelectChoice(int index)
    {
        if (!isChoiceShowing || currentScenario == null ||
            index < 0 || index >= currentScenario.choices.Length)
        {
            return;
        }

        string nextId = currentScenario.choices[index].nextId;
        if (string.IsNullOrEmpty(nextId))
        {
            Debug.LogError("Choice nextId is empty.");
            return;
        }

        isChoiceShowing = false;

        EventSystem eventSystem = EventSystem.current;
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(null);

        choicePanel.SetActive(false);
        ShowScenario(nextId);
    }
}
