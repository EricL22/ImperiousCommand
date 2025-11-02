using UnityEngine;
using System.Collections.Generic;

public class NameGenerator : MonoBehaviour
{
    private static System.Random rng = new System.Random();

    /** <summary>
     * Generates a specific name composed of a random element from nl_names and one from nl_surnames.
     * Meant to be used as constructor parameters for generating a new character.
     * </summary>
     * <returns>a tuple composed of a first name, a last name, and the order of elements.</returns>
     */
    public static (string, string, bool) GenerateFullName(string nameListId, Gender gender)
    {
        NameList list = NameListLoader.instance.GetNameList(nameListId);
        if (list == null) return ("Unknown", "Name", false);

        string first = gender == Gender.Female ? GetRandomElement(list.FemNames) : GetRandomElement(list.FirstNames);
        string last = GetRandomElement(list.LastNames);

        return (first, last, list.DynastyFirst);
    }

    /**<summary>
     * Meant to be used to generate an element of the name individually when the other is given.
     * </summary>
     */
    public static string GenerateFirstName(string nameListId, Gender gender)
    {
        NameList list = NameListLoader.instance.GetNameList(nameListId);
        return gender == Gender.Female ? GetRandomElement(list.FemNames) : GetRandomElement(list.FirstNames);
    }

    /**<summary>
     * Meant to be used to generate an element of the name individually when the other is given.
     * </summary>
     */
    public static string GenerateLastName(string nameListId)
    {
        NameList list = NameListLoader.instance.GetNameList(nameListId);
        return GetRandomElement(list.LastNames);
    }

    public static string GenerateShipName(string nameListId)
    {
        NameList list = NameListLoader.instance.GetNameList(nameListId);
        string shipName = list != null ? GetRandomElement(list.ShipNames) : "Unnamed Ship";

        return shipName;
    }

    private static string GetRandomElement(List<string> list)
    {
        return list.Count > 0 ? list[rng.Next(list.Count)] : "Unknown";
    }
}
