using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaMargin : MonoBehaviour
{
    public float top;
    public float bottom;
    public float left;
    public float right;

    private Canvas _canvas;
    private RectTransform _rectTransform;
    private Rect _lastSafeArea;
    private Vector2Int _lastScreenSize;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        _rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (Screen.safeArea != _lastSafeArea || Screen.width != _lastScreenSize.x || Screen.height != _lastScreenSize.y)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        _lastSafeArea = Screen.safeArea;
        _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        
        if (ReferenceEquals(_canvas, null) || _canvas.renderMode == RenderMode.WorldSpace) return;

        var scaleFactor = _canvas.scaleFactor;

        var safeArea = Screen.safeArea;
        var safeLeft = safeArea.xMin / scaleFactor;
        var safeRight = (Screen.width - safeArea.xMax) / scaleFactor;
        var safeTop = (Screen.height - safeArea.yMax) / scaleFactor;
        var safeBottom = safeArea.yMin / scaleFactor;

        var finalLeft = Mathf.Max(left, safeLeft);
        var finalRight = Mathf.Max(right, safeRight);
        var finalTop = Mathf.Max(top, safeTop);
        var finalBottom = Mathf.Max(bottom, safeBottom);

        _rectTransform.offsetMin = new Vector2(finalLeft, finalBottom); // bottom-left
        _rectTransform.offsetMax = new Vector2(-finalRight, -finalTop); // top-right (negative)
    }
}