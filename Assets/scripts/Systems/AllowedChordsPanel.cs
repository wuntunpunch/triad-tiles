using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the current grade and chords that can be built with notes currently on the board.
/// Updates dynamically as tiles spawn, move, merge, and are destroyed.
/// </summary>
public class AllowedChordsPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text gradeLabel;
    [SerializeField] private Transform chordContainer;
    [SerializeField] private GameObject chordBadgePrefab;
    
    [Header("Font")]
    [SerializeField] private TMP_FontAsset font;
    
    [Header("Styling")]
    [SerializeField] private Color badgeColor = new Color(0.2f, 0.3f, 0.5f, 0.9f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 18;
    
    [Header("Display Options")]
    [SerializeField] private bool useShortNames = true;
    [SerializeField] private string noBuildableText = "—";
    
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
    private void OnBoardChanged(Vector2Int pos, TileData data) => RefreshBuildableChords();
    private void OnBoardChanged(Vector2Int from, Vector2Int to, TileData data) => RefreshBuildableChords();
    private void OnBoardChanged(Vector2Int pos) => RefreshBuildableChords();
    
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
        
        RefreshBuildableChords();
    }
    
    private void RefreshBuildableChords()
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
        
        // Find buildable chords
        List<ChordDefinition> buildable = GetBuildableChords(availableNotes);
        
        // Create badges
        if (buildable.Count == 0)
        {
            // Show placeholder when no chords are buildable
            CreateBadge(noBuildableText, badgeColor);
        }
        else
        {
            foreach (var chord in buildable)
            {
                string displayText = useShortNames ? chord.chordName : chord.displayName;
                if (string.IsNullOrEmpty(displayText)) displayText = chord.chordName;
                
                CreateBadge(displayText, badgeColor);
            }
        }
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
    
    private List<ChordDefinition> GetBuildableChords(HashSet<string> availableNotes)
    {
        List<ChordDefinition> buildable = new List<ChordDefinition>();
        
        if (currentDifficulty == null || currentDifficulty.validChords == null)
            return buildable;
        
        foreach (var chord in currentDifficulty.validChords)
        {
            if (CanBuildChord(chord, availableNotes))
            {
                buildable.Add(chord);
            }
        }
        
        return buildable;
    }
    
    private bool CanBuildChord(ChordDefinition chord, HashSet<string> availableNotes)
    {
        if (chord.notes == null || chord.notes.Length == 0)
            return false;
        
        // Check if ALL notes required for this chord are available on the board
        foreach (var note in chord.notes)
        {
            if (!availableNotes.Contains(note))
                return false;
        }
        
        return true;
    }
    
    private void CreateBadge(string text, Color bgColor)
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
            badge = CreateDefaultBadge(text);
        }
        
        var image = badge.GetComponent<Image>();
        if (image != null)
        {
            image.color = bgColor;
        }
        
        activeBadges.Add(badge);
    }
    
    private GameObject CreateDefaultBadge(string text)
    {
        // Create badge container
        GameObject badge = new GameObject("ChordBadge", typeof(RectTransform), typeof(Image));
        badge.transform.SetParent(chordContainer, false);
        
        var badgeRect = badge.GetComponent<RectTransform>();
        badgeRect.sizeDelta = new Vector2(70, 32);
        
        var badgeImage = badge.GetComponent<Image>();
        badgeImage.color = badgeColor;
        
        // Create TMP text child
        GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(badge.transform, false);
        
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.fontStyle = FontStyles.Bold;
        
        if (font != null)
        {
            tmp.font = font;
        }
        
        return badge;
    }
}