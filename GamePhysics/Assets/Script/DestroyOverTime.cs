using UnityEngine;
using UnityEngine.UI;

public class DestroyOverTime : MonoBehaviour
{
    [SerializeField] private GameObject prefabMiss;
    [SerializeField] private float lifetime;

    [SerializeField] private Image miss01;
    [SerializeField] private Image miss02;
    [SerializeField] private Image miss03;
    [SerializeField] private int missMax = 3;
    private int missCount;


    private void Start() {
        missCount = 0;
    }

    private void Update() {
        if (missCount >= missMax) {
            GameManager.Instance.GameOver();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Block")) {
            Debug.Log("Miss");
            other.gameObject.GetComponent<BlockController>().LockBlock();
            Destroy(other.gameObject,1.5f);

            GameObject miss = Instantiate(prefabMiss,other.transform.position,Quaternion.identity);
            Destroy(miss,lifetime);

            if(missCount < missMax )
                missCount++;

            switch (missCount) {
                case 0:
                    break;
                case 1:
                    miss01.color = Color.white; break;
                case 2:
                    miss02.color = Color.white; break;
                case 3:
                    miss03.color = Color.white; break;
                default:
                    break;
            }


        }
    }
}
