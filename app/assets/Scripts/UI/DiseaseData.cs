using UnityEngine;

namespace Pharmath.Core.Data
{
    /// <summary>
    /// Enum untuk menentukan tingkat urgensi penanganan medis.
    /// </summary>
    public enum UrgencyLevel
    {
        Low,      // Level 1: Tension Headache
        Medium,   // Level 2: GERD
        High,     // Level 3: Pharyngitis
        Critical, // Level 4: DKA
        Emergency // Level 5: Asthma
    }

    [System.Serializable]
    public struct VitalSigns
    {
        public string dbp;
        public string sbp;
        public string temperature;
        public string respRate;
        public string spo2;
        public string heartRate;
    }

    [System.Serializable]
    public struct LabAssessment
    {
        public string gds;
        public string wbc;
        public string vas;
        public string bmi;
    }

    [CreateAssetMenu(fileName = "New Disease", menuName = "Pharmath/Disease Data")]
    public class DiseaseData : ScriptableObject
    {
        [Header("General Identity")]
        public string diseaseName;
        public UrgencyLevel urgency;
        [TextArea(3, 10)] public string description;
        public Sprite icon;

        [Header("Clinical Data")]
        public VitalSigns vitals;
        public LabAssessment labResults;

        [Header("Treatment Protocol")]
        public string correctMedication; // Menghubungkan ke database obat
        [TextArea] public string medicalNote; // Catatan tambahan untuk dokter
    }
}