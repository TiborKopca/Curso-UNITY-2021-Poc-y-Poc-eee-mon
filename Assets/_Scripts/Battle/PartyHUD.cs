using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyHUD : MonoBehaviour
{
    [SerializeField]private Text messageText;
    private PartyMemberHUD[] memberHuds;            //ARRAY!
    private List<Pokemon> pokemons;                 

    public float timeToWaitAfterText = 1f;
    public float charactersPerSecond = 60.0f;       //typewriter style, In Editor we set 60
    public bool isWriting = false;                  //FLAG = the state for the Machine, if we press input, PC wont do anything when true

    //instead of Start(), this way we can actualize the content of the HUDs.
    //Get UI to the variable
    public void InitPartyHUD(){
        //with parameter true, it will return also INACTIVE components
        memberHuds = GetComponentsInChildren<PartyMemberHUD>(true);
    }

    //Update the Party UI with the list of Pokemons that we have ONLY
    public void SetPartyData(List<Pokemon> pokemons){
        this.pokemons = pokemons;                               //Set the info about pokemons from parameters to be work with
        messageText.text = "Selectiona un Pokemon";             //Set text to the Party Selection Text

        for(int i = 0; i < memberHuds.Length; i++){             //Arrays.Length , List.Count
            if(i < pokemons.Count){
                memberHuds[i].SetPokemonData(pokemons[i]);
                memberHuds[i].gameObject.SetActive(true);       //update if we had previously hidden the UI
            }else{
                memberHuds[i].gameObject.SetActive(false);      //if there is none, dont show anything
            }
        }
    }

    //Call to the PartyMemberHUD ==> to change color of the cell
    //Called by the BattleManager
    public void UpdateSelectedPokemon(int selectedPokemon){
        for (int i = 0; i < pokemons.Count; i++){
            // if(i==selectedPokemon){                                   //This can be shortened to the line bellow
            //     memberHuds[i].SetSelectedPokemon(true);            
            // }else{
            //      memberHuds[i].SetSelectedPokemon(false);
            // }
                memberHuds[i].SetSelectedPokemon(i == selectedPokemon);
            }
    }

    //Enables message from outside the class
    public void SetMessage(string message){
        //Choose either messageText.text | SetDialog()
        messageText.text = message; 
        // Debug.Log("Setting Dialog in PartyHUD1");     
        // StartCoroutine(SetDialog(message));          //TODO Characters typewriter effect
    }

    //COROUTINE == IENUMERATOR
    public IEnumerator SetDialogPartyHUD(string message){
        isWriting = true;
        Debug.Log("Setting Dialog in PartyHUD2");
        //This will write the text letter by letter
        messageText.text = "";
        foreach (var character in message)
        {
            messageText.text += character;
            yield return new WaitForSeconds(1/charactersPerSecond); // 1/10 == 0.1s, 30characters/s => 1/30 == 0.033
        }
        
        yield return new WaitForSeconds(timeToWaitAfterText);  // 1s
        isWriting = false;
    }
}