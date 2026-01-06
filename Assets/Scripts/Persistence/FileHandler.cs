using UnityEngine;
using System.IO;

namespace Persistence
{
    public static class FileHandler
    {
        public static void SaveToFile<T>(T data, string fileName)
        {
            try
            {
                var json = JsonUtility.ToJson(data, true);
                var path = Path.Combine(Application.persistentDataPath, fileName);
                File.WriteAllText(path, json);
#if UNITY_EDITOR
                Debug.Log($"<color=green>[Save Success]</color> Data saved to: {path}");
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>[Save Failed]</color> Error saving {fileName}: {e.Message}");
            }
        }

        public static T LoadFromFile<T>(string fileName)
        {
            var path = Path.Combine(Application.persistentDataPath, fileName);

            if (!File.Exists(path))
            {
                Debug.LogWarning($"<color=yellow>[Load]</color> No save file found at: {path}");
                return default;
            }

            try
            {
                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<T>(json);
#if UNITY_EDITOR
                Debug.Log($"<color=green>[Load Success]</color> Data loaded from: {path}");
#endif
                return data;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"<color=red>[Load Failed]</color> Error loading {fileName}: {e.Message}");
                return default;
            }
        }
    }
}
