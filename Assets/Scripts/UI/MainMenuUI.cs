using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public void PlayGame(){
        SceneManager.Instance.LoadNextLevel();
    }
}
