using UnityEngine;

public enum Gender { Male, Female, None }
public enum Role { Ruler, Admiral, Captain, General, Scientist, Diplomat }

public class Character
{
    public CharacterData data;

    public string GetFullName()
    {
        return data.dynastyFirstName ? $"{data.lastName} {data.firstName}" : $"{data.firstName} {data.lastName}";
    }
}
