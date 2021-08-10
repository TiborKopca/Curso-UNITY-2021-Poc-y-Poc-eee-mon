using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;  //advanced movement of the objects on the screen via 3rd party software

[RequireComponent(typeof(Image))]  //image of the UNITY UI
public class BattleUnit : MonoBehaviour
{
  public PokemonBase _base;
  public int _level;

  //to distinguish if pokemon is player or enemy
  [SerializeField] bool isPlayer;
  public bool IsPlayer => isPlayer;       //{get => isPlayer;}

  //Replaced the Player/Enemy Huds
  [SerializeField] BattleHUD hud;
  public BattleHUD Hud => hud;

  public Pokemon Pokemon { get; set; } //this means that we can use pokemon class but we wont alter anything

  private Image pokemonImage;
  private Vector3 initialPosition;    //from where the pokemon will move, with DOTween
  private Color initialColor;
  
  [SerializeField] private float startTimeAnim = 1.0f,  //DOTween data for animations
                                attackTimeAnim = 0.3f, 
                                hitTimeAnim = 0.15f, 
                                dieTimeAnim = 1f, 
                                capturedTimeAnim = 0.6f;




  //get image of pokemon and set it to the screen
  private void Awake()
  {
    pokemonImage = GetComponent<Image>();   //caching of the Pokemon Images/Sprites

    initialPosition = pokemonImage.transform.localPosition; //localPosition = relative position respetive to parent
    initialColor = pokemonImage.color;            //if we were in 3D, this could be material
  }



  //New pokemon, passing into it the parameters from ?
  public void SetupPokemon(Pokemon pokemon){
    Pokemon = pokemon;
    // Pokemon = new Pokemon(_base, _level); //new object => each time we take memory space, OBSOLETE
    
    //This select the sprite of the pokemon to be Front OR Back, depending on variable isPlayer
    pokemonImage.sprite = (isPlayer ? Pokemon.Base.BackSprite : Pokemon.Base.FrontSprite);

    pokemonImage.color = initialColor;

    //Set the data of player and enemy to the HUDs
    hud.SetPokemonData(pokemon);
    //Reset the size of the pokemon, before after capture stood small
    transform.localScale = new Vector3(1,1,1);
    
    PlayStartAnimation();
  }


  //Animation starts ==> both pokemons will come to the screen 
  public void PlayStartAnimation()
  {
    //this will move the player pokemon from position x + (-1)*400 
    pokemonImage.transform.localPosition = new Vector3(initialPosition.x+(isPlayer ? -1 : 1)*400, initialPosition.y);
    //DOTween Animator object translation
    pokemonImage.transform.DOLocalMoveX(initialPosition.x, startTimeAnim);
  }

  //ATTACK animation ==> pokemon moves a bit towards the other
  public void PlayAttackAnimation()
  {
    var seq = DOTween.Sequence();
    seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x + (isPlayer ? 1 : -1) * 60, attackTimeAnim));
    seq.Append(pokemonImage.transform.DOLocalMoveX(initialPosition.x, attackTimeAnim));
  }

  //The pokemon will Flash
  public void PlayReceiveAttackAnimation()
  {
    var seq = DOTween.Sequence();
    seq.Append(pokemonImage.DOColor(Color.grey, hitTimeAnim));
    seq.Append(pokemonImage.DOColor(initialColor, hitTimeAnim));
  }

  //Dead animation => Pokemon will Fade and move out of screen
  public void PlayFaintAnimation()
  {
    var seq = DOTween.Sequence();
    seq.Append(pokemonImage.transform.DOLocalMoveY(initialPosition.y - 200, dieTimeAnim));
    seq.Join(pokemonImage.DOFade(0f, dieTimeAnim)); 
  }
  
  //Pokeball Capture Animation, called by BattleManager
  public IEnumerator PlayCapturedAnimation()
  {
    var seq = DOTween.Sequence();
    seq.Append(pokemonImage.DOFade(0, capturedTimeAnim));
    seq.Join(transform.DOScale(new Vector3(0.25f, 0.25f, 1f), capturedTimeAnim));
    seq.Join(transform.DOLocalMoveY(initialPosition.y + 50f, capturedTimeAnim));
    yield return seq.WaitForCompletion();
  }
  
  public IEnumerator PlayBreakOutAnimation()
  {
    var seq = DOTween.Sequence();
    seq.Append(pokemonImage.DOFade(1, capturedTimeAnim));
    seq.Join(transform.DOScale(new Vector3(1f, 1f, 1f), capturedTimeAnim));
    seq.Join(transform.DOLocalMoveY(initialPosition.y, capturedTimeAnim));
    yield return seq.WaitForCompletion();
  }
  
}
