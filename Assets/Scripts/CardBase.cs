using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardBase : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    public Sprite backgroundSprite;
    public Sprite foregroundSprite;
    public Sprite hologramSprite;
    
    public float selectionOffset = 20f;
    public bool isInEffect;
    
    public CardVisual cardVisual;
    [SerializeField] private GameObject visualPrefab;
    [HideInInspector] public Transform visualInstance;
    
    [HideInInspector] public bool isHovering;
    [HideInInspector] public bool isDragging;
    [HideInInspector] public bool isSelected;
    [HideInInspector] public bool wasDragged;

    private CardGroup _cardGroup;
    private VisualHandler _visualHandler;
    private Image _image;
    private Vector3 _offset;

    private void Start()
    {
        _image = GetComponent<Image>();
        _cardGroup = GetComponentInParent<CardGroup>();

        if (_image != null)
            _image.color = new Color(1, 1, 1, 0);

        // Disable all graphics except the main image
        foreach (var graphic in GetComponentsInChildren<Graphic>())
            if (graphic != _image)
                graphic.enabled = false;

        if (visualPrefab == null) return;

        _visualHandler = FindObjectOfType<VisualHandler>();
        if (_visualHandler == null)
        {
            Debug.LogWarning("VisualHandler not found in scene!");
            return;
        }

        _cardGroup?.RegisterCard(this);

        var visual = Instantiate(visualPrefab, transform.position, Quaternion.identity, _visualHandler.transform);
        visualInstance = visual.transform;

        cardVisual = visual.GetComponent<CardVisual>();
        cardVisual?.Initialize(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        wasDragged = true;

        if (Camera.main == null) return;
        
        var mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
        _offset = mousePosition - transform.position;
        _offset.z = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || Camera.main == null) return;

        var mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
        mousePosition.z = transform.position.z;
        transform.position = mousePosition - _offset;

        _cardGroup?.Reorder();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        wasDragged = false;
        
        transform.localPosition = isSelected ? Vector3.up * selectionOffset : Vector3.zero;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_cardGroup?.draggingCard != null) return;

        isHovering = true;
        
        cardVisual?.OnPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        cardVisual?.OnPointerExit();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        
        isSelected = !wasDragged && !isSelected;
        
        transform.localPosition = isSelected ? Vector3.up * selectionOffset : Vector3.zero;
        
        cardVisual?.OnPointerUp();

        if (_cardGroup != null)
        {
            _cardGroup.draggingCard = null;
            _cardGroup.Reorder();
        }

        _visualHandler?.UpdateVisualOrder(_cardGroup.cards.ToArray());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        isDragging = true;
        
        transform.SetAsLastSibling();
        
        cardVisual?.OnPointerDown();

        if (_cardGroup != null)
        {
            _cardGroup.draggingCard = this;
            _cardGroup.Reorder();
        }

        _visualHandler?.UpdateVisualOrder(_cardGroup.cards.ToArray());
    }

    public int ParentIndex()
    {
        return transform.parent.GetSiblingIndex();
    }
}
