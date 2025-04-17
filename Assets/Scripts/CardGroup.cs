using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardGroup : MonoBehaviour
{
    public bool allowReorder = true;
    public bool autoSpacing;
    public float spacing = 50f;

    [HideInInspector] public List<CardBase> cards = new();
    [HideInInspector] public CardBase selectedCard;
    [HideInInspector] public CardBase draggingCard;
    [HideInInspector] public bool isDragging;

    private RectTransform _rectTransform;
    private VisualHandler _visualHandler;

    private void Start()
    {
        cards.Clear();

        _rectTransform = GetComponent<RectTransform>();
        _visualHandler = FindObjectOfType<VisualHandler>();

        foreach (Transform slot in transform)
        {
            if (slot.childCount == 0) continue;

            var card = slot.GetChild(0).GetComponent<CardBase>();
            if (card != null)
                cards.Add(card);
        }

        StartCoroutine(StartAfterFirstFrame());
    }

    private void LateUpdate()
    {
        UpdateCardPositions();
    }

    private IEnumerator StartAfterFirstFrame()
    {
        yield return new WaitForEndOfFrame();
        _visualHandler?.UpdateVisualOrder(cards.ToArray());
        UpdateCardPositions();
    }

    public void RegisterCard(CardBase card)
    {
        if (cards.Contains(card)) return;

        cards.Add(card);
        _visualHandler?.UpdateVisualOrder(cards.ToArray());
        UpdateCardPositions();
    }

    public void Reorder()
    {
        if (!allowReorder || isDragging || selectedCard == null) return;

        var selectedIndex = selectedCard.ParentIndex();
        var selectedX = selectedCard.transform.position.x;
        var selectedSlot = selectedCard.transform.parent;

        foreach (var card in cards.Where(c => c != selectedCard))
        {
            var targetX = card.transform.position.x;
            var targetIndex = card.ParentIndex();
            var targetSlot = card.transform.parent;

            var crossedRight = selectedX > targetX && selectedIndex < targetIndex;
            var crossedLeft = selectedX < targetX && selectedIndex > targetIndex;

            if (!crossedRight && !crossedLeft) continue;

            selectedCard.transform.SetParent(targetSlot);
            card.transform.SetParent(selectedSlot);

            card.transform.localPosition = card.isSelected ? Vector3.up * card.selectionOffset : Vector3.zero;

            cards = GetComponentsInChildren<CardBase>().ToList();

            _visualHandler?.UpdateVisualOrder(cards.ToArray());
            UpdateCardPositions();
            return;
        }
    }

    private void UpdateCardPositions()
    {
        if (cards.Count == 1)
        {
            var slot = cards[0].transform.parent;
            slot.localPosition = Vector3.zero;
            return;
        }

        var slots = cards.Select(c => c.transform.parent).ToList();
        var count = slots.Count;
        var centerIndex = (count - 1) / 2f;

        var maxSpacing = spacing;

        if (autoSpacing)
        {
            var maxWidth = _rectTransform.rect.width;
            maxSpacing = count > 1 ? maxWidth / (count - 1) : 0f;
        }

        for (var i = 0; i < count; i++)
        {
            var offsetX = (i - centerIndex) * maxSpacing;
            var slot = slots[i];
            slot.localPosition = new Vector3(offsetX, 0f, 0f);
        }
    }
}