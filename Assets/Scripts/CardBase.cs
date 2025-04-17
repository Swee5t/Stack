using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardBase : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler,
    IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    public Sprite backgroundSprite;
    public Sprite foregroundSprite;
    public Sprite hologramSprite;
    public CardVisual cardVisual;
    public float selectionOffset = 20f;
    
    public bool isInEffect = false;

    [HideInInspector] public bool isHovering;
    [HideInInspector] public bool isDragging;
    [HideInInspector] public bool isSelected;
    [HideInInspector] public bool wasDragged;

    [HideInInspector] public Transform visualInstance;

    [SerializeField] private GameObject visualPrefab;
    
    private CardGroup _cardGroup;
    private Image _image;
    private Vector3 _offset;
    private VisualHandler _visualHandler;

    private void Start()
    {
        _image = GetComponent<Image>();
        _cardGroup = GetComponentInParent<CardGroup>();

        if (_image != null)
            _image.color = new Color(1, 1, 1, 0);

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

        _cardGroup.RegisterCard(this);

        var visual = Instantiate(visualPrefab, transform.position, Quaternion.identity, _visualHandler.transform);
        visualInstance = visual.transform;

        cardVisual = visual.GetComponent<CardVisual>();

        if (cardVisual != null) cardVisual.Initialize(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Camera.main == null || _cardGroup.draggingCard != null) return;

        var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        _offset = mousePosition - transform.position;

        isDragging = true;
        wasDragged = true;
        _image.raycastTarget = false;

        transform.SetAsLastSibling();

        if (_cardGroup != null)
        {
            _cardGroup.selectedCard = this;
            _cardGroup.draggingCard = this;
        }

        _visualHandler?.UpdateVisualOrder(_cardGroup.cards.ToArray());
        cardVisual.OnBeginDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || Camera.main == null) return;

        var mousePosition = Camera.main.ScreenToWorldPoint(eventData.position);
        transform.position = mousePosition - _offset;

        if (_cardGroup != null)
            _cardGroup.Reorder();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        wasDragged = false;
        _image.raycastTarget = true;

        transform.localPosition = isSelected ? Vector3.up * selectionOffset : Vector3.zero;

        if (_cardGroup.draggingCard == this)
            _cardGroup.draggingCard = null;

        _visualHandler?.UpdateVisualOrder(_cardGroup.cards.ToArray());
        cardVisual.OnEndDrag();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        cardVisual.OnPointerDown();

        if (_cardGroup != null)
            _cardGroup.Reorder();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_cardGroup.draggingCard != null) return;
        
        isHovering = true;

        cardVisual.OnPointerEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        
        cardVisual.OnPointerExit();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (wasDragged) return;

        isSelected = !isSelected;

        cardVisual.OnPointerUp();

        transform.localPosition = isSelected ? Vector3.up * selectionOffset : Vector3.zero;
    }

    public int ParentIndex()
    {
        return transform.parent.GetSiblingIndex();
    }
}