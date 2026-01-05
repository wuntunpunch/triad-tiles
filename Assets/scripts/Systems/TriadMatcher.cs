using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles chord matching and merge validation.
/// Now includes early validation to prevent merging notes that can't form valid triads.
/// </summary>
public class TriadMatcher
{
    private DifficultyConfig config;
    
    // Pre-computed valid pairs for quick lookup
    // Key format: "NoteA|NoteB" where notes are alphabetically sorted
    private HashSet<string> validNotePairs;
    
    public TriadMatcher(DifficultyConfig config)
    {
        this.config = config;
        BuildValidPairsCache();
    }
    
    /// <summary>
    /// Pre-computes all valid note pairs from the current difficulty's chords.
    /// A pair is valid if both notes appear together in at least one chord.
    /// </summary>
    private void BuildValidPairsCache()
    {
        validNotePairs = new HashSet<string>();
        
        if (config == null || config.validChords == null) return;
        
        foreach (var chord in config.validChords)
        {
            if (chord.notes == null || chord.notes.Length < 2) continue;
            
            // Generate all pairs from this chord's notes
            for (int i = 0; i < chord.notes.Length; i++)
            {
                for (int j = i + 1; j < chord.notes.Length; j++)
                {
                    string pairKey = GetPairKey(chord.notes[i], chord.notes[j]);
                    validNotePairs.Add(pairKey);
                }
            }
        }
        
        Debug.Log($"[TriadMatcher] Built pairs cache with {validNotePairs.Count} valid pairs for {config.gradeName}");
    }
    
    /// <summary>
    /// Creates a consistent key for a note pair (alphabetically sorted).
    /// </summary>
    private string GetPairKey(string note1, string note2)
    {
        // Sort alphabetically for consistent key regardless of order
        if (string.CompareOrdinal(note1, note2) <= 0)
            return $"{note1}|{note2}";
        else
            return $"{note2}|{note1}";
    }
    
    /// <summary>
    /// Checks if two notes can potentially form part of a valid triad.
    /// </summary>
    public bool IsValidPair(string note1, string note2)
    {
        if (note1 == note2) return false; // Same note, no point merging
        string pairKey = GetPairKey(note1, note2);
        return validNotePairs.Contains(pairKey);
    }
    
    /// <summary>
    /// Checks if a set of notes (2 or 3) can be merged.
    /// For 2 notes: checks if the pair exists in any valid chord (blocks dead-end pairs).
    /// For 3 notes: always allows (GameController handles punishment for invalid triads).
    /// </summary>
    public bool CanFormValidChord(List<string> notes)
    {
        if (notes == null || notes.Count < 2) return true; // Single notes are always valid
        
        var uniqueNotes = new HashSet<string>(notes);
        
        if (uniqueNotes.Count == 2)
        {
            // Check if this pair exists in any valid chord
            var noteList = new List<string>(uniqueNotes);
            return IsValidPair(noteList[0], noteList[1]);
        }
        else if (uniqueNotes.Count == 3)
        {
            // Allow all 3-note merges through - GameController will check validity
            // and destroy/punish if it's not a valid chord
            return true;
        }
        
        return false; // More than 3 unique notes is never valid
    }
    
    /// <summary>
    /// Checks if notes form a valid chord.
    /// Returns chord name if valid, null otherwise.
    /// </summary>
    public string CheckForChord(List<string> notes)
    {
        if (notes == null || notes.Count != 3) return null;
        if (config == null || config.validChords == null) return null;
        
        foreach (var chord in config.validChords)
        {
            if (chord.MatchesNotes(notes))
                return chord.chordName;
        }
        return null;
    }
    
    /// <summary>
    /// Checks if two note sets can be merged.
    /// Validates that:
    /// 1. Combined total is <= 3 unique notes
    /// 2. The combined notes can form (or contribute to) a valid chord
    /// </summary>
    public bool CanMerge(List<string> notes1, List<string> notes2)
    {
        if (notes1 == null || notes2 == null) return false;
        
        var combined = new HashSet<string>(notes1);
        foreach (var note in notes2)
            combined.Add(note);
        
        // Can't have more than 3 unique notes
        if (combined.Count > 3) return false;
        
        // Validate the combined notes can form a valid chord
        return CanFormValidChord(new List<string>(combined));
    }
    
    /// <summary>
    /// Gets the reason why a merge would fail (for UI feedback).
    /// Returns null if merge is valid.
    /// </summary>
    public string GetMergeFailureReason(List<string> notes1, List<string> notes2)
    {
        if (notes1 == null || notes2 == null) return "Invalid notes";
        
        var combined = new HashSet<string>(notes1);
        foreach (var note in notes2)
            combined.Add(note);
        
        if (combined.Count > 3)
            return "Too many notes";
        
        if (!CanFormValidChord(new List<string>(combined)))
            return "No matching chord";
        
        return null;
    }
    
    /// <summary>
    /// Merges two note sets into one.
    /// Call CanMerge first to validate!
    /// </summary>
    public List<string> MergeNotes(List<string> notes1, List<string> notes2)
    {
        var combined = new HashSet<string>(notes1);
        foreach (var note in notes2)
            combined.Add(note);
        return new List<string>(combined);
    }
    
    /// <summary>
    /// Finds all triad matches on the board.
    /// Checks merged tiles containing 3 notes that form a valid chord.
    /// Returns list of matches (each match is list of positions + chord name).
    /// </summary>
    public List<TriadMatch> FindMatches(BoardModel board)
    {
        var matches = new List<TriadMatch>();
        int size = board.GridSize;
        
        // Check for merged tiles that form complete triads
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col < size; col++)
            {
                var tile = board.GetTile(row, col);
                if (tile != null && tile.notes.Count == 3)
                {
                    string chordName = CheckForChord(tile.notes);
                    if (chordName != null)
                    {
                        var positions = new List<Vector2Int> { new Vector2Int(row, col) };
                        matches.Add(new TriadMatch(positions, chordName, tile.notes));
                    }
                }
            }
        }
        
        return matches;
    }
    
    /// <summary>
    /// Call this when difficulty changes to rebuild the pairs cache.
    /// </summary>
    public void UpdateConfig(DifficultyConfig newConfig)
    {
        config = newConfig;
        BuildValidPairsCache();
    }
    
    /// <summary>
    /// Gets all valid pairs for debugging/UI purposes.
    /// </summary>
    public IEnumerable<string> GetValidPairs() => validNotePairs;
}

/// <summary>
/// Represents a matched triad on the board.
/// </summary>
public class TriadMatch
{
    public List<Vector2Int> Positions { get; private set; }
    public string ChordName { get; private set; }
    public List<string> Notes { get; private set; }
    
    public TriadMatch(List<Vector2Int> positions, string chordName, List<string> notes)
    {
        Positions = positions;
        ChordName = chordName;
        Notes = notes;
    }
    
    public Vector2Int CenterPosition => Positions.Count > 1 ? Positions[1] : Positions[0];
}