using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactersDatabase", menuName = "Visual Novel/Characters Database")]
public class CharacterDatabase : ScriptableObject
{
    public List<Character> charactersList = new List<Character>();
}
