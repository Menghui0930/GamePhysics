using UnityEngine;

[CreateAssetMenu(fileName = "AnimalData", menuName = "GameSO/AnimalData")]
public class AnimalData : ScriptableObject {
    public string animalName;
    public GameObject prefabCard;
    public GameObject prefab;
    public GameObject prefabRun;

    public int mass;
    public int speed;

}

