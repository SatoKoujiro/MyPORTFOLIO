using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class BattleHUD : MonoBehaviour
{
    private sealed class BoardElement
    {
        public RectTransform Rect;
        public Vector3 Center;
        public float Width;
        public float Height;
    }

    private sealed class CellElement
    {
        public Image Indicator;
        public Outline SelectionOutline;
        public int Column;
        public int Lane;
    }

    private static readonly Color PanelColor = new Color(0.045f, 0.075f, 0.08f, 0.94f);
    private static readonly Color TextColor = new Color(0.86f, 0.95f, 0.9f);
    private static Font uiFont;

    private GameConfig config;
    private BattleController battleController;
    private DeploymentController deploymentController;
    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform boardRoot;
    private RectTransform statusBar;
    private RectTransform operatorBar;
    private Text stageLabel;
    private Text livesLabel;
    private Text waveLabel;
    private Text deploymentPointsLabel;
    private Text actionHintLabel;
    private Text resultTitleLabel;
    private Text resultMessageLabel;
    private RectTransform resultPanel;
    private readonly List<BoardElement> boardElements = new List<BoardElement>();
    private readonly List<CellElement> cellElements = new List<CellElement>();
    private readonly List<RectTransform> operatorCards = new List<RectTransform>();
    private readonly List<Text> operatorNames = new List<Text>();
    private readonly List<Text> operatorStats = new List<Text>();
    private readonly List<Image> operatorCardImages = new List<Image>();
    private readonly List<Button> operatorButtons = new List<Button>();
    private Button nextStageButton;
    private Button restartButton;
    private StageDefinition builtStage;
    private bool boardBuilt;
    private float previousCanvasWidth = -1f;

    public void Initialize(GameConfig gameConfig, BattleController battle, DeploymentController deployment)
    {
        config = gameConfig;
        battleController = battle;
        deploymentController = deployment;
        CreateCanvas();
        CreateHud();
        BuildBoard();
        BuildOperatorCards();
        RefreshHud();
    }

    private void LateUpdate()
    {
        if (canvas == null || battleController == null || deploymentController == null)
        {
            return;
        }

        if (builtStage != stage || !boardBuilt)
        {
            BuildBoard();
        }

        UpdateBoardPositions();
        RefreshHud();
        if (!Mathf.Approximately(previousCanvasWidth, canvasRect.rect.width))
        {
            LayoutOperatorCards();
        }
    }

    private void OnDestroy()
    {
        if (canvas != null)
        {
            Destroy(canvas.gameObject);
        }
    }

    private StageDefinition stage => battleController.Stage;

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Battle HUD Canvas", typeof(RectTransform), typeof(Canvas),
            typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;
        canvas.sortingOrder = 10;
        canvasRect = canvasObject.GetComponent<RectTransform>();

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (EventSystem.current == null)
        {
            GameObject eventSystemObject = new GameObject("Battle UI EventSystem", typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            eventSystemObject.transform.SetParent(canvas.transform, false);
            eventSystemObject.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
        else if (EventSystem.current.GetComponent<BaseInputModule>() == null)
        {
            EventSystem.current.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }
    }

    private void CreateHud()
    {
        boardRoot = CreateRect(canvasRect, "Board", Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);

        statusBar = CreatePanel("Status Bar", new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(-32f, 68f));
        stageLabel = CreateText(statusBar, "Stage", "STAGE", 22, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -5f), new Vector2(300f, 28f));
        livesLabel = CreateText(statusBar, "Lives", "", 18, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(14f, -34f), new Vector2(200f, 24f));
        waveLabel = CreateText(statusBar, "Wave", "", 21, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420f, 34f));
        deploymentPointsLabel = CreateText(statusBar, "Deployment Points", "", 21, FontStyle.Bold, TextAnchor.MiddleRight,
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(180f, 34f));

        operatorBar = CreatePanel("Operator Bar", new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(-32f, 76f));
        actionHintLabel = CreateText(canvasRect, "Action Hint", "", 20, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(680f, 30f));

        restartButton = CreateButton(operatorBar, "Restart", "再戦", new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-12f, 0f), new Vector2(96f, 44f),
            new Color(0.25f, 0.36f, 0.35f));
        restartButton.onClick.AddListener(() => GetComponent<GameManager>().RestartBattle());
        nextStageButton = CreateButton(operatorBar, "Next Stage", "次のステージ", new Vector2(1f, 0.5f),
            new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-120f, 0f), new Vector2(102f, 44f),
            new Color(0.25f, 0.36f, 0.35f));
        nextStageButton.onClick.AddListener(() => GetComponent<GameManager>().StartNextStage());

        resultPanel = CreatePanel("Battle Result", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 116f));
        resultTitleLabel = CreateText(resultPanel, "Result Title", "", 24, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(340f, 36f));
        resultMessageLabel = CreateText(resultPanel, "Result Message", "", 16, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(340f, 30f));
    }

    private void BuildBoard()
    {
        for (int i = boardRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(boardRoot.GetChild(i).gameObject);
        }
        boardElements.Clear();
        cellElements.Clear();
        builtStage = stage;
        boardBuilt = false;
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        if (stage.mapLayout == null || stage.mapLayout.Length == 0)
        {
            for (int lane = 0; lane < stage.laneCount; lane++)
            {
                float y = LaneY(lane);
                Color color = lane % 2 == 0
                    ? new Color(0.11f, 0.17f, 0.19f, 0.5f)
                    : new Color(0.14f, 0.2f, 0.2f, 0.5f);
                AddBoardImage("Lane", new Vector3(0f, y, 0f), stage.columns * stage.cellSpacing,
                    stage.laneSpacing * 0.88f, color);
                AddBoardText("Base", "拠点", new Vector3(deploymentController.CellPosition(0, lane).x - 0.55f, y, 0f),
                    62f, 28f, new Color(1f, 0.52f, 0.34f), 18);
            }
        }
        else
        {
            float mapHeight = stage.mapLayout.Length;
            for (int row = 0; row < stage.mapLayout.Length; row++)
            {
                string mapRow = stage.mapLayout[row] ?? string.Empty;
                for (int column = 0; column < mapRow.Length; column++)
                {
                    char tile = mapRow[column];
                    if (tile == ' ')
                    {
                        continue;
                    }

                    float x = (column - (stage.MapWidth - 1) * 0.5f) * stage.mapCellSpacing;
                    float y = ((mapHeight - 1f) * 0.5f - row) * stage.mapRowSpacing;
                    Vector3 center = new Vector3(x, y, 0f);
                    AddBoardImage("Tile", center, stage.mapCellSpacing * 0.96f, stage.mapRowSpacing * 0.94f, TileColor(tile));
                    if (tile == 'B' || tile == 'E')
                    {
                        AddBoardText("Map Marker", tile == 'B' ? "自" : "敵", center,
                            stage.mapCellSpacing * 0.96f, stage.mapRowSpacing * 0.94f, Color.white, 22);
                    }
                }
            }
        }

        for (int lane = 0; lane < stage.MapHeight; lane++)
        {
            for (int column = 0; column < stage.MapWidth; column++)
            {
                if (!deploymentController.IsDeployableCell(column, lane) &&
                    deploymentController.GetUnitAt(column, lane) == null)
                {
                    continue;
                }

                int cellColumn = column;
                int cellLane = lane;
                Vector3 center = deploymentController.CellPosition(column, lane);
                float width = stage.mapLayout != null && stage.mapLayout.Length > 0 ? stage.mapCellSpacing : stage.cellSpacing;
                float height = stage.mapLayout != null && stage.mapLayout.Length > 0 ? stage.mapRowSpacing : stage.laneSpacing;
                RectTransform buttonRect = CreateRect(boardRoot, "Deployment Cell", new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                Image image = buttonRect.gameObject.AddComponent<Image>();
                image.color = Color.clear;
                Button button = buttonRect.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => HandleCellClick(cellColumn, cellLane));
                Outline outline = buttonRect.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.76f, 0.24f, 0.95f);
                outline.effectDistance = new Vector2(2f, 2f);
                outline.enabled = false;
                RectTransform indicatorRect = CreateRect(buttonRect, "Empty Cell Indicator", new Vector2(1f, 0f),
                    new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-5f, 5f), new Vector2(3f, 3f));
                Image indicator = indicatorRect.gameObject.AddComponent<Image>();
                indicator.color = new Color(0.72f, 0.95f, 0.79f, 0.65f);
                boardElements.Add(new BoardElement { Rect = buttonRect, Center = center, Width = width * 0.94f, Height = height * 0.88f });
                cellElements.Add(new CellElement
                {
                    Indicator = indicator,
                    SelectionOutline = outline,
                    Column = column,
                    Lane = lane
                });
            }
        }

        RectTransform hintRect = (RectTransform)actionHintLabel.transform;
        hintRect.SetAsLastSibling();
        statusBar.SetAsLastSibling();
        operatorBar.SetAsLastSibling();
        resultPanel.SetAsLastSibling();
        UpdateBoardPositions();
        boardBuilt = true;
    }

    private void AddBoardImage(string name, Vector3 center, float width, float height, Color color)
    {
        RectTransform rect = CreateRect(boardRoot, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        rect.SetAsFirstSibling();
        rect.gameObject.AddComponent<Image>().color = color;
        boardElements.Add(new BoardElement { Rect = rect, Center = center, Width = width, Height = height });
    }

    private void AddBoardText(string name, string value, Vector3 center, float width, float height, Color color, int fontSize)
    {
        Text text = CreateText(boardRoot, name, value, fontSize, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        text.color = color;
        boardElements.Add(new BoardElement
        {
            Rect = (RectTransform)text.transform,
            Center = center,
            Width = width,
            Height = height
        });
    }

    private void BuildOperatorCards()
    {
        for (int i = operatorCards.Count - 1; i >= 0; i--)
        {
            Destroy(operatorCards[i].gameObject);
        }
        operatorCards.Clear();
        operatorNames.Clear();
        operatorStats.Clear();
        operatorCardImages.Clear();
        operatorButtons.Clear();

        if (config.operators != null)
        {
            for (int index = 0; index < config.operators.Length; index++)
            {
                int operatorIndex = index;
                RectTransform card = CreateRect(operatorBar, "Operator Card", new Vector2(0f, 0.5f),
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(152f, 56f));
                Image image = card.gameObject.AddComponent<Image>();
                image.color = new Color(0.22f, 0.3f, 0.31f);
                Button button = card.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => deploymentController.SelectOperator(operatorIndex));
                Text name = CreateText(card, "Operator Name", config.operators[index].displayName, 18,
                    FontStyle.Bold, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(0f, 1f), new Vector2(8f, -5f), new Vector2(-16f, 22f));
                Text stats = CreateText(card, "Operator Stats", "", 16, FontStyle.Normal, TextAnchor.MiddleLeft,
                    new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(8f, 4f),
                    new Vector2(-16f, 22f));
                operatorCards.Add(card);
                operatorNames.Add(name);
                operatorStats.Add(stats);
                operatorCardImages.Add(image);
                operatorButtons.Add(button);
            }
        }

        LayoutOperatorCards();
    }

    private void LayoutOperatorCards()
    {
        if (operatorBar == null)
        {
            return;
        }

        float width = operatorBar.rect.width;
        float gap = 8f;
        float available = Mathf.Max(0f, width - 264f - gap * Mathf.Max(0, operatorCards.Count - 1));
        float cardWidth = operatorCards.Count > 0
            ? Mathf.Min(152f, available / operatorCards.Count)
            : 0f;
        float x = 12f;
        for (int i = 0; i < operatorCards.Count; i++)
        {
            RectTransform card = operatorCards[i];
            card.anchoredPosition = new Vector2(x, 0f);
            card.sizeDelta = new Vector2(cardWidth, 56f);
            x += cardWidth + gap;
        }
        previousCanvasWidth = canvasRect.rect.width;
    }

    private void UpdateBoardPositions()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        for (int i = 0; i < boardElements.Count; i++)
        {
            BoardElement element = boardElements[i];
            Vector3 screenCenter = camera.WorldToScreenPoint(element.Center);
            Vector3 screenTopLeft = camera.WorldToScreenPoint(element.Center + new Vector3(-element.Width * 0.5f, element.Height * 0.5f, 0f));
            Vector3 screenBottomRight = camera.WorldToScreenPoint(element.Center + new Vector3(element.Width * 0.5f, -element.Height * 0.5f, 0f));
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenCenter, null, out Vector2 localCenter);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenTopLeft, null, out Vector2 localTopLeft);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenBottomRight, null, out Vector2 localBottomRight);
            element.Rect.anchoredPosition = localCenter;
            element.Rect.sizeDelta = new Vector2(Mathf.Abs(localBottomRight.x - localTopLeft.x),
                Mathf.Abs(localBottomRight.y - localTopLeft.y));
        }

        for (int i = 0; i < cellElements.Count; i++)
        {
            CellElement cell = cellElements[i];
            UnitActor unit = deploymentController.GetUnitAt(cell.Column, cell.Lane);
            bool selected = unit != null && unit == deploymentController.SelectedUnit;
            cell.SelectionOutline.enabled = selected;
            cell.Indicator.enabled = unit == null;
        }
    }

    private void RefreshHud()
    {
        stageLabel.text = stage.displayName;
        livesLabel.text = "拠点 " + battleController.Lives;
        string wave = battleController.TotalWaves == 0
            ? "WAVE 0 / 0"
            : "WAVE " + Mathf.Min(battleController.CurrentWave + 1, battleController.TotalWaves) + " / " + battleController.TotalWaves;
        waveLabel.text = wave + "   敵 " + battleController.ActiveEnemyCount;
        deploymentPointsLabel.text = "配置 " + deploymentController.DeploymentPoints;
        actionHintLabel.text = deploymentController.SelectedUnit == null
            ? "ユニットを選んでマスに配置"
            : "移動先を選択  /  選択中: " + deploymentController.SelectedUnit.DisplayName;

        for (int i = 0; i < operatorNames.Count; i++)
        {
            GameConfig.OperatorDefinition definition = config.operators[i];
            operatorNames[i].text = definition.displayName;
            operatorStats[i].text = definition.role == OperatorRole.Healer
                ? "費 " + definition.deploymentCost + " 回 " + definition.healingAmount
                : "費 " + definition.deploymentCost + " 攻 " + definition.attackDamage;
            bool selected = i == deploymentController.SelectedOperatorIndex;
            operatorCardImages[i].color = selected
                ? new Color(0.34f, 0.72f, 0.61f)
                : new Color(0.22f, 0.3f, 0.31f);
            operatorButtons[i].interactable = battleController.IsRunning;
        }

        restartButton.interactable = true;
        bool canAdvance = battleController.State == BattleState.Victory &&
            battleController.StageIndex < config.stages.Length - 1;
        nextStageButton.gameObject.SetActive(canAdvance);
        bool resultVisible = battleController.State != BattleState.Running;
        resultPanel.gameObject.SetActive(resultVisible);
        if (resultVisible)
        {
            resultTitleLabel.text = battleController.State == BattleState.Victory ? "CLEAR" : "DEFEAT";
            resultMessageLabel.text = battleController.ResultMessage;
        }
    }

    private void HandleCellClick(int column, int lane)
    {
        if (battleController.IsRunning)
        {
            deploymentController.HandleCellClick(column, lane);
        }
    }

    private RectTransform CreatePanel(string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        RectTransform rect = CreateRect(canvasRect, name, anchorMin, anchorMax, pivot, position, size);
        rect.gameObject.AddComponent<Image>().color = PanelColor;
        return rect;
    }

    private Button CreateButton(RectTransform parent, string name, string label, Vector2 anchorMin,
        Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, Color color)
    {
        RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax, pivot, position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        CreateText(rect, "Label", label, 18, FontStyle.Bold, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        return button;
    }

    private static Text CreateText(Transform parent, string name, string value, int fontSize, FontStyle fontStyle,
        TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size)
    {
        RectTransform rect = CreateRect(parent, name, anchorMin, anchorMax, pivot, position, size);
        Text text = rect.gameObject.AddComponent<Text>();
        if (uiFont == null)
        {
            uiFont = Font.CreateDynamicFontFromOSFont(new[] { "Yu Gothic", "Meiryo", "MS Gothic" }, 32);
        }
        text.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = TextColor;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static RectTransform CreateRect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 position, Vector2 size)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        item.transform.SetParent(parent, false);
        RectTransform rect = item.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private float LaneY(int lane)
    {
        return stage.laneOffsetY + (lane - (stage.MapHeight - 1) * 0.5f) * stage.laneSpacing;
    }

    private static Color TileColor(char tile)
    {
        switch (tile)
        {
            case '#': return new Color(0.04f, 0.07f, 0.08f, 0.46f);
            case 'B': return new Color(0.16f, 0.62f, 0.39f, 0.78f);
            case 'E': return new Color(0.78f, 0.24f, 0.17f, 0.78f);
            default: return new Color(0.22f, 0.29f, 0.27f, 0.28f);
        }
    }
}
