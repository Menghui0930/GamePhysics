using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManagerGame02 : MonoBehaviour {
    public static UIManagerGame02 Instance;

    public enum State {
        Tutorial,
        WaitToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
        Win
    }

    public State state = State.Tutorial;  

    private float waitToStartTimer = 1f;
    private float countdownToStartTimer = 3f;

    [SerializeField] private GameObject TutoralPanel;
    [SerializeField] private GameObject countDownPanel;
    [SerializeField] private TMP_Text CountDownText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    public HouseHealth leftHealth;
    public HouseHealth rightHealth;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() {
        TutoralPanel.SetActive(true);   
        countDownPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        winPanel.SetActive(false);
    }

    void Update() {
        switch (state) {
            case State.Tutorial:
                break;
            case State.WaitToStart:
                waitToStartTimer -= Time.deltaTime;
                if (waitToStartTimer < 0f) {
                    state = State.CountdownToStart;
                    countDownPanel.SetActive(true);
                }
                break;

            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                CountDownText.text = Mathf.Ceil(countdownToStartTimer).ToString();

                if (countdownToStartTimer < 0f) {
                    state = State.GamePlaying;
                    countDownPanel.SetActive(false);

                    GameManager02.Instance.DealNewHand();
                }
                break;

            case State.GamePlaying:
                break;

            case State.GameOver:
                gameOverPanel.SetActive(true);
                break;

            case State.Win:
                winPanel.SetActive(true);
                break;
        }

        if(leftHealth.houseHealth <= 0) {
            ShowGameOver();
        }

        if(rightHealth.houseHealth <= 0) {
            ShowWin();
        }
    }

    public void ShowGameOver() {
        state = State.GameOver;
        StopGame();
    }

    public void ShowWin() {
        state = State.Win;
        StopGame();
    }

    public void Restart() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnTutorialClose() {
        TutoralPanel.SetActive(false);
        state = State.WaitToStart;
    }


    void StopGame() {
        GameManager02.Instance.StopAllCoroutines();
        GameManager02.Instance.IsGameStopped = true;

        EnemyManager.Instance.StopAllCoroutines();

        AnimalMovement[] animals = FindObjectsByType<AnimalMovement>(FindObjectsSortMode.None);
        foreach (AnimalMovement animal in animals)
            animal.Rb.linearVelocity = Vector2.zero;

        foreach (GameObject card in GameManager02.Instance.SpawnedCards) {
            if (card == null) continue;
            CardDraggable draggable = card.GetComponent<CardDraggable>();
            if (draggable != null) draggable.isExternallyLocked = true;
        }
    }
}