using System.Collections.Generic;

public static class MatchChecker
{
    public static HashSet<Block> FindMatches(Block[,] grid, int width, int height)
    {
        HashSet<Block> matches = new HashSet<Block>();

        for (int y = 0; y < height; y++)
        {
            int x = 0;
            while (x < width)
            {
                Block first = grid[x, y];
                if (first == null) { x++; continue; }

                int end = x + 1;
                while (end < width && grid[end, y] != null &&
                       grid[end, y].Type == first.Type) end++;

                if (end - x >= 3)
                    for (int i = x; i < end; i++) matches.Add(grid[i, y]);

                x = end;
            }
        }

        for (int x = 0; x < width; x++)
        {
            int y = 0;
            while (y < height)
            {
                Block first = grid[x, y];
                if (first == null) { y++; continue; }

                int end = y + 1;
                while (end < height && grid[x, end] != null &&
                       grid[x, end].Type == first.Type) end++;

                if (end - y >= 3)
                    for (int i = y; i < end; i++) matches.Add(grid[x, i]);

                y = end;
            }
        }

        return matches;
    }
}
