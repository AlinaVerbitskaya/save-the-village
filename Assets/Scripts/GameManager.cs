using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject resButtonsHolder; //for hiding buttons when game is paused
    [SerializeField] private GameObject interfaceIconsHolder; //for hiding buttons when game is paused or over
    [SerializeField] private GameObject gameOverPanel, pausePanel;
    [SerializeField] private GameObject winText, loseText; 
    [SerializeField] private Text Statistics;
    [SerializeField] private TimerButton[] resButtons; //interactable buttons with timers displayed on them
    [SerializeField] private GameObject[] interfaceRes; //for displaying number of resources on screen
    
    public enum buttonType { food = 0, warriors = 1, farmers = 2, enemies = 3, foodExpense = 4 };
    public int[] resCount = { 0, 0, 0, 0 }; //corresponds with button types, number of resources of each type
    private int[] stats = {0, 0, 0, 0, 0 }; //corresponds with button types, statistics for each button type + number of cycles player survived 
    [SerializeField] private int costPerFarmer = 2, costPerWarrior = 8, foodPerWarrior = 5; //cost in food points to press a button
    [SerializeField] private float timePerHarvest = 5, timePerFarmer = 4, 
        timePerWarrior = 5, timePerCycle = 15, timePerEating = 6;  //time in seconds needed for timers to finish
    private int cycle = 0; //number of enemy attacks player survived
    [SerializeField] int cyclesToWin = 5; //number of enemy attacks needed for player to survive to win
    [SerializeField] private Sprite soundOn, soundOff; 
    [SerializeField] private Sprite pauseOn, pauseOff;  
    [SerializeField] private Image soundButtonImage, pauseButtonImage; //images that need changing sprites
    private bool gamePaused = false, soundEnabled = true;
    public Canvas myCanvas;


    // Start is called before the first frame update
    void Start()
    {
        TimerButtonsSetUp();
        ResCountToInterface();
    }

    // Update is called once per frame
    void Update()
    {
        ResCountChangeCheck();
        ResCountToInterface();
        CheckIfInteractable();
    }

    private void TimerButtonsSetUp() //assigning timer durations to buttons
    {
        foreach (TimerButton resButton in resButtons)
        {
            switch (resButton.resType)
            {
                case buttonType.food:
                    {
                        resButton.SetCycle(timePerHarvest);
                        break;
                    }
                case buttonType.warriors:
                    {
                        resButton.SetCycle(timePerWarrior);
                        break;
                    }
                case buttonType.farmers:
                    {
                        resButton.SetCycle(timePerFarmer);
                        break;
                    }
                case buttonType.enemies:
                    {
                        resButton.SetCycle(timePerCycle);
                        break;
                    }
                case buttonType.foodExpense:
                    {
                        resButton.SetCycle(timePerEating);
                        break;
                    }
            }
        }
    }

    private void ResCountChangeCheck() //check if any timers finished
    {
        foreach(TimerButton resButton in resButtons)
        {
            if (resButton.done) {
                resButton.done = false;
                switch (resButton.resType)
                {
                    case buttonType.enemies: //enemy attack
                        resCount[(int)buttonType.warriors] -= resCount[(int)buttonType.enemies];
                        if (resCount[(int)buttonType.warriors] >= 0) //check if player survives the attack
                        {
                            stats[(int)buttonType.enemies] += resCount[(int)buttonType.enemies]; //"enemies defeated" stat 
                        }
                        else
                        {
                            GameOver(false); //player loses when warriors < 0
                            break;
                        }
                        cycle++;
                        if (cycle >= cyclesToWin) GameOver(true); //player wins after a number of enemy attacks
                        resCount[(int)buttonType.enemies] = cycle / 2; //calculating number od enemies in next attack
                        resButton.TimerStart(1); //this timer is always on
                        break;

                    case buttonType.farmers: //got new farmers
                        resCount[(int)buttonType.farmers] += resButton.amountOfPeopleInTraining; 
                        stats[(int)buttonType.farmers] = resCount[(int)buttonType.farmers]; //"farmers total" stat 
                        resButton.ButtonReset();
                        break;

                    case buttonType.food: //harvested food
                        int foodHarvested = (resCount[(int)buttonType.farmers] > 0) ? resCount[(int)buttonType.farmers] * 2 : 1;
                        resCount[(int)buttonType.food] += foodHarvested;
                        stats[(int)buttonType.food] += foodHarvested; //"food harvested" stat 
                        resButton.TimerStart(1); //this timer is always on
                        break;

                    case buttonType.warriors: //got new warriors
                        resCount[(int)buttonType.warriors] += resButton.amountOfPeopleInTraining;
                        stats[(int)buttonType.warriors] += resButton.amountOfPeopleInTraining; // "warriors trained" stat
                        resButton.ButtonReset();
                        break;

                    case buttonType.foodExpense: //warriors eat food
                        int foodEaten = resCount[(int)buttonType.warriors] * foodPerWarrior;
                        if (resCount[(int)buttonType.food] < foodEaten) //if there is not enough food to feed warriors
                        {
                            int debt = foodEaten - resCount[(int)buttonType.food];
                            int discardedWarriors = (int)Math.Ceiling(debt / (double)foodPerWarrior); //how many warriors can't be fed
                            resCount[(int)buttonType.warriors] -= discardedWarriors;
                            resCount[(int)buttonType.farmers] += discardedWarriors;
                            foodEaten -= debt;
                            /* warriors who can't be fed turn into farmers */
                        }
                        resCount[(int)buttonType.food] -= foodEaten;
                        stats[(int)buttonType.foodExpense] += foodEaten; //"food eaten" stat
                        resButton.TimerStart(1); //this timer is always on
                        break;

                    default:
                        break;
                }
                resButton.timerEndAudio.Play();
            }
        }
        
    }


    private void ResCountToInterface() //show number of resources on screen
    {
        for (int i = 0; i < interfaceRes.Length; i++)
        {
            interfaceRes[i].transform.Find("Count").gameObject.GetComponent<Text>().text = resCount[i].ToString("000");
        }
    }

    public bool HaveEnoughFood(int cost)  
    {
        return cost <= resCount[(int)buttonType.food];
    }

    public void PayForNewPerson(TimerButton button)
    {
        switch (button.resType)
        {
            case buttonType.farmers:
                resCount[(int)buttonType.food] -= costPerFarmer * button.amountOfPeopleInTraining;
                break;
            case buttonType.warriors:
                resCount[(int)buttonType.food] -= costPerWarrior * button.amountOfPeopleInTraining;
                break;

        }
    }


    private void CheckIfInteractable()
    {
        foreach (TimerButton resButton in resButtons)
        {
            switch (resButton.resType)
            {
                case buttonType.farmers:
                    resButton.GetComponent<Button>().interactable = HaveEnoughFood(costPerFarmer * resButton.amountOfPeopleInTraining) &&
                        !resButtons[(int)buttonType.farmers].timerOn;
                    break;
                case buttonType.warriors:
                    resButton.GetComponent<Button>().interactable = HaveEnoughFood(costPerWarrior * resButton.amountOfPeopleInTraining) &&
                        !resButtons[(int)buttonType.warriors].timerOn;
                    break;
            }
        }
    }

    public void GameOver(bool win)
    {
        Time.timeScale = 0;
        resButtonsHolder.SetActive(false);
        resButtonsHolder.GetComponent<Canvas>.enabled = false;
        interfaceIconsHolder.SetActive(false); //hide all interactable buttons
        loseText.SetActive(!win);
        winText.SetActive(win); //choose a headline
        Statistics.text = $"ATTACKS SURVIVED: " + cycle.ToString("000") + "\n" +
                          $"ENEMIES DEFEATED: " + stats[(int)buttonType.enemies].ToString("000") + "\n" +
                          $"FOOD HARVESTED: " + stats[(int)buttonType.food].ToString("000") + "\n" +
                          $"FARMERS TOTAL: " + stats[(int)buttonType.farmers].ToString("000") + "\n" +
                          $"WARRIORS TRAINED: " + stats[(int)buttonType.warriors].ToString("000") + "\n" +
                          $"FOOD EATEN: " + stats[(int)buttonType.foodExpense].ToString("000");
        gameOverPanel.SetActive(true);
    }

    public void SoundToggle()
    {
        soundEnabled = !soundEnabled;
        soundButtonImage.sprite = soundEnabled ? soundOn : soundOff;
        AudioListener.volume = soundEnabled ? 1f : 0f;
    }

    public void PauseToggle()
    {
        gamePaused = !gamePaused;
        pauseButtonImage.sprite = gamePaused ? pauseOn : pauseOff;
        Time.timeScale = gamePaused ? 0 : 1;
        resButtonsHolder.SetActive(!gamePaused);
        pausePanel.SetActive(gamePaused);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
