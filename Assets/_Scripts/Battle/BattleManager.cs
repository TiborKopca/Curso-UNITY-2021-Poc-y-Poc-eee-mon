using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;
using System.Linq;

public enum BattleType{
   WildPokemon,
   Trainer,
   Leader
}

//for the actions can be accesible outside the class anywhere! 
//Generic, it is not dependable on Manager
public enum BattleState{
   StartBattle,         //this will show text
   ActionSelection,     //player will be able to select action = fight/run
   MovementSelection,   //player OR enemy chosen attacks
   Busy,                //nothing can be chosen = when somebody is paralyzed or blocked
   PartySelectScreen,   //UI where we choose from team of our pokemons
   PerformMovement,     //the attack is happening
   ForgetMovement,      //The player enters the UI for selection of Attacks
   LoseTurn,            //the player turn is over
   FinishBattle         //the battle end
}

public class BattleManager : MonoBehaviour{
   [SerializeField] BattleUnit playerUnit;
   // [SerializeField] BattleHUD playerHUD;
   
   [SerializeField] BattleUnit enemyUnit;
   // [SerializeField] BattleHUD enemyHUD;

   [SerializeField] BattleDialogBox battleDialogBox;  //this holds Text that will print

   public BattleState state;                          //here can be only states from the ENUM above

   [Tooltip("Here drag the Party Selection object.")]
   [SerializeField] PartyHUD partyHUD;                //Team of our Pokemons

   [SerializeField] SelectionMovementUI selectMoveUI;

   public event Action<bool> OnBattleFinish;          //this holds if the battle is over, this returns boolean
   
   private PokemonParty playerParty;                  //we use this in StartBattle
   private Pokemon wildPokemon;                       //we use this in StartBattle

   //sole purpose of these 2 variables is to slow down the input of the player, so he is not able scroll too fast
   //timeSinceLastClick is incremented by real time in UPDATE()
   private float timeSinceLastClick;
   [SerializeField] float timeBetweenClicks = 1.0f;
   
   //there is no button, nor event, this will hold the CURRENT selected action
   //the value is set in PlayerActionSelection(), before called PlayerAction()
   private int currentSelectedAction;  

   //selected pokemon used in HandlePlayerPartySelection
   private int currentSelectedPokemon;                   

   //there is no button, nor event, this will hold the CURRENT selected action
   //the value is set in PlayerMovemen()
   private int currentSelectedMovement;

   [SerializeField] GameObject pokeball;              //Implementing pokeball throw via UI

   public BattleType type;                            //According to Enum above, we would be able to select the type of the pokemon player would fight

   private int escapeAttempts;                        //used in TryToEscapeFromBattle()
   private MoveBase moveToLearn;                      //used in ChooseMovementToForget()

   //AUDIO
   public AudioClip attackClip, damageClip, levelUpClip, pokeballClip, faintedClip, endBattleClip;






   //Start Battle Handler, pass the Player Pokemon and Wi d Pokemon
   //we change the Start() to normal function and it will be public
   public void HandleStartBattle(PokemonParty playerParty, Pokemon wildPokemon){
      
      type = BattleType.WildPokemon;   //type of the gameplay == against wild pokemons
      escapeAttempts = 0;              //reset the escapeAttempts for the player @ each battle start

      this.playerParty = playerParty;
      this.wildPokemon = wildPokemon;
      
      //Invoke Coroutine 
      StartCoroutine(SetupBattle());
   }

   //Other type of battle type, prepared for the future updates. GameManager is calling the choice
   public void HandleStartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty, bool isLeader)
   {
      type = (isLeader? BattleType.Leader: BattleType.Trainer);
      //TODO: el resto dde batalla contra NPC
   }

