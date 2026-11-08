
using UnityEngine;
using System.Collections.Generic;

public class ScenarioLoader
{
    private Dictionary<string, ScenarioData> scenarioDictionary
        = new Dictionary<string, ScenarioData>();


    public bool Load(string resourcePath)
    {
        scenarioDictionary.Clear();

        TextAsset json = Resources.Load<TextAsset>(resourcePath);

        if (json == null)
        {
            Debug.LogError("JSONファイルが見つかりません: " + resourcePath);
            return false;
        }

        Debug.Log("読み込んだファイル名: " + json.name);
        Debug.Log("JSON文字数: " + json.text.Length);
        Debug.Log("JSON内容:\n" + json.text);

        try
        {
            NovelData data =
                JsonUtility.FromJson<NovelData>(json.text);

            if (data == null || data.scenarios == null)
            {
                Debug.LogError("JSONの形式が正しくありません");
                return false;
            }

            foreach (ScenarioData scenario in data.scenarios)
            {
                if (string.IsNullOrWhiteSpace(scenario.id))
                {
                    Debug.LogError("シナリオIDが空です");
                    return false;
                }

                if (scenarioDictionary.ContainsKey(scenario.id))
                {
                    Debug.LogError("シナリオIDが重複しています: " + scenario.id);
                    return false;
                }

                scenarioDictionary.Add(scenario.id, scenario);
            }

            Debug.Log("シナリオ読み込み成功: " + scenarioDictionary.Count);

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("JSON読み込みエラー: " + e.Message);
            return false;
        }
    }

    public ScenarioData GetScenario(string id)
    {
        if (scenarioDictionary.TryGetValue(id, out ScenarioData scenario))
        {
            return scenario;
        }

        Debug.LogError("シナリオが見つかりません: " + id);
        return null;
    }
}
