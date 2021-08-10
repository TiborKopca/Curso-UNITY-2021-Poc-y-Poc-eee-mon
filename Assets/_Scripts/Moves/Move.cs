using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move
{
    private MoveBase _base;         //just like in Pokemon, which has PokemonBase
    public MoveBase Base{
        get => _base;
        set => _base = value;
    }

    private int _pp;
    public int Pp{
        get => _pp;
        set => _pp = value;       //this is for the modification of the powerpoints
    }

    //CONSTRUCTOR
    public Move(MoveBase mBase)
    {
        Base = mBase;
        Pp = mBase.PP;      //this value comes from PowerPoints of the MoveBase!
    }
}
