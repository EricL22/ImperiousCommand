using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;

public class NameList
{
    public string Id;
    public string DisplayName;
    public List<string> FirstNames = new List<string>();
    public List<string> FemNames = new List<string>();
    public List<string> LastNames = new List<string>();
    public List<string> ShipNames = new List<string>();
    public bool DynastyFirst = false;
}

public class NameListLoader : MonoBehaviour
{
    public static NameListLoader instance;

    private Dictionary<string, NameList> nameLists = new Dictionary<string, NameList>();
    private string nameListPath;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        nameListPath = Path.Combine(Application.persistentDataPath, "data/name_lists/config.txt");
        LoadNameLists();
    }

    private void LoadNameLists()
    {
        if (!File.Exists(nameListPath))
        {
            Debug.LogError("Config file not found: " + nameListPath);
            return;
        }

        string configText = File.ReadAllText(nameListPath);
        ParseConfig(configText);
    }

    private void ParseConfig(string configText)
    {
        Regex listRegex = new Regex(@"(\w+)\s*=\s*\{([^}]*)\}", RegexOptions.Multiline);
        Regex fieldRegex = new Regex(@"(\w+)\s*=\s*(.*?);", RegexOptions.Multiline);

        foreach (Match match in listRegex.Matches(configText))
        {
            string id = match.Groups[1].Value.Trim();
            string body = match.Groups[2].Value.Trim();

            NameList nameList = new NameList { Id = id };

            foreach (Match field in fieldRegex.Matches(body))
            {
                string key = field.Groups[1].Value.Trim();
                string value = field.Groups[2].Value.Trim().Trim(';');

                switch (key)
                {
                    case "name":
                        nameList.DisplayName = value.Replace("\"", "");
                        break;
                    case "first":
                        nameList.FirstNames = LoadNameFile(value);
                        break;
                    case "fem":
                        nameList.FemNames = LoadNameFile(value);
                        break;
                    case "last":
                        nameList.LastNames = LoadNameFile(value);
                        break;
                    case "ship":
                        nameList.ShipNames = LoadNameFile(value);
                        break;
                    case "dynasty_first":
                        nameList.DynastyFirst = value.ToLower() == "yes";
                        break;
                }
            }

            nameLists[id] = nameList;
        }
    }

    private List<string> LoadNameFile(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "data/name_lists/" + fileName);
        List<string> names = new List<string>();

        if (File.Exists(filePath))
        {
            foreach (string line in File.ReadLines(filePath))
            {
                string trimmedLine = line.Trim(); // Remove extra spaces
                if (!string.IsNullOrEmpty(trimmedLine)) // Ignore empty lines
                {
                    names.Add(trimmedLine);
                }
            }
        }
        else
        {
            Debug.LogError($"Name file not found: {filePath}");
        }
        return names;
    }

    public NameList GetNameList(string id)
    {
        return nameLists.ContainsKey(id) ? nameLists[id] : null;
    }

    public List<string> GetAvailableLists()
    {
        return new List<string>(nameLists.Keys);
    }
}
