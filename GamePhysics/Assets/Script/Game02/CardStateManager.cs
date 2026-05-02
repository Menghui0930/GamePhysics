using UnityEngine;
using System.Collections;

public class CardStateManager : MonoBehaviour {
    public static CardStateManager Instance;

    [Header("Settings")]
    public float greyDuration = 3f;       // 灰色持续时间
    public float bounceHeight = 1f;       // 往上弹的高度
    public float bounceSpeed = 5f;        // 弹跳速度
    public float colorFadeSpeed = 2f;     // 颜色渐变速度

    private static readonly Color GreyColor = new Color(0.498f, 0.498f, 0.498f); // 7F7F7F
    private static readonly Color WhiteColor = Color.white;                        // FFFFFF

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 外部调用这个
    public void OnCardPlaced() {
        StartCoroutine(CardPlacedSequence());
    }

    IEnumerator CardPlacedSequence() {
        // 1. 锁住所有卡片 + 变灰
        SetAllCardsLocked(true);
        SetAllCardsColor(GreyColor);

        // 2. 等 3 秒，期间慢慢变回白色
        float elapsed = 0f;
        while (elapsed < greyDuration) {
            elapsed += Time.deltaTime;
            float t = elapsed / greyDuration;
            Color current = Color.Lerp(GreyColor, WhiteColor, t);
            SetAllCardsColor(current);
            yield return null;
        }
        SetAllCardsColor(WhiteColor);

        // 3. 所有卡片往上弹一格再回原位
        yield return StartCoroutine(BounceAllCards());

        // 4. 解锁所有卡片
        SetAllCardsLocked(false);
    }

    IEnumerator BounceAllCards() {
        var cards = GameManager02.Instance.SpawnedCards;

        // 记录原始位置
        Vector3[] origins = new Vector3[cards.Count];
        for (int i = 0; i < cards.Count; i++)
            origins[i] = cards[i] != null ? cards[i].transform.position : Vector3.zero;

        // 往上
        yield return StartCoroutine(MoveAllCards(cards, origins, bounceHeight, bounceSpeed));

        // 回原位
        Vector3[] tops = new Vector3[cards.Count];
        for (int i = 0; i < cards.Count; i++)
            tops[i] = cards[i] != null ? cards[i].transform.position : Vector3.zero;

        yield return StartCoroutine(MoveAllCards(cards, tops, -bounceHeight, bounceSpeed));

        // 强制归位
        for (int i = 0; i < cards.Count; i++)
            if (cards[i] != null)
                cards[i].transform.position = origins[i];
    }

    IEnumerator MoveAllCards(System.Collections.Generic.List<GameObject> cards, Vector3[] targets, float offsetY, float speed) {
        Vector3[] destinations = new Vector3[cards.Count];
        for (int i = 0; i < cards.Count; i++)
            destinations[i] = targets[i] + new Vector3(0, offsetY, 0);

        bool allArrived = false;
        while (!allArrived) {
            allArrived = true;
            for (int i = 0; i < cards.Count; i++) {
                if (cards[i] == null) continue;
                cards[i].transform.position = Vector3.Lerp(
                    cards[i].transform.position,
                    destinations[i],
                    Time.deltaTime * speed
                );
                if (Vector3.Distance(cards[i].transform.position, destinations[i]) > 0.01f)
                    allArrived = false;
            }
            yield return null;
        }

        // 强制到位
        for (int i = 0; i < cards.Count; i++)
            if (cards[i] != null)
                cards[i].transform.position = destinations[i];
    }

    void SetAllCardsColor(Color color) {
        foreach (GameObject card in GameManager02.Instance.SpawnedCards) {
            if (card == null) continue;

            SpriteRenderer[] allSR = card.GetComponentsInChildren<SpriteRenderer>();

            // 跳过最后两个
            int limit = allSR.Length - 2;
            for (int i = 0; i < limit; i++)
                allSR[i].color = color;
        }
    }

    void SetAllCardsLocked(bool locked) {
        foreach (GameObject card in GameManager02.Instance.SpawnedCards) {
            if (card == null) continue;
            CardDraggable draggable = card.GetComponent<CardDraggable>();
            if (draggable != null) draggable.isExternallyLocked = locked;
        }
    }
}