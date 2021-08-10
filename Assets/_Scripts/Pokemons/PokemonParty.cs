using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class PokemonParty : MonoBehaviour
{
    [SerializeField]
    private List<Pokemon> pokemons;
    public List<Pokemon> Pokemons{
        get => pokemons;                //this can be written as "get{return pokemons;}"
        set => pokemons = value;        //this we will never use
    }

    public const int NUM_MAX_POKEMON_IN_PARTY = 6;  //constant number, wont change

    private List<List<Pokemon>> pcBillBoxes;        //inicialization is in Start()
    // private List<List<Pokemon>> pcBillBoxes = new List<List<Pokemon>>(6); //normally we can inicialize directly the list from parameters        
    // public List<Pokemon> Pokemons{get => pokemons;}



    //Inicialize all the pokemons in the list
    private void Start(){
        foreach(var pokemon in pokemons){
            pokemon.InitPokemon();  
        }

        //were not using this structure, notice this holds 15 pokemons
        //var box = new List<Pokemon>(15);
        //for (int i = 0; i < 6; i++)
        //{
        //pcBillBoxes.Add(box);
        //}
    }

    //Search for the first Healthy | non dead pokemon with LAMBDA
    public Pokemon GetFirstNonFaintedPokemon(){
        // foreach(var pokemon in pokemons){
        //     if(pokemon.HP > 0){
        //         return pokemon;
        //     }
        // }
        
        //Where is like foreach, e.g. all pokemons with HP major 0, sort them, if there is none, it will return null
        return pokemons.Where(pokemonX => pokemonX.HP > 0).FirstOrDefault();
    }

    //before called GetPositionFirstNonFaintedPokemon()
    public int GetPositionFromPokemon(Pokemon pokemon){
        for (int i = 0; i < Pokemons.Count; i++){
            if (pokemon == Pokemons[i]){
                return i;
            }
        }
        return -1;              //this is default value, in for almost obligatory, if this value pop up, we know this is not valid value == should not happen
    }
    
    //Returns True/False. To the list of OUR pokemons will be added inserted pokemon. 
    public bool AddPokemonToParty(Pokemon pokemon){
        if (pokemons.Count < NUM_MAX_POKEMON_IN_PARTY){
            pokemons.Add(pokemon);
            return true;
        }else{
            //TODO: Añadir la funcionalidad de enviar al PC de Bill
            return false;
        }
    }
}