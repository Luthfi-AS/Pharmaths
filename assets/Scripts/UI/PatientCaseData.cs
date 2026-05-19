using UnityEngine;

[CreateAssetMenu(fileName = "NewPatientCase", menuName = "Pharmath/Patient Case Data")]
public class PatientCaseData : ScriptableObject
{
    [Header("Identitas Pasien")]
    public string patientName;
    public string gender;
    public int age;

    [Header("Tanda Vital (Vitals)")]
    public int sbp;
    public int dbp;
    public int hr;
    public float temperature;
    public int gds;
    public int spo2;
    public int rr;

    [Header("Indikator & Keluhan")]
    public int vas;
    public int wbc;
    public float bmi;
    [TextArea(3, 5)] public string conditionDescription;
    public string diseaseIdentification;
}