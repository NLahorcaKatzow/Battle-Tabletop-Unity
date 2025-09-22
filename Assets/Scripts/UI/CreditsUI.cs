using UnityEngine;

public class CreditsUI : MonoBehaviour{
    public void OnBackButtonClicked(){
        SceneManager.Instance.LoadMainMenu();
    }
}