//START BATTLE
   public IEnumerator SetupBattle()
   {
      //Set the state => so we know what part of the program is curently in action
      state = BattleState.StartBattle; 

      //Configure of the Pokemon -- This updates HP bars of the Player/Enemy Pokemons
      //This will efectively Instanciate(OBSOLETE)
      //Create existing pre-listed pokemon(OBSOLETE)
      //Instanciate the Pre-configured Player Pokemons
      playerUnit.SetupPokemon(playerParty.GetFirstNonFaintedPokemon()); 

      //Player Unit which has Pokemon data will set it to BattleHUD
      //This is update for the HUD
      ///playerHUD.SetPokemonData(playerUnit.Pokemon); //OBSOLETE with BattleUnit where are now sorted huds
      
      //This will call the method for filling the moves(or attacks) of our player pokemon
      battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
      

      //Simmilar as Player Pokemon had above
      //This will efectively Instanciate(OBSOLETE)
      //Create existing pre-listed pokemon(OBSOLETE)
      //Enemy pokemon will be inicialized Wild pokemon
      enemyUnit.SetupPokemon(wildPokemon);
      
      //enemyHUD.SetPokemonData(enemyUnit.Pokemon); //OBSOLETE with BattleUnit where are now sorted huds
      
      //Inicialize of the PartyHUD
      partyHUD.InitPartyHUD();

      //this will take the data from class BattlediaLogBox == text
      //After Battle is set up, we will enable the Dialog with this text, thus rewriting WHAT we want WHEN we want.
      yield return battleDialogBox.SetDialog($"Un {enemyUnit.Pokemon.Base.Name} salvaje apareció.");

      //FIRST ATTACK RESOLUTION == who attacks first
      if (enemyUnit.Pokemon.Speed > playerUnit.Pokemon.Speed){
         //Showing the dialogText
         battleDialogBox.ToggleDialogText(true);
         //hiding the text before another box(before the enemy will complete the attack) will show
         battleDialogBox.ToggleActions(false);
         //hiding the movement dialogbox
         battleDialogBox.ToggleMovements(false);
         yield return battleDialogBox.SetDialog("El enemigo ataca primero.");
         yield return PerformEnemyMovement();      //before it was EnemyAction()
      }  
      else
      {
         PlayerActionSelection();                  //this is not a coroutine, because it depends on player, enemy action doesnt
      }
   }

//END BATTLE
   void BattleFinish(bool playerHasWon)
   {
      //AUDIO | SFX
      SoundManager.SharedInstance.PlaySound(endBattleClip);

      state = BattleState.FinishBattle;
      OnBattleFinish(playerHasWon);
   }

//PLAYER ACTION 
   void PlayerActionSelection(){
      state = BattleState.ActionSelection;  //setting current state

      StartCoroutine(battleDialogBox.SetDialog("Selecciona una acción"));  
      battleDialogBox.ToggleDialogText(true);   //show the Dialog text we just set line above
      battleDialogBox.ToggleActions(true);      //show action selection
      battleDialogBox.ToggleMovements(false);   //hide Move selection
      currentSelectedAction = 0;                //set that we are going to highlight the first or second line of Action Selector Text
      battleDialogBox.SelectAction(currentSelectedAction);
   }
//PLAYER ATTACK
   void PlayerMovementSelection(){
      state = BattleState.MovementSelection;

      battleDialogBox.ToggleDialogText(false);  
      battleDialogBox.ToggleActions(false);
      battleDialogBox.ToggleMovements(true);
      currentSelectedMovement = 0;             //reset the last move
      battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
   }


    //Previously called Update, but now we have GameManager, which controls/handles updates
    //changed from private to public
   public void HandleUpdate(){
      //TIME MANAGEMENT -- when player inputs faster than set, wont do nothing
      //basically if the current counter is more than 1, dont do anything and return == END THE FUNCTION!
      timeSinceLastClick += Time.deltaTime;
      if (timeSinceLastClick < timeBetweenClicks || battleDialogBox.isWriting){
         return;
      }

      //If text is still writing => this frame do nothing, game wont take our input until the text from DialogBox 
      if (battleDialogBox.isWriting){
         return;
      }
      
      //Player selected action, we invoke Next fuction that will handle the selection. 
      if (state == BattleState.ActionSelection){
         HandlePlayerActionSelection();            //Handle Fight/Run selection
      }else if (state == BattleState.MovementSelection){
         HandlePlayerMovementSelection();          //Handle Attack
      }else if(state == BattleState.PartySelectScreen){
         HandlePlayerPartySelection();             //Choose from UI a pokemon
      }else if (state == BattleState.LoseTurn){
         StartCoroutine(PerformEnemyMovement());   //If Player Loses Turn, continues enemy with his turn
      }else if (state == BattleState.ForgetMovement){

         //Uses ACTION as parameter, HandleForgetMoveSelection(ACTION int)
         selectMoveUI.HandleForgetMoveSelection(         //some of the code will be occuring in the UI panel and some in BattleManager
            (moveIndex) => {                             //moveindex == whole number 
               if (moveIndex < 0){                       //if the number is -1 for example, negative, reset time and do nothing
                  timeSinceLastClick = 0;
                  return;
               }
               //number 0-4
               StartCoroutine(ForgetOldMove(moveIndex)); //if moveindex != negative number, pass the number
            });
         }
   }
   
   //REPLACES OLD MOVE/ATTACK WITH THE NEW ONE, Also prints DIALOGBOX
   IEnumerator ForgetOldMove(int moveIndex){
      selectMoveUI.gameObject.SetActive(false);
      if (moveIndex == PokemonBase.NUMBER_OF_LEARNABLE_MOVES){
         yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} no ha aprendido {moveToLearn.Name}");
      }else{
         var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
         yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} olvidó {selectedMove.Name} y aprendió {moveToLearn.Name}");
         
         //take selected (OLD) move and put the NEW move in
         playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);         
      }

      //clear the moveToLearn variable         
      moveToLearn = null;
      //TODO: revisar más adelante cuando haya entrenadores 
      state = BattleState.FinishBattle;
   }


   //Handle Fight/Run selection
   void HandlePlayerActionSelection(){
      
      //Player pressed UP or DOWN key
      if (Input.GetAxisRaw("Vertical")!=0)
      {
         timeSinceLastClick = 0; //reset the time, so it can be incremented by UPDATE again

         //this will select the text on the dialog box when we press input == arrow keys
         currentSelectedAction = (currentSelectedAction + 2) % 4; 
         //if selected action 0 + 1 = 1, 1%2 = 1 (1/1 = 1, the rest to the 2 is 1. result == 1)
         //if selected action 1 + 1 = 2, 2%2 = 0 (2/2 = 0, the rest to the 2 is 0. result == 0)

         //Coloring of the text
         battleDialogBox.SelectAction(currentSelectedAction);

      }else if(Input.GetAxisRaw("Horizontal") != 0){
         timeSinceLastClick = 0;
         currentSelectedAction = (currentSelectedAction + 1) % 2 + 2 * Mathf.FloorToInt(currentSelectedAction / 2);
      
         //Coloring of the text
         battleDialogBox.SelectAction(currentSelectedAction);
      }

      //Player pressed ENTER
      if (Input.GetAxisRaw("Submit")!=0)
      {
         timeSinceLastClick = 0; //reset the time, so it can be incremented by UPDATE again
         
         if (currentSelectedAction == 0)     //player selected Battle
         {
            // PlayerMovement();
            PlayerMovementSelection();
         }else if (currentSelectedAction == 1) //player selected Change Pokemon
         {  
            OpenPartySelectionScreen();
         }else if (currentSelectedAction == 2) //player selected Open Backpack
         {
            OpenInventoryScreen();
         }else if (currentSelectedAction == 3) //player selected Run Away
         {
            //OnBattleFinish(false);      //OBSOLETE BY TryToEscapeFromBattle()
            StartCoroutine(TryToEscapeFromBattle());
         }
      }
   }


   //Logic of the Player move/attack selection
   void HandlePlayerMovementSelection(){
      /// 0  1
      /// 2  3
      /*--------------------------VERTICAL INPUT--------------------------*/
      /*There are only 2 states possible ==> 0 and 2, this will have module 4 */
      if (Input.GetAxisRaw("Vertical")!=0)
      {
         timeSinceLastClick = 0;
         var oldSelectedMovement = currentSelectedMovement;
         currentSelectedMovement = (currentSelectedMovement + 2) % 4;
         if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count)
         {
            currentSelectedMovement = oldSelectedMovement;
         }
         
         battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);

      /*--------------------------HORIZONTAL INPUT--------------------------*/ 
      /*There are only 2 states possible ==> 0 and 1, this will have module 2 */     
      } else if (Input.GetAxisRaw("Horizontal") != 0)
      {
         timeSinceLastClick = 0;
         var oldSelectedMovement = currentSelectedMovement;

         if (currentSelectedMovement <= 1){
            currentSelectedMovement = (currentSelectedMovement + 1) % 2;
         }else{ //currentSelectedMovement >= 2
            currentSelectedMovement = (currentSelectedMovement + 1) % 2 + 2;
         }

         //Current move is larger that what player Pokemon has?
         if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count){
            currentSelectedMovement = oldSelectedMovement;
         }
         battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);  
      }
      
      //ON ENTER
      if (Input.GetAxisRaw("Submit") != 0){

         timeSinceLastClick = 0;
         //on attack hide the movement texts and enable battle toggle texts
         battleDialogBox.ToggleMovements(false);
         battleDialogBox.ToggleDialogText(true);

         //Execute actual Battle Coroutine!
         StartCoroutine(PerformPlayerMovement());
      }

      //ESC == Cancel
      if (Input.GetAxisRaw("Cancel") != 0){
         //the hiding of the not needed dialogs are done in the PlayerActionSelection, no need to close them from here
         PlayerActionSelection();
      }
   }



   //OPENS CANVAS WITH POKEMON SELECTION
   void OpenPartySelectionScreen(){
      // print("Abrir la pantalla para selectionar Pokemons.");
      state = BattleState.PartySelectScreen;                   //state machine update
      partyHUD.SetPartyData(playerParty.Pokemons);
      partyHUD.gameObject.SetActive(true);
      currentSelectedPokemon = playerParty.GetPositionFromPokemon(playerUnit.Pokemon);

      //call to the PartyMemberHUD class, this changes color to the selected pokemon cell
      partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);

      //ESCAPE PRESSED
      if(Input.GetAxisRaw("Cancel") != 0){
         PlayerActionSelection();
      }

   }



   //OPENS CANVAS WITH POKEMON SELECTION
   void OpenInventoryScreen(){
      //TODO: Implementar Inventario y lógica de ítems
      print("Abrir inventario");

      //Throw Pokeballs
      battleDialogBox.ToggleActions(false);  
      StartCoroutine(ThrowPokeball());

      //ESCAPE PRESSED , doesnt make sense here
      // if(Input.GetAxisRaw("Cancel") != 0){
      //    PlayerActionSelection();
      // }
   }



   //Selection of the Pokemon from UI
   void HandlePlayerPartySelection(){
      /// 0  1
      /// 2  3
      /// 4  5

      //VERTICAL;
      if (Input.GetAxisRaw("Vertical") != 0){
         
         timeSinceLastClick = 0;
         currentSelectedPokemon -= (int)Input.GetAxisRaw("Vertical")*2;       //rest because we need to move down 0-2-4
         //Debug.Log("Vertical:" + currentSelectedPokemon);
      //HORIZONTAL
      } else if (Input.GetAxisRaw("Horizontal") != 0){
         
         timeSinceLastClick = 0;
         currentSelectedPokemon += (int)Input.GetAxisRaw("Horizontal");       //sum because we need to move right 0-1
         //Debug.Log("Horizontal:" + currentSelectedPokemon);
      }

      //PlayerParty.Pokemons.Count -1 because the first one is position 0 ??
      //Clamp(int value, int min, int max);
      currentSelectedPokemon = Mathf.Clamp(currentSelectedPokemon, 0, playerParty.Pokemons.Count - 1);
      //Debug.Log("AfterClamp:" + currentSelectedPokemon);
      //call to the PartyMemberHUD class, this changes color to the selected pokemon cell
      partyHUD.UpdateSelectedPokemon(currentSelectedPokemon);

      //SUBMIT
      if (Input.GetAxisRaw("Submit")!=0)
      {
         timeSinceLastClick = 0;
         //Selected pokemon, only if its not dead
         var selectedPokemon = playerParty.Pokemons[currentSelectedPokemon]; 
         if (selectedPokemon.HP <= 0){

            partyHUD.SetMessage("No puedes enviar un pokemon debilitado"); //we need to have Method in PartyHUD to do this.
            return;

         }else if (selectedPokemon == playerUnit.Pokemon){        //If you have already selected 1 pokemon already is in the battle
            //We are comparing position in memory or Objects
            partyHUD.SetMessage("No puedes seleccionar el pokemon en batalla");
            return;
         }

         partyHUD.gameObject.SetActive(false);              //disable the PartyHUD
         state = BattleState.Busy;                          //while the change is happening, state == busy

         //If we select Not Dead, Not the same pokemon, coroutine will switch the current pokemon
         StartCoroutine(SwitchPokemon(selectedPokemon));
      }

      //CANCEL
      if (Input.GetAxisRaw("Cancel")!=0)
      {
         partyHUD.gameObject.SetActive(false);
         // PlayerAction();
         PlayerActionSelection();
      }
   }

   //CHECK FOR ENDBATTLE CONDITIONS == the Player has not a single alive Pokemon
   void CheckForBattleFinish(BattleUnit faintedUnit){
      if (faintedUnit.IsPlayer){
         var nextPokemon = playerParty.GetFirstNonFaintedPokemon();
         if (nextPokemon != null){
            OpenPartySelectionScreen();
         }else{
            BattleFinish(false);
         }
      }else{                     //the dead pokemon is enemy
         BattleFinish(true);
      }
   }


//COROUTINES
   //Print to the Battle Dialog Box what attack player or enemy pokemon has used
   IEnumerator PerformPlayerMovement(){
      state = BattleState.PerformMovement;

      Move move = playerUnit.Pokemon.Moves[currentSelectedMovement]; //what move it was used
      
      //Call another coroutine to execute movement/attack
      yield return RunMovement(playerUnit, enemyUnit, move);
      //Check for Power units
      if (move.Pp <= 0){
         PlayerMovementSelection();
         yield break;
      }
      
      if (state == BattleState.PerformMovement){ //if during the attack there is no change to the state of the battle
         StartCoroutine(PerformEnemyMovement());
      }
   }

   //ENEMY ACTION
   IEnumerator PerformEnemyMovement(){
      state = BattleState.PerformMovement;

      //enemy pokemon will select random move to attack player
      Move move = enemyUnit.Pokemon.RandomMove();

      yield return RunMovement(enemyUnit, playerUnit, move);

      if (state == BattleState.PerformMovement)
      {
         PlayerActionSelection();
      }



      //    var nextPokemon = playerParty.GetFirstNonFaintedPokemon(); 
      //    //if there is no pokemon left in our posession
      //    if(nextPokemon == null){                     
      //       OnBattleFinish(false);                 //the battle is over,we lost pokemon, until we program the pokemon party
      //    }else{
      //    //NEXT FREE NON DEAD POKEMON OBSOLETE
      //       // playerUnit.SetupPokemon(nextPokemon);   
      //       // playerHUD.SetPokemonData(nextPokemon);

      //       // //show attacks of the next pokemon, so we wont have the attacks of the previous one
      //       // battleDialogBox.SetPokemonMovements(nextPokemon.Moves); 

      //       // yield return battleDialogBox.SetDialog($"Adelante {nextPokemon.Base.Name}!");
      //       // PlayerAction();

      //       OpenPartySelectionScreen();
      //    }
         
      // }else{
      //    PlayerActionSelection();
      // }
   }

   //THIS WILL SHOW DATA TO THE PLAYER IF ATTACK IS CRITICAL
   IEnumerator ShowDamageDescription(DamageDescription desc)
   {
      //only above 1 modifier can be critical
      if (desc.Critical > 1f)
      {
         yield return battleDialogBox.SetDialog("¡Un golpe crítico!");
      }


      //if the type of special attack has a damage modifier ?
      if (desc.Type > 1)
      {
         yield return battleDialogBox.SetDialog("¡Es super efectivo!");
      }else if (desc.Type < 1)
      {
         yield return battleDialogBox.SetDialog("No es muy efectivo...");
      }
      
   }



   //Changes current pokemon to another selected in PartyHUD
   IEnumerator SwitchPokemon(Pokemon newPokemon){
      //The Pokemon is alive
      if (playerUnit.Pokemon.HP > 0)
      {
         yield return battleDialogBox.SetDialog($"¡Vuelve {playerUnit.Pokemon.Base.Name}!");
         //animation to disapear the old pokemon?
         playerUnit.PlayFaintAnimation();
         yield return new WaitForSeconds(1.5f);
      }
      
      //We need to configurate a new pokemon
      playerUnit.SetupPokemon(newPokemon);

      playerUnit.Hud.SetPokemonData(newPokemon);

      battleDialogBox.SetPokemonMovements(newPokemon.Moves);
      
      yield return battleDialogBox.SetDialog($"¡Ve {newPokemon.Base.Name}!");
      // StartCoroutine(PerformEnemyMovement());
      StartCoroutine(PerformEnemyMovement());

   }

   //EXECUTES movement, parameters - source of attac, target of attack, what type of attack
   //This is for all the movement/action coroutines that has the same code
   IEnumerator RunMovement(BattleUnit attacker, BattleUnit target, Move move){
      
      move.Pp--;                           //this will decrement the units of Power in each attack
      yield return battleDialogBox.SetDialog($"{attacker.Pokemon.Base.Name} ha usado {move.Base.Name}");

      var oldHPVal = target.Pokemon.HP;
      //DOTween Animation
      attacker.PlayAttackAnimation();

      //AUDIO | SFX
      SoundManager.SharedInstance.PlaySound(attackClip);
      yield return new WaitForSeconds(1f);
      //ANIMATION
      target.PlayReceiveAttackAnimation();
      //AUDIO | SFX
      SoundManager.SharedInstance.PlaySound(damageClip);
      yield return new WaitForSeconds(0.5f);
      
      //damage formula
      //the ReceiveDamage will modify HP and return true|false if Pokemon is dead
      var damageDesc = target.Pokemon.ReceiveDamage(attacker.Pokemon, move);
      
      //HUD update
      yield return target.Hud.UpdatePokemonData(oldHPVal);

      //Description of the attack
      yield return ShowDamageDescription(damageDesc);
      
      //on death
      if (damageDesc.Fainted){
         // yield return battleDialogBox.SetDialog($"{target.Pokemon.Base.Name} se ha debilitado");   //OBSOLETE
         // target.PlayFaintAnimation();                                                              //OBSOLETE

         // //after animation happens! 
         // yield return new WaitForSeconds(1.5f);                                                    //OBSOLETE
        
         // //Check for End Battle Conditions
         // CheckForBattleFinish(target);                                                             //OBSOLETE

         yield return HandlePokemonFainted(target);
      }
   }

   //Called by OpenInventoryScreen()
   IEnumerator ThrowPokeball(){
      state = BattleState.Busy;

      //Only if the enemy is wild we can capture, if the pokemon has a master != capture
      if (type != BattleType.WildPokemon){
         yield return battleDialogBox.SetDialog("No puedes robar los pokemon de otros entrenadores");
         state = BattleState.LoseTurn;
         yield break;
      }

      //DialogBox Text
      yield return battleDialogBox.SetDialog($"Has lanzado una {pokeball.name}!");

      //AUDIO | SFX
      SoundManager.SharedInstance.PlaySound(pokeballClip);

      //INSTANCIATING
      //This holds the information about position of the spawned pokeball
      var pokeballInst = Instantiate(pokeball, playerUnit.transform.position + new Vector3(-2,0),Quaternion.identity);
      //Sprite variable
      var pokeballSpt = pokeballInst.GetComponent<SpriteRenderer>();

      //Tranform/Translate to the new position, this is aninmation, can be added .WaitForCompletion()
      //DOLocalJump(target position(enemy pokemon) + a bit above him == Vector3(x,y coordinates), force of the throw-parabolic arc, number of jump, duration)
      yield return pokeballSpt.transform.DOLocalJump(enemyUnit.transform.position + new Vector3(-1, -1.5f), 2f, 2, 1f).WaitForCompletion();

      //Rotate when fallen down and move to the enemy.
      yield return pokeballSpt.transform.DOLocalRotate(new Vector3(0, 0, 300.0f), 1f);
      yield return pokeballSpt.transform.DOLocalMoveX(enemyUnit.transform.position.x + 0.4f, 1f).WaitForCompletion();

      //capturing enemy pokemon animation
      yield return enemyUnit.PlayCapturedAnimation();
      
      //The pokeball will fall down to the position of the enemy pokemon
      // yield return pokeballSpt.transform.DOLocalMoveY(enemyUnit.transform.position.y - 2f, 0.3f).WaitForCompletion();

      //The animation when the enemy pokemon will try to free up
      //#of shakes will determine the Function TryToCatchPokemon() which returns number 0-4
      var numberOfShakes = TryToCatchPokemon(enemyUnit.Pokemon);
      for (int i = 0; i < Mathf.Min(numberOfShakes, 3); i++){  //.Min will return set number from the number which is above the set number. 
         yield return new WaitForSeconds(0.5f);
         yield return pokeballSpt.transform.DOPunchRotation(new Vector3(0, 0, 15f), 0.6f).WaitForCompletion();
      }

      //The Enemy pokemon is CAPTURED
      if (numberOfShakes == 4){
         yield return battleDialogBox.SetDialog($"¡{enemyUnit.Pokemon.Base.Name} capturado!");
         yield return pokeballSpt.DOFade(0, 1.5f).WaitForCompletion();

         //If the captured pokemon was added to our list of pokemons, or not.
         if (playerParty.AddPokemonToParty(enemyUnit.Pokemon)){
            yield return battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} se ha añadido a tu equipo.");
         }else{
            yield return battleDialogBox.SetDialog("En algun momento, lo mandaremos al PC de Bill...");
         }

         Destroy(pokeballInst);     //Destroy Pokeball
         BattleFinish(true);        //End battle
      }else{
      //The Enemy Pokemon ESCAPES
         yield return new WaitForSeconds(0.5f);
         pokeballSpt.DOFade(0, 0.2f);           //the pokeball will disappear
         yield return enemyUnit.PlayBreakOutAnimation();

         if (numberOfShakes <2){
            yield return battleDialogBox.SetDialog($"¡{enemyUnit.Pokemon.Base.Name} ha escapado!");
         }else{
            yield return battleDialogBox.SetDialog("¡Casi lo has atrapado!");
      }

      //DESTROY POKEBALL INSTANCE   
      Destroy(pokeballInst);
      state = BattleState.LoseTurn;
      }
      
   }

   //Returns the shakeCount
   int TryToCatchPokemon(Pokemon pokemon)
   {
      float bonusPokeball = 1;      //TODO: clase pokeball con su multiplicador
      float bonusStat = 1;          //TODO: stats para chequear condición de modificación
      float a = (3 * pokemon.MaxHP - 2 * pokemon.HP) * pokemon.Base.CatchRate * bonusPokeball * bonusStat/(3*pokemon.MaxHP);
      //the enemy pokemon will NOT escape
      if (a >= 255){
         return 4; 
      }
      //Enemy Pokemon WILL escape and the ShakeCount will return number of shakes
      float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680/a));
      int shakeCount = 0;              //reset the shakecount to 0, when 4 it will always capture!!
      while (shakeCount < 4){
         if (Random.Range(0, 65535) >= b){
            break;                  //will return shakeCount==4
         }else{
            shakeCount++;           
         }
      }
      return shakeCount;
   }

   //Returns Player Escaped from battle or not
   IEnumerator TryToEscapeFromBattle(){
      state = BattleState.Busy;

      //NOT WILD POKEMON != CANNOT ESCAPE
      if (type != BattleType.WildPokemon){
         yield return battleDialogBox.SetDialog("No puedes huir de combates contra entrenadores Pokemon");
         state = BattleState.LoseTurn;
         yield break;
      }

      //IS WILD POKEMON == CAN ESCAPE
      int playerSpeed = playerUnit.Pokemon.Speed;
      int enemySpeed = enemyUnit.Pokemon.Speed;
      escapeAttempts++; //Each escape attempts will decrease later the probability
      //Player Pokemon Speed DETERMINES THE SUCCESS
      if (playerSpeed >= enemySpeed){
         yield return battleDialogBox.SetDialog("Has escapado con éxito");
         yield return new WaitForSeconds(1);
         OnBattleFinish(true);
      }else{
         int oddsScape = (Mathf.FloorToInt(playerSpeed * 128 / enemySpeed) + 30 * escapeAttempts) % 256;
         if (Random.Range(0, 256) < oddsScape){
            yield return battleDialogBox.SetDialog("Has escapado con éxito");
            yield return new WaitForSeconds(1);
            OnBattleFinish(true);
         }else{
            yield return battleDialogBox.SetDialog("No puedes escapar");
            state = BattleState.LoseTurn;
         }
      }
   }


   IEnumerator HandlePokemonFainted(BattleUnit faintedUnit){
      yield return battleDialogBox.SetDialog($"{faintedUnit.Pokemon.Base.Name} se ha debilitado");
      
      //AUDIO | SFX
      SoundManager.SharedInstance.PlaySound(faintedClip);

      faintedUnit.PlayFaintAnimation();
      yield return new WaitForSeconds(1.5f);

      //PLAYER POKEMON WON
      if (!faintedUnit.IsPlayer){
         //EXP ++
         int expBase = faintedUnit.Pokemon.Base.ExpBase;                      //base experience by level 1
         int level = faintedUnit.Pokemon.Level;                               //level of the pokemon
         float multiplier = (type == BattleType.WildPokemon ? 1 : 1.5f);      //Wild pokemon will yield less experience
         int wonExp = Mathf.FloorToInt(expBase * level * multiplier / 7);     //FORMULA for total experience gained
         
         playerUnit.Pokemon.Experience += wonExp;
         yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha ganado {wonExp} puntos de experiencia");
         yield return playerUnit.Hud.SetExpSmooth();                          //invoked coroutine with the update to the EXP bar 
         yield return new WaitForSeconds(0.5f);
         
         //CHECK THE NEW LEVEL
         while (playerUnit.Pokemon.NeedsToLevelUp()){
               //AUDIO | SFX
               SoundManager.SharedInstance.PlaySound(levelUpClip);

               playerUnit.Hud.SetLevelText();                                          //Update to the level in HUD
               yield return playerUnit.Hud.UpdatePokemonData(playerUnit.Pokemon.HP);   //Update to the Pokemon data in HUD
               yield return new WaitForSeconds(1);
               yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} sube de nivel!");
            
         //INTENTAR APRENDER UN NUEVO MOVIMIENTO
            var newLearnableMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();      //get default or first learnable move on the level pokemon is
            if (newLearnableMove != null){                                                   //if the pokemon already has more
               if (playerUnit.Pokemon.Moves.Count < PokemonBase.NUMBER_OF_LEARNABLE_MOVES){  //but Pokemon can learn more moves
                  //Pokemon will learn new move
                  playerUnit.Pokemon.LearnMove(newLearnableMove);
                  yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha aprendido {newLearnableMove.Move.Name}");
                  battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
               }else{
                  //Pokemon will forget some of the old moves/attack
                  yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} intenta aprender {newLearnableMove.Move.Name}");
                  yield return battleDialogBox.SetDialog($"Pero no puedo aprener más de {PokemonBase.NUMBER_OF_LEARNABLE_MOVES} movimientos");
                  yield return ChooseMovementToForget(playerUnit.Pokemon, newLearnableMove.Move);     //pass who will learn, what new move
                  yield return new WaitUntil(() => state != BattleState.ForgetMovement);              //wait until the variable we set has/has not the value
               }
           }
            yield return playerUnit.Hud.SetExpSmooth(true);                            //update to the EXP Bar == reset it to 0 on levelUP
         }
      }
      CheckForBattleFinish(faintedUnit);
   }

   //Handles which movement/attack player wants to drop to learn new
   IEnumerator ChooseMovementToForget(Pokemon learner, MoveBase newMove){
      state = BattleState.Busy;

      yield return battleDialogBox.SetDialog("Selecciona el movimiento que quieres olvidar");
      selectMoveUI.gameObject.SetActive(true);
      //to SetMovement() we need to pass all movements, new movement to learn
      //this can be done by looping the pokemon moveBase which holds all the moves
      //Or we can use LAMBDA .Select, dont forget to import library
      selectMoveUI.SetMovements(learner.Moves.Select(mv => mv.Base).ToList(), newMove); 
      moveToLearn = newMove; //moveBase is object, we set the new move to the variable

      state = BattleState.ForgetMovement;
   }

}