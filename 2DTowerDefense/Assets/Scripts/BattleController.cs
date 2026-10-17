using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Running,
    Defeat,
    Victory
}

public class BattleController : MonoBehaviour
{
    private GameConfig config;
    private DeploymentController deploymentController;
    private StageDefinition stage;
    private readonly List<EnemyActor> enemies = new List<EnemyActor>();
    private readonly List<UnitActor> units = new List<UnitActor>();
    private int lives;
    private int currentWaveIndex;
    private int spawnedInWave;
    private int totalSpawned;
    private float spawnTimer;
    private float waveTransitionTimer;

    public BattleState State { get; private set; }
    public int Lives => lives;
    public int CurrentWave => currentWaveIndex;
    public int TotalWaves => stage != null && stage.waves != null ? stage.waves.Length : 0;
    public int StageIndex { get; private set; }
    public StageDefinition Stage => stage;
    public int ActiveEnemyCount => enemies.Count;
    public bool IsRunning => State == BattleState.Running;
    public string ResultMessage { get; private set; }

    public void Initialize(GameConfig gameConfig, DeploymentController deployment, int initialStageIndex)
    {
        config = gameConfig;
        deploymentController = deployment;
        LoadStage(initialStageIndex);
    }

    private void Update()
    {
        if (!IsRunning)
        {
            return;
        }

        UpdateSpawning();
        UpdateUnits();
        UpdateEnemies();
        CheckForVictory();
    }

    public void RegisterUnit(UnitActor unit)
    {
        if (unit != null && !units.Contains(unit))
        {
            units.Add(unit);
        }
    }

    public void ResetBattle()
    {
        LoadStage(StageIndex);
    }

    public void LoadStage(int stageIndex)
    {
        for (int index = 0; index < enemies.Count; index++)
        {
            if (enemies[index] != null)
            {
                Destroy(enemies[index].gameObject);
            }
        }

        enemies.Clear();
        units.Clear();
        StageIndex = Mathf.Clamp(stageIndex, 0, config.stages.Length - 1);
        stage = config.stages[StageIndex];
        lives = config.startingLives;
        currentWaveIndex = 0;
        spawnedInWave = 0;
        totalSpawned = 0;
        spawnTimer = 1f;
        waveTransitionTimer = 0f;
        ResultMessage = string.Empty;
        State = BattleState.Running;
    }

    private void UpdateSpawning()
    {
        if (stage.waves == null || currentWaveIndex >= stage.waves.Length)
        {
            return;
        }

        if (spawnedInWave == 0 && currentWaveIndex > 0)
        {
            waveTransitionTimer += Time.deltaTime;
            if (waveTransitionTimer < config.timeBetweenWaves)
            {
                return;
            }
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f)
        {
            return;
        }

        GameConfig.WaveDefinition wave = stage.waves[currentWaveIndex];
        SpawnEnemy(wave, spawnedInWave);
        spawnedInWave++;
        totalSpawned++;
        waveTransitionTimer = 0f;

        if (spawnedInWave >= GetWaveEnemyCount(wave))
        {
            currentWaveIndex++;
            spawnedInWave = 0;
            spawnTimer = 0f;
        }
        else
        {
            spawnTimer = wave.spawnInterval;
        }
    }

