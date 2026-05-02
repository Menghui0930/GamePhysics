using TMPro;
using UnityEngine;

public class CountdownUI : MonoBehaviour
{
    private enum State {
        WaitToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private State state = State.WaitToStart;
    private float waitToStartTimer = 1f;
    private float countdownToStartTimer = 3f;

    [SerializeField] private GameObject CountDownPanel;
    [SerializeField] private TMP_Text CountDownText;

    private void Start() {
        CountDownPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        switch (state) {
            case State.WaitToStart:
                waitToStartTimer -= Time.deltaTime;
                if (waitToStartTimer < 0f) {
                    state = State.CountdownToStart;
                    CountDownShow(true);
                }
                break;

            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                CountDownText.text = Mathf.Ceil(countdownToStartTimer).ToString();

                if (countdownToStartTimer < 0f) {
                    state = State.GamePlaying;
                }
                break;

            case State.GamePlaying:
                CountDownShow(false);
                break;
            case State.GameOver:
                break;
            default:
                break;


        }
    }

    private void CountDownShow(bool state) {
        CountDownPanel.SetActive(state);
    }
}
