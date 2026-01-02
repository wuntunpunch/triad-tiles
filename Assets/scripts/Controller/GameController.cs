using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main game controller. Orchestrates game flow, connects model to views.
/// </summary>
public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private NoteColors noteColors;
    [SerializeField] private DifficultyConfig[] difficulties;
    
    [Header("Views")]
    [SerializeField] private BoardView boardView;
    
    // Current game state
    private BoardModel board;
    private TriadMatcher matcher;
    private DifficultyConfig currentDifficulty;
    
    private int score;
    private float timeRemaining;
    private int combo;
    private float lastMatchTime;
    private bool isPlaying;
    private float nextSpawnTime;
    
    private string requestedChord;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void OnEnable()
    {
        GameEvents.OnTileDragEnded += HandleTileDrop;
        GameEvents.OnGameStart += StartGame;
    }
    
    void OnDisable()
    {
        GameEvents.OnTileDragEnded -= HandleTileDrop;
        GameEvents.OnGameStart -= StartGame;
    }
    
    void Update()
    {
        if (!isPlaying) return;
        
        // Timer
        timeRemaining -= Time.deltaTime;
        GameEvents.FireTimerTick(timeRemaining);
        
        if (timeRemaining <= gameConfig.timerWarningThreshold && timeRemaining > 0)
            GameEvents.FireTimerWarning();
        
        if (timeRemaining <= 0)
        {
            EndGame();
            return;
        }
        
        // Combo decay
        if (combo > 0 && Time.time - lastMatchTime > gameConfig.comboResetTime)
        {
            combo = 0;
            GameEvents.FireComboChanged(0, 1);
        }
        
        // Tile spawning
        if (Time.time >= nextSpawnTime)
        {
            SpawnRandomTile();
            nextSpawnTime = Time.time + GetSpawnInterval();
        }
    }
    
    // ===== DIFFICULTY SELECTION =====
    
    public void SelectDifficulty(int index)
    {
        if (index >= 0 && index < difficulties.Length)
        {
            currentDifficulty = difficulties[index];
            GameEvents.FireGameStart();
        }
    }
    
    // ===== GAME FLOW =====
    
    private void StartGame()
    {
        if (currentDifficulty == null && difficulties.Length > 0)
            currentDifficulty = difficulties[0];
        
        // Initialize model
        board = new BoardModel(gameConfig.gridSize);
        matcher = new TriadMatcher(currentDifficulty);
        
        // Initialize view
        boardView.Initialize(gameConfig, noteColors);
        boardView.ClearBoard();
        
        // Reset state
        score = 0;
        timeRemaining = GetGameDuration();
        combo = 0;
        isPlaying = true;
        nextSpawnTime = Time.time + gameConfig.spawnInterval;
        
        // Fire initial events
        GameEvents.FireScoreChanged(0);
        GameEvents.FireComboChanged(0, 1);
        
        // Request a chord
        RequestNewChord();

        // Show game panel BEFORE spawning (ensures BoardView is subscribed)
        GameEvents.FirePanelRequested("Game");
        
        // Spawn initial tiles
        for (int i = 0; i < gameConfig.initialTileCount; i++)
        {
            SpawnRandomTile();
        }
    }
    
    private void EndGame()
    {
        isPlaying = false;
        GameEvents.FireGameOver(score);
    }
    
    private float GetSpawnInterval()
    {
        float multiplier = currentDifficulty != null 
            ? currentDifficulty.spawnIntervalMultiplier 
            : 1f;
        return gameConfig.spawnInterval * multiplier;
    }
    
    private float GetGameDuration()
    {
        float multiplier = currentDifficulty != null 
            ? currentDifficulty.durationMultiplier 
            : 1f;
        return gameConfig.gameDuration * multiplier;
    }
    
    // ===== TILE SPAWNING =====
    
    private void SpawnRandomTile()
    {
        List<Vector2Int> emptyCells = board.GetEmptyCells();
        Debug.Log($"SpawnRandomTile: {emptyCells.Count} empty cells");
        
        if (emptyCells.Count == 0)
        {
            EndGame();
            return;
        }
        
        Vector2Int pos = emptyCells[Random.Range(0, emptyCells.Count)];
        string note = GetRandomNote();
        Debug.Log($"Spawning tile '{note}' at {pos}");
        
        TileData data = new TileData(note, pos.x, pos.y);
        board.PlaceTile(data);
        
        GameEvents.FireTileSpawned(pos, data);
    }
    
    private string GetRandomNote()
    {
        if (currentDifficulty == null || currentDifficulty.availableNotes.Length == 0)
            return "C";
        
        var notes = currentDifficulty.availableNotes;
        return notes[Random.Range(0, notes.Length)];
    }
    
    // ===== TILE MOVEMENT =====
    
    private void HandleTileDrop(TileView tileView, Vector2 screenPos)
    {
        if (!isPlaying || tileView == null || tileView.Data == null) return;
        
        Vector2Int? targetCell = boardView.ScreenToGrid(screenPos);
        Vector2Int fromPos = new Vector2Int(tileView.Data.row, tileView.Data.col);
        
        if (!targetCell.HasValue || !IsValidDrop(tileView.Data, targetCell.Value))
        {
            // Invalid drop - snap back
            tileView.PlayErrorAnimation();
            tileView.AnimateToPosition(boardView.GridToLocal(fromPos));
            return;
        }
        
        Vector2Int toPos = targetCell.Value;
        
        // Same position - just snap back
        if (fromPos == toPos)
        {
            tileView.AnimateToPosition(boardView.GridToLocal(fromPos));
            return;
        }
        
        TileData targetTile = board.GetTile(toPos);
        
        if (targetTile != null)
        {
            // Merge
            TileData merged = board.MergeTiles(fromPos, toPos);
            if (merged != null)
            {
                GameEvents.FireTilesMerged(fromPos, toPos, merged);
                
                // Check for triads after merge
                StartCoroutine(CheckForTriadsDelayed());
            }
        }
        else
        {
            // Move to empty cell
            board.MoveTile(fromPos, toPos);
            GameEvents.FireTileMoved(fromPos, toPos, tileView.Data);
            
            // Check for triads after move
            StartCoroutine(CheckForTriadsDelayed());
        }
    }
    
    private bool IsValidDrop(TileData tile, Vector2Int target)
    {
        Vector2Int current = new Vector2Int(tile.row, tile.col);
        
        // Same position is valid (will snap back)
        if (current == target) return true;
        
        // Empty cell is valid
        if (board.IsEmpty(target)) return true;
        
        // Check if merge is possible
        TileData targetTile = board.GetTile(target);
        if (targetTile != null)
        {
            return matcher.CanMerge(tile.notes, targetTile.notes);
        }
        
        return false;
    }
    
    // ===== TRIAD MATCHING =====
    
    private IEnumerator CheckForTriadsDelayed()
    {
        yield return new WaitForSeconds(0.15f);
        CheckForTriads();
    }
    
    private void CheckForTriads()
    {
        List<TriadMatch> matches = matcher.FindMatches(board);
        
        if (matches.Count == 0) return;
        
        // Collect all positions to remove (avoid duplicates)
        HashSet<Vector2Int> toRemove = new HashSet<Vector2Int>();
        bool matchedRequestedChord = false;
        
        foreach (var match in matches)
        {
            foreach (var pos in match.Positions)
                toRemove.Add(pos);
            
            if (match.ChordName == requestedChord)
                matchedRequestedChord = true;
            
            GameEvents.FireTriadMatched(match.Positions, match.ChordName);
        }
        
        // Calculate score
        int baseScore = gameConfig.basePointsPerTriad * matches.Count;
        int bonusScore = matchedRequestedChord ? gameConfig.requestedChordBonus : 0;
        int multiplier = GetComboMultiplier();
        int totalScore = (baseScore + bonusScore) * multiplier;
        
        // Update combo
        combo++;
        lastMatchTime = Time.time;
        GameEvents.FireComboChanged(combo, GetComboMultiplier());
        
        // Update score
        score += totalScore;
        GameEvents.FireScoreChanged(score);
        
        // Show score popup at middle match position
        if (matches.Count > 0)
        {
            GameEvents.FireScorePopup(totalScore, matches[0].CenterPosition);
        }
        
        // Remove matched tiles
        board.RemoveTiles(new List<Vector2Int>(toRemove));
        foreach (var pos in toRemove)
        {
            GameEvents.FireTileDestroyed(pos);
        }
        
        // Request new chord if matched
        if (matchedRequestedChord)
        {
            GameEvents.FireRequestedChordCompleted(requestedChord);
            RequestNewChord();
        }
        
        // Chain check
        StartCoroutine(CheckForTriadsDelayed());
    }
    
    private int GetComboMultiplier()
    {
        if (gameConfig.comboMultipliers == null || gameConfig.comboMultipliers.Length == 0)
            return 1;
        
        int index = Mathf.Min(combo, gameConfig.comboMultipliers.Length - 1);
        return gameConfig.comboMultipliers[index];
    }
    
    // ===== REQUESTED CHORD =====
    
    private void RequestNewChord()
    {
        if (currentDifficulty == null || currentDifficulty.validChords.Length == 0)
        {
            requestedChord = "";
            return;
        }
        
        var chords = currentDifficulty.validChords;
        var selected = chords[Random.Range(0, chords.Length)];
        requestedChord = selected.chordName;
        
        string displayName = selected.displayName;
        if (string.IsNullOrEmpty(displayName))
            displayName = selected.chordName;
        
        GameEvents.FireChordRequested(displayName);
    }
}
