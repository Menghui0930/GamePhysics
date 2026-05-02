using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public bool isGameOver { get; private set; } = false;

    public enum State {
        WaitToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    public State state = State.WaitToStart;
    private float waitToStartTimer = 1f;
    private float countdownToStartTimer = 3f;

    [SerializeField] private GameObject CountDownPanel;
    [SerializeField] private TMP_Text CountDownText;
    [SerializeField] private GameObject GameOverPanel;


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        CountDownPanel.SetActive(false);
        GameOverPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
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
                GameOverPanel.SetActive(true);
                break;
            default:
                break;


        }
    }

    private void CountDownShow(bool state) {
        CountDownPanel.SetActive(state);
    }

    public void GameOver() {
        if (isGameOver) return;
        isGameOver = true;

        Debug.Log("Game Over!");

        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject block in blocks) {
            BlockController bc = block.GetComponent<BlockController>();
            if (bc != null && bc.isControllable) {
                Destroy(block);
            }
        }
        state = State.GameOver;

    }

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}