using System;

/// <summary>
/// Represents a single requested chord slot.
/// Tracks the chord, when it was requested, and slot state.
/// </summary>
[Serializable]
public class RequestedChordSlot
{
    public int SlotIndex { get; private set; }
    public string ChordName { get; private set; }
    public string DisplayName { get; private set; }
    public bool IsUnlocked { get; private set; }
    public bool IsActive => IsUnlocked && !string.IsNullOrEmpty(ChordName);
    public float TimeRequested { get; private set; }
    
    public RequestedChordSlot(int index)
    {
        SlotIndex = index;
        ChordName = null;
        DisplayName = null;
        IsUnlocked = index == 0; // First slot always unlocked
        TimeRequested = 0f;
    }
    
    public void Unlock()
    {
        IsUnlocked = true;
    }
    
    public void SetChord(string chordName, string displayName, float currentTime)
    {
        ChordName = chordName;
        DisplayName = displayName;
        TimeRequested = currentTime;
    }
    
    public void Clear()
    {
        ChordName = null;
        DisplayName = null;
        TimeRequested = 0f;
    }
    
    public void Reset()
    {
        ChordName = null;
        DisplayName = null;
        IsUnlocked = SlotIndex == 0;
        TimeRequested = 0f;
    }
}
