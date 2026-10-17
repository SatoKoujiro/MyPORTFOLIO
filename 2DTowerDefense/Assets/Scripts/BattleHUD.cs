using UnityEngine;

public class BattleHUD : MonoBehaviour
{
    private GameConfig config;
    private BattleController battleController;
    private DeploymentController deploymentController;
    private Texture2D panelTexture;
    private Texture2D transparentTexture;
    private GUIStyle panelStyle;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle mapMarkerStyle;
    private GUIStyle cellButtonStyle;
    private GUIStyle selectedButtonStyle;

    public void Initialize(GameConfig gameConfig, BattleController battle, DeploymentController deployment)
    {
        config = gameConfig;
        battleController = battle;
        deploymentController = deployment;
    }

    private void OnGUI()
    {
        if (config == null || battleController == null || deploymentController == null)
        {
            return;
        }

        EnsureStyles();
        DrawBoard();
        DrawStatusBar();
        DrawOperatorBar();
        DrawResult();
    }

    private void DrawBoard()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        if (stage.mapLayout != null && stage.mapLayout.Length > 0)
        {
            DrawTileMap(camera);
        }
        else
        {
            for (int lane = 0; lane < stage.laneCount; lane++)
            {
                Vector2 left = WorldToGui(camera, new Vector3(-stage.columns * stage.cellSpacing * 0.5f, LaneY(lane) - stage.laneSpacing * 0.44f, 0f));
                Vector2 right = WorldToGui(camera, new Vector3(stage.columns * stage.cellSpacing * 0.5f, LaneY(lane) + stage.laneSpacing * 0.44f, 0f));
                Rect laneRect = Rect.MinMaxRect(left.x, left.y, right.x, right.y);
                GUI.color = lane % 2 == 0 ? new Color(0.11f, 0.17f, 0.19f, 0.5f) : new Color(0.14f, 0.2f, 0.2f, 0.5f);
                GUI.DrawTexture(laneRect, panelTexture);
                GUI.color = Color.white;
            }
        }

        for (int lane = 0; lane < stage.MapHeight; lane++)
        {
            for (int column = 0; column < stage.MapWidth; column++)
            {
                Vector3 center = deploymentController.CellPosition(column, lane);
                float cellWidth = stage.mapLayout != null && stage.mapLayout.Length > 0 ? stage.mapCellSpacing : stage.cellSpacing;
                float cellHeight = stage.mapLayout != null && stage.mapLayout.Length > 0 ? stage.mapRowSpacing : stage.laneSpacing;
                Rect cell = WorldRect(camera, center, cellWidth * 0.94f, cellHeight * 0.88f);
                UnitActor unit = deploymentController.GetUnitAt(column, lane);
                if (unit == null && !deploymentController.IsDeployableCell(column, lane))
                {
                    continue;
                }

                string label = unit == null ? "+" : RoleMarker(unit.Role);
                bool selected = unit != null && unit == deploymentController.SelectedUnit;
                bool clicked = GUI.Button(cell, GUIContent.none, cellButtonStyle);
                GUI.color = Color.white;
                if (clicked && battleController.IsRunning)
                {
                    deploymentController.HandleCellClick(column, lane);
                }

                if (selected)
                {
                    DrawCellOutline(cell, new Color(1f, 0.76f, 0.24f, 0.95f), 2f);
                }
                else if (unit == null)
                {
                    GUI.color = new Color(0.72f, 0.95f, 0.79f, 0.65f);
                    GUI.DrawTexture(new Rect(cell.xMax - 5f, cell.yMax - 5f, 3f, 3f), Texture2D.whiteTexture);
                    GUI.color = Color.white;
                }
            }
        }

        if (stage.mapLayout == null || stage.mapLayout.Length == 0)
        {
            for (int lane = 0; lane < stage.MapHeight; lane++)
            {
                Vector2 basePoint = WorldToGui(camera, deploymentController.CellPosition(0, lane) + Vector3.left * 0.55f);
                GUI.color = new Color(1f, 0.52f, 0.34f);
                GUI.Label(new Rect(basePoint.x - 32f, basePoint.y - 14f, 62f, 28f), "拠点", bodyStyle);
                GUI.color = Color.white;
            }
        }

