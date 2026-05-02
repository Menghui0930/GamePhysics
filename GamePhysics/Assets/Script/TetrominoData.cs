using UnityEngine;

[CreateAssetMenu(fileName = "TetrominoData", menuName = "GameSO/TetrominoData")]
public class TetrominoData : ScriptableObject {
    public string tetrominoName;   
    public GameObject prefab;
    [Tooltip("Each indicator middle - x offset, One block xSize")]
    public float[] indicatorColumns;

}