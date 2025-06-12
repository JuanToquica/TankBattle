using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveManager
{
    private static string savePath = Application.persistentDataPath + "/player.dat";

    public static void SavePlayerData(PlayerData playerData)
    {
        FileStream fileStream = new FileStream(savePath, FileMode.Create);
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(fileStream, playerData);
        fileStream.Close();
    }

    public static PlayerData LoadPlayerData()
    {
        if (File.Exists(savePath))
        {
            FileStream fileStream = new FileStream(savePath, FileMode.Open);
            BinaryFormatter formatter = new BinaryFormatter();
            PlayerData playerData = formatter.Deserialize(fileStream) as PlayerData;
            fileStream.Close();
            return playerData;
        }
        else
        {
            Debug.LogError("No se encontro el archivo");
            return null;
        }
    }

    public static void DeletePlayerDataFile()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);
    }
}
