using UnityEngine;
using TMPro;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI txtRoomId;
    [SerializeField] private TextMeshProUGUI txtDoctorName;
    [SerializeField] private TextMeshProUGUI txtPharmacistName;
    [SerializeField] private GameObject badgeDoctorReady;
    [SerializeField] private GameObject badgePharmacistReady;

    [Header("Asymmetric Scene Settings")]
    [SerializeField] private string doctorSceneName = "DoctorScan";
    [SerializeField] private string pharmacistSceneName = "PharmacistScan";
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Sesuaikan dengan nama scene Main Menu kamu

    private string myRolePath;
    private DatabaseReference roomRef;
    private bool isListening = false;

    private void Start()
    {
        // Putar BGM Lobby otomatis pas masuk scene ini
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgm2);
        }

        // 1. Inisialisasi UI lokal dengan ID Room dari sesi saat ini
        if (txtRoomId != null) txtRoomId.text = GameSession.RoomID;

        // 2. Validasi Role untuk menentukan path di database
        if (GameSession.SelectedRole == PlayerRole.None)
        {
            Debug.LogWarning("Role belum dipilih!");
            return;
        }
        
        myRolePath = GameSession.SelectedRole.ToString().ToLower();
        
        // 3. Tunggu hingga Firebase siap sebelum melakukan operasi data
        StartCoroutine(WaitForFirebaseAndJoin());
    }

    private System.Collections.IEnumerator WaitForFirebaseAndJoin()
    {
        while (FirebaseManager.Instance == null || FirebaseManager.Instance.DBReference == null)
        {
            yield return null;
        }

        roomRef = FirebaseManager.Instance.DBReference.Child("rooms").Child(GameSession.RoomID);
        JoinRoomInDatabase();
        StartListeningForChanges();
    }

    private void JoinRoomInDatabase()
    {
        // 1. Buat data dengan isReady yang pasti false dari awal
        PlayerData myData = new PlayerData(SystemInfo.deviceName);
        myData.isReady = false; // Garansi paksa false sebelum dikirim
        
        string json = JsonUtility.ToJson(myData);

        // 2. Set nilai ke Firebase menggunakan async task
        roomRef.Child("players").Child(myRolePath).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"[{myRolePath.ToUpper()}] Gagal join ke database: " + task.Exception);
                return;
            }

            if (task.IsCompleted)
            {
                // 3. AMAN: Bersihkan sisa status 'true' di cloud akibat session game sebelumnya
                roomRef.Child("players").Child(myRolePath).Child("isReady").SetValueAsync(false);
                Debug.Log($"[{myRolePath.ToUpper()}] Berhasil masuk database dengan status NOT READY.");
            }
        });
    }

    public void OnReadyButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.clickGeneral);
        }

        // Menentukan status ready berdasarkan badge yang aktif saat ini (toggle)
        bool currentStatus = (myRolePath == "doctor") ? badgeDoctorReady.activeSelf : badgePharmacistReady.activeSelf;
        roomRef.Child("players").Child(myRolePath).Child("isReady").SetValueAsync(!currentStatus);
    }

    // --- FUNGSI BARU: LEAVE LOBBY ---
    public void OnLeaveButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.clickGeneral);
        }

        Debug.Log($"[{myRolePath.ToUpper()}] Meninggalkan lobby, menghapus data di Firebase...");

        // 1. Matikan listener terlebih dahulu agar tidak memicu pembaruan UI saat data dihapus
        StopListeningForChanges();

        // 2. Hapus data role player ini dari Firebase agar slot kembali kosong
        if (roomRef != null && !string.IsNullOrEmpty(myRolePath))
        {
            roomRef.Child("players").Child(myRolePath).RemoveValueAsync().ContinueWithOnMainThread(task => {
                // 3. Kembali ke Main Menu setelah data dihapus dari cloud
                SceneManager.LoadScene(mainMenuSceneName);
            });
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void StartListeningForChanges()
    {
        if (isListening) return;

        // Mendengarkan perubahan pada daftar pemain (untuk UI dan Host Trigger)
        roomRef.Child("players").ValueChanged += HandlePlayersChanged;
        
        // Mendengarkan perubahan pada metadata (agar Client otomatis pindah scene saat Host memulai)
        roomRef.Child("metadata").Child("gameStarted").ValueChanged += HandleGameStartedChanged;
        
        isListening = true;
    }

    private void StopListeningForChanges()
    {
        if (!isListening) return;

        if (roomRef != null)
        {
            roomRef.Child("players").ValueChanged -= HandlePlayersChanged;
            roomRef.Child("metadata").Child("gameStarted").ValueChanged -= HandleGameStartedChanged;
        }

        isListening = false;
    }

    private void HandlePlayersChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;

        DataSnapshot playersSnapshot = args.Snapshot;
        bool doctorReady = false;
        bool pharmacistReady = false;

        // Update UI dan status Dokter (Validasi Ketat)
        if (playersSnapshot.HasChild("doctor"))
        {
            var doc = playersSnapshot.Child("doctor");
            
            if (doc.HasChild("name") && doc.Child("name").Value != null)
                txtDoctorName.text = doc.Child("name").Value.ToString();
            
            if (doc.HasChild("isReady") && doc.Child("isReady").Value != null)
                doctorReady = (bool)doc.Child("isReady").Value;
            else
                doctorReady = false;

            badgeDoctorReady.SetActive(doctorReady);
        }
        else
        {
            // Jika data dokter belum ada/keluar, reset UI ke default
            txtDoctorName.text = "Waiting for Player...";
            badgeDoctorReady.SetActive(false);
        }

        // Update UI dan status Apoteker (Validasi Ketat)
        if (playersSnapshot.HasChild("pharmacist"))
        {
            var phar = playersSnapshot.Child("pharmacist");
            
            if (phar.HasChild("name") && phar.Child("name").Value != null)
                txtPharmacistName.text = phar.Child("name").Value.ToString();
            
            if (phar.HasChild("isReady") && phar.Child("isReady").Value != null)
                pharmacistReady = (bool)phar.Child("isReady").Value;
            else
                pharmacistReady = false;

            badgePharmacistReady.SetActive(pharmacistReady);
        }
        else
        {
            // Jika data apoteker belum ada/keluar, reset UI ke default
            txtPharmacistName.text = "Waiting for Player...";
            badgePharmacistReady.SetActive(false);
        }

        // Hanya Host yang memiliki otoritas untuk mengubah status gameStarted di Firebase
        if (doctorReady && pharmacistReady && GameSession.IsHost)
        {
            StartGame();
        }
    }

    private void HandleGameStartedChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;

        // Jika nilai gameStarted berubah menjadi 'true', semua pemain pindah scene
        if (args.Snapshot.Exists && args.Snapshot.Value != null && (bool)args.Snapshot.Value == true)
        {
            TransitionToAsymmetricScene();
        }
    }

    private void StartGame()
    {
        // Host memperbarui metadata di cloud
        roomRef.Child("metadata").Child("gameStarted").SetValueAsync(true);
    }

    private void TransitionToAsymmetricScene()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.transitionAR);
        }

        // Menentukan scene mana yang dimuat berdasarkan peran masing-masing pemain
        if (GameSession.SelectedRole == PlayerRole.Doctor)
        {
            SceneManager.LoadScene(doctorSceneName);
        }
        else if (GameSession.SelectedRole == PlayerRole.Pharmacist)
        {
            SceneManager.LoadScene(pharmacistSceneName);
        }
    }

    private void OnDestroy()
    {
        StopListeningForChanges();
    }
}