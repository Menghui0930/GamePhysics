using TMPro;
using UnityEngine;

public class HouseHealth : MonoBehaviour
{

    [Header("House Heart")]
    public int maxHealth = 3;
    public int houseHealth;

    public TMP_Text healthText;

    void Start()
    {
        houseHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Animal")) { 
            Destroy(other.gameObject);
            houseHealth--;

            if (!(houseHealth < 0)) {
                healthText.text = houseHealth.ToString();
            }
        }
    }
}
