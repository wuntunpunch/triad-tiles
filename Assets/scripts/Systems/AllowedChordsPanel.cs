using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the current grade and chord hints based on notes on the board.
/// Shows fully buildable chords and "almost there" chords (2 of 3 notes).
/// </summary>
public class AllowedChordsPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text gradeLabel;
    [SerializeField] private Transform chordContainer;
    [SerializeField] private GameObject chordBadgePrefab;
    
    [Header("Font")]
    [SerializeField] private TMP_FontAsset font;
    
    [Header("Styling - Buildable")]
    [SerializeField] private Color buildableColor = new Color(0.2f, 0.5f, 0.3f, 0.9f);
    [SerializeField] private Color buildableTextColor = Color.white;
    
    [Header("Styling - Almost There")]
    [SerializeField] private Color almostColor = new Color(0.4f, 0.4f, 0.5f, 0.9f);
    [SerializeField] private Color almostTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    
    [Header("Badge Size")]
    [SerializeField] private Vector2 badgeSize = new Vector2(130, 45);
    
    [Header("Display Options")]
    [SerializeField] private bool useShortNames = true;
    [SerializeField] private int fontSize = 18;
    [SerializeField] private bool autoSizeText = true;
    [SerializeField] private string noChordsText = "—";
    
    private DifficultyConfig currentDifficulty;
    private List<GameObject> activeBadges = new List<GameObject>();
    
    void OnEnable()
    {
        // Subscribe to board change events
        GameEvents.OnTileSpawned += OnBoardChanged;
        GameEvents.OnTileMoved += OnBoardChanged;
        GameEvents.OnTilesMerged += OnBoardChanged;
        GameEvents.OnTileDestroyed += OnBoardChanged;
        
        // Initial refresh
        RefreshPanel();
    }
    
    void OnDisable()
    {
        GameEvents.OnTileSpawned -= OnBoardChanged;
        GameEvents.OnTileMoved -= OnBoardChanged;
        GameEvents.OnTilesMerged -= OnBoardChanged;
        GameEvents.OnTileDestroyed -= OnBoardChanged;
    }
    
    // Event handlers - different signatures but all trigger refresh
    private void OnBoardChanged(Vector2Int pos, TileData data) => RefreshChordHints();
    private void OnBoardChanged(Vector2Int from, Vector2Int to, TileData data) => RefreshChordHints();
    private void OnBoardChanged(Vector2Int pos) => RefreshChordHints();
    
    private void RefreshPanel()
    {
        // Get current difficulty
        if (GameController.Instance != null)
        {
            currentDifficulty = GameController.Instance.CurrentDifficulty;
        }
        
        if (currentDifficulty == null)
        {
            if (gradeLabel != null) gradeLabel.text = "";
            return;
        }
        
        // Update grade label
        if (gradeLabel != null)
        {
            gradeLabel.text = $"Chords available now for {currentDifficulty.gradeName}";
            if (font != null) gradeLabel.font = font;
        }
        
        RefreshChordHints();
    }
    
    private void RefreshChordHints()
    {
        if (chordContainer == null) return;
        
        // Clear existing badges
        foreach (var badge in activeBadges)
        {
            if (badge != null) Destroy(badge);
        }
        activeBadges.Clear();
        
        // Get current state
        if (GameController.Instance == null) return;
        
        currentDifficulty = GameController.Instance.CurrentDifficulty;
        BoardModel board = GameController.Instance.Board;
        
        if (currentDifficulty == null || board == null) return;
        
        // Collect all available notes on the board
        HashSet<string> availableNotes = GetAvailableNotes(board);
        
        // Find buildable and almost-there chords
        List<ChordHint> hints = GetChordHints(availableNotes);
        
        // Create badges
        if (hints.Count == 0)
        {
            CreateBadge(noChordsText, almostColor, almostTextColor);
        }
        else
        {
            // Show buildable chords first, then almost-there
            foreach (var hint in hints)
            {
                if (hint.isBuildable)
                {
                    string text = GetChordDisplayName(hint.chord);
                    CreateBadge(text, buildableColor, buildableTextColor);
                }
            }
            
            foreach (var hint in hints)
            {
                if (!hint.isBuildable)
                {
                    string text = $"{GetChordDisplayName(hint.chord)} (need {hint.missingNote})";
                    CreateBadge(text, almostColor, almostTextColor);
                }
            }
        }
    }
    
    private string GetChordDisplayName(ChordDefinition chord)
    {
        if (useShortNames)
        {
            return chord.chordName;
        }
        return string.IsNullOrEmpty(chord.displayName) ? chord.chordName : chord.displayName;
    }
    
    private HashSet<string> GetAvailableNotes(BoardModel board)
    {
        HashSet<string> notes = new HashSet<string>();
        
        var occupiedCells = board.GetOccupiedCells();
        foreach (var pos in occupiedCells)
        {
            TileData tile = board.GetTile(pos);
            if (tile != null && tile.notes != null)
            {
                foreach (var note in tile.notes)
                {
                    notes.Add(note);
                }
            }
        }
        
        return notes;
    }
    
    private List<ChordHint> GetChordHints(HashSet<string> availableNotes)
    {
        List<ChordHint> hints = new List<ChordHint>();
        
        if (currentDifficulty == null || currentDifficulty.validChords == null)
            return hints;
        
        foreach (var chord in currentDifficulty.validChords)
        {
            if (chord.notes == null || chord.notes.Length == 0)
                continue;
            
            // Count how many notes we have
            int matchCount = 0;
            string missingNote = null;
            
            foreach (var note in chord.notes)
            {
                if (availableNotes.Contains(note))
                {
                    matchCount++;
                }
                else
                {
                    missingNote = note;
                }
            }
            
            // Fully buildable (all 3 notes)
            if (matchCount == chord.notes.Length)
            {
                hints.Add(new ChordHint
                {
                    chord = chord,
                    isBuildable = true,
                    missingNote = null
                });
            }
            // Almost there (2 of 3 notes)
            else if (matchCount == chord.notes.Length - 1)
            {
                hints.Add(new ChordHint
                {
                    chord = chord,
                    isBuildable = false,
                    missingNote = missingNote
                });
            }
        }
        
        return hints;
    }
    
    private void CreateBadge(string text, Color bgColor, Color textColor)
    {
        GameObject badge;
        
        if (chordBadgePrefab != null)
        {
            badge = Instantiate(chordBadgePrefab, chordContainer);
            
            var tmpLabel = badge.GetComponentInChildren<TMP_Text>();
            if (tmpLabel != null)
            {
                tmpLabel.text = text;
                tmpLabel.color = textColor;
                if (font != null) tmpLabel.font = font;
            }
        }
        else
        {
            badge = CreateDefaultBadge(text, textColor);
        }
        
        var image = badge.GetComponent<Image>();
        if (image != null)
        {
            image.color = bgColor;
        }
        
        activeBadges.Add(badge);
    }
    
    private GameObject CreateDefaultBadge(string text, Color textColor)
    {
        // Create badge container
        GameObject badge = new GameObject("ChordBadge", typeof(RectTransform), typeof(Image));
        badge.transform.SetParent(chordContainer, false);
        
        var badgeRect = badge.GetComponent<RectTransform>();
        badgeRect.sizeDelta = badgeSize;
        
        // Create TMP text child
        GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(badge.transform, false);
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(5, 0);
        textRect.offsetMax = new Vector2(-5, 0);
        
        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.fontStyle = FontStyles.Bold;
        
        if (autoSizeText)
        {
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 10;
            tmp.fontSizeMax = fontSize;
        }
        else
        {
            tmp.enableAutoSizing = false;
        }
        
        if (font != null)
        {
            tmp.font = font;
        }
        
        return badge;
    }
    
    private struct ChordHint
    {
        public ChordDefinition chord;
        public bool isBuildable;
        public string missingNote;
    }
}