using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BlockSpawner : MonoBehaviour {
    [Header("Spawn Settings")]
    public TetrominoData[] tetrominoes;
    public Transform spawnPoint;
    public float spawnDelay = 0.5f;

    [Header("Reference")]
    public DropIndicator dropIndicator;
    public Transform heightLine;  
    public TMP_Text heightText;

    // Save bag
    private Queue<TetrominoData> bag = new Queue<TetrominoData>();

    private bool DoStart = false;

    void Start() {

    }

    private void Update() {
        if (GameManager.Instance.state == GameManager.State.GamePlaying && !DoStart) {
            DoStart = true;
            SpawnNextBlock();
        }

    }

    public void SpawnNextBlock() {
        Invoke(nameof(DoSpawn), spawnDelay);
    }

    void DoSpawn() {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return; 

        if (tetrominoes.Length == 0) {
            Debug.LogWarning("BlockSpawner no tetrominoes");
            return;
        }

        // bag empty then we redraw a newBag
        if (bag.Count == 0)
            RefillBag();

        TetrominoData data = bag.Dequeue();  // take one from the bag

        GameObject newBlock = Instantiate(data.prefab, spawnPoint.position, Quaternion.identity);
        BlockController controller = newBlock.GetComponent<BlockController>();

        controller.heightLine = heightLine;  
        controller.heightText = heightText;

        if (controller != null) {
            controller.spawner = this;
            controller.dropIndicator = dropIndicator;
            dropIndicator.AttachBlock(newBlock.transform, data);
        } else
            Debug.LogWarning("BlockSpawner block Prefab no BlockController Script!");
    }

    void RefillBag() {
        // add all tetrominord to the list and shffled
        List<TetrominoData> shuffled = new List<TetrominoData>(tetrominoes);

        for (int i = shuffled.Count - 1; i > 0; i--) {
            int rand = Random.Range(0, i + 1);
            TetrominoData temp = shuffled[i];
            shuffled[i] = shuffled[rand];
            shuffled[rand] = temp;
        }

        // fill to the Queue
        foreach (var t in shuffled)
            bag.Enqueue(t);

        Debug.Log("New bag shuffled!");
    }
}