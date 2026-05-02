using UnityEngine;

public class HitEffect : MonoBehaviour {
    private float timer = 0f;
    private float totalDuration = 60f / 60f; 

    
    private float[] keyTimes = { 0f, 9f / 60f, 59f / 60f };  
    private float[] keyY = { -6.33f, -4.32f, -3.18f };

    void Start() {
        Destroy(gameObject, totalDuration);
    }

    void Update() {
        timer += Time.deltaTime;

        float y = GetY(timer);

        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    float GetY(float t) {

        if (t >= keyTimes[keyTimes.Length - 1])
            return keyY[keyY.Length - 1];

        for (int i = 0; i < keyTimes.Length - 1; i++) {
            if (t >= keyTimes[i] && t < keyTimes[i + 1]) {
                float progress = (t - keyTimes[i]) / (keyTimes[i + 1] - keyTimes[i]);
                return Mathf.Lerp(keyY[i], keyY[i + 1], progress);
            }
        }

        return keyY[0];
    }
}