using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject mapping notes to colors.
/// Create instance: Right-click > Create > TriadTiles > Note Colors
/// </summary>
[CreateAssetMenu(fileName = "NoteColors", menuName = "TriadTiles/Note Colors")]
public class NoteColors : ScriptableObject
{
    [System.Serializable]
    public class NoteColorMapping
    {
        public string note;
        public Color color = Color.white;
    }
    
    [Header("Natural Notes")]
    public Color C = new Color(1f, 0.3f, 0.3f);      // Red
    public Color D = new Color(1f, 0.7f, 0.2f);      // Orange
    public Color E = new Color(1f, 1f, 0.3f);        // Yellow
    public Color F = new Color(0.4f, 1f, 0.4f);      // Green
    public Color G = new Color(0.3f, 0.7f, 1f);      // Light Blue
    public Color A = new Color(0.5f, 0.4f, 1f);      // Purple
    public Color B = new Color(1f, 0.4f, 0.8f);      // Pink
    
    [Header("Sharps")]
    public Color CSharp = new Color(0.8f, 0.2f, 0.2f);
    public Color DSharp = new Color(0.8f, 0.5f, 0.1f);
    public Color FSharp = new Color(0.3f, 0.8f, 0.3f);
    public Color GSharp = new Color(0.2f, 0.5f, 0.8f);
    public Color ASharp = new Color(0.4f, 0.3f, 0.8f);
    
    [Header("Flats")]
    public Color Db = new Color(0.9f, 0.4f, 0.2f);
    public Color Eb = new Color(0.9f, 0.9f, 0.2f);
    public Color Gb = new Color(0.3f, 0.9f, 0.7f);
    public Color Ab = new Color(0.4f, 0.5f, 0.9f);
    public Color Bb = new Color(0.9f, 0.5f, 0.7f);
    
    [Header("Special")]
    public Color fallback = Color.white;
    public Color merged2Notes = new Color(0.8f, 0.8f, 0.8f);
    public Color merged3Notes = new Color(1f, 0.9f, 0.7f);
    
    private Dictionary<string, Color> colorMap;
    
    public Color GetColor(string note)
    {
        if (colorMap == null) BuildColorMap();
        return colorMap.ContainsKey(note) ? colorMap[note] : fallback;
    }
    
    public Color GetColorForNotes(List<string> notes)
    {
        if (notes == null || notes.Count == 0) return fallback;
        
        if (notes.Count == 1)
            return GetColor(notes[0]);
        
        if (notes.Count == 2)
        {
            Color c1 = GetColor(notes[0]);
            Color c2 = GetColor(notes[1]);
            return Color.Lerp(c1, c2, 0.5f);
        }
        
        // 3 notes - blend all three
        Color c = GetColor(notes[0]);
        c = Color.Lerp(c, GetColor(notes[1]), 0.5f);
        c = Color.Lerp(c, GetColor(notes[2]), 0.33f);
        return c;
    }
    
    private void BuildColorMap()
    {
        colorMap = new Dictionary<string, Color>
        {
            { "C", C }, { "D", D }, { "E", E }, { "F", F },
            { "G", G }, { "A", A }, { "B", B },
            { "C#", CSharp }, { "D#", DSharp }, { "F#", FSharp },
            { "G#", GSharp }, { "A#", ASharp },
            { "Db", Db }, { "Eb", Eb }, { "Gb", Gb },
            { "Ab", Ab }, { "Bb", Bb }
        };
    }
    
    private void OnValidate()
    {
        // Rebuild map when values change in inspector
        colorMap = null;
    }
}
