using UnityEngine;
using UnityEngine.SceneManagement;

public class LoseScreenController : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
    
    public void Show()
    {
        gameObject.SetActive(true);
    }
    
    public void Retry()
    {
        SceneManager.LoadScene(0);
    }
}
