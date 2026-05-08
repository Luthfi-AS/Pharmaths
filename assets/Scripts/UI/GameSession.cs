using UnityEngine;

public enum PlayerRole { None, Doctor, Pharmacist }

public static class GameSession
{
    public static string RoomID = "";
    public static bool IsHost = false;
    public static string LocalPlayerID = SystemInfo.deviceUniqueIdentifier;
    public static PlayerRole SelectedRole = PlayerRole.None; // Data peran dipusat di sini
}