    private void SpawnEnemy(GameConfig.WaveDefinition wave, int spawnIndex)
    {
        int enemyDefinitionIndex = GetEnemyDefinitionIndex(wave, spawnIndex);
        if (config.enemyPrefab == null || config.enemies == null || enemyDefinitionIndex < 0 || enemyDefinitionIndex >= config.enemies.Length)
        {
            Debug.LogError("GameConfig has an invalid enemy prefab or wave enemy index.");
            State = BattleState.Defeat;
            ResultMessage = "設定エラー";
            return;
        }

        int configuredLane = wave.lanes != null && wave.lanes.Length > 0 ? wave.lanes[spawnIndex % wave.lanes.Length] : 0;
        int lane = Mathf.Clamp(configuredLane, 0, stage.MapHeight - 1);
        Vector3[] route = BuildEnemyRoute(wave, lane);
        if (route == null || route.Length < 2)
        {
            Debug.LogError("Stage must provide an enemy route with at least two waypoints.");
            State = BattleState.Defeat;
            ResultMessage = "設定エラー";
            return;
        }

        GameObject instance = Instantiate(config.enemyPrefab, route[0], Quaternion.identity);
        EnemyActor enemy = instance.GetComponent<EnemyActor>();
        if (enemy == null)
        {
            Debug.LogError("Enemy prefab must have an EnemyActor component.");
            Destroy(instance);
            State = BattleState.Defeat;
            ResultMessage = "設定エラー";
            return;
        }

        enemy.Initialize(config.enemies[enemyDefinitionIndex], lane, route);
        enemies.Add(enemy);
    }

    private static int GetWaveEnemyCount(GameConfig.WaveDefinition wave)
    {
        if (wave.enemyGroups == null || wave.enemyGroups.Length == 0)
        {
            return wave.enemyCount;
        }

        int total = 0;
        for (int index = 0; index < wave.enemyGroups.Length; index++)
        {
            if (wave.enemyGroups[index] != null)
            {
                total += Mathf.Max(0, wave.enemyGroups[index].enemyCount);
            }
        }

        return total;
    }

    private static int GetEnemyDefinitionIndex(GameConfig.WaveDefinition wave, int spawnIndex)
    {
        if (wave.enemyGroups == null || wave.enemyGroups.Length == 0)
        {
            return wave.enemyDefinitionIndex;
        }

        int remainingSpawnIndex = spawnIndex;
        for (int index = 0; index < wave.enemyGroups.Length; index++)
        {
            GameConfig.EnemyGroupDefinition group = wave.enemyGroups[index];
            if (group == null)
            {
                continue;
            }

            if (remainingSpawnIndex < group.enemyCount)
            {
                return group.enemyDefinitionIndex;
            }

            remainingSpawnIndex -= group.enemyCount;
        }

        return wave.enemyDefinitionIndex;
    }

    private Vector3[] BuildEnemyRoute(GameConfig.WaveDefinition wave, int lane)
    {
        if (stage.enemyRoutes != null && stage.enemyRoutes.Length > 0)
        {
            if (wave.routeIndex >= 0 && wave.routeIndex < stage.enemyRoutes.Length)
            {
                Vector2Int[] waypoints = stage.enemyRoutes[wave.routeIndex].waypoints;
                if (IsGridRouteWalkable(waypoints))
                {
                    Vector3[] worldPoints = new Vector3[waypoints.Length];
                    for (int index = 0; index < waypoints.Length; index++)
                    {
                        worldPoints[index] = GridToWorld(waypoints[index]);
                    }

                    return worldPoints;
                }
            }

            Debug.LogWarning("Configured enemy route is missing or crosses a wall. Searching a path through open map tiles.");
        }

        if (stage.mapLayout != null && stage.mapLayout.Length > 0)
        {
            return FindMapRoute();
        }

        float y = stage.laneOffsetY + (lane - (stage.MapHeight - 1) * 0.5f) * stage.laneSpacing;
        float startX = (stage.columns - 1) * 0.5f * stage.cellSpacing + 1f;
        float goalX = -(stage.columns - 1) * 0.5f * stage.cellSpacing - 0.65f;
        return new[] { new Vector3(startX, y, 0f), new Vector3(goalX, y, 0f) };
    }

