using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random; //Random of the Unity Engine
// using UnityEngine.Random;

[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    private bool isMoving;  //holds value if the character is moving
    public float speed;     //we can modify its speed
    private Vector2 input;  //Vector of 2 dimensions
    private Animator _animator;
    public LayerMask solidObjectsLayer, pokemonLayer;

    public event Action OnPokemonEncountered;   //this will "de-chain" the action-on this event the PlayerControler retransmit the event

    [SerializeField] float verticalOffset = 0.2f; //we substract the Y position from its altered current position


    void Awake()
    {
        _animator = GetComponent<Animator>();
    }




    //Previously called Update, but now we have GameManager, which controls/handles updates
    //changed from private to public
    public void HandleUpdate()
    {
        //If the player is not moving => ismoving == true,this negated ==> if it is NOT moving
        //If the player is moving, it is not possible to move to other direction
        if (!isMoving){
            
            input.x = Input.GetAxisRaw("Horizontal"); //with GetAxis we obtain -1, 1 with decimals
            input.y = Input.GetAxisRaw("Vertical");   //with GetAxisRaw we obtain only -1 OR 1, nothing in between
            //input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); //this is the same as lines above

            //Disable the diagonal movement - we set the 1 vector to be always 0
            if (input.x != 0)
            {
                input.y = 0;
            }
            
            //if input is not 0, there is movement and the code wont continue in this frame
            if (input != Vector2.zero)
            {
                _animator.SetFloat("Move X", input.x);
                _animator.SetFloat("Move Y", input.y);
                
                //targetPosition is in 3D
                var targetPosition = transform.position;
                //ADD the input to the side the player wants to go to the Current position
                targetPosition.x += input.x;    
                targetPosition.y += input.y;

                //COUROUTINE calling with the Collision checking for the solid objects
                if (IsAvailable(targetPosition))
                {
                    StartCoroutine(MoveTowards(targetPosition));
                }
            }
        }
    }


    private void LateUpdate()
    {
        //Update will run the movement => the coroutine will set the isMoving
        //LateUpdate() after Update() will transmit this lastly to the animator
        _animator.SetBool("Is Moving", isMoving);
    }

    //COROUTINE
    IEnumerator MoveTowards(Vector3 destination)
    {
        //we set that player is moving, so ANOTHER input for movement is blocked until this runs
        isMoving = true;
        
        //while distance between now and the destination is greater than basically 0
        //destination = targetposition + input from the player
        while (Vector3.Distance(transform.position, destination) > Mathf.Epsilon)
        {
            //we move only the distance that is set by the speed multiplied by the time of the computer
            //this will give us precise movement by the grid
            transform.position = Vector3.MoveTowards(transform.position,
                destination, speed * Time.deltaTime);
            //this will make the code do nothing more this frame, until the next frame
            yield return null;
        }
        //Reset of the current position 
        transform.position = destination;
        //unblock of the isMoving flag, so the next movement is possible
        isMoving = false;

        //At the end we check for PokemonBattle
        CheckForPokemon();
    }

/// <summary>
/// El método comprueba que la zona a la que queremos acceder, esté disponible
/// </summary>
/// <param name="target">Zona a la que queremos acceder</param>
/// <returns>Devuelve true, si el target está disponible y false  en caso contrario</returns>
    private bool IsAvailable(Vector3 targetPosition)
    {   //if there is collision with objects that have 'solidObjectLayer'
        //target = the tile where the player wants the go
        //number = small circle, not the circle that fill whole tile
        if (Physics2D.OverlapCircle(targetPosition, 0.2f, solidObjectsLayer)!=null)
        {
            return false;
        }
        //if there is no collision with Physics Collider 
        return true;
    }



    //This will check if the player will collide with pokemonLayer
    private void CheckForPokemon()
    {  
        if (Physics2D.OverlapCircle(transform.position - new Vector3(0,verticalOffset), 0.2f, pokemonLayer)!=null)
        {
            //Random means the encounter will happen randomly, adjust numbers for 'Randomity', this has 15% of encounter
            //Random of the Unity Engine, need to import on the top!
            if (Random.Range(0, 100) < 15)
            {
                //This will contain Coroutine or function which calls the battle
                //Debug.Log("Empezar Batalla Pokemon");
                //TODO: Si no se para, forzar animación IsMoving = false;
                OnPokemonEncountered();             //EVENT, which will be retransmittered to the GameManager
            }
        }
    }

}
