using System;
using UnityEngine;

public static class SaveSystem
{
    private const string SaveKey = "SaveData";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
        Debug.Log("[SaveSystem] Game saved.");
    }

    public static SaveData Load()
    {
        if (!HasSave())
            return null;

        try
        {
            string json = PlayerPrefs.GetString(SaveKey);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load save: {e.Message}");
            return null;
        }
    }

    public static bool HasSave() => PlayerPrefs.HasKey(SaveKey);

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
    }
}
