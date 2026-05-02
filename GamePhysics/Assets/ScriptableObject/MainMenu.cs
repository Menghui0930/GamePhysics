using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {
    public void GoToGame01() {
        SceneManager.LoadScene("Game01");
    }

    public void GoToGame02() {
        SceneManager.LoadScene("Game02");
    }
}