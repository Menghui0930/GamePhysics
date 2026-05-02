using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class GameManager02 : MonoBehaviour {
    public static GameManager02 Instance;

    [Header("6 animal Data")]
    public AnimalData[] allCards = new AnimalData[6];

    [Header("Card Slot")]
    public Transform[] cardSlots = new Transform[4];

    [Header("Settings")]
    public float revealDelay = 10f;
    public float moveSpeed = 5f;       
    public float targetY = -2f;

    [Header("DropZone")]
    public DropZone[] dropZones;  

    private List<int> shuffleBag = new List<int>();
    private List<int> drawnCards = new List<int>();

    // Card instances generated on the field at the moment
    private List<GameObject> spawnedCards = new List<GameObject>();

    private bool isRevealing = false;   
    private bool isMoving = false;     

    public bool IsLocked => isRevealing || isMoving;
    public List<GameObject> SpawnedCards => spawnedCards;

    private InputAction deleteCardOne;
    private InputAction deleteCardTwo;
    private InputAction deleteCardThree;

    public bool IsGameStopped = false;

    void Awake() {
        Instance = this;

        deleteCardOne = InputSystem.actions.FindAction("RotateCW");
        deleteCardTwo = InputSystem.actions.FindAction("RotateCCW");
        deleteCardThree = InputSystem.actions.FindAction("3");
    }

    void Start() {
        if (allCards.Length != 6) {
            Debug.LogError("[GameManager02] allCards need 6");
            return;
        }
        if (cardSlots.Length != 4) {
            Debug.LogError("[GameManager02] cardSlots need 4");
            return;
        }

        //DealNewHand();
    }

    public void DealNewHand() {

        if (IsGameStopped) return;

        StopAllCoroutines();
        ClearSpawnedCards();
        drawnCards.Clear();

        // draw 4 card
        for (int i = 0; i < 4; i++) {
            drawnCards.Add(DrawOne());
        }

        string log = "Card index: ";
        foreach (int idx in drawnCards) log += idx + " ";
        Debug.Log(log);

        // spawn card to slot
        for (int i = 0; i < 4; i++) {
            SpawnCard(i);
        }

        StartCoroutine(RevealSequence());
    }

    void SpawnCard(int slotIndex) {
        AnimalData data = allCards[drawnCards[slotIndex]];

        if (data.prefabCard == null) {
            spawnedCards.Add(null);
            return;
        }

        GameObject card = Instantiate(data.prefabCard, cardSlots[slotIndex].position, cardSlots[slotIndex].rotation);

        CardDraggable draggable = card.GetComponent<CardDraggable>();
        if (draggable != null) {
            draggable.animalData = data;
            draggable.slotIndex = slotIndex;
            draggable.originalPosition = card.transform.position; 
        }

        spawnedCards.Add(card);
        Debug.Log($"[GameManager02] Slot{slotIndex + 1} → {data.animalName}");
    }

    IEnumerator RevealSequence() {
        isRevealing = true;

        for (int i = 0; i < spawnedCards.Count; i++) {
            if (spawnedCards[i] == null) { yield return new WaitForSeconds(1f); continue; }

            Vector3 target = new Vector3(cardSlots[i].position.x, targetY, cardSlots[i].position.z);
            yield return StartCoroutine(MoveToTarget(spawnedCards[i], target));

            CardDraggable draggable = spawnedCards[i].GetComponent<CardDraggable>();
            if (draggable != null) draggable.originalPosition = target;

            if (i < spawnedCards.Count - 1)
                yield return new WaitForSeconds(1f);
        }

        isRevealing = false;

        EnemyManager.Instance.OnGameStart();
    }

    IEnumerator MoveToTarget(GameObject obj, Vector3 target) {
        while (obj != null && Vector3.Distance(obj.transform.position, target) > 0.01f) {
            obj.transform.position = Vector3.Lerp(obj.transform.position, target, Time.deltaTime * moveSpeed);
            yield return null;
        }
        if (obj != null) obj.transform.position = target;
    }

    void OnDeleteCard(InputAction.CallbackContext ctx) {
        if (isRevealing || isMoving) return;
        StartCoroutine(DeleteAndShiftCards(0));
    }

    void OnDeleteCardTwo(InputAction.CallbackContext ctx) {
        if (isRevealing || isMoving) return;
        StartCoroutine(DeleteAndShiftCards(1));
    }

    void OnDeleteCardThree(InputAction.CallbackContext ctx) {
        if (isRevealing || isMoving) return;
        StartCoroutine(DeleteAndShiftCards(2));
    }

    public IEnumerator DeleteAndShiftCards(int deleteIndex) {

        if (IsGameStopped) yield break;

        isMoving = true;

        if (spawnedCards[deleteIndex] != null)
            Destroy(spawnedCards[deleteIndex]);
        spawnedCards.RemoveAt(deleteIndex);
        drawnCards.RemoveAt(deleteIndex);

        List<Coroutine> moves = new List<Coroutine>();
        for (int i = deleteIndex; i < spawnedCards.Count; i++) {
            if (spawnedCards[i] == null) continue;

            Vector3 target = new Vector3(cardSlots[i].position.x, targetY, cardSlots[i].position.z);
            moves.Add(StartCoroutine(MoveToTarget(spawnedCards[i], target)));
            CardDraggable draggable = spawnedCards[i].GetComponent<CardDraggable>();
            if (draggable != null) {
                draggable.slotIndex = i;
                draggable.originalPosition = target;
            }
        }
        foreach (var m in moves) yield return m;

        int newIdx = DrawOne();
        drawnCards.Add(newIdx);

        AnimalData data = allCards[newIdx];
        GameObject newCard = null;
        if (data.prefabCard != null) {
            newCard = Instantiate(data.prefabCard, cardSlots[3].position, cardSlots[3].rotation);
            Debug.Log($"[GameManager02]  Slot4 → {data.animalName}");
        }
        spawnedCards.Add(newCard);

        if (newCard != null) {
            Vector3 target = new Vector3(cardSlots[3].position.x, targetY, cardSlots[3].position.z);
            yield return StartCoroutine(MoveToTarget(newCard, target));

            CardDraggable draggable = newCard.GetComponent<CardDraggable>();
            if (draggable != null) {
                draggable.animalData = allCards[newIdx];
                draggable.slotIndex = 3;
                draggable.originalPosition = target;
            }
        }

        foreach (var zone in dropZones)
            zone.ResetZone();

        isMoving = false;
    }


    // clear all card
    void ClearSpawnedCards() {
        foreach (GameObject card in spawnedCards) {
            if (card != null) Destroy(card);
        }
        spawnedCards.Clear();
    }

    // Shuffle and draw one
    int DrawOne() {
        if (shuffleBag.Count == 0) RefillAndShuffle();

        int picked = shuffleBag[0];
        shuffleBag.RemoveAt(0);
        return picked;
    }

    void RefillAndShuffle() {
        shuffleBag.Clear();
        for (int i = 0; i < 6; i++) shuffleBag.Add(i);

        // Fisher-Yates Shuffle
        for (int i = shuffleBag.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (shuffleBag[i], shuffleBag[j]) = (shuffleBag[j], shuffleBag[i]);
        }

        string log = "Complete Shuffle: ";
        foreach (int idx in shuffleBag) log += idx + " ";
        Debug.Log(log);
    }

    void OnEnable() {
        deleteCardOne.performed += OnDeleteCard;
        deleteCardTwo.performed += OnDeleteCardTwo;
        deleteCardThree.performed += OnDeleteCardThree;
    }

    void OnDisable() {
        deleteCardOne.performed -= OnDeleteCard;
        deleteCardTwo.performed -= OnDeleteCardTwo;
        deleteCardThree.performed -= OnDeleteCardThree;
    }
}