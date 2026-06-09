using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SummaryManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Masukkan objek 'Badge' yang memiliki komponen Image di sini")]
    [SerializeField] private Image imgBadge;
    
    [Tooltip("Masukkan Text (TMP) dari dalam Succeed_Stat")]
    [SerializeField] private TextMeshProUGUI txtSucceed;
    
    [Tooltip("Masukkan Text (TMP) dari dalam Malpractice_Stat")]
    [SerializeField] private TextMeshProUGUI txtMalpractice;
    
    [Tooltip("Masukkan Text (TMP) dari avg_time")]
    [SerializeField] private TextMeshProUGUI txtAvgTime;
    
    [Tooltip("Masukkan objek Btn_Back di sini")]
    [SerializeField] private Button btnBack;

    [Header("Badge Sprites")]
    [SerializeField] private Sprite spriteBadgeA;
    [SerializeField] private Sprite spriteBadgeB;
    [SerializeField] private Sprite spriteBadgeC;

    [Header("Scene Navigation")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        // Pasang fungsi pada tombol kembali
        if (btnBack != null)
        {
            btnBack.onClick.AddListener(OnBackButtonClicked);
        }

        // ==========================================
        // MENGAMBIL DATA ASLI DARI GAMEPLAY SEBELUMNYA
        // (Nilai 0 di belakang adalah default jika data tidak ditemukan)
        // ==========================================
        int totalSaved = PlayerPrefs.GetInt("TotalSaved", 0);           
        int totalMalpractice = PlayerPrefs.GetInt("TotalMalpractice", 0);     
        float averageTime = PlayerPrefs.GetFloat("AverageTime", 0f);    

        // Tampilkan data ke UI
        UpdateSummaryUI(totalSaved, totalMalpractice, averageTime);
    }

    private void UpdateSummaryUI(int saved, int malpractice, float avgTime)
    {
        // 1. Tampilkan jumlah pasien yang selamat (Maksimal 5)
        if (txtSucceed != null) txtSucceed.text = $"{saved} / 5"; 
        
        // 2. Tampilkan jumlah malapraktik
        if (txtMalpractice != null) txtMalpractice.text = malpractice.ToString();
        
        // 3. Format Waktu menjadi MM:SS (Menit:Detik)
        if (txtAvgTime != null) 
        {
            int minutes = Mathf.FloorToInt(avgTime / 60F);
            int seconds = Mathf.FloorToInt(avgTime - (minutes * 60));
            txtAvgTime.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        // 4. Kalkulasi dan Tampilkan Grade
        char finalGrade = CalculateGrade(saved, malpractice);
        ApplyBadgeSprite(finalGrade);
    }

    private char CalculateGrade(int saved, int malpractice)
    {
        // Logika penilaian untuk 5 Kasus
        if (saved >= 4) return 'A';
        if (saved >= 2) return 'B';
        return 'C';
    }

    private void ApplyBadgeSprite(char grade)
    {
        if (imgBadge == null) return;

        switch (grade)
        {
            case 'A':
                imgBadge.sprite = spriteBadgeA;
                break;
            case 'B':
                imgBadge.sprite = spriteBadgeB;
                break;
            case 'C':
                imgBadge.sprite = spriteBadgeC;
                break;
            default:
                imgBadge.sprite = spriteBadgeC;
                break;
        }
    }

    private void OnBackButtonClicked()
    {
        // Mainkan SFX Klik jika ada AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.clickGeneral);
        }

        // Pindah ke Main Menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}