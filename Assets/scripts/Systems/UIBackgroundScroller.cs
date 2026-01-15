using UnityEngine;
using UnityEngine.UI;

public class UIBackgroundScroller : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 0.03f;
    
    private RawImage rawImage;
    
    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        Rect uvRect = rawImage.uvRect;
        uvRect.x += Time.deltaTime * scrollSpeed;
        rawImage.uvRect = uvRect;
    }
}