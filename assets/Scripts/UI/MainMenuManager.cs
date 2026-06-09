using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Database; 
using Firebase.Extensions; 

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI versionText;
    [SerializeField] private TMP_InputField inputFieldCode;
    
    [Header("Modals")]
    [SerializeField] private GameObject modalRoomSelection;
    [SerializeField] private GameObject modalAbout; // <-- BARU: Referensi untuk UI Modal About

    [Header("Settings")]
    [SerializeField] private string playSceneName = "RoleSelection";
    [SerializeField] private string buildVersion = "v1.0.4 LAB_BUILD";

    private void Start()
    {
        // Reset data sesi agar bersih setiap kembali ke menu
        AudioManager.Instance.PlayBGM(AudioManager.Instance.bgm1);
        GameSession.RoomID = "";
        GameSession.IsHost = false;
        GameSession.SelectedRole = PlayerRole.None;

        if (versionText != null) versionText.text = buildVersion;
        
        // Pastikan semua modal tertutup saat game baru dimulai
        if (modalRoomSelection != null) modalRoomSelection.SetActive(false);
        if (modalAbout != null) modalAbout.SetActive(false); // <-- BARU
    }

    public void OnPlayButtonClicked()
    {
        // 1. Putar SFX Klik
        AudioManager.Instance.PlaySFX(AudioManager.Instance.clickGeneral);
        
        // 2. Tampilkan Modal Room Selection
        if (modalRoomSelection != null) modalRoomSelection.SetActive(true);
    }

    // --- BARU: Logika untuk membuka Modal About ---
    public void OnAboutButtonClicked()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.clickGeneral);
        
        if (modalAbout != null) modalAbout.SetActive(true);
    }

    // --- DIREVISI: Fungsi universal untuk menutup modal mana pun yang sedang terbuka ---
    public void CloseAllModals() 
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.clickGeneral);
        
        if (modalRoomSelection != null) modalRoomSelection.SetActive(false);
        if (modalAbout != null) modalAbout.SetActive(false);
    }

    // --- LOGIKA CREATE ROOM ---
    public void CreateRoom()
    {
        string newRoomID = GenerateRandomCode(6);

        // Simpan data ke Static Class
        GameSession.RoomID = newRoomID;
        GameSession.IsHost = true;

        // Daftarkan metadata room ke Firebase
        RegisterRoomToFirebase(newRoomID);
    }

    private void RegisterRoomToFirebase(string roomCode)
    {
        if (FirebaseManager.Instance == null || FirebaseManager.Instance.DBReference == null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.failedStatus);
            
            Debug.LogError("Gagal Create Room: Firebase belum siap!");
            return;
        }

        // Gunakan model RoomMetadata yang sudah kita buat sebelumnya
        RoomMetadata meta = new RoomMetadata();
        string json = JsonUtility.ToJson(meta);

        // Path: rooms/KODE/metadata
        FirebaseManager.Instance.DBReference
            .Child("rooms").Child(roomCode)
            .Child("metadata")
            .SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Gagal mendaftarkan room ke Firebase: " + task.Exception);
                }
                else
                {
                    Debug.Log($"Room {roomCode} berhasil didaftarkan. Pindah ke scene...");
                    SceneManager.LoadScene(playSceneName);
                }
            });
    }

    // --- LOGIKA JOIN ROOM ---
    public void JoinRoom()
    {
        string enteredCode = inputFieldCode.text.ToUpper();

        // Ubah dari >= 4 menjadi == 6 agar sinkron dengan GenerateRandomCode(6)
        if (enteredCode.Length == 6)
        {
            GameSession.RoomID = enteredCode;
            GameSession.IsHost = false;

            AudioManager.Instance.PlaySFX(AudioManager.Instance.succeedStatus);

            Debug.Log($"Mencoba bergabung ke Room: {enteredCode}");
            SceneManager.LoadScene(playSceneName);
        }
        else
        {
            // Berikan feedback yang lebih spesifik
            AudioManager.Instance.PlaySFX(AudioManager.Instance.failedStatus);
            Debug.LogWarning("Kode ruangan harus 6 karakter!");
        }
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] stringChars = new char[length];
        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[Random.Range(0, chars.Length)];
        }
        return new string(stringChars);
    }

    public void OnExitButtonClicked() => Application.Quit();
}