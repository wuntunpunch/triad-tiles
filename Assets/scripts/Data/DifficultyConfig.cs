using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject defining a difficulty level's note pool and valid chords.
/// Create instances: Right-click > Create > TriadTiles > Difficulty Config
/// </summary>
[CreateAssetMenu(fileName = "Grade1", menuName = "TriadTiles/Difficulty Config")]
public class DifficultyConfig : ScriptableObject
{
    [Header("Difficulty Info")]
    public string gradeName = "Grade 1";
    public string description = "C Major - Natural notes only";
    
    [Header("Note Pool")]
    [Tooltip("Notes that can spawn at this difficulty")]
    public string[] availableNotes = { "C", "D", "E", "F", "G", "A", "B" };
    
    [Header("Valid Chords")]
    public ChordDefinition[] validChords;
    
    [Header("Timing Adjustments")]
    [Tooltip("Multiplier for spawn interval (higher = slower spawning)")]
    public float spawnIntervalMultiplier = 1f;
    
    [Tooltip("Multiplier for game duration (higher = longer game)")]
    public float durationMultiplier = 1f;
}

[System.Serializable]
public class ChordDefinition
{
    public string chordName;        // e.g., "C", "Dm", "Em"
    public string displayName;      // e.g., "C Major", "D Minor"
    public string[] notes;          // e.g., ["C", "E", "G"]
    
    public bool MatchesNotes(List<string> testNotes)
    {
        if (testNotes == null || testNotes.Count != notes.Length) return false;
        
        // Sort both for comparison
        var sorted1 = new List<string>(notes);
        var sorted2 = new List<string>(testNotes);
        sorted1.Sort();
        sorted2.Sort();
        
        for (int i = 0; i < sorted1.Count; i++)
        {
            if (sorted1[i] != sorted2[i]) return false;
        }
        return true;
    }
}
