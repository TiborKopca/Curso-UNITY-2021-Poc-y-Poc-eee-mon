using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum GameState { Travel, Battle }  //player move on world and battle

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController; //Player Object
    [SerializeField] private BattleManager battleManager;       //BattleManager Object
    [SerializeField] private Camera worldMainCamera;
    
    private GameState _gameState;          //implements 2 gamestates from ENUM

    public AudioClip worldClip, battleClip; //these holds the tracks that will be played



    //The Game will start at Travel mode
    private void Awake(){
        _gameState = GameState.Travel; 
    }


    //On EVENT in PlayerController, we Invoke Battle
    void Start(){
        SoundManager.SharedInstance.PlayMusic(worldClip);   //audio on start, at awake to me did problems(not a instance of an object)

        playerController.OnPokemonEncountered += StartPokemonBattle;

        //when OnBattleFinish == true? 
        battleManager.OnBattleFinish += FinishPokemonBattle;
    }

    //Invoking the Battle
    void StartPokemonBattle(){
        //audio played from SINGLETON
        SoundManager.SharedInstance.PlayMusic(battleClip);
        
        //on battle => change state of the game
        _gameState = GameState.Battle;

        //Enable the game object of the BattleManager Object => this will switch us to battle screen
        //On battle end, battleManager will return to be SetActive(false) => this will deactivate the 2nd camera under it, but this wont occur with the first one
        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);    //we need to deactivate the player camera

        //Get Player Pokemons
        var playerParty = playerController.GetComponent<PokemonParty>();
        //Get Wild pokemons from the Pokemon Map Area
        //If we have more Pokemon Map Areas, use FindObjectsOfType
        var wildPokemon = FindObjectOfType<PokemonMapArea>().GetComponent<PokemonMapArea>().GetRandomWildPokemon();  
        
        //MAKE A COPY OF A WILD POKEMON, SO WE ARE NOT GETTING AS WILD POKEMON OUR, ALREADY CAPTURED POKEMONS
        //we pass the Base and Level parameters and call the constructor
        var wildPokemonCopy  = new Pokemon(wildPokemon.Base, wildPokemon.Level);
        
        //invoking the BattleManager Start Battle
        //battleManager.HandleStartBattle(); //OBSOLETE
        battleManager.HandleStartBattle(playerParty, wildPokemonCopy);
    }

    //We end the battle, even we lose or win
    //this capture the boolean state of the battleManager.OnBattleFinish
    void FinishPokemonBattle(bool playerHasWon)
    {
        SoundManager.SharedInstance.PlayMusic(worldClip);
        
        //we switch the gamestate to travel
        _gameState = GameState.Travel;
        //enable the camera and disable the battleManager
        battleManager.gameObject.SetActive(false);
        worldMainCamera.gameObject.SetActive(true);

        //if player has lost with last pokemon?
        if (!playerHasWon)
        {
            //TODO: diferenciar entre victoria y derrota
        }
    }
    

    //EVERY FRAME CHECKS FOR STATE CHANGE AND SWITCHES TO PLAYERCONTROLLER | BATTLEMANAGER
    private void Update()
    {
        if (_gameState == GameState.Travel){
           playerController.HandleUpdate();

        } else if (_gameState == GameState.Battle){
            battleManager.HandleUpdate();
        }
    }
}