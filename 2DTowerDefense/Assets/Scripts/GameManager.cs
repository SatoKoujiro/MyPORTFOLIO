using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    private BattleController battleController;
    private DeploymentController deploymentController;
    private BattleHUD battleHud;
    private int currentStageIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<GameManager>() != null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        GameObject managerObject = mainCamera != null ? mainCamera.gameObject : new GameObject("Game Manager");
        managerObject.AddComponent<GameManager>();
    }

    private void Awake()
    {
        if (config == null)
        {
            config = Resources.Load<GameConfig>("GameConfig");
        }
        if (config == null)
        {
            Debug.LogError("Missing Assets/Resources/GameConfig.asset.");
            enabled = false;
            return;
        }

        if (config.stages == null || config.stages.Length == 0)
        {
            Debug.LogError("GameConfig must reference at least one stage.");
            enabled = false;
            return;
        }

        currentStageIndex = Mathf.Clamp(config.initialStageIndex, 0, config.stages.Length - 1);
        ConfigureCamera(config.stages[currentStageIndex]);

        deploymentController = gameObject.AddComponent<DeploymentController>();
        battleController = gameObject.AddComponent<BattleController>();
        battleHud = gameObject.AddComponent<BattleHUD>();

        deploymentController.Initialize(config, battleController, config.stages[currentStageIndex]);
        battleController.Initialize(config, deploymentController, currentStageIndex);
        battleHud.Initialize(config, battleController, deploymentController);
    }

    public void RestartBattle()
    {
        deploymentController.ResetDeployment();
        battleController.ResetBattle();
    }

    public bool StartNextStage()
    {
        if (battleController.State != BattleState.Victory || currentStageIndex >= config.stages.Length - 1)
        {
            return false;
        }

        currentStageIndex++;
        deploymentController.SetStage(config.stages[currentStageIndex]);
        battleController.LoadStage(currentStageIndex);
        ConfigureCamera(config.stages[currentStageIndex]);
        return true;
    }

    private static void ConfigureCamera(StageDefinition stage)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        mainCamera.orthographic = true;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        float width = stage.mapLayout != null && stage.mapLayout.Length > 0
            ? stage.MapWidth * stage.mapCellSpacing
            : stage.columns * stage.cellSpacing;
        float height = stage.mapLayout != null && stage.mapLayout.Length > 0
            ? stage.mapLayout.Length * stage.mapRowSpacing
            : stage.laneCount * stage.laneSpacing;
        float requiredWidthSize = (width + 1f) / (2f * Mathf.Max(0.1f, mainCamera.aspect));
        float requiredHeightSize = height * 0.5f + 1.2f;
        mainCamera.orthographicSize = Mathf.Max(3.4f, requiredWidthSize, requiredHeightSize);
    }
}