    private bool IsGridRouteWalkable(Vector2Int[] waypoints)
    {
        if (waypoints == null || waypoints.Length < 2)
        {
            return false;
        }

        for (int waypointIndex = 0; waypointIndex < waypoints.Length; waypointIndex++)
        {
            if (!IsMapCellWalkable(waypoints[waypointIndex]))
            {
                return false;
            }

            if (waypointIndex == 0)
            {
                continue;
            }

            Vector2Int previous = waypoints[waypointIndex - 1];
            Vector2Int current = waypoints[waypointIndex];
            int deltaX = current.x - previous.x;
            int deltaY = current.y - previous.y;
            if (deltaX != 0 && deltaY != 0)
            {
                return false;
            }

            int steps = Mathf.Max(Mathf.Abs(deltaX), Mathf.Abs(deltaY));
            Vector2Int direction = new Vector2Int(System.Math.Sign(deltaX), System.Math.Sign(deltaY));
            for (int step = 1; step < steps; step++)
            {
                if (!IsMapCellWalkable(previous + direction * step))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private Vector3[] FindMapRoute()
    {
        Vector2Int start = new Vector2Int(-1, -1);
        Vector2Int goal = new Vector2Int(-1, -1);
        for (int y = 0; y < stage.MapHeight; y++)
        {
            string row = stage.mapLayout[y] ?? string.Empty;
            for (int x = 0; x < row.Length; x++)
            {
                if (row[x] == 'E' && start.x < 0)
                {
                    start = new Vector2Int(x, y);
                }
                else if (row[x] == 'B' && goal.x < 0)
                {
                    goal = new Vector2Int(x, y);
                }
            }
        }

        if (start.x < 0 || goal.x < 0)
        {
            return null;
        }

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        bool[,] visited = new bool[stage.MapHeight, stage.MapWidth];
        Vector2Int[,] previous = new Vector2Int[stage.MapHeight, stage.MapWidth];
        Vector2Int[] directions =
        {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
        };
        frontier.Enqueue(start);
        visited[start.y, start.x] = true;

        while (frontier.Count > 0 && !visited[goal.y, goal.x])
        {
            Vector2Int current = frontier.Dequeue();
            for (int directionIndex = 0; directionIndex < directions.Length; directionIndex++)
            {
                Vector2Int next = current + directions[directionIndex];
                if (next.x < 0 || next.x >= stage.MapWidth || next.y < 0 || next.y >= stage.MapHeight ||
                    visited[next.y, next.x] || !IsMapCellWalkable(next))
                {
                    continue;
                }

                visited[next.y, next.x] = true;
                previous[next.y, next.x] = current;
                frontier.Enqueue(next);
            }
        }

        if (!visited[goal.y, goal.x])
        {
            return null;
        }

        List<Vector2Int> gridPath = new List<Vector2Int>();
        Vector2Int cursor = goal;
        while (cursor != start)
        {
            gridPath.Add(cursor);
            cursor = previous[cursor.y, cursor.x];
        }

        gridPath.Add(start);
        gridPath.Reverse();
        Vector3[] worldPath = new Vector3[gridPath.Count];
        for (int index = 0; index < gridPath.Count; index++)
        {
            worldPath[index] = GridToWorld(gridPath[index]);
        }

        return worldPath;
    }

    private bool IsMapCellWalkable(Vector2Int cell)
    {
        if (cell.x < 0 || cell.x >= stage.MapWidth || cell.y < 0 || cell.y >= stage.MapHeight)
        {
            return false;
        }

        string row = stage.mapLayout[cell.y];
        return row != null && cell.x < row.Length && row[cell.x] != '#';
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        float x = (gridPosition.x - (stage.MapWidth - 1) * 0.5f) * stage.mapCellSpacing;
        float y = ((stage.MapHeight - 1f) * 0.5f - gridPosition.y) * stage.mapRowSpacing;
        return new Vector3(x, y, 0f);
    }

    private void UpdateUnits()
    {
        for (int unitIndex = units.Count - 1; unitIndex >= 0; unitIndex--)
        {
            UnitActor unit = units[unitIndex];
            if (unit == null || !unit.IsAlive)
            {
                continue;
            }

            unit.AdvanceAttackClock(Time.deltaTime);
            if (unit.Role == OperatorRole.Healer)
            {
                UnitActor ally = FindHealingTarget(unit);
                if (ally != null)
                {
                    unit.Heal(ally);
                }
            }
            else
            {
                EnemyActor target = FindTarget(unit);
                if (target != null)
                {
                    unit.Attack(target, unit.AttackInterval);
                }
            }
        }
    }

    private UnitActor FindHealingTarget(UnitActor healer)
    {
        UnitActor target = null;
        int greatestMissingHealth = 0;
        for (int index = 0; index < units.Count; index++)
        {
            UnitActor ally = units[index];
            if (ally == null || !ally.IsAlive || ally.CurrentHealth >= ally.MaxHealth)
            {
                continue;
            }

            float distance = Vector2.Distance(ally.transform.position, healer.transform.position);
            int missingHealth = ally.MaxHealth - ally.CurrentHealth;
            if (distance <= healer.AttackRange && missingHealth > greatestMissingHealth)
            {
                target = ally;
                greatestMissingHealth = missingHealth;
            }
        }

        return target;
    }

    private EnemyActor FindTarget(UnitActor unit)
    {
        EnemyActor closest = null;
        float closestDistance = float.MaxValue;
        for (int index = 0; index < enemies.Count; index++)
        {
            EnemyActor enemy = enemies[index];
            if (enemy == null || !enemy.IsAlive)
            {
                continue;
            }

            float distance = Vector2.Distance(enemy.transform.position, unit.transform.position);
            if (distance <= unit.AttackRange && distance < closestDistance)
            {
                closest = enemy;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private void UpdateEnemies()
    {
        for (int enemyIndex = enemies.Count - 1; enemyIndex >= 0; enemyIndex--)
        {
            EnemyActor enemy = enemies[enemyIndex];
            if (enemy == null || !enemy.IsAlive)
            {
                RemoveEnemyAt(enemyIndex);
                continue;
            }

            UnitActor blocker = FindBlocker(enemy);
            enemy.AdvanceAttackClock(Time.deltaTime);
            if (blocker != null && Vector2.Distance(enemy.transform.position, blocker.transform.position) <= 0.55f)
            {
                enemy.Attack(blocker);
            }
            else
            {
                enemy.MoveAlongRoute(Time.deltaTime);
                if (enemy.HasReachedGoal)
                {
                    lives--;
                    RemoveEnemyAt(enemyIndex);
                    if (lives <= 0)
                    {
                        State = BattleState.Defeat;
                        ResultMessage = "拠点を守れませんでした";
                        return;
                    }
                }
            }
        }
    }

    private UnitActor FindBlocker(EnemyActor enemy)
    {
        UnitActor closest = null;
        float closestDistance = float.MaxValue;
        for (int unitIndex = 0; unitIndex < units.Count; unitIndex++)
        {
            UnitActor unit = units[unitIndex];
            if (unit == null || !unit.IsAlive || unit.BlockCapacity <= 0)
            {
                continue;
            }

            float distance = Vector2.Distance(enemy.transform.position, unit.transform.position);
            if (distance > 0.55f || distance >= closestDistance)
            {
                continue;
            }

            int blockedCount = 0;
            for (int otherIndex = 0; otherIndex < enemies.Count; otherIndex++)
            {
                EnemyActor other = enemies[otherIndex];
                if (other != null && other != enemy && Vector2.Distance(other.transform.position, unit.transform.position) <= 0.55f)
                {
                    blockedCount++;
                }
            }

            if (blockedCount < unit.BlockCapacity)
            {
                closest = unit;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private void RemoveEnemyAt(int index)
    {
        if (enemies[index] != null)
        {
            Destroy(enemies[index].gameObject);
        }

        enemies.RemoveAt(index);
    }

    private void CheckForVictory()
    {
        if (stage.waves != null && currentWaveIndex >= stage.waves.Length && enemies.Count == 0 && totalSpawned > 0)
        {
            State = BattleState.Victory;
            ResultMessage = "全ウェーブを撃退しました";
        }
    }
}