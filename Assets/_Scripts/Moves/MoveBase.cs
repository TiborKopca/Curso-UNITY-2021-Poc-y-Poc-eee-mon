using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Pokemon/Nuevo Movimiento")]
public class MoveBase : ScriptableObject
{
    [SerializeField] private string name;  //name of the base move / attack
    public string Name => name;
    
    [TextArea] [SerializeField] private string description;
    public string Description => description;

    [SerializeField] private PokemonType type;  //
    [SerializeField] private int power;         //how many points the base attack make  
    [SerializeField] private int accuracy;      //randomness
    [SerializeField] private int pp;            //how many times can use the attack until it exhausts, need to reload
    
    public PokemonType Type => type;
    public int Power => power;
    public int Accuracy => accuracy;
    public int PP => pp;

    //RESOLUTION OF THE SPECIAL ATTACKS
    public bool IsSpecialMove{
        get{
            if (type == PokemonType.Fire || 
                type == PokemonType.Water ||
                type == PokemonType.Grass || 
                type == PokemonType.Ice ||
                type == PokemonType.Electric || 
                type == PokemonType.Dragon ||
                type == PokemonType.Dark || 
                type == PokemonType.Psychic){
                return true;
            }else{
                return false;
            }
        }
    }

}