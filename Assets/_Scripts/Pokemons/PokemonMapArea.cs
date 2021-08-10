using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonMapArea : MonoBehaviour
{
    [SerializeField] private List<Pokemon> wildPokemons;        //this will hold the wild pokemons => we need to set them in Inspector

    //Random selection of the listed wild pokemons
    public Pokemon GetRandomWildPokemon(){
        var pokemon = wildPokemons[Random.Range(0,wildPokemons.Count)];

        //inicialize the random pokemon, so the instance will exist in game
        pokemon.InitPokemon(); 
        return pokemon;
    }
}
