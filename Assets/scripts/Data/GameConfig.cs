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
    
    [Header("Initial Setup")]
    public int initialTileCount = 4;
    
    [Header("Animation Durations")]
    public float tileSpawnDuration = 0.3f;
    public float tileMergeDuration = 0.2f;
    public float tileMoveDuration = 0.2f;
    public float tileDestroyDuration = 0.3f;
}