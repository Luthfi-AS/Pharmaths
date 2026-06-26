using UnityEngine;
using UnityEngine.UI;

public class GoogleDriveOpener : MonoBehaviour
{
    [Header("UI Elements")]
    public Button downloadButton;
    
    [Header("Link Settings")]
    [Tooltip("Masukkan link share Google Drive di bawah ini")]
    public string googleDriveUrl = "https://drive.google.com/link_ke_file";

    void Start()
    {
        // Validasi jika tombol belum dimasukkan di Inspector
        if (downloadButton != null)
        {
            downloadButton.onClick.AddListener(BukaLinkDrive);
        }
        else
        {
            Debug.LogError("Tombol 'Download Button' belum dimasukkan di Inspector!");
        }
    }

    void BukaLinkDrive()
    {
        if (!string.IsNullOrEmpty(googleDriveUrl))
        {
            Debug.Log("Membuka URL: " + googleDriveUrl);
            
            // Perintah universal untuk membuka browser / aplikasi eksternal
            Application.OpenURL(googleDriveUrl);
        }
        else
        {
            Debug.LogError("Link Google Drive kosong!");
        }
    }
}