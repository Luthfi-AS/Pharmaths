using System;

[Serializable]
public class PlayerData {
    public string name;
    public bool isReady;
    public bool isConnected;

    public PlayerData(string playerName) {
        name = playerName;
        isReady = false;
        isConnected = true;
    }
}

[Serializable]
public class RoomMetadata {
    public bool gameStarted;
    public long createdAt;

    public RoomMetadata() {
        gameStarted = false;
        // Perbaikan di baris ini:
        createdAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}

[Serializable]
public class RoomStructure {
    public RoomMetadata metadata;
    // Structure ini akan menampung data pemain di level Firebase yang sama
}