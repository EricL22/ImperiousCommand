using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

[System.Serializable]
public class CountryData
{
    private int countryId => PlayerData.instance.countries.IndexOf(this);
    public string nameListId;
    public string shipPrefix;
    private string _rulerId;
    public string rulerId
    {
        get => _rulerId;
        set
        {
            // only allow assigning Rulers who are part of the country
            if (PlayerData.instance.characters.Find(c => c.characterId == value).role != Role.Ruler ||
                PlayerData.instance.characters.Find(c => c.characterId == value).countryId != countryId)
                throw new InvalidOperationException($"Illegal ruler assignment for country {countryId}");
            _rulerId = value;
        }
    }
}

[System.Serializable]
public class CharacterData
{
    public string characterId = Guid.NewGuid().ToString();  // Unique ID
    public string firstName;
    public string lastName;
    public bool dynastyFirstName;
    public Gender gender;
    public int countryId { get; private set; }    // ID of the country this character belongs to
                                                  // for now, cannot change after initialization
    private Role _role;
    public Role role
    {
        get => _role;
        set
        {
            // only allow changing if not already ruler of a country
            if (PlayerData.instance.countries.Any(c => c.rulerId == characterId) && value != Role.Ruler)
                throw new InvalidOperationException($"Illegal role assignment for ruler of country {countryId}");
            _role = value;
        }
    }

    public CharacterData(string first, string last, bool dynFirst, Gender gen, int owner, Role rol)
    {
        firstName = first;
        lastName = last;
        dynastyFirstName = dynFirst;
        gender = gen;
        countryId = owner;
        role = rol;
    }
}

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    public List<CountryData> countries = new List<CountryData> { new CountryData() };
    public List<CharacterData> characters = new List<CharacterData>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    [System.Serializable]
    class SaveData
    {
        public List<CountryData> countries;
        public List<CharacterData> characters;
    }

    public void SaveToFile()
    {
        SaveData data = new SaveData();
        data.countries = countries;
        data.characters = characters;
        string filePath = Path.Combine(Application.persistentDataPath, "player_data.json");
        File.WriteAllText(filePath, JsonUtility.ToJson(data));
    }

    public void LoadFromFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "player_data.json");
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            countries = data.countries;
            characters = data.characters;
        }
    }
}
