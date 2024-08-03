using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridWorldAgent : MonoBehaviour
{
    private const int MAX_EPISODE_STEPS = 1000;
    private const int EPISODES_PER_YIELD = 25;
    private const int NUM_ACTIONS = 4;

    public enum ControlType
    {
        Keyboard,
        QLearning,
        SARSA
    }

    public ControlType controlType;

    private int episodeCount;
    private float epsilon;

    [Range(1, 10)]
    public int endOfTrainFPS;

    private GridWorld env;
    private GridWorldUI ui;

    public GridWorldTrainingParams[] trainingParams;
    private GridWorldTrainingParams tp;

    private Dictionary<(int, int, bool, string, (float, float)?), float[]> Q;
    private Dictionary<(int, int, bool, string, (float, float)?), int> countState;

    private void Start()
    {
        episodeCount = 0;
        ui = GameObject.Find("Canvas").GetComponent<GridWorldUI>();

        env = GameObject.Find("MapLoader").GetComponent<GridWorld>();
        env.Initialise();

        tp = trainingParams[env.mapNum];

        epsilon = tp.epsilonStart;

        Q = new Dictionary<(int, int, bool, string, (float, float)?), float[]>();
        countState = new Dictionary<(int, int, bool, string, (float, float)?), int>();
        if (controlType == ControlType.QLearning)
        {
            StartCoroutine(RunQLearning());
        }
        else if (controlType == ControlType.SARSA)
        {
            StartCoroutine(RunSARSA());
        }
    }

    private (int, int, bool, string, (float, float)?) GetState()
    {
        IntVector2 playerPos = env.GetPlayerPosition();
        (float, float)? closestMonster = null;

        foreach (var monster in GameObject.FindGameObjectsWithTag("Monster"))
        {
            if (
                Math.Abs(monster.transform.position.x - playerPos.x) <= 1
                && Math.Abs(monster.transform.position.y - playerPos.y) <= 1
            )
            {
                closestMonster = (
                    monster.transform.position.x - playerPos.x,
                    monster.transform.position.y - playerPos.y
                );
                break; // You can break out of the loop early if a monster is found close.
            }
        }
        return (playerPos.x, playerPos.y, env.HasKey(), GetRemainingApplesState(), closestMonster);
    }

    private string GetRemainingApplesState()
    {
        string state = "";
        foreach (var apple in GameObject.FindGameObjectsWithTag("Apples"))
        {
            state += apple.activeSelf ? "1" : "0";
        }
        return state;
    }

    private int ChooseAction((int, int, bool, string, (float, float)?) state)
    {
        if (!Q.ContainsKey(state))
        {
            Q[state] = new float[NUM_ACTIONS];
        }

        if (UnityEngine.Random.value < epsilon)
        {
            return UnityEngine.Random.Range(0, NUM_ACTIONS);
        }
        else
        {
            float[] actionValues = Q[state];
            int bestAction = 0;
            for (int i = 1; i < NUM_ACTIONS; i++)
            {
                if (actionValues[i] > actionValues[bestAction])
                {
                    bestAction = i;
                }
            }
            return bestAction;
        }
    }

    IEnumerator RunQLearning()
    {
        while (true)
        {
            env.Reset();
            (int, int, bool, string, (float, float)?) state = GetState();

            epsilon = Mathf.Lerp(
                tp.epsilonStart,
                tp.epsilonEnd,
                (float)episodeCount / tp.numTrainEpisodes
            );

            int steps = 0;

            while (!env.IsGameOver() && steps < MAX_EPISODE_STEPS)
            {
                int action = ChooseAction(state);
                (float reward, bool gameOver) = env.Act(action);
                (int, int, bool, string, (float, float)?) nextState = GetState();
                if (env.mapNum == 6)
                {
                    if (!countState.ContainsKey(nextState))
                    {
                        countState[nextState] = 1;
                    }
                    else
                    {
                        countState[nextState] += 1;
                    }

                    reward =
                        reward + tp.intrinsicRewardStrength * 1 / Mathf.Sqrt(countState[nextState] + 1);
                }
                if (!Q.ContainsKey(nextState))
                {
                    Q[nextState] = new float[NUM_ACTIONS];
                }

                float maxNextQ = Mathf.Max(Q[nextState]);
                Q[state][action] =
                    (1 - tp.alpha) * Q[state][action] + tp.alpha * (reward + tp.gamma * maxNextQ);

                state = nextState;
                steps++;
                if (episodeCount >= tp.numTrainEpisodes)
                {
                    yield return new WaitForSeconds(1.0f / endOfTrainFPS);
                }
            }

            if (episodeCount % EPISODES_PER_YIELD == 0)
            {
                yield return null;
            }

            episodeCount++;
        }
    }

    IEnumerator RunSARSA()
    {
        while (true)
        {
            env.Reset();
            (int, int, bool, string, (float, float)?) state = GetState();
            int action = ChooseAction(state);

            epsilon = Mathf.Lerp(
                tp.epsilonStart,
                tp.epsilonEnd,
                (float)episodeCount / tp.numTrainEpisodes
            );

            int steps = 0;

            while (!env.IsGameOver() && steps < MAX_EPISODE_STEPS)
            {
                (float reward, bool gameOver) = env.Act(action);
                (int, int, bool, string, (float, float)?) nextState = GetState();
                if (env.mapNum == 6)
                {
                    if (!countState.ContainsKey(nextState))
                    {
                        countState[nextState] = 1;
                    }
                    else
                    {
                        countState[nextState] += 1;
                    }

                    reward =
                        reward + tp.intrinsicRewardStrength * 1 / Mathf.Sqrt(countState[nextState] + 1);
                }
                int nextAction = ChooseAction(nextState);
                if (!Q.ContainsKey(nextState))
                {
                    Q[nextState] = new float[NUM_ACTIONS];
                }

                Q[state][action] =
                    (1 - tp.alpha) * Q[state][action]
                    + tp.alpha * (reward + tp.gamma * Q[nextState][nextAction]);

                state = nextState;
                action = nextAction;
                steps++;

                if (episodeCount >= tp.numTrainEpisodes)
                {
                    yield return new WaitForSeconds(1.0f / endOfTrainFPS);
                }
            }

            if (episodeCount % EPISODES_PER_YIELD == 0)
            {
                yield return null;
            }

            episodeCount++;
        }
    }

    private void Update()
    {
        ui.UpdateTopRightText("Total reward: " + env.GetTotalEpisodeReward());
        ui.UpdateBottomRightText("Epsilon: " + epsilon.ToString("0.00"));

        if (controlType == ControlType.Keyboard)
        {
            ui.UpdateTopLeftText("Keyboard control mode");

            if (env.IsGameOver())
            {
                ui.UpdateBottomLeftText("Game over, press 'R' to reset");

                if (Input.GetKeyDown(KeyCode.R))
                {
                    env.Reset();
                }
            }
            else
            {
                ui.UpdateBottomLeftText("Use arrow keys to move");

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    env.Act(0);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    env.Act(1);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    env.Act(2);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    env.Act(3);
                }
            }
        }
        else
        {
            ui.UpdateTopLeftText("Training Episode");

            if (episodeCount < tp.numTrainEpisodes)
            {
                ui.UpdateBottomLeftText(
                    episodeCount.ToString()
                        + " ("
                        + (100.0f * episodeCount / tp.numTrainEpisodes).ToString("0.0")
                        + "%)"
                );
            }
            else
            {
                ui.UpdateBottomLeftText(episodeCount.ToString());
            }
        }
    }
}
