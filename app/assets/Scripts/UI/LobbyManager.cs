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

    private string myRolePath;
    private DatabaseReference roomRef;
    private bool isListening = false;

    private void Start()
    {
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
        // Mengirim data perangkat ke node 'players' sesuai role
        PlayerData myData = new PlayerData(SystemInfo.deviceName);
        string json = JsonUtility.ToJson(myData);

        roomRef.Child("players").Child(myRolePath).SetRawJsonValueAsync(json);
    }

    public void OnReadyButtonClicked()
    {
        // Menentukan status ready berdasarkan badge yang aktif saat ini (toggle)
        bool currentStatus = (myRolePath == "doctor") ? badgeDoctorReady.activeSelf : badgePharmacistReady.activeSelf;
        roomRef.Child("players").Child(myRolePath).Child("isReady").SetValueAsync(!currentStatus);
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

    private void HandlePlayersChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null) return;

        DataSnapshot playersSnapshot = args.Snapshot;
        bool doctorReady = false;
        bool pharmacistReady = false;

        // Update UI dan status Dokter
        if (playersSnapshot.HasChild("doctor"))
        {
            var doc = playersSnapshot.Child("doctor");
            txtDoctorName.text = doc.Child("name").Value.ToString();
            doctorReady = (bool)doc.Child("isReady").Value;
            badgeDoctorReady.SetActive(doctorReady);
        }

        // Update UI dan status Apoteker
        if (playersSnapshot.HasChild("pharmacist"))
        {
            var phar = playersSnapshot.Child("pharmacist");
            txtPharmacistName.text = phar.Child("name").Value.ToString();
            pharmacistReady = (bool)phar.Child("isReady").Value;
            badgePharmacistReady.SetActive(pharmacistReady);
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
        if (args.Snapshot.Exists && (bool)args.Snapshot.Value == true)
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
        // Membersihkan listener untuk mencegah kebocoran memori (Memory Leak)
        if (roomRef != null && isListening)
        {
            roomRef.Child("players").ValueChanged -= HandlePlayersChanged;
            roomRef.Child("metadata").Child("gameStarted").ValueChanged -= HandleGameStartedChanged;
        }
    }
}