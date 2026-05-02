using UnityEngine;
using UnityEngine.InputSystem;

public class CardDraggable : MonoBehaviour {
    private static CardDraggable draggingCard = null;
    [HideInInspector] public bool isExternallyLocked = false;

    [HideInInspector] public AnimalData animalData;
    [HideInInspector] public int slotIndex;
    [HideInInspector] public Vector3 originalPosition;

    private bool isDragging = false;
    private Vector3 dragOffset;
    private DropZone currentDropZone = null;

    private InputAction clickAction;
    private InputAction pointAction;

    private int dropZoneLayer;

    void Awake() {
        clickAction = InputSystem.actions.FindAction("Attack");   
        pointAction = InputSystem.actions.FindAction("Point");    
        dropZoneLayer = LayerMask.GetMask("DropZone");
    }

    void Start() {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            Debug.LogError($"[CardDraggable] {gameObject.name} no Collider2D!");
        else
            Debug.Log($"[CardDraggable] {gameObject.name} Collider2D OK, isTrigger = {col.isTrigger}");
    }

    void OnEnable() {
        clickAction.started += OnClickStarted;
        clickAction.canceled += OnClickCanceled;
    }

    void OnDisable() {
        clickAction.started -= OnClickStarted;
        clickAction.canceled -= OnClickCanceled;
    }

    void OnClickStarted(InputAction.CallbackContext ctx) {
        if (draggingCard != null && draggingCard != this) return;

        Vector2 mouseWorld = GetMouseWorldPos();
        Collider2D col = GetComponent<Collider2D>();
        if (col == null || !col.OverlapPoint(mouseWorld)) return;

        if (GameManager02.Instance.IsLocked || isExternallyLocked) return;

        isDragging = true;
        draggingCard = this;
        dragOffset = transform.position - (Vector3)mouseWorld;
    }

    void OnClickCanceled(InputAction.CallbackContext ctx) {
        if (!isDragging) return;
        isDragging = false;
        draggingCard = null;

        if (currentDropZone != null) {
            currentDropZone.OnCardDropped(this);
            Destroy(gameObject);
        } else {
            SetCardVisible(true); 
            StartCoroutine(ReturnToOrigin());
        }
    }

    void Update() {
        if (!isDragging) return;

        Vector2 mouseWorld = GetMouseWorldPos();
        transform.position = (Vector3)mouseWorld + dragOffset;
        CheckDropZone();
    }

    void CheckDropZone() {
        Vector2 mouseWorld = GetMouseWorldPos();
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, Mathf.Infinity, dropZoneLayer);

        if (hit.collider != null) {
            DropZone zone = hit.collider.GetComponent<DropZone>();
            if (zone != null) {
                if (zone != currentDropZone) {
                    if (currentDropZone != null) currentDropZone.OnCardExit(this);
                    currentDropZone = zone;
                    currentDropZone.OnCardEnter(this);
                    SetCardVisible(false); 
                }
                return;
            }
        }

        if (currentDropZone != null) {
            currentDropZone.OnCardExit(this);
            currentDropZone = null;
            SetCardVisible(true); 
        }
    }

    void SetCardVisible(bool visible) {
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = visible;
    }

    System.Collections.IEnumerator ReturnToOrigin() {
        Vector3 start = transform.position;
        float t = 0f;
        float duration = 0.3f;

        while (t < 1f) {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, originalPosition, t);
            yield return null;
        }
        transform.position = originalPosition;
    }

    Vector2 GetMouseWorldPos() {
        Vector2 screenPos = pointAction.ReadValue<Vector2>();
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}