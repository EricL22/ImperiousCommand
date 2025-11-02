using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Transform referenceTransform;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // DEBUG ONLY: if the player ruler is not set when the scene loads, create a random character
        if (string.IsNullOrEmpty(PlayerData.instance.countries[0].rulerId))
        {
            string curNLID = PlayerData.instance.countries[0].nameListId;
            bool dynFirst = NameListLoader.instance.GetNameList(curNLID).DynastyFirst;
            Gender gender = (Random.value > 0.5f) ? Gender.Male : Gender.Female;

            CharacterData newRuler = new CharacterData(NameGenerator.GenerateFirstName(curNLID, gender),
                NameGenerator.GenerateLastName(curNLID), dynFirst, gender, 0, Role.Ruler);
            PlayerData.instance.characters.Add(newRuler);
            PlayerData.instance.countries[0].rulerId = newRuler.characterId;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
