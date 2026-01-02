using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles chord matching and merge validation.
/// </summary>
public class TriadMatcher
{
    private DifficultyConfig config;
    
    public TriadMatcher(DifficultyConfig config)
    {
        this.config = config;
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
    /// Checks if two note sets can be merged (total <= 3 unique notes).
    /// </summary>
    public bool CanMerge(List<string> notes1, List<string> notes2)
    {
        if (notes1 == null || notes2 == null) return false;
        
        var combined = new HashSet<string>(notes1);
        foreach (var note in notes2)
            combined.Add(note);
        
        return combined.Count <= 3;
    }
    
    /// <summary>
    /// Merges two note sets into one.
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
    /// Checks both:
    /// 1. Merged tiles containing 3 notes that form a valid chord
    /// 2. Three adjacent single-note tiles in a line forming a valid chord
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
        
        // Check horizontal lines of 3 single-note tiles
        for (int row = 0; row < size; row++)
        {
            for (int col = 0; col <= size - 3; col++)
            {
                var match = CheckLine(board, row, col, 0, 1);
                if (match != null) matches.Add(match);
            }
        }
        
        // Check vertical lines of 3 single-note tiles
        for (int col = 0; col < size; col++)
        {
            for (int row = 0; row <= size - 3; row++)
            {
                var match = CheckLine(board, row, col, 1, 0);
                if (match != null) matches.Add(match);
            }
        }
        
        // Check diagonal (top-left to bottom-right)
        for (int row = 0; row <= size - 3; row++)
        {
            for (int col = 0; col <= size - 3; col++)
            {
                var match = CheckLine(board, row, col, 1, 1);
                if (match != null) matches.Add(match);
            }
        }
        
        // Check diagonal (top-right to bottom-left)
        for (int row = 0; row <= size - 3; row++)
        {
            for (int col = 2; col < size; col++)
            {
                var match = CheckLine(board, row, col, 1, -1);
                if (match != null) matches.Add(match);
            }
        }
        
        return matches;
    }
    
    private TriadMatch CheckLine(BoardModel board, int startRow, int startCol, int dRow, int dCol)
    {
        var positions = new List<Vector2Int>();
        var notes = new List<string>();
        
        for (int i = 0; i < 3; i++)
        {
            int row = startRow + i * dRow;
            int col = startCol + i * dCol;
            
            var tile = board.GetTile(row, col);
            
            // Only match single-note tiles in a line
            if (tile == null || !tile.IsSingleNote)
                return null;
            
            positions.Add(new Vector2Int(row, col));
            notes.Add(tile.PrimaryNote);
        }
        
        string chordName = CheckForChord(notes);
        if (chordName == null) return null;
        
        return new TriadMatch(positions, chordName, notes);
    }
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