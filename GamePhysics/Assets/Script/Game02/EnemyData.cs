using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "GameSO/EnemyData")]
public class EnemyData : ScriptableObject {
    public string enemyName;
    public GameObject prefabRun;
    public int mass;
    public int speed;
}