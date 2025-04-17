using UnityEngine;

public class VisualHandler : MonoBehaviour
{
    public void UpdateVisualOrder(CardBase[] cards)
    {
        var visualIndex = 0;

        foreach (var card in cards)
        {
            if (ReferenceEquals(card.visualInstance, null)) continue;

            if (card.isDragging)
            {
                card.visualInstance.SetAsLastSibling();
            }
            else
            {
                card.visualInstance.SetSiblingIndex(visualIndex);
                visualIndex++;
            }
        }
    }
}