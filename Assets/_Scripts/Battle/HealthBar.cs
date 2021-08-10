using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class HealthBar : MonoBehaviour{
    public GameObject healthBar;

    //Bar Color will be depending on the size or scale of the bar
    public Color BarColor(float finalScale){                                 
            //var localScale = healthBar.transform.localScale.x;
        if (finalScale < 0.15f){
                return new Color(193f/255, 45f/255, 45f/255);   //Color.red
        }else if (finalScale < 0.5f){
                return new Color(211f/255, 211f/255, 29f/255);  //Color.yellow
        }else{
                return new Color(98f/255, 178f/255, 61f/255);   //Color.green
        }
    }

    //OBSOLETE by making BarColor property
    // private void Start() {
    //     healthBar.transform.localScale = new Vector3(0.5f, 1.0f); //fist value x = 50% to test functionality
    //     _color = healthBar.GetComponent<Image>().color;
    // }
    


    /// <summary>
    /// Actualiza la barra de vida a partir del valor normalizado de la misma
    /// </summary>
    /// <param name="normalizedValue">Valor de la vida normalizado entre 0 y 1</param>
    //ASSIGNS the value for the HealthBar for the first time when game/match starts
    public void SetHP(float normalizedValue){

        healthBar.transform.localScale = new Vector3(normalizedValue, 1.0f);
        healthBar.GetComponent<Image>().color = BarColor(normalizedValue);

    }


    //Called from BattleHUD
    //Smoothly decrease the Health bar
    /*public IEnumerator SetSmoothHP(float normalizedValue){                   //OBSOLETE BY COROUTINE BELOW
        float currentScale = healthBar.transform.localScale.x;
        float updateQuantity = currentScale - normalizedValue;
        while (currentScale - normalizedValue > Mathf.Epsilon)
        {
            currentScale -= updateQuantity * Time.deltaTime;
            healthBar.transform.localScale = new Vector3(currentScale, 1);
            healthBar.GetComponent<Image>().color = BarColor(normalizedValue);      
            yield return null;
        }
        healthBar.transform.localScale = new Vector3(normalizedValue, 1);
    }*/

    //THE SAME Result as SetSmoothHPOBSOLETE(), here we use DOTween library 
    //Called from BattleHUD
    //Smoothly decrease the Health bar, the color is calculated on the start
    public IEnumerator SetSmoothHP(float normalizedValue){
        //yield return healthBar.transform.DOScaleX(normalizedValue, 1f);
        //healthBar.GetComponent<Image>().color = BarColor(normalizedValue);        //These 2 lines we can add to the DOTween sequence below

        var seq = DOTween.Sequence();
        seq.Append(healthBar.transform.DOScaleX(normalizedValue, 1f));
        seq.Join(healthBar.GetComponent<Image>().DOColor(BarColor(normalizedValue), 1f));    //the same normalized value as function above
        yield return seq.WaitForCompletion();                                                
    }
}
