using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SelectionMovementUI : MonoBehaviour
{
    [SerializeField] private Text[] movementTexts;      //not using the Start()--> GetComponentsInChildren to get objects
    [SerializeField] private Color selectedColor;
    private int currentSelectedMovement = 0;            //at start will be selected first movement
    

    //This will be commented/not used
    // private void Start(){
    //     movementTexts = GetComponentsInChildren<Text>(true);
    // }


    //From passed new move will actualize the list of Pokemon moves/attacks
    public void SetMovements(List<MoveBase> pokemonMoves, MoveBase newMove){
        currentSelectedMovement = 0;                        //each time the list with movements be shown, the first will be selected

        for (int i = 0; i < pokemonMoves.Count; i++){
            movementTexts[i].text = pokemonMoves[i].Name;
        }
        //to he movement Texts objects we add new move(at the end)
        movementTexts[pokemonMoves.Count].text = newMove.Name;
    }



    //called from BattleManager
    //Handles Input from the Player
    //Parameter is ACTION with number with Name == onSelected
    public void HandleForgetMoveSelection(Action<int> onSelected){
      
        //INPUT UP DOWN
        if (Input.GetAxisRaw("Vertical")!=0){
            //-1 to 1 when used GetAxisRaw
            int direction = Mathf.FloorToInt(Input.GetAxisRaw("Vertical"));
            currentSelectedMovement -= direction;
            //Action fullfiled, but we dont want to do nothing, we assign NON-Valid number
            onSelected?.Invoke(-1);
        }

        currentSelectedMovement = Mathf.Clamp(currentSelectedMovement, 0, PokemonBase.NUMBER_OF_LEARNABLE_MOVES);
        
        //update color of the move/attack in list
        UpdateColorForgetMoveSelection(currentSelectedMovement); 

        //ENTER
        if (Input.GetAxisRaw("Submit")!=0){
            //onSelected action is fulfilled ==> that will launch the method Invoke with number of selected movement/attack in list
            onSelected?.Invoke(currentSelectedMovement);
        }
    }

    //we need to loop 4+1 position to change color on the selected row
    public void UpdateColorForgetMoveSelection(int selectedMove){

        for (int i = 0; i <= PokemonBase.NUMBER_OF_LEARNABLE_MOVES; i++){
            movementTexts[i].color = (i == selectedMove ? selectedColor : Color.black);
        }
    }
}