        string actionHint = deploymentController.SelectedUnit == null
            ? "ユニットを選んでマスに配置"
            : "移動先を選択  /  選択中: " + deploymentController.SelectedUnit.DisplayName;
        GUI.Label(new Rect(Screen.width * 0.5f - 150f, 88f, 300f, 24f), actionHint, bodyStyle);
    }

    private void DrawTileMap(Camera camera)
    {
        int mapWidth = stage.MapWidth;
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

                float x = (column - (mapWidth - 1) * 0.5f) * stage.mapCellSpacing;
                float y = ((mapHeight - 1f) * 0.5f - row) * stage.mapRowSpacing;
                Rect tileRect = WorldRect(camera, new Vector3(x, y, 0f), stage.mapCellSpacing * 0.96f, stage.mapRowSpacing * 0.94f);
                GUI.color = TileColor(tile);
                GUI.DrawTexture(tileRect, panelTexture);
                GUI.color = Color.white;

                if (tile == 'B' || tile == 'E')
                {
                    GUI.Label(tileRect, tile == 'B' ? "自" : "敵", mapMarkerStyle);
                }
            }
        }
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

    private static void DrawCellOutline(Rect rect, Color color, float thickness)
    {
        GUI.color = color;
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void DrawStatusBar()
    {
        Rect bar = new Rect(16f, 10f, Screen.width - 32f, 54f);
        GUI.Box(bar, GUIContent.none, panelStyle);
        GUI.Label(new Rect(bar.x + 14f, bar.y + 5f, 190f, 22f), stage.displayName, titleStyle);
        GUI.Label(new Rect(bar.x + 14f, bar.y + 28f, 150f, 20f), "拠点 " + battleController.Lives, bodyStyle);

        string waveLabel = battleController.TotalWaves == 0
            ? "WAVE 0 / 0"
            : "WAVE " + Mathf.Min(battleController.CurrentWave + 1, battleController.TotalWaves) + " / " + battleController.TotalWaves;
        GUI.Label(new Rect(bar.x + bar.width * 0.43f, bar.y + 15f, 220f, 26f), waveLabel + "   敵 " + battleController.ActiveEnemyCount, bodyStyle);
        GUI.Label(new Rect(bar.x + bar.width - 136f, bar.y + 15f, 122f, 28f), "配置 " + deploymentController.DeploymentPoints, bodyStyle);
    }

    private void DrawOperatorBar()
    {
        float barHeight = 76f;
        Rect bar = new Rect(16f, Screen.height - barHeight - 14f, Screen.width - 32f, barHeight);
        GUI.Box(bar, GUIContent.none, panelStyle);
        if (config.operators == null)
        {
            return;
        }

        float cardGap = 8f;
        float cardWidth = Mathf.Min(152f, (bar.width - 264f - cardGap * (config.operators.Length - 1)) / config.operators.Length);
        float x = bar.x + 12f;
        for (int index = 0; index < config.operators.Length; index++)
        {
            GameConfig.OperatorDefinition definition = config.operators[index];
            Rect button = new Rect(x, bar.y + 10f, cardWidth, 56f);
            bool selected = index == deploymentController.SelectedOperatorIndex;
            GUI.backgroundColor = selected ? new Color(0.34f, 0.72f, 0.61f) : new Color(0.22f, 0.3f, 0.31f);
            if (GUI.Button(button, GUIContent.none, selected ? selectedButtonStyle : GUI.skin.button))
            {
                deploymentController.SelectOperator(index);
            }

            GUI.backgroundColor = Color.white;
            GUI.Label(new Rect(button.x + 8f, button.y + 5f, button.width - 12f, 20f), definition.displayName, bodyStyle);
            string stat = definition.role == OperatorRole.Healer
                ? "費 " + definition.deploymentCost + " 回 " + definition.healingAmount
                : "費 " + definition.deploymentCost + " 攻 " + definition.attackDamage;
            GUI.Label(new Rect(button.x + 8f, button.y + 30f, button.width - 12f, 18f), stat, bodyStyle);
            x += button.width + cardGap;
        }

        if (GUI.Button(new Rect(bar.x + bar.width - 112f, bar.y + 15f, 96f, 44f), "再戦"))
        {
            GetComponent<GameManager>().RestartBattle();
        }

        if (battleController.State == BattleState.Victory && battleController.StageIndex < config.stages.Length - 1 &&
            GUI.Button(new Rect(bar.x + bar.width - 222f, bar.y + 15f, 102f, 44f), "次のステージ"))
        {
            GetComponent<GameManager>().StartNextStage();
        }
    }

    private void DrawResult()
    {
        if (battleController.State == BattleState.Running)
        {
            return;
        }

        Rect result = new Rect(Screen.width * 0.5f - 190f, Screen.height * 0.5f - 58f, 380f, 116f);
        GUI.Box(result, GUIContent.none, panelStyle);
        GUI.Label(new Rect(result.x + 20f, result.y + 16f, result.width - 40f, 34f),
            battleController.State == BattleState.Victory ? "CLEAR" : "DEFEAT", titleStyle);
        GUI.Label(new Rect(result.x + 20f, result.y + 52f, result.width - 40f, 28f), battleController.ResultMessage, bodyStyle);
    }

    private void EnsureStyles()
    {
        if (panelStyle != null)
        {
            return;
        }

        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, new Color(0.045f, 0.075f, 0.08f, 0.94f));
        panelTexture.Apply();
        panelStyle = new GUIStyle(GUI.skin.box)
        {
            normal = { background = panelTexture }
        };
        titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 19,
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(0.86f, 0.95f, 0.9f) }
        };
        bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            normal = { textColor = Color.white }
        };
        mapMarkerStyle = new GUIStyle(bodyStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        selectedButtonStyle = new GUIStyle(GUI.skin.button);
        transparentTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        transparentTexture.SetPixel(0, 0, new Color(1f, 1f, 1f, 0f));
        transparentTexture.Apply();
        cellButtonStyle = new GUIStyle(GUI.skin.button)
        {
            normal = { background = transparentTexture },
            hover = { background = transparentTexture },
            active = { background = transparentTexture },
            focused = { background = transparentTexture }
        };
    }

    private StageDefinition stage => battleController.Stage;

    private float LaneY(int lane)
    {
        return stage.laneOffsetY + (lane - (stage.MapHeight - 1) * 0.5f) * stage.laneSpacing;
    }

    private static string RoleMarker(OperatorRole role)
    {
        switch (role)
        {
            case OperatorRole.Melee: return "近";
            case OperatorRole.Ranged: return "遠";
            case OperatorRole.Healer: return "癒";
            default: return "盾";
        }
    }

    private static Vector2 WorldToGui(Camera camera, Vector3 world)
    {
        Vector3 screen = camera.WorldToScreenPoint(world);
        return new Vector2(screen.x, Screen.height - screen.y);
    }

    private static Rect WorldRect(Camera camera, Vector3 center, float width, float height)
    {
        Vector2 topLeft = WorldToGui(camera, center + new Vector3(-width * 0.5f, height * 0.5f, 0f));
        Vector2 bottomRight = WorldToGui(camera, center + new Vector3(width * 0.5f, -height * 0.5f, 0f));
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }
}