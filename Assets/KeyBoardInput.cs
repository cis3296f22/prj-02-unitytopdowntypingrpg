using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class KeyBoardInput : MonoBehaviour
{
    public TMP_Text text;
    public TMP_Text TimerText;
    public float timeRemaining = 10;
    public HealthManager enemyHealthManager;
    public HealthManager playerHealthManager;
    public ButtonUI buttonUI;
    public ParticleSystem particalSystem;
    public float currentDamage;
    public TMP_Text PlayerMessText;
    public GameObject PlayerMessageObject;
    
    public TMP_Text EnemyMessText;
    public GameObject EnemyMessageObject;
    
    public PlayerController playerController;
    public Enemy enemy;
    private StringBuilder playerInput = new StringBuilder("");
	private string sentence = "";
	private bool playing = false;
	private int[] compare;
	private string CORRECT_COLOR_OPEN_TAG = "<color=#207F20>";
	private string INCORRECT_COLOR_OPEN_TAG = "<color=#de0e3a>";
	private string NOT_TYPING_COLOR_TAG = "<color=#F0E1C5>";
	private string COLOR_END_TAG = "</color>";
	private string COLOR_TIMER_TAG = "<color=#442A14>";
	
	private float totalChar;
	private float correctChar;
	private float accuracy;
	private int EnemyId;

	private int size;
    // Start is called before the first frame update
    void Start()
    {
	    Debug.Log("Inputplayer Start");
		PlayerMessageObject.SetActive(false);
		EnemyMessageObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
		if (playing == true) {
			if (Input.GetKeyDown(KeyCode.Backspace)) {
				// only remove when the player input something
				if (playerInput.Length > 0)
				{
					playerInput.Length --;
				}
				
			}
        	else if (Input.anyKeyDown) {
            	playerInput.Append(Input.inputString);
            	Debug.Log(playerInput.ToString());
            
			}	
			CompareInput();
			ShowText();
			ShowTime();
			
		}
		if (timeRemaining <= 0)
		{
			playing = false;
			Debug.Log("Correct: " + accuracy);
			if (enemyHealthManager.healthAmount > 0 && playerHealthManager.healthAmount > 0)
			{
				// reset all the status of the game
				StartCoroutine(FightTurn());
				Reset();
				StartCoroutine(ResetButton());
				
			}
			
		}
    }

	public void Begin() {
		if (!playing) {
			sentence = WordGenerator.GenerateSentence();
			playing = true;
			size = sentence.Length;
		}
	}
	// control both turn. 
	IEnumerator FightTurn()
	{
		if (timeRemaining <= 0)
		{
			float fc = (float)Math.Round(currentDamage * 100f) / 100f;
			Debug.Log("Wait Turn");
			enemyHealthManager.TakeDamage(currentDamage);
			float checkEnemyHealth = enemyHealthManager.healthAmount;
			EnemyMessageObject.SetActive(true);
			EnemyMessText.SetText("Enemy got " + fc + " Damages");
			yield return new WaitForSeconds(1f);
			EnemyMessageObject.SetActive(false);
			
			if (checkEnemyHealth <= 0)
			{
				//If win, save player data and return back last position with data. 
				Debug.Log("Player win");
				PlayerPrefs.SetInt("IsWin", 1);
				PlayerMessageObject.SetActive(true);
				PlayerMessText.SetText("Player Win");
				yield return new WaitForSeconds(1f);
				PlayerMessageObject.SetActive(false);
				SwitchScene();
			}
			else
			{
				playerHealthManager.TakeDamage(5);
				float checkPlayerHealth = playerHealthManager.healthAmount;
				yield return new WaitForSeconds(1f);
				if (checkPlayerHealth <= 0)
				{
					//If lose, player dead, reset all player data and return back to Starting point
					Debug.Log("Enemy win");
					PlayerPrefs.SetInt("IsWin", 0);
					PlayerPrefs.DeleteKey("p_x");
					PlayerPrefs.DeleteKey("p_y");
					PlayerPrefs.DeleteKey("TimeToLoad");
					PlayerMessageObject.SetActive(true);
					PlayerMessText.SetText("Player Lose");
					yield return new WaitForSeconds(0.5f);
					PlayerMessageObject.SetActive(false);
					SwitchScene();
				}
				else
				{
					PlayerMessageObject.SetActive(true);
					PlayerMessText.SetText("Player got " + 5 + " Damages");
					yield return new WaitForSeconds(1f);
					PlayerMessageObject.SetActive(false);
				}
			}
			
			

			
		}
	}
	// Player attack turn
	private void PlayerTurn()
	{
		Debug.Log("Player Turn");
		enemyHealthManager.TakeDamage(currentDamage);
		
	}
	//Enemy attack turn.
	private void EmenyTurn()
	{
		Debug.Log("Enemy Turn");
		playerHealthManager.TakeDamage(5);
		
	}

	public void Reset()
	{
		sentence = "";
		playerInput = new StringBuilder("");
		compare = new int[0];
		ShowText();
		timeRemaining = 10;
	}

	// Slow show button event.
	IEnumerator ResetButton()
	{
		yield return new WaitForSeconds(5f);
		buttonUI.show();
	}

	private void CompareInput() {
		int min = Math.Min(sentence.Length, playerInput.Length);
		int max = Math.Max(sentence.Length, playerInput.Length);
		compare = new int[max];
		totalChar = max;
		correctChar = 0;
		for (int i = 0; i < min;i++)
		{
			if (sentence[i] == playerInput[i]) {
				compare[i] = 1;
				correctChar++;
			}
			else {
				compare[i] = -1;
			}
		}
		// calculate accuracy based on character
		accuracy = correctChar / totalChar;
		currentDamage = accuracy * 120;
	}
	

	private void ShowText() {
		StringBuilder sb = new StringBuilder();
		for (int i = 0; i < compare.Length; i++) {
			if (compare[i] == 1) {
				sb.Append(CORRECT_COLOR_OPEN_TAG);
				sb.Append(sentence[i]);
				sb.Append(COLOR_END_TAG);
			}
			else if (compare[i] == -1) {
				sb.Append(INCORRECT_COLOR_OPEN_TAG);
				sb.Append(sentence[i]);
				sb.Append(COLOR_END_TAG);
			}
			else if (compare[i] == 0) {
				sb.Append(NOT_TYPING_COLOR_TAG);
				sb.Append(sentence[i]);
				sb.Append(COLOR_END_TAG);
			}
		}
		text.text = sb.ToString();
	}

	public void ShowTime()
	{
		StringBuilder sb = new StringBuilder();
		if (playing && timeRemaining >= 0)
		{
			timeRemaining -= Time.deltaTime;
			float seconds = Mathf.FloorToInt(timeRemaining % 60);
			sb.Append(COLOR_TIMER_TAG);	
			sb.Append(seconds);
			sb.Append(COLOR_END_TAG);
			TimerText.text = sb.ToString();
			// if (timeRemaining == 0)
			// {
			// 	playing = false;
			// }
		}
		if (timeRemaining <= 0)
		{
			sb = new StringBuilder();
			sb.Append(COLOR_TIMER_TAG);	
			sb.Append("0");
			sb.Append(COLOR_END_TAG);
			TimerText.text = sb.ToString();
		}

	}
	
	public void SwitchScene()
	{
		StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex -1));
        
	}

	IEnumerator LoadScene(int SceneIndex)
	{
		//wait
		yield return new WaitForSeconds(1f);
		//load scene
		SceneManager.LoadScene(SceneIndex);
	}
}
