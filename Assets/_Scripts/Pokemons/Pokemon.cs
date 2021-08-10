using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;               //This needs to be present if is used [Serializable]
using Random = UnityEngine.Random;
using System.Linq;

[Serializable]                      //this will make the class Pokemon visible in Inspector when used in other classes (f.e. PokemonParty which is used by Player)
public class Pokemon{
    [SerializeField] private PokemonBase _base;
    public PokemonBase Base{
        get => _base;
    }

    [SerializeField] private int _level;
    public int Level{
        get => _level;
        set => _level = value;
    }

    private List<Move> _moves;  //This will contain the actual attack moves that have pokemon
    public List<Move> Moves{    //for the list to be able to be inicialized, we need put it to constructor
        get => _moves;  
        set => _moves = value;
    }

    //Vida actual del Pokemon
    private int _hp;
    public int HP{
        get => _hp;
        set{ 
            _hp = value;
            _hp = Mathf.FloorToInt(Mathf.Clamp(_hp, 0, MaxHP));         //this will round the value to the close to the bigger value
        }
    }

    //Experience number
    private int _experience;
    public int Experience{
        get => _experience;
        set => _experience = value;
    }





//CONSTRUCTOR
    //this we will use to make a copy of a pokemon
    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        _level = pLevel;
        InitPokemon();
    }

    //Initialize the Pokemon //this was a constructor before
    public void InitPokemon(){
        _hp = MaxHP;                    //hp will be maximum, according to the formula below Constructor
        _moves = new List<Move>();      //the list of moves will be from list PokemonBase.LearnableMoves
        _experience = Base.GetNecessaryExpForLevel(_level);     //obtains the experience we have right now

        //this will asign the possible learnable attacks from the Class PokemonBase
        //and in PokemonBase have parameter of class LearnableMoves
        //so if there are 3 learnable moves in PokemonBase, we need to loop through them
        foreach (var learnable in _base.LearnableMoves)
        {
            if (learnable.Level <= _level){       //the learnable move must be less or equal to the level of the pokemon
                //Instantiating == new Move, dont forget if there are 3, this will set 3 Moves
                _moves.Add(new Move(learnable.Move)); //learnable == LearnableMoves, which have parameter named Move (of class MoveBase)
            }
            //There wont be more than 3 learnable moves
            if (_moves.Count >= 4)
            {
                break;
            }
        }
    }

    //Formula for the numbers, calculated by Game Designer
    public int MaxHP => Mathf.FloorToInt((_base.MaxHP * _level)/20.0f)+10;   //we add more points, so they wont be so weak in lower levels
    public int Attack => Mathf.FloorToInt((_base.Attack * _level)/100.0f)+2; //base attack by level
    public int Defense => Mathf.FloorToInt((_base.Defense * _level)/100.0f)+2;
    public int SpAttack => Mathf.FloorToInt((_base.SpAttack * _level)/100.0f)+2;
    public int SpDefense => Mathf.FloorToInt((_base.SpDefense * _level)/100.0f)+2;
    public int Speed => Mathf.FloorToInt((_base.Speed * _level)/100.0f)+2;


    //Attack method called by BattleManager
    public DamageDescription ReceiveDamage(Pokemon attacker, Move move)
    {
        //CRITICAL ATTACK PROBABILITY
        float critical = 1f;
        if (Random.Range(0f, 100f) < 8f) //8% probability of Critical
        {
            critical = 2f;              //if critical attack is happening, the value of damage is multiplied by this number == 2
        }

        //Type of Attack Effectiveness multiplier is from PokemonBase -> TypeMatrix class
        //GetMultEffectiveness(PokemonType attackType, PokemonType pokemonDefenderType)
        float type1 = TypeMatrix.GetMultEffectiveness(move.Base.Type, this.Base.Type1);
        float type2 = TypeMatrix.GetMultEffectiveness(move.Base.Type, this.Base.Type2);

        //Here we will store damage info to be shown later for player.
        //No need for constructor here, invoking directly the object, INSIDE we can 
        //directly TAKE existing variables and WRITE it to the class
        var damageDesc = new DamageDescription()
        {
            Critical = critical,
            Type = type1*type2,
            Fainted = false
        };

        //SPECIAL ATTACK/DEFENSE MODIFIERS
        float attack = (move.Base.IsSpecialMove ? attacker.SpAttack : attacker.Attack);
        float defense = (move.Base.IsSpecialMove ? this.SpDefense : this.Defense);

        //RNG * Matrix Effectivity Type
        float modifiers = Random.Range(0.85f, 1.0f) * type1 * type2 * critical;
        float baseDamage = ((2 * attacker.Level / 5f + 2) * move.Base.Power * ((float) attack/defense)) / 50f + 2; //modified by Special attack/defense
        int totalDamage = Mathf.FloorToInt(baseDamage * modifiers);

        HP -= totalDamage; //substract the damage of the attack from HP
        if (HP <= 0){
            HP = 0;
            //IS DEAD
            //return true; //OBSOLETE
            damageDesc.Fainted = true;    
        }
        //ISNT DEAD
        //return false; //OBSOLETE   
        return damageDesc;
    }
    

    //Returns random move/attack from the list of moves
    public Move RandomMove(){
        //the move of the enemy will be possible only if has the Power Points more than 0
        var movesWithPP = Moves.Where(m => m.Pp > 0).ToList();
        if (movesWithPP.Count > 0){
            //int randId = Random.Range(0, Moves.Count); //OBSOLETE
            //return Moves[randId];                      //OBSOLETE   
            int randId = Random.Range(0, movesWithPP.Count);
            return movesWithPP[randId];
        }
        //NO HAY PPs en ningún ataque
        //TODO: implementar combate, que hace daño al enemigo y a ti mismo
        return null;  
    }

    //Returns true/false on experience BASED ON BASE NECESSARY for its level
    public bool NeedsToLevelUp(){
        if (Experience > Base.GetNecessaryExpForLevel(_level+1)){
            int currentMaxHP = MaxHP;               //the HP will be filled?
            _level++;
            HP += (MaxHP - currentMaxHP);
            return true;
        }else{
            return false;
        }
    }

    //Lambda return first OR default.Learnable move from the list of moves/attacks on the level the pokemon is
    public LearnableMove GetLearnableMoveAtCurrentLevel(){
        return Base.LearnableMoves.Where(lm => lm.Level == _level).FirstOrDefault();
    }

    //Add move passed as atribute to the list of Moves/Attack
    public void LearnMove(LearnableMove learnableMove){
        if (Moves.Count >= PokemonBase.NUMBER_OF_LEARNABLE_MOVES){  //Pokemon already has max moves/attacks
            return;
        }
        Moves.Add(new Move(learnableMove.Move));
    }


}

//DAMAGE WILL BE USED BY GIVE THE DETAILS OF THE ATTACK, WHAT IS HAPPENING == INFO TO PLAYER
public class DamageDescription
{
    public float Critical { get; set; }
    public float Type { get; set; } 
    public bool Fainted { get; set; }
}