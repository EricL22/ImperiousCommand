using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Sprite[] backgrounds;
    public Image bgImage;

    private void Awake()
    {
        bgImage.sprite = backgrounds[Random.Range(0, backgrounds.Length)];
    }

    public void GetScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
