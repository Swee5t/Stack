using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardGroup : MonoBehaviour
{
    public bool allowReorder = true;
    public bool autoSpacing;
    public float spacing = 50f;

    [HideInInspector] public List<CardBase> cards = new();
    [HideInInspector] public CardBase draggingCard;

    private RectTransform _rectTransform;
    private VisualHandler _visualHandler;
    private TMP_Text _countText;

    private void Start()
    {
        cards.Clear();

        _rectTransform = GetComponent<RectTransform>();
        _visualHandler = FindObjectOfType<VisualHandler>();
        _countText = GetComponentInChildren<TMP_Text>();

        foreach (Transform slot in transform)
        {
            if (slot.childCount == 0) continue;

            var card = slot.GetChild(0).GetComponent<CardBase>();
            if (card != null)
                cards.Add(card);
        }

        StartCoroutine(StartAfterFirstFrame());
    }

    private void Update()
    {
        UpdateCardPositions();
        UpdateCountText();
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
        if (!allowReorder || draggingCard == null) return;

        var draggingCardIndex = draggingCard.ParentIndex();
        var draggingCardX = draggingCard.transform.position.x;
        var draggingCardSlot = draggingCard.transform.parent;

        foreach (var card in cards.Where(c => c != draggingCard))
        {
            var targetX = card.transform.position.x;
            var targetIndex = card.ParentIndex();
            var targetSlot = card.transform.parent;

            var crossedRight = draggingCardX > targetX && draggingCardIndex < targetIndex;
            var crossedLeft = draggingCardX < targetX && draggingCardIndex > targetIndex;

            if (!crossedRight && !crossedLeft) continue;

            draggingCard.transform.SetParent(targetSlot);
            card.transform.SetParent(draggingCardSlot);

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
    
    private void UpdateCountText()
    {
        if (ReferenceEquals(_countText, null)) return;

        var selected = cards.Count(c => c.isSelected);
        var total = cards.Count;

        _countText.text = $"{selected}/{total}";
    }
}