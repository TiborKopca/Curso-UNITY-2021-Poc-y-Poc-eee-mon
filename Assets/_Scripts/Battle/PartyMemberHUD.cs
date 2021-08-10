using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberHUD : MonoBehaviour
{
    public Text nameText, levelText, typeText, healthPoints; //variables for texts
    public HealthBar healthBar;
    public Image pokemonImage;

    private Pokemon _pokemon;                          //From here we will fill the rest
    [SerializeField] private Color selectedColor = Color.blue;


    public void SetPokemonData(Pokemon pokemon){
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = $"Lv {pokemon.Level}";
      
        //POKEMON HAS NO TYPE2
        if (pokemon.Base.Type2 == PokemonType.None){
          typeText.text = pokemon.Base.Type1.ToString().ToUpper(); 
        }else{
          typeText.text = $"{pokemon.Base.Type1.ToString().ToUpper()} - {pokemon.Base.Type2.ToString().ToUpper()}";
        }

        healthPoints.text = $"{pokemon.HP} / {pokemon.MaxHP}";
        healthBar.SetHP((float)pokemon.HP/pokemon.MaxHP);
        pokemonImage.sprite = pokemon.Base.FrontSprite;

        //CARD COLOR BY TYPE OF THE POKEMON
        GetComponent<Image>().color = TypeColor.GetColorFromType(pokemon.Base.Type1);
    }

    //Marks the selected Cell by color, Called by PartyHUD class
    public void SetSelectedPokemon(bool selected){
      if (selected){
         nameText.color = selectedColor;
      }else{
         nameText.color = Color.black;
      }
    }
}
