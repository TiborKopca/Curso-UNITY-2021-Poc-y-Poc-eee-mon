using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHUD : MonoBehaviour
{
    public Text pokemonName;
    public Text pokemonLevel;
    public HealthBar healthBar;
    public Text pokemonHealth;
    public GameObject expBar;       
    private Pokemon _pokemon;


    //When Battle starts, this refresh HUD with Pokemon Data
    public void SetPokemonData(Pokemon pokemon){
        _pokemon = pokemon;
        
        pokemonName.text = pokemon.Base.Name;
        SetLevelText();                             //updates the HUD with current data == current level

        //Si con el Update se ve mal, actualizar vida aqui al inicio de batalla.
        //This next function will show CHANGING data.
        healthBar.SetHP((float)_pokemon.HP / _pokemon.MaxHP);
        //pokemonHealth.text = $"{pokemon.HP} / {pokemon.MaxHP}";       //OBSOLETE by UpdatePokemonData
        
        SetExp();                               //call to set Experience bar
        StartCoroutine(UpdatePokemonData(pokemon.HP));
    }

    //Update to Health Bar 
    public IEnumerator UpdatePokemonData(int oldHPVal){   
        //Thanks to this coroutine Health Bar will change SMOOTHLY
        StartCoroutine(healthBar.SetSmoothHP((float)_pokemon.HP/_pokemon.MaxHP));
        StartCoroutine(DecreaseHealthPoints(oldHPVal));
        yield return null;
    }

    //Called by UpdatePokemonData, actualize the HP text
    private IEnumerator DecreaseHealthPoints(int oldHPVal){
        while (oldHPVal > _pokemon.HP){
            oldHPVal--;
            pokemonHealth.text = $"{oldHPVal}/{_pokemon.MaxHP}";
            yield return new WaitForSeconds(0.1f);
        }
        pokemonHealth.text = $"{_pokemon.HP}/{_pokemon.MaxHP}";
    }


    //Get Experience bar data from EXPERIENCE BAR
    //Called by SetPokemonData()
    public void SetExp(){
        if (expBar == null){
            return;
        }
        expBar.transform.localScale = new Vector3(NormalizedExp(), 1, 1);   //localScale would obtain value 0-1 from normalized EXP
    }

    //Smooth Fill of the Exp Bar with DOTween
    //Called from BattleManager HandlePokemonFainted()
    public IEnumerator SetExpSmooth(bool needsToResetBar = false){          //default = false, but when passed by BattleManager - will override with true
        if (expBar==null){
            yield break;
        }

        //BattleManager code will command that upon leveling up, we reset the EXP bar
        if (needsToResetBar){
            expBar.transform.localScale = new Vector3(0, 1, 1);
        }
        
        yield return expBar.transform.DOScaleX(NormalizedExp(), 2f).WaitForCompletion(); //SCALE X, DOScale is always local
    }


    //Returns float number between 0-1 to modify/show the status of the EXP bar
    float NormalizedExp(){
        float currentLevelExp = _pokemon.Base.GetNecessaryExpForLevel(_pokemon.Level);
        float nextLevelExp = _pokemon.Base.GetNecessaryExpForLevel(_pokemon.Level+1);
        float normalizedExp = (_pokemon.Experience - currentLevelExp) / (nextLevelExp - currentLevelExp);
        
        Debug.Log($"current {currentLevelExp}; next {nextLevelExp}, norml {normalizedExp}");
        //Mathf.Clamp01 returns the number like round == near 0 will be 0, near 1 will be 1
        return Mathf.Clamp01(normalizedExp);    
    }


    //Update Level of the HUD, called by BattleManager
    public void SetLevelText(){
        pokemonLevel.text = $"Lv {_pokemon.Level}";
    }
    
}