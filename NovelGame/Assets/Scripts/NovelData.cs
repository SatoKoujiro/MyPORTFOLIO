
using System;

[Serializable]
public class NovelData
{
    public ScenarioData[] scenarios;
}

[Serializable]
public class ScenarioData
{
    public string id;
    public string name;
    public string text;

    public string background;
    public string character;

    public ChoiceData[] choices;

    public string nextId;
}

[Serializable]
public class ChoiceData
{
    public string text;
    public string nextId;
}
