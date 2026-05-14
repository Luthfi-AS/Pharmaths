using UnityEngine;
using Vuforia;

public class ARModelSwitcher : MonoBehaviour
{
    public GameObject modelToActivate;
    private ObserverBehaviour mObserverBehaviour;

    void Start()
    {
        mObserverBehaviour = GetComponent<ObserverBehaviour>();
        if (mObserverBehaviour)
        {
            mObserverBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
    {
        if (targetStatus.Status == Status.TRACKED || targetStatus.Status == Status.EXTENDED_TRACKED)
        {
            // Saat QR terdeteksi, aktifkan model
            modelToActivate.SetActive(true);
            Debug.Log(behaviour.TargetName + " terdeteksi.");
        }
        else
        {
            // Saat QR hilang dari kamera, nonaktifkan model
            modelToActivate.SetActive(false);
        }
    }
}