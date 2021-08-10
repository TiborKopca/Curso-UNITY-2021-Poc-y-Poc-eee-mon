using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] Text dialogText;           //introductive text=="a wild pokemon appears"

    [SerializeField] GameObject actionSelect;
    [SerializeField] GameObject movementSelect;
    [SerializeField] GameObject movementDesc;

    [SerializeField] List<Text> actionTexts;        //List of Actions = fight or run
    [SerializeField] List<Text> movementTexts;     //List of movements/attacks

    [SerializeField] Text ppText;                   //power points numbers 
    [SerializeField] Text typeText;                 //type of power point moves
    public float timeToWaitAfterText = 1f;
    public float charactersPerSecond = 10.0f; //typewriter style, In Editor we set 60
    [SerializeField] Color selectedColor = Color.blue;

    public bool isWriting = false;              //FLAG = the state for the Machine, if we press input, PC wont do anything when true
    public AudioClip[] characterSounds;         //sounds when texts is presented



//COROUTINE == IENUMERATOR
    public IEnumerator SetDialog(string message){
        isWriting = true;
        
        //This will write the text letter by letter
        dialogText.text = "";
        foreach (var character in message)
        {
            //SFX
            if (character!=' '){
                SoundManager.SharedInstance.RandomSoundEffect(characterSounds);
            }

            dialogText.text += character;
            yield return new WaitForSeconds(1/charactersPerSecond); // 1/10 == 0.1s, 30characters/s => 1/30 == 0.033
        }
        
        yield return new WaitForSeconds(timeToWaitAfterText);  // 1s
        isWriting = false;
    }

//TOGGLES
    //This will activate/desactivate the DialogText == the text IN object
    public void ToggleDialogText(bool state){
        dialogText.enabled = state;  //with state=true it will be shown, with state=false it wont be shown.
    }

    //Action Selector == Fight/Run
    //Action select is Game object, this is set similarly
    public void ToggleActions(bool state){
        actionSelect.SetActive(state);
    }

    //Game object "Movement Selector" == 4 movements/attacks we can make with pokemon
    //Game object "Movement Description"
    public void ToggleMovements(bool state){
        movementSelect.SetActive(state);
        movementDesc.SetActive(state);
    }
//END TOGGLES

//METHODS CALLED BY BATTLE MANAGER 
    //In BattleManager we call this funtion ==> it will based on number 0 or 1 select the color of the text
    //Black or Blue text, Blue color is predefined in parameters
    public void SelectAction(int selectedAction){
        for (int i = 0; i < actionTexts.Count; i++) //actionTexts are 2 in total 
        {
            actionTexts[i].color = (i == selectedAction ? selectedColor : Color.black);
        }
    }

    //In the list we have 4 moves/attacks
    //loop the moves and 
    public void SetPokemonMovements(List<Move> moves){
        for (int i = 0; i < movementTexts.Count; i++)
        {
            //if position of every move a.k.a index is less than position in list of the moves player has
            if (i < moves.Count)                
            {
                movementTexts[i].text = moves[i].Base.Name;
            }
            else //if there is some pokemon that has only 3 or less attacks, there will be empty text.
            {
                movementTexts[i].text = "---";
            }
        }
    }
    
    //This method calls Battle Manager, it needs current selected movement (int) and Move class object
    //Black or Blue text, Blue color is predefined in parameters
    public void SelectMovement(int selectedMovement, Move move)
    {
        for (int i = 0; i < movementTexts.Count; i++){
            movementTexts[i].color = (i == selectedMovement ? selectedColor : Color.black);
        }
        
        ppText.text = $"PP {move.Pp}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString().ToUpper();    //Type of the Pokemon or attack type?
        
        //Feedback to the player - color of the Power is set to red if there is no more Power available
        ppText.color = (move.Pp <= 0 ? Color.red : Color.black);
    }
//END METHODS CALLED BY BATTLE MANAGER   
}