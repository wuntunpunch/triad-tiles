using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Renders the game board and manages TileViews.
/// Subscribes to GameEvents to update visuals.
/// </summary>
public class BoardView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform boardContainer;
    [SerializeField] private GameObject tilePrefab;
    
    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private NoteColors noteColors;
    
    // Tile view tracking
    private Dictionary<Vector2Int, TileView> tileViews = new Dictionary<Vector2Int, TileView>();
    
    // Calculated dimensions
    private float cellSize;
    private float spacing;
    private int gridSize;
    
    void OnEnable()
    {
        // Subscribe to events
        GameEvents.OnTileSpawned += HandleTileSpawned;
        GameEvents.OnTileMoved += HandleTileMoved;
        GameEvents.OnTilesMerged += HandleTilesMerged;
        GameEvents.OnTileDestroyed += HandleTileDestroyed;
        GameEvents.OnTriadMatched += HandleTriadMatched;
        GameEvents.OnGameStart += HandleGameStart;
    }
    
    void OnDisable()
    {
        // Unsubscribe
        GameEvents.OnTileSpawned -= HandleTileSpawned;
        GameEvents.OnTileMoved -= HandleTileMoved;
        GameEvents.OnTilesMerged -= HandleTilesMerged;
        GameEvents.OnTileDestroyed -= HandleTileDestroyed;
        GameEvents.OnTriadMatched -= HandleTriadMatched;
        GameEvents.OnGameStart -= HandleGameStart;
    }
    
    // ===== INITIALIZATION =====
    
    public void Initialize(GameConfig config, NoteColors colors)
    {
        gameConfig = config;
        noteColors = colors;
        gridSize = config.gridSize;
        
        CalculateDimensions();
    }
    
    private void CalculateDimensions()
    {
        if (boardContainer == null || gameConfig == null) return;
        
        float boardSize = Mathf.Min(boardContainer.rect.width, boardContainer.rect.height);
        float spacingRatio = gameConfig.spacingRatio;
        
        float divisor = gridSize + spacingRatio * (gridSize - 1);
        cellSize = boardSize / divisor;
        spacing = cellSize * spacingRatio;
        
        Debug.Log($"BoardView: size={boardSize}, cell={cellSize}, spacing={spacing}");
    }
    
    public float GetCellSize() => cellSize;
    public float GetSpacing() => spacing;
    
    // ===== COORDINATE CONVERSION =====
    
    public Vector2 GridToLocal(int row, int col)
    {
        float totalSize = (cellSize * gridSize) + (spacing * (gridSize - 1));
        float startOffset = -totalSize / 2f + cellSize / 2f;
        
        float x = startOffset + col * (cellSize + spacing);
        float y = -startOffset - row * (cellSize + spacing);
        
        return new Vector2(x, y);
    }
    
    public Vector2 GridToLocal(Vector2Int pos) => GridToLocal(pos.x, pos.y);
    
    public Vector2Int? ScreenToGrid(Vector2 screenPos)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            boardContainer, screenPos, null, out Vector2 localPoint))
            return null;
        
        float totalSize = (cellSize * gridSize) + (spacing * (gridSize - 1));
        float startOffset = -totalSize / 2f;
        
        float relX = localPoint.x - startOffset;
        float relY = -localPoint.y - startOffset;
        
        int col = Mathf.FloorToInt(relX / (cellSize + spacing));
        int row = Mathf.FloorToInt(relY / (cellSize + spacing));
        
        if (row >= 0 && row < gridSize && col >= 0 && col < gridSize)
            return new Vector2Int(row, col);
        
        return null;
    }
    
    // ===== EVENT HANDLERS =====
    
    private void HandleGameStart()
    {
        CalculateDimensions();
    }

    public void ClearBoard()
    {
        ClearAllTiles();
    }
    
    private void HandleTileSpawned(Vector2Int pos, TileData data)
    {
        Debug.Log($"HandleTileSpawned: pos={pos}, cellSize={cellSize}");
        CreateTileView(pos, data, true);
    }
    
    private void HandleTileMoved(Vector2Int from, Vector2Int to, TileData data)
    {
        if (!tileViews.ContainsKey(from)) return;
        
        TileView view = tileViews[from];
        tileViews.Remove(from);
        tileViews[to] = view;
        
        view.AnimateToPosition(GridToLocal(to));
    }
    
    private void HandleTilesMerged(Vector2Int from, Vector2Int to, TileData mergedData)
    {
        // Remove the dragged tile view
        if (tileViews.ContainsKey(from))
        {
            Destroy(tileViews[from].gameObject);
            tileViews.Remove(from);
        }
        
        // Update the target tile view
        if (tileViews.ContainsKey(to))
        {
            TileView view = tileViews[to];
            view.Initialize(mergedData, noteColors, gameConfig);
            view.PlayMergeAnimation();
        }
    }
    
    private void HandleTileDestroyed(Vector2Int pos)
    {
        if (!tileViews.ContainsKey(pos)) return;
        
        TileView view = tileViews[pos];
        tileViews.Remove(pos);
        
        view.PlayDestroyAnimation(() => {
            if (view != null)
                Destroy(view.gameObject);
        });
    }
    
    private void HandleTriadMatched(List<Vector2Int> positions, string chordName)
    {
        // Destruction is handled by individual OnTileDestroyed events
        // Could add extra visual effects here
    }
    
    // ===== TILE VIEW MANAGEMENT =====
    
    private TileView CreateTileView(Vector2Int pos, TileData data, bool animate = false)
    {
        if (tilePrefab == null || boardContainer == null) return null;
        
        GameObject tileObj = Instantiate(tilePrefab, boardContainer);
        TileView view = tileObj.GetComponent<TileView>();
        
        if (view == null)
        {
            Debug.LogError("Tile prefab missing TileView component!");
            Destroy(tileObj);
            return null;
        }
        
        view.Initialize(data, noteColors, gameConfig);
        view.SetSize(cellSize);
        view.SetPosition(GridToLocal(pos));
        
        if (animate)
            view.PlaySpawnAnimation();
        
        tileViews[pos] = view;
        return view;
    }
    
    public TileView GetTileView(Vector2Int pos)
    {
        return tileViews.ContainsKey(pos) ? tileViews[pos] : null;
    }
    
    public void AnimateTileToPosition(Vector2Int pos)
    {
        if (!tileViews.ContainsKey(pos)) return;
        tileViews[pos].AnimateToPosition(GridToLocal(pos));
    }
    
    public void SnapTileToPosition(Vector2Int pos)
    {
        if (!tileViews.ContainsKey(pos)) return;
        tileViews[pos].SetPosition(GridToLocal(pos));
    }
    
    private void ClearAllTiles()
    {
        foreach (var view in tileViews.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }
        tileViews.Clear();
    }
    
    // ===== SCREEN RESIZE HANDLING =====
    
    void OnRectTransformDimensionsChange()
    {
        if (boardContainer == null || gameConfig == null) return;
        if (tileViews.Count == 0) return;
        
        CalculateDimensions();
        RefreshAllTilePositions();
    }
    
    private void RefreshAllTilePositions()
    {
        foreach (var kvp in tileViews)
        {
            Vector2Int pos = kvp.Key;
            TileView view = kvp.Value;
            
            if (view != null)
            {
                view.SetSize(cellSize);
                view.SetPosition(GridToLocal(pos));
            }
        }
    }
}
