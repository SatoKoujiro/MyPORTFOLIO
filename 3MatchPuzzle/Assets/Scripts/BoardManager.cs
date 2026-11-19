using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [Header("盤面設定")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private float spacing = 1.1f;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Transform blockParent;

    [Header("アニメーション")]
    [SerializeField] private float moveDuration = 0.18f;
    [SerializeField] private float refillHeight = 2f;

    [Header("参照")]
    [SerializeField] private ScoreManager scoreManager;

    private Block[,] grid;
    private Block selectedBlock;
    private bool isBusy;
    private const int TypeCount = 5;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (blockPrefab == null)
        {
            Debug.LogError("BoardManager: Block Prefabが設定されていません。");
            enabled = false;
            return;
        }

        if (scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();
        grid = new Block[width, height];
        CreateBoard();
    }

    private void CreateBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BlockType type;
                int safety = 0;
                do
                {
                    type = RandomType();
                    safety++;
                }
                while (WouldCreateMatch(x, y, type) && safety < 100);

                CreateBlock(x, y, type, false);
            }
        }
    }

    private bool WouldCreateMatch(int x, int y, BlockType type)
    {
        if (x >= 2 && grid[x - 1, y] != null && grid[x - 2, y] != null &&
            grid[x - 1, y].Type == type && grid[x - 2, y].Type == type) return true;

        if (y >= 2 && grid[x, y - 1] != null && grid[x, y - 2] != null &&
            grid[x, y - 1].Type == type && grid[x, y - 2].Type == type) return true;

        return false;
    }

    private BlockType RandomType() { return (BlockType)Random.Range(0, TypeCount); }

    private void CreateBlock(int x, int y, BlockType type, bool animateFromAbove)
    {
        Vector3 target = GridToWorld(x, y);
        Vector3 start = animateFromAbove ? target + Vector3.up * refillHeight : target;
        Transform parent = blockParent != null ? blockParent : transform;

        Block block = Instantiate(blockPrefab, start, Quaternion.identity, parent);
        block.name = "Block_" + x + "_" + y;
        block.Initialize(this, x, y, type);
        grid[x, y] = block;

        // 補充ブロックは盤面より上に生成し、MoveBlocksToGridで一緒に落下させる。
    }

    public void OnBlockClicked(Block block)
    {
        if (isBusy || block == null) return;

        if (selectedBlock == null)
        {
            selectedBlock = block;
            selectedBlock.SetSelected(true);
            return;
        }

        if (selectedBlock == block)
        {
            selectedBlock.SetSelected(false);
            selectedBlock = null;
            return;
        }

        if (AreAdjacent(selectedBlock, block))
        {
            Block first = selectedBlock;
            first.SetSelected(false);
            selectedBlock = null;
            StartCoroutine(SwapRoutine(first, block));
        }
        else
        {
            selectedBlock.SetSelected(false);
            selectedBlock = block;
            selectedBlock.SetSelected(true);
        }
    }

    private bool AreAdjacent(Block a, Block b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y) == 1;
    }

    private IEnumerator SwapRoutine(Block a, Block b)
    {
        isBusy = true;
        SwapInGrid(a, b);
        yield return MoveBlocksToGrid();

        HashSet<Block> matches = MatchChecker.FindMatches(grid, width, height);
        if (matches.Count == 0)
        {
            SwapInGrid(a, b);
            yield return MoveBlocksToGrid();
        }
        else
        {
            yield return ResolveMatches();
        }

        isBusy = false;
    }

    private void SwapInGrid(Block a, Block b)
    {
        int ax = a.X, ay = a.Y, bx = b.X, by = b.Y;
        grid[ax, ay] = b;
        grid[bx, by] = a;
        a.SetPosition(bx, by);
        b.SetPosition(ax, ay);
    }

    private IEnumerator ResolveMatches()
    {
        while (true)
        {
            HashSet<Block> matches = MatchChecker.FindMatches(grid, width, height);
            if (matches.Count == 0) yield break;

            if (scoreManager != null) scoreManager.AddScore(matches.Count);

            foreach (Block block in matches)
            {
                if (block == null) continue;
                grid[block.X, block.Y] = null;
                Destroy(block.gameObject);
            }

            yield return new WaitForSeconds(0.12f);
            yield return DropBlocks();
            yield return RefillBlocks();
            yield return MoveBlocksToGrid();
            yield return new WaitForSeconds(moveDuration);
        }
    }

    private IEnumerator DropBlocks()
    {
        for (int x = 0; x < width; x++)
        {
            int writeY = 0;
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null) continue;

                if (writeY != y)
                {
                    Block block = grid[x, y];
                    grid[x, writeY] = block;
                    grid[x, y] = null;
                    block.SetPosition(x, writeY);
                }
                writeY++;
            }
        }
        yield return null;
    }

    private IEnumerator RefillBlocks()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (grid[x, y] == null)
                    CreateBlock(x, y, RandomType(), true);
            }
        }
        yield return null;
    }

    private IEnumerator MoveBlocksToGrid()
    {
        List<Transform> moving = new List<Transform>();
        List<Vector3> starts = new List<Vector3>();
        List<Vector3> targets = new List<Vector3>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Block block = grid[x, y];
                if (block == null) continue;

                Vector3 target = GridToWorld(x, y);
                if ((block.transform.position - target).sqrMagnitude > 0.0001f)
                {
                    moving.Add(block.transform);
                    starts.Add(block.transform.position);
                    targets.Add(target);
                }
            }
        }

        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < moving.Count; i++)
            {
                if (moving[i] != null)
                    moving[i].position = Vector3.Lerp(starts[i], targets[i], smoothT);
            }
            yield return null;
        }

        for (int i = 0; i < moving.Count; i++)
            if (moving[i] != null) moving[i].position = targets[i];
    }

    private Vector3 GridToWorld(int x, int y)
    {
        float startX = -(width - 1) * spacing * 0.5f;
        float startY = -(height - 1) * spacing * 0.5f;
        return transform.position + new Vector3(startX + x * spacing,
                                                startY + y * spacing, 0f);
    }
}
