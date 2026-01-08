using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages multiple requested chord slots that unlock over time.
/// Validates chord matches against all active slots and handles rewards.
/// </summary>
public class RequestedChordsManager : MonoBehaviour
{
    public static RequestedChordsManager Instance { get; private set; }
    
    [Header("Configuration")]
    [SerializeField] private GameConfig gameConfig;
    
    private RequestedChordSlot[] slots;
    private DifficultyConfig currentDifficulty;
    private float gameStartTime;
    private bool isActive;
    
    // Track which chords are currently requested to avoid duplicates
    private HashSet<string> activeChordNames = new HashSet<string>();
    
    public const int MaxSlots = 3;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSlots();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void OnEnable()
    {
        GameEvents.OnGameStart += HandleGameStart;
        GameEvents.OnGameOver += HandleGameOver;
    }
    
    void OnDisable()
    {
        GameEvents.OnGameStart -= HandleGameStart;
        GameEvents.OnGameOver -= HandleGameOver;
    }
    
    void Update()
    {
        if (!isActive) return;
        
        CheckSlotUnlocks();
    }
    
    private void InitializeSlots()
    {
        slots = new RequestedChordSlot[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
        {
            slots[i] = new RequestedChordSlot(i);
        }
    }
    
    private void HandleGameStart()
    {
        // Get current difficulty from GameController
        currentDifficulty = GameController.Instance?.CurrentDifficulty;
        
        // Reset all slots
        foreach (var slot in slots)
        {
            slot.Reset();
        }
        activeChordNames.Clear();
        
        gameStartTime = Time.time;
        isActive = true;
        
        // Request chord for first slot immediately
        RequestChordForSlot(0);
    }
    
    private void HandleGameOver(int finalScore)
    {
        isActive = false;
    }
    
    private void CheckSlotUnlocks()
    {
        float elapsed = Time.time - gameStartTime;
        
        // Check slot 2 unlock
        if (!slots[1].IsUnlocked && elapsed >= gameConfig.slot2UnlockTime)
        {
            UnlockSlot(1);
        }
        
        // Check slot 3 unlock
        if (!slots[2].IsUnlocked && elapsed >= gameConfig.slot3UnlockTime)
        {
            UnlockSlot(2);
        }
    }
    
    private void UnlockSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return;
        if (slots[slotIndex].IsUnlocked) return;
        
        slots[slotIndex].Unlock();
        GameEvents.FireSlotUnlocked(slotIndex);
        
        // Immediately request a chord for the newly unlocked slot
        RequestChordForSlot(slotIndex);
    }
    
    private void RequestChordForSlot(int slotIndex)
    {
        if (currentDifficulty == null || currentDifficulty.validChords.Length == 0)
            return;
        
        var slot = slots[slotIndex];
        if (!slot.IsUnlocked) return;
        
        // Build list of available chords (not already in another slot)
        List<ChordDefinition> available = new List<ChordDefinition>();
        foreach (var chord in currentDifficulty.validChords)
        {
            if (!activeChordNames.Contains(chord.chordName))
            {
                available.Add(chord);
            }
        }
        
        // If all chords are taken, allow duplicates (edge case with few chords)
        if (available.Count == 0)
        {
            available.AddRange(currentDifficulty.validChords);
        }
        
        // Select random chord
        var selected = available[Random.Range(0, available.Count)];
        
        // Clear old chord from tracking if slot had one
        if (!string.IsNullOrEmpty(slot.ChordName))
        {
            activeChordNames.Remove(slot.ChordName);
        }
        
        // Set new chord
        slot.SetChord(selected.chordName, selected.displayName, Time.time);
        activeChordNames.Add(selected.chordName);
        
        // Fire event for UI
        string displayName = string.IsNullOrEmpty(selected.displayName) 
            ? selected.chordName 
            : selected.displayName;
        GameEvents.FireSlotChordChanged(slotIndex, displayName);
    }
    
    /// <summary>
    /// Called by GameController when a chord is matched.
    /// Returns the slot index if matched, -1 if no match.
    /// </summary>
    public int CheckChordMatch(string chordName)
    {
        for (int i = 0; i < MaxSlots; i++)
        {
            if (slots[i].IsActive && slots[i].ChordName == chordName)
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Called when a requested chord is successfully matched.
    /// Clears the slot and requests a new chord.
    /// </summary>
    public void CompleteSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return;
        
        var slot = slots[slotIndex];
        string completedChord = slot.DisplayName;
        
        // Remove from active tracking
        if (!string.IsNullOrEmpty(slot.ChordName))
        {
            activeChordNames.Remove(slot.ChordName);
        }
        
        // Clear and request new
        slot.Clear();
        GameEvents.FireSlotCompleted(slotIndex, completedChord);
        
        // Request new chord for this slot
        RequestChordForSlot(slotIndex);
    }
    
    /// <summary>
    /// Get the current state of a slot for UI purposes.
    /// </summary>
    public RequestedChordSlot GetSlot(int index)
    {
        if (index < 0 || index >= MaxSlots) return null;
        return slots[index];
    }
    
    /// <summary>
    /// Check if any slot has this chord requested.
    /// </summary>
    public bool IsChordRequested(string chordName)
    {
        return activeChordNames.Contains(chordName);
    }
}
