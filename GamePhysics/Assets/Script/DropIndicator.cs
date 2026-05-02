using System.Collections.Generic;
using UnityEngine;

public class DropIndicator : MonoBehaviour {
    public GameObject indicatorStripPrefab;

    private Transform currentBlock;
    private string currentName;
    private List<GameObject> activeStrips = new List<GameObject>();

    void LateUpdate() {
        if (currentBlock == null) {
            HideAllStrips();
            return;
        }

        UpdateStrips();
    }

    void UpdateStrips() {

        Collider2D[] children = currentBlock.GetComponentsInChildren<Collider2D>();

        List<float> uniqueX = GetUniqueColumns(children);

        EnsureStripCount(uniqueX.Count);
        //Debug.Log(uniqueX.Count);

        for (int i = 0; i < activeStrips.Count; i++) {
            if (i < uniqueX.Count) {
                activeStrips[i].SetActive(true);
                Vector3 pos = activeStrips[i].transform.position;
                pos.x = uniqueX[i]; 
                activeStrips[i].transform.position = pos;
            } else {
                activeStrips[i].SetActive(false);
            }

            
        }

        if (currentName == "TetrominoO") {
            activeStrips[0].SetActive(false);
        }

        if (currentName == "TetrominoI") {
            if (uniqueX.Count == 5) {
                activeStrips[0].SetActive(false);
            }
        }
    }

    List<float> GetUniqueColumns(Collider2D[] children) {
        List<float> result = new List<float>();

        foreach (Collider2D child in children) {
            float worldX = Mathf.Round(child.transform.position.x * 10f) / 10f;

            // check is that x-point already have block (Error 0.1)
            bool alreadyExists = false;
            foreach (float x in result) {
                if (Mathf.Abs(x - worldX) < 0.1f) {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
                result.Add(worldX);
        }

        return result;
    }

    void EnsureStripCount(int count) {
        while (activeStrips.Count < count) {
            GameObject strip = Instantiate(indicatorStripPrefab, this.transform);
            activeStrips.Add(strip);
        }
    }

    void HideAllStrips() {
        foreach (var strip in activeStrips)
            strip.SetActive(false);
    }

    public void AttachBlock(Transform block, TetrominoData indicatorColumns) {
        currentBlock = block;
        currentName = indicatorColumns.tetrominoName;

    }

    public void DetachBlock() {
        currentBlock = null;
        HideAllStrips();
    }
}