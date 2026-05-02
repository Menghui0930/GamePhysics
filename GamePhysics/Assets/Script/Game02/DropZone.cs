using UnityEngine;

public class DropZone : MonoBehaviour {
    private GameObject previewInstance;
    private CardDraggable currentCard;

    public void OnCardEnter(CardDraggable card) {
        currentCard = card;
        if (previewInstance != null) Destroy(previewInstance);

        if (card.animalData.prefab != null)
            previewInstance = Instantiate(card.animalData.prefab, transform.position, transform.rotation);
    }

    public void OnCardExit(CardDraggable card) {
        if (currentCard != card) return;
        currentCard = null;

        if (previewInstance != null) {
            Destroy(previewInstance);
            previewInstance = null;
        }
    }

    public void OnCardDropped(CardDraggable card) {
        if (previewInstance != null) {
            Destroy(previewInstance);
            previewInstance = null;
        }

        currentCard = null;

        if (card.animalData.prefabRun != null)
            Instantiate(card.animalData.prefabRun, transform.position, transform.rotation);

        GameManager02.Instance.StartCoroutine(
            GameManager02.Instance.DeleteAndShiftCards(card.slotIndex)
        );

        CardStateManager.Instance.OnCardPlaced();
    }

    public void OnCardCancelled(CardDraggable card) {
        if (previewInstance != null) {
            Destroy(previewInstance);
            previewInstance = null;
        }
        currentCard = null;
    }

    // GameManager 补牌完成后调用，清空预览
    public void ResetZone() {
        if (previewInstance != null) {
            Destroy(previewInstance);
            previewInstance = null;
        }
        currentCard = null;
    }
}