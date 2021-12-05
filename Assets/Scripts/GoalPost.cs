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
                goalSound.Play();

            ball.SetActive(false);
            Invoke("ResetBall", 1f);

            foreach (PlayerMovement playerMovement in playerMovements) {
                var playerNum = playerMovement.playerNumber;
                if ((gameObject.name.StartsWith("TutP1") && playerNum != PlayerData.PlayerNumber.PlayerOne) || (gameObject.name.StartsWith("TutP2") && playerNum != PlayerData.PlayerNumber.PlayerTwo))
                    continue;
                    

                float distance = (playerMovement.transform.position - transform.position).magnitude;
                playerMovement.StartExplosion(GameConfigurations.goalExplosionSpeed * 10f / distance, GameConfigurations.goalExplosionFrame, transform.position);
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