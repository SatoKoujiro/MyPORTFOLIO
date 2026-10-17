using System.Collections.Generic;
using UnityEngine;

public class DeploymentController : MonoBehaviour
{
    private GameConfig config;
    private BattleController battleController;
    private StageDefinition stage;
    private UnitActor[,] deployedUnits;
    private readonly List<UnitActor> unitList = new List<UnitActor>();
    private float deploymentPointTimer;

    public int DeploymentPoints { get; private set; }
    public int SelectedOperatorIndex { get; private set; }
    public UnitActor SelectedUnit { get; private set; }
    public IReadOnlyList<UnitActor> Units => unitList;
    public StageDefinition Stage => stage;

    public void Initialize(GameConfig gameConfig, BattleController battle, StageDefinition initialStage)
    {
        config = gameConfig;
        battleController = battle;
        stage = initialStage;
        deployedUnits = new UnitActor[stage.MapWidth, stage.MapHeight];
        DeploymentPoints = config.startingDeploymentPoints;
        SelectedOperatorIndex = config.operators != null && config.operators.Length > 0 ? 0 : -1;
    }

    private void Update()
    {
        if (battleController == null || !battleController.IsRunning || DeploymentPoints >= config.maximumDeploymentPoints)
        {
            return;
        }

        deploymentPointTimer += Time.deltaTime;
        if (deploymentPointTimer >= config.deploymentPointInterval)
        {
            deploymentPointTimer -= config.deploymentPointInterval;
            DeploymentPoints = Mathf.Min(config.maximumDeploymentPoints, DeploymentPoints + 1);
        }
    }

    public void SelectOperator(int index)
    {
        if (config.operators != null && index >= 0 && index < config.operators.Length)
        {
            SelectedOperatorIndex = index;
            SelectedUnit = null;
        }
    }

    public bool HandleCellClick(int column, int lane)
    {
        if (!battleController.IsRunning || !IsDeployableCell(column, lane))
        {
            return false;
        }

        UnitActor occupant = deployedUnits[column, lane];
        if (SelectedUnit != null)
        {
            if (occupant == SelectedUnit)
            {
                SelectedUnit = null;
                return true;
            }

            if (occupant == null)
            {
                MoveSelectedUnit(column, lane);
                return true;
            }
        }

        if (occupant != null)
        {
            SelectedUnit = occupant;
            return true;
        }

        return TryDeploy(column, lane);
    }

    public bool TryDeploy(int column, int lane)
    {
        if (!battleController.IsRunning || SelectedOperatorIndex < 0 || !IsDeployableCell(column, lane) || deployedUnits[column, lane] != null)
        {
            return false;
        }

        GameConfig.OperatorDefinition definition = config.operators[SelectedOperatorIndex];
        if (DeploymentPoints < definition.deploymentCost || config.operatorPrefab == null)
        {
            return false;
        }

        Vector3 position = CellPosition(column, lane);
        GameObject instance = Instantiate(config.operatorPrefab, position, Quaternion.identity);
        UnitActor unit = instance.GetComponent<UnitActor>();
        if (unit == null)
        {
            Debug.LogError("Operator prefab must have a UnitActor component.");
            Destroy(instance);
            return false;
        }

        unit.Initialize(definition, new Vector2Int(column, lane));
        deployedUnits[column, lane] = unit;
        unitList.Add(unit);
        battleController.RegisterUnit(unit);
        DeploymentPoints -= definition.deploymentCost;
        return true;
    }

    private void MoveSelectedUnit(int column, int lane)
    {
        UnitActor unit = SelectedUnit;
        if (unit == null)
        {
            return;
        }

        Vector2Int oldCell = unit.GridPosition;
        deployedUnits[oldCell.x, oldCell.y] = null;
        deployedUnits[column, lane] = unit;
        unit.Relocate(new Vector2Int(column, lane), CellPosition(column, lane));
        SelectedUnit = null;
    }

    public UnitActor GetUnitAt(int column, int lane)
    {
        if (deployedUnits == null || column < 0 || column >= stage.MapWidth || lane < 0 || lane >= stage.MapHeight)
        {
            return null;
        }

        return deployedUnits[column, lane];
    }

    public Vector3 CellPosition(int column, int lane)
    {
        if (stage.mapLayout != null && stage.mapLayout.Length > 0)
        {
            float mapX = (column - (stage.MapWidth - 1) * 0.5f) * stage.mapCellSpacing;
            float mapY = ((stage.MapHeight - 1f) * 0.5f - lane) * stage.mapRowSpacing;
            return new Vector3(mapX, mapY, 0f);
        }

        float x = (column - (stage.columns - 1) * 0.5f) * stage.cellSpacing;
        float y = stage.laneOffsetY + (lane - (stage.MapHeight - 1) * 0.5f) * stage.laneSpacing;
        return new Vector3(x, y, 0f);
    }

    public bool IsDeployableCell(int column, int lane)
    {
        if (column < 0 || column >= stage.MapWidth || lane < 0 || lane >= stage.MapHeight)
        {
            return false;
        }

        if (stage.mapLayout == null || stage.mapLayout.Length == 0)
        {
            return true;
        }

        string row = stage.mapLayout[lane];
        if (row == null || column >= row.Length)
        {
            return false;
        }

        char tile = row[column];
        return tile != '#' && tile != ' ' && tile != 'B' && tile != 'E';
    }

    public void SetStage(StageDefinition newStage)
    {
        stage = newStage;
        ResetDeployment();
    }

    public void ResetDeployment()
    {
        for (int index = 0; index < unitList.Count; index++)
        {
            if (unitList[index] != null)
            {
                Destroy(unitList[index].gameObject);
            }
        }

        unitList.Clear();
        deployedUnits = new UnitActor[stage.MapWidth, stage.MapHeight];
        DeploymentPoints = config.startingDeploymentPoints;
        deploymentPointTimer = 0f;
        SelectedUnit = null;
        SelectedOperatorIndex = config.operators != null && config.operators.Length > 0 ? 0 : -1;
    }
}