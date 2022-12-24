using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerButton : MonoBehaviour
{
    [SerializeField] private GameObject timerImage; //image on the button that represents time left
    private float timerCycle = 3; //the amount of time, in seconds
    private float timerMultiplier = 1; //  1 for normal buttons, different for "xN" buttons
    public int amountOfPeopleInTraining = 1; // how many people are trainung at once
    public bool timerOn = false; //if timer is going
    public bool done = false; //when timer runs out
    public GameManager.buttonType resType; //type of resource that the button represents
    public AudioSource timerEndAudio; //audio that plays at the end of the timer


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (timerOn) TimerTick();
    }


    public void SetCycle(float time)
    {
        timerCycle = time;
    }


    public void TimerStart(int multiplier) //multiplier is how many people are created at once
    {
        amountOfPeopleInTraining = multiplier;
        timerMultiplier = (multiplier <= 1) ? 1 : ((float)2) / multiplier; //timer is slower when creating multiple people at once
        timerOn = true;
        timerImage.GetComponent<Image>().fillAmount = 1;
        gameObject.GetComponent<Button>().interactable = false;
    }

    private void TimerFinish() //when timer runs out
    {
        timerOn = false;
        done = true;
    }

    public void ButtonReset() 
    {
        timerMultiplier = 1; 
        amountOfPeopleInTraining = 1;
    }

    private void TimerTick()
    {
        timerImage.GetComponent<Image>().fillAmount -= Time.deltaTime/timerCycle * timerMultiplier;
        if (timerImage.GetComponent<Image>().fillAmount <= 0) { 
            TimerFinish();
        };
        
    }

}
