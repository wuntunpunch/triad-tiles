using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central event hub for decoupled communication between game systems.
/// Components subscribe to events they care about without knowing who fires them.
/// </summary>
public static class GameEvents
{
    // ===== GAME STATE EVENTS =====
    public static event Action OnGameStart;
    public static event Action OnGamePause;
    public static event Action OnGameResume;
    public static event Action<int> OnGameOver; // final score
    
    // ===== TILE EVENTS =====
    public static event Action<Vector2Int, TileData> OnTileSpawned;
    public static event Action<Vector2Int, Vector2Int, TileData> OnTileMoved; // from, to, data
    public static event Action<Vector2Int, Vector2Int, TileData> OnTilesMerged; // from, to, merged data
    public static event Action<Vector2Int> OnTileDestroyed;
    
    // ===== MATCH EVENTS =====
    public static event Action<List<Vector2Int>, string> OnTriadMatched; // positions, chord name
    public static event Action<string> OnRequestedChordCompleted; // chord name (legacy, kept for compatibility)
    
    // ===== SCORE EVENTS =====
    public static event Action<int> OnScoreChanged; // new total
    public static event Action<int, Vector2Int> OnScorePopup; // points, position
    public static event Action<int, int> OnComboChanged; // combo count, multiplier
    
    // ===== TIMER EVENTS =====
    public static event Action<float> OnTimerTick; // time remaining
    public static event Action OnTimerWarning; // low time
    
    // ===== UI EVENTS =====
    public static event Action<string> OnPanelRequested; // panel name
    public static event Action<string> OnChordRequested; // chord to display (legacy, kept for compatibility)
    
    // ===== REQUESTED CHORD SLOT EVENTS =====
    public static event Action<int> OnSlotUnlocked; // slot index
    public static event Action<int, string> OnSlotChordChanged; // slot index, display name
    public static event Action<int, string> OnSlotCompleted; // slot index, chord name
    
    // ===== INPUT EVENTS =====
    public static event Action<TileView> OnTileDragStarted;
    public static event Action<TileView, Vector2> OnTileDragging;
    public static event Action<TileView, Vector2> OnTileDragEnded;
    public static event Action<Vector2Int> OnCellHovered; // grid position being hovered
    public static event Action OnHoverCleared;

    // ---- REQUESTED CHORD EVENTS (Legacy - consider migrating to slot events) ----
    
    /// <summary>
    /// Fired when the requested chord changes (or becomes null when none is active)
    /// </summary>
    public static Action<ChordDefinition> RequestedChordChanged;
    
    /// <summary>
    /// Fired when the player successfully completes the requested chord
    /// Parameters: completed chord, bonus points awarded, bonus time awarded
    /// </summary>
    public static Action<ChordDefinition, int, float> RequestedChordCompleted;
    
    /// <summary>
    /// Fired when bonus time is awarded (for timer to listen to)
    /// Parameter: seconds to add
    /// </summary>
    public static Action<float> BonusTimeAwarded;
    
    // ===== FIRE METHODS =====
    // These are called by the systems that own each event
    
    public static void FireGameStart() => OnGameStart?.Invoke();
    public static void FireGamePause() => OnGamePause?.Invoke();
    public static void FireGameResume() => OnGameResume?.Invoke();
    public static void FireGameOver(int score) => OnGameOver?.Invoke(score);
    
    public static void FireTileSpawned(Vector2Int pos, TileData data) => OnTileSpawned?.Invoke(pos, data);
    public static void FireTileMoved(Vector2Int from, Vector2Int to, TileData data) => OnTileMoved?.Invoke(from, to, data);
    public static void FireTilesMerged(Vector2Int from, Vector2Int to, TileData data) => OnTilesMerged?.Invoke(from, to, data);
    public static void FireTileDestroyed(Vector2Int pos) => OnTileDestroyed?.Invoke(pos);
    
    public static void FireTriadMatched(List<Vector2Int> positions, string chordName) => OnTriadMatched?.Invoke(positions, chordName);
    public static void FireRequestedChordCompleted(string chordName) => OnRequestedChordCompleted?.Invoke(chordName);
    
    public static void FireScoreChanged(int total) => OnScoreChanged?.Invoke(total);
    public static void FireScorePopup(int points, Vector2Int pos) => OnScorePopup?.Invoke(points, pos);
    public static void FireComboChanged(int combo, int multiplier) => OnComboChanged?.Invoke(combo, multiplier);
    
    public static void FireTimerTick(float remaining) => OnTimerTick?.Invoke(remaining);
    public static void FireTimerWarning() => OnTimerWarning?.Invoke();
    
    public static void FirePanelRequested(string panelName) => OnPanelRequested?.Invoke(panelName);
    public static void FireChordRequested(string chordName) => OnChordRequested?.Invoke(chordName);
    
    // Slot events
    public static void FireSlotUnlocked(int slotIndex) => OnSlotUnlocked?.Invoke(slotIndex);
    public static void FireSlotChordChanged(int slotIndex, string displayName) => OnSlotChordChanged?.Invoke(slotIndex, displayName);
    public static void FireSlotCompleted(int slotIndex, string chordName) => OnSlotCompleted?.Invoke(slotIndex, chordName);
    
    public static void FireTileDragStarted(TileView tile) => OnTileDragStarted?.Invoke(tile);
    public static void FireTileDragging(TileView tile, Vector2 pos) => OnTileDragging?.Invoke(tile, pos);
    public static void FireTileDragEnded(TileView tile, Vector2 pos) => OnTileDragEnded?.Invoke(tile, pos);
    public static void FireCellHovered(Vector2Int pos) => OnCellHovered?.Invoke(pos);
    public static void FireHoverCleared() => OnHoverCleared?.Invoke();
    
    /// <summary>
    /// Call this when changing scenes or restarting to prevent memory leaks
    /// </summary>
    public static void ClearAllListeners()
    {
        OnGameStart = null;
        OnGamePause = null;
        OnGameResume = null;
        OnGameOver = null;
        
        OnTileSpawned = null;
        OnTileMoved = null;
        OnTilesMerged = null;
        OnTileDestroyed = null;
        
        OnTriadMatched = null;
        OnRequestedChordCompleted = null;
        
        OnScoreChanged = null;
        OnScorePopup = null;
        OnComboChanged = null;
        
        OnTimerTick = null;
        OnTimerWarning = null;
        
        OnPanelRequested = null;
        OnChordRequested = null;
        
        OnSlotUnlocked = null;
        OnSlotChordChanged = null;
        OnSlotCompleted = null;
        
        OnTileDragStarted = null;
        OnTileDragging = null;
        OnTileDragEnded = null;
        OnCellHovered = null;
        OnHoverCleared = null;
    }
}