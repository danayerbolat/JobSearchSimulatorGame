using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadDesktop()
    {
        SceneManager.LoadScene("Desktop");
    }

    public void LoadCVBuilder()
    {
        SceneManager.LoadScene("3CVBuilder");
    }

    public void LoadJobBoard()
    {
        SceneManager.LoadScene("4JobBoard");
    }

    public void LoadEmailInbox()
    {
        SceneManager.LoadScene("EmailInbox");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}