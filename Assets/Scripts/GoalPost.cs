using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPost : MonoBehaviour
{
    [SerializeField]
    private ScoringManager scoringManager;

    [SerializeField]
    private int playerGoal;

    [SerializeField]
    private GameObject ball;

    public PlayerMovement[] playerMovements;

    private AudioManager audioManager;
    private AudioSource goalSound;

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        goalSound = audioManager.GetAudio("Goal");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ball")
        {
            Debug.Log("goal scored");
            int score = ball.GetComponent<BallScript>().GetCharge();
            if (playerGoal == 1)
            {
                scoringManager.PlayerGoal(2, score);
            }
            else
            {
                scoringManager.PlayerGoal(1, score);
            }

            if (goalSound.isPlaying == false)
                PlayGoalSound();

            ball.SetActive(false);
            Invoke("ResetBall", 1);

            foreach (PlayerMovement playerMovement in playerMovements)
            {
                playerMovement.ResetOnGoal();
            }
        }
    }

    private void ResetBall()
    {
        ball.SetActive(true);
        ball.GetComponent<BallScript>().Reset();
    }

    public void PlayGoalSound()
    {
        IEnumerator coroutine = GoalSoundCoroutine(5);
        StartCoroutine(coroutine);
    }

    private IEnumerator GoalSoundCoroutine(float seconds)
    {
        goalSound.Play();
        yield return new WaitForSeconds(seconds);
        goalSound.Stop();
    }
}