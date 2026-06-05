using System.IO;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    public static FileManager Instance { get; private set; }

    private string saveFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
    }

    public void SaveGame(int hp, int sp, bool bite, bool fireball, bool fracture, bool absorption, PlayerInventory inventory)
    {
        PlayerData data = new PlayerData
        {
            HP = hp,
            SP = sp,
            hasBite = bite,
            hasFireball = fireball,
            hasFracture = fracture,
            hasAbsorption = absorption
        };

        // Conversion du dictionnaire d'items
        foreach (var pair in inventory.itemsPossessed)
        {
            if (pair.Key != null)
            {
                data.items.Add(new ItemSaveData { itemId = pair.Key.itemId, amount = pair.Value });
            }
        }

        // Conversion du dictionnaire de dents
        foreach (var pair in inventory.teethPossessed)
        {
            if (pair.Key != null)
            {
                data.teeth.Add(new ToothSaveData { itemId = pair.Key.itemId, amount = pair.Value });
            }
        }

        // Conversion en JSON et écriture sur le disque
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"[FileManager] Jeu sauvegardé à l'emplacement : {saveFilePath}");
    }

    public PlayerData LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("[FileManager] Aucun fichier de sauvegarde trouvé.");
            return null;
        }

        string json = File.ReadAllText(saveFilePath);
        PlayerData data = JsonUtility.FromJson<PlayerData>(json);
        Debug.Log("[FileManager] Fichier de sauvegarde chargé avec succès.");
        return data;
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("[FileManager] Sauvegarde supprimée.");
        }
    }
}