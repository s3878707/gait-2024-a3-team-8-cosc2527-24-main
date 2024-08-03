using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PlayerAgent : Agent
{
    public Player _playerScript;
    public GameManager _gameManager;
    public GameObject boss; // Reference to the boss object

    // Start is called before the first frame update
    void Start()
    {
        // Allows training to run in the background when the Unity window loses focus.
        Application.runInBackground = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add player's position
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.y);
    }

    public void HandleHitEnemy()
    {
        int phaseReached = _gameManager.phase;
        if (_playerScript.getLives() > 1)
        {
            AddReward(-0.5f);
        }
        else
        {
            // Additional penalty if player is low on lives or dead
            Academy.Instance.StatsRecorder.Add("Phase Reached", phaseReached);
            EndEpisode();
        }
    }

    public void HandleHitBoss()
    {
        int phaseReached = _gameManager.phase;
        if (_playerScript.getLives() > 3)
        {
            AddReward(-0.5f);
        }
        else
        {
            // Additional penalty if player is low on lives or dead
            Academy.Instance.StatsRecorder.Add("Phase Reached", phaseReached);
            EndEpisode();
        }
    }

    public void HandlePowerup()
    {
        AddReward(2.0f);
    }

    public void HandleShootEnemy()
    {
        AddReward(0.5f);
    }

    public void HandleShootSpawners()
    {
        AddReward(1.0f);
    }

    public void HandleShootBoss()
    {
        AddReward(2.0f);
    }

    public void HandleDestroySpawner()
    {
        AddReward(1.0f);
    }

    public void HandleCloseToBoss()
    {
        AddReward(0.0f);
    }

    public void HandleCloseToBorder()
    {
        AddReward(-1.0f);
    }

    public void HandleDestroyBoss()
    {
        AddReward(2.0f);
    }

    public float ClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minEnemyDistance = Mathf.Infinity;
        GameObject closestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < minEnemyDistance)
            {
                minEnemyDistance = distanceToEnemy;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy != null)
        {
            return minEnemyDistance;
        }

        return Mathf.Infinity;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int rotate = actionBuffers.DiscreteActions[0];
        int strafe = actionBuffers.DiscreteActions[1];
        _playerScript.Movement_V1(rotate, strafe);
    }
}
