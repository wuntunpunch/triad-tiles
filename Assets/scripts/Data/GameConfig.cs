using UnityEngine;

/// <summary>
/// ScriptableObject for all game-wide configuration.
/// Create instances in Unity: Right-click > Create > TriadTiles > Game Config
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "TriadTiles/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Grid Settings")]
    public int gridSize = 5;
    [Range(0f, 0.2f)]
    public float spacingRatio = 0.08f;
    
    [Header("Timing")]
    public float gameDuration = 60f;
    public float spawnInterval = 3f;
    public float timerWarningThreshold = 10f;
    
    [Header("Scoring")]
    public int basePointsPerTriad = 100;
    public int requestedChordBonus = 200;
    public float requestedChordBonusTime = 5f;
    public int[] comboMultipliers = { 1, 2, 3, 4, 5 };
    public float comboResetTime = 3f;
    
    [Header("Requested Chord Slots")]
    [Tooltip("Time in seconds after game start when slot 2 unlocks")]
    public float slot2UnlockTime = 15f;
    [Tooltip("Time in seconds after game start when slot 3 unlocks")]
    public float slot3UnlockTime = 30f;
    
    [Header("Initial Setup")]
    [Tooltip("Starting tiles. 6 recommended for good early-game pacing with seeded start.")]
    public int initialTileCount = 6;
    
    [Header("Spawn Weighting")]
    [Tooltip("Chance (0-1) to spawn a note that completes an existing 2-note pair. 0.4-0.5 recommended.")]
    [Range(0f, 1f)]
    public float completionSpawnWeight = 0.45f;
    
    [Header("Animation Durations")]
    public float tileSpawnDuration = 0.3f;
    public float tileMergeDuration = 0.2f;
    public float tileMoveDuration = 0.2f;
    public float tileDestroyDuration = 0.3f;
}