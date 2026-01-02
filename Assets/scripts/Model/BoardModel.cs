using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure model class for the game board.
/// Handles tile placement, movement, and state - no rendering.
/// </summary>
public class BoardModel
{
    public int GridSize { get; private set; }
    
    private TileData[,] grid;
    private int occupiedCount = 0;
    
    public BoardModel(int gridSize)
    {
        GridSize = gridSize;
        grid = new TileData[gridSize, gridSize];
    }
    
    // ===== QUERIES =====
    
    public bool IsValidPosition(int row, int col)
    {
        return row >= 0 && row < GridSize && col >= 0 && col < GridSize;
    }
    
    public bool IsValidPosition(Vector2Int pos) => IsValidPosition(pos.x, pos.y);
    
    public bool IsEmpty(int row, int col)
    {
        return IsValidPosition(row, col) && grid[row, col] == null;
    }
    
    public bool IsEmpty(Vector2Int pos) => IsEmpty(pos.x, pos.y);
    
    public bool IsFull => occupiedCount >= GridSize * GridSize;
    
    public float FillPercentage => (float)occupiedCount / (GridSize * GridSize);
    
    public TileData GetTile(int row, int col)
    {
        if (!IsValidPosition(row, col)) return null;
        return grid[row, col];
    }
    
    public TileData GetTile(Vector2Int pos) => GetTile(pos.x, pos.y);
    
    public List<Vector2Int> GetEmptyCells()
    {
        var empty = new List<Vector2Int>();
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                if (grid[row, col] == null)
                    empty.Add(new Vector2Int(row, col));
            }
        }
        return empty;
    }
    
    public List<Vector2Int> GetOccupiedCells()
    {
        var occupied = new List<Vector2Int>();
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                if (grid[row, col] != null)
                    occupied.Add(new Vector2Int(row, col));
            }
        }
        return occupied;
    }
    
    // ===== MUTATIONS =====
    
    public bool PlaceTile(TileData tile)
    {
        if (!IsValidPosition(tile.row, tile.col)) return false;
        if (!IsEmpty(tile.row, tile.col)) return false;
        
        grid[tile.row, tile.col] = tile;
        occupiedCount++;
        return true;
    }
    
    public TileData RemoveTile(int row, int col)
    {
        if (!IsValidPosition(row, col)) return null;
        
        TileData tile = grid[row, col];
        if (tile != null)
        {
            grid[row, col] = null;
            occupiedCount--;
        }
        return tile;
    }
    
    public TileData RemoveTile(Vector2Int pos) => RemoveTile(pos.x, pos.y);
    
    /// <summary>
    /// Moves a tile from one position to another.
    /// Returns true if successful.
    /// </summary>
    public bool MoveTile(Vector2Int from, Vector2Int to)
    {
        if (!IsValidPosition(from) || !IsValidPosition(to)) return false;
        if (IsEmpty(from)) return false;
        if (!IsEmpty(to)) return false;
        
        TileData tile = grid[from.x, from.y];
        grid[from.x, from.y] = null;
        
        tile.row = to.x;
        tile.col = to.y;
        grid[to.x, to.y] = tile;
        
        return true;
    }
    
    /// <summary>
    /// Merges tile at 'from' into tile at 'to'.
    /// Returns the merged TileData, or null if merge not possible.
    /// </summary>
    public TileData MergeTiles(Vector2Int from, Vector2Int to)
    {
        if (!IsValidPosition(from) || !IsValidPosition(to)) return null;
        
        TileData fromTile = GetTile(from);
        TileData toTile = GetTile(to);
        
        if (fromTile == null || toTile == null) return null;
        
        // Check if merge is valid (combined notes <= 3)
        var combinedNotes = new HashSet<string>(fromTile.notes);
        foreach (var note in toTile.notes)
            combinedNotes.Add(note);
        
        if (combinedNotes.Count > 3) return null;
        
        // Perform merge
        grid[from.x, from.y] = null;
        occupiedCount--;
        
        toTile.notes = new List<string>(combinedNotes);
        
        return toTile;
    }
    
    public void Clear()
    {
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                grid[row, col] = null;
            }
        }
        occupiedCount = 0;
    }
    
    /// <summary>
    /// Removes tiles at specified positions.
    /// Returns list of removed TileData.
    /// </summary>
    public List<TileData> RemoveTiles(List<Vector2Int> positions)
    {
        var removed = new List<TileData>();
        foreach (var pos in positions)
        {
            var tile = RemoveTile(pos);
            if (tile != null)
                removed.Add(tile);
        }
        return removed;
    }
}
