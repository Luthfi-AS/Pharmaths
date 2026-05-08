using UnityEngine;
using UnityEngine.SceneManagement;

public class RoleSelectionManager : MonoBehaviour
{
    [SerializeField] private string lobbySceneName = "GameLobby"; 

    public void SelectDoctor()
    {
        GameSession.SelectedRole = PlayerRole.Doctor;
        GoToLobby();
    }

    public void SelectPharmacist()
    {
        GameSession.SelectedRole = PlayerRole.Pharmacist;
        GoToLobby();
    }

    private void GoToLobby()
    {
        // Tidak perlu PlayerPrefs.Save() karena data ada di memori (GameSession)
        SceneManager.LoadScene(lobbySceneName);
    }
}