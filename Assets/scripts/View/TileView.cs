using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

/// <summary>
/// Visual representation of a tile. Handles rendering and input only.
/// Game logic lives in GameController.
/// </summary>
public class TileView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI noteText;
    [SerializeField] private Image backgroundImage;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color errorFlashColor = Color.red;
    [SerializeField] private Color successColor = Color.green;
    
    // Data this view represents
    public TileData Data { get; private set; }
    
    // Components
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Vector2 dragOffset;
    
    // Configuration (set by BoardView)
    private NoteColors colorConfig;
    private GameConfig gameConfig;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    // ===== INITIALIZATION =====
    
    public void Initialize(TileData data, NoteColors colors, GameConfig config)
    {
        Data = data;
        colorConfig = colors;
        gameConfig = config;
        UpdateVisuals();
    }
    
    public void SetSize(float size)
    {
        rectTransform.sizeDelta = new Vector2(size, size);
    }
    
    // ===== VISUAL UPDATES =====
    
    public void UpdateVisuals()
    {
        if (Data == null) return;
        
        // Update text
        if (noteText != null)
        {
            noteText.text = Data.IsSingleNote 
                ? Data.PrimaryNote 
                : string.Join(" ", Data.notes);
        }
        
        // Update color
        if (backgroundImage != null && colorConfig != null)
        {
            backgroundImage.color = colorConfig.GetColorForNotes(Data.notes);
        }
    }
    
    // ===== POSITIONING =====
    
    public void SetPosition(Vector2 anchoredPosition)
    {
        rectTransform.anchoredPosition = anchoredPosition;
    }
    
    public void AnimateToPosition(Vector2 targetPosition, float duration = 0.2f)
    {
        rectTransform.DOAnchorPos(targetPosition, duration).SetEase(Ease.OutQuad);
    }
    
    public Vector2 GetGridPosition(int row, int col, float cellSize, float spacing, int gridSize)
    {
        float totalSize = (cellSize * gridSize) + (spacing * (gridSize - 1));
        float startOffset = -totalSize / 2f + cellSize / 2f;
        
        float x = startOffset + col * (cellSize + spacing);
        float y = -startOffset - row * (cellSize + spacing);
        
        return new Vector2(x, y);
    }
    
    // ===== ANIMATIONS =====
    
    public void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        float duration = gameConfig != null ? gameConfig.tileSpawnDuration : 0.3f;
        transform.DOScale(originalScale, duration).SetEase(Ease.OutBack);
    }
    
    public void PlayMergeAnimation()
    {
        float duration = gameConfig != null ? gameConfig.tileMergeDuration : 0.2f;
        transform.DOPunchScale(Vector3.one * 0.2f, duration);
    }
    
    public void PlayDestroyAnimation(System.Action onComplete = null)
    {
        float duration = gameConfig != null ? gameConfig.tileDestroyDuration : 0.3f;
        
        if (backgroundImage != null)
            backgroundImage.DOColor(successColor, duration * 0.5f);
        
        transform.DOScale(originalScale * 1.3f, duration).SetEase(Ease.OutQuad);
        canvasGroup.DOFade(0f, duration).OnComplete(() => {
            onComplete?.Invoke();
        });
    }
    
    public void PlayErrorAnimation()
    {
        if (backgroundImage != null)
        {
            Color original = backgroundImage.color;
            backgroundImage.color = errorFlashColor;
            backgroundImage.DOColor(original, 0.3f);
        }
        transform.DOShakePosition(0.3f, 10f, 20);
    }
    
    // ===== DRAG INPUT =====
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Calculate offset from center
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        dragOffset = (Vector2)rectTransform.anchoredPosition - localPoint;
        
        // Visual feedback
        transform.DOScale(originalScale * 1.1f, 0.1f);
        canvasGroup.DOFade(0.8f, 0.1f);
        canvasGroup.blocksRaycasts = false;
        
        // Bring to front
        transform.SetAsLastSibling();
        
        // Notify via events
        GameEvents.FireTileDragStarted(this);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );
        rectTransform.anchoredPosition = localPoint + dragOffset;
        
        GameEvents.FireTileDragging(this, eventData.position);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.DOScale(originalScale, 0.2f);
        canvasGroup.DOFade(1f, 0.2f);
        canvasGroup.blocksRaycasts = true;
        
        GameEvents.FireTileDragEnded(this, eventData.position);
    }
    
    // ===== CLEANUP =====
    
    void OnDestroy()
    {
        transform.DOKill();
        backgroundImage?.DOKill();
        canvasGroup?.DOKill();
    }
}
