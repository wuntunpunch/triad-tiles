using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// UI component for a single requested chord slot.
/// Handles display, animations, and state transitions.
/// </summary>
public class RequestedChordSlotView : MonoBehaviour
{
    [Header("Slot Identity")]
    [SerializeField] private int slotIndex;
    
    [Header("UI References")]
    [SerializeField] private GameObject contentContainer;
    [SerializeField] private TextMeshProUGUI chordNameText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Visual Settings")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color completedFlashColor = Color.green;
    
    [Header("Animation Settings")]
    [SerializeField] private float appearDuration = 0.3f;
    [SerializeField] private float completedFlashDuration = 0.2f;
    
    private bool isUnlocked;
    private bool isVisible;
    
    void Awake()
    {
        // Ensure we have a CanvasGroup for fading
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Start hidden if not slot 0
        if (slotIndex > 0)
        {
            canvasGroup.alpha = 0f;
            isVisible = false;
            isUnlocked = false;
        }
        else
        {
            canvasGroup.alpha = 1f;
            isVisible = true;
            isUnlocked = true;
        }
    }
    
    void OnEnable()
    {
        GameEvents.OnSlotUnlocked += HandleSlotUnlocked;
        GameEvents.OnSlotChordChanged += HandleChordChanged;
        GameEvents.OnSlotCompleted += HandleSlotCompleted;
        GameEvents.OnSlotsReset += HandleSlotsReset;
    }
    
    void OnDisable()
    {
        GameEvents.OnSlotUnlocked -= HandleSlotUnlocked;
        GameEvents.OnSlotChordChanged -= HandleChordChanged;
        GameEvents.OnSlotCompleted -= HandleSlotCompleted;
        GameEvents.OnSlotsReset -= HandleSlotsReset;
    }
    
    private void HandleSlotsReset()
    {
        // Reset state
        isUnlocked = slotIndex == 0;
        
        if (slotIndex == 0)
        {
            // First slot is always visible
            canvasGroup.alpha = 1f;
            isVisible = true;
        }
        else
        {
            // Other slots start hidden
            canvasGroup.alpha = 0f;
            isVisible = false;
        }
        
        // Clear text until chord is assigned
        if (chordNameText != null)
        {
            chordNameText.text = "";
        }
    }
    
    private void HandleSlotUnlocked(int index)
    {
        if (index != slotIndex) return;
        
        isUnlocked = true;
        AnimateAppear();
    }
    
    private void HandleChordChanged(int index, string displayName)
    {
        if (index != slotIndex) return;
        
        Debug.Log($"[RequestedChordSlotView] Slot {slotIndex} received chord: {displayName}");
        
        if (chordNameText != null)
        {
            chordNameText.text = displayName;
        }
        else
        {
            Debug.LogWarning($"[RequestedChordSlotView] Slot {slotIndex} has no chordNameText assigned!");
        }
        
        // If this is the first chord for this slot and it's not visible yet, appear
        if (isUnlocked && !isVisible)
        {
            AnimateAppear();
        }
    }
    
    private void HandleSlotCompleted(int index, string chordName)
    {
        if (index != slotIndex) return;
        
        AnimateCompleted();
    }
    
    private void AnimateAppear()
    {
        isVisible = true;
        
        // Scale and fade in
        transform.localScale = Vector3.one * 0.5f;
        canvasGroup.alpha = 0f;
        
        DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, appearDuration))
            .Join(transform.DOScale(1f, appearDuration).SetEase(Ease.OutBack));
    }
    
    private void AnimateCompleted()
    {
        // Flash green then return to normal
        if (backgroundImage != null)
        {
            DOTween.Sequence()
                .Append(backgroundImage.DOColor(completedFlashColor, completedFlashDuration * 0.5f))
                .Append(backgroundImage.DOColor(activeColor, completedFlashDuration * 0.5f));
        }
        
        // Subtle scale punch
        transform.DOPunchScale(Vector3.one * 0.1f, completedFlashDuration, 1, 0.5f);
    }
    
    /// <summary>
    /// Set the slot index (useful if setting up programmatically)
    /// </summary>
    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }
}