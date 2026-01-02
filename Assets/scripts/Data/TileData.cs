using System;
using System.Collections.Generic;

/// <summary>
/// Pure data class representing a tile's state.
/// No Unity dependencies - can be used in tests.
/// </summary>
[Serializable]
public class TileData
{
    public List<string> notes;
    public int row;
    public int col;
    
    public TileData(string note, int row, int col)
    {
        this.notes = new List<string> { note };
        this.row = row;
        this.col = col;
    }
    
    public TileData(List<string> notes, int row, int col)
    {
        this.notes = new List<string>(notes);
        this.row = row;
        this.col = col;
    }
    
    public TileData Clone()
    {
        return new TileData(new List<string>(notes), row, col);
    }
    
    public bool IsSingleNote => notes.Count == 1;
    public bool IsComplete => notes.Count == 3;
    
    public string PrimaryNote => notes.Count > 0 ? notes[0] : "";
    
    public override string ToString()
    {
        return $"[{string.Join(",", notes)}] at ({row},{col})";
    }
}
