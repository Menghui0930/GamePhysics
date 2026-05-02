using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour {
    public static EnemyManager Instance;

    [Header("6 个敌人数据")]
    public EnemyData[] allEnemies = new EnemyData[6];

    [Header("3 个敌人生成位置")]
    public Transform[] enemySlots = new Transform[3];

    [Header("生成间隔")]
    public float firstSpawnDelay = 5f;
    public float spawnInterval = 10f;

    private List<int> shuffleBag = new List<int>();

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() {
        if (allEnemies.Length != 6) {
            Debug.LogError("[EnemyManager] allEnemies 需要 6 个！");
            return;
        }
        if (enemySlots.Length != 3) {
            Debug.LogError("[EnemyManager] enemySlots 需要 3 个！");
            return;
        }
    }

    public void OnGameStart() {
        Debug.Log("[EnemyManager] 游戏开始，准备生成敌人");
        StartCoroutine(SpawnSequence());
    }

    IEnumerator SpawnSequence() {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true) {
            SpawnOneEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOneEnemy() {
        // 随机选一个 slot 位置
        Transform slot = enemySlots[Random.Range(0, enemySlots.Length)];

        // Shuffle Bag 抽一个敌人
        EnemyData data = allEnemies[DrawOne()];

        if (data.prefabRun == null) {
            Debug.LogWarning($"[EnemyManager] {data.enemyName} 的 prefabRun 为空");
            return;
        }

        Instantiate(data.prefabRun, slot.position, slot.rotation);
        Debug.Log($"[EnemyManager] 生成 {data.enemyName} 在 {slot.name}");
    }

    int DrawOne() {
        if (shuffleBag.Count == 0) RefillAndShuffle();
        int picked = shuffleBag[0];
        shuffleBag.RemoveAt(0);
        return picked;
    }

    void RefillAndShuffle() {
        shuffleBag.Clear();
        for (int i = 0; i < 6; i++) shuffleBag.Add(i);

        for (int i = shuffleBag.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (shuffleBag[i], shuffleBag[j]) = (shuffleBag[j], shuffleBag[i]);
        }

        string log = "[EnemyManager] Shuffle: ";
        foreach (int idx in shuffleBag) log += idx + " ";
        Debug.Log(log);
    }
}