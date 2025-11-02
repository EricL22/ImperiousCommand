using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountryUI : MonoBehaviour
{
    public TMP_InputField rulerFirst;
    public TMP_InputField rulerLast;
    public Toggle femaleToggle;

    public void SaveCountry()
    {
        string curNLID = PlayerData.instance.countries[0].nameListId;
        bool dynFirst = NameListLoader.instance.GetNameList(curNLID).DynastyFirst;

        // Create the ruler of the player country
        Gender gender = femaleToggle.isOn ? Gender.Female : Gender.Male;
        string firstName = string.IsNullOrEmpty(rulerFirst.text.Trim()) ? NameGenerator.GenerateFirstName(curNLID, gender)
            : rulerFirst.text.Trim();
        string lastName = string.IsNullOrEmpty(rulerLast.text.Trim()) ? NameGenerator.GenerateLastName(curNLID) : rulerLast.text.Trim();

        CharacterData newRuler = new CharacterData(firstName, lastName, dynFirst, gender, 0, Role.Ruler);
        PlayerData.instance.characters.Add(newRuler);
        PlayerData.instance.countries[0].rulerId = newRuler.characterId;
    }
}
