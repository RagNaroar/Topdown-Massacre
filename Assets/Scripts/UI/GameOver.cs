using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOver : MonoBehaviour
{
  public void returnToMainMenu()
  {
    SceneManager.LoadScene("MainMenu");
  }
}
