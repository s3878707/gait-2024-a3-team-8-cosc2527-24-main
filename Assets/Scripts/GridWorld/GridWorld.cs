using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridWorld : MonoBehaviour
{
    private const float MONSTER_MOVE_PROB = 0.4f;

    private const float CHEST_REWARD = 2.0f;
    private const float APPLE_REWARD = 1.0f;
    private const float KEY_REWARD = 0.0f;
    private const float DIED_REWARD = 0.0f;

    [Range(0, 6)]
    public int mapNum;

    public GameObject floorTile;
    public GameObject wallTile;
    public GameObject fireTile;
    public GameObject applesTile;
    public GameObject keyTile;
    public GameObject chestTile;
    public GameObject monsterTile;
    public GameObject player;

    private char[,] map;

    private bool gameOver;
    private float totalEpisodeReward;

    private List<GameObject> monsters;
    private List<IntVector2> monsterPositions;
    private List<IntVector2> monsterSpawnPositions;

    private GameObject key;
    private IntVector2 keyPosition;

    private List<GameObject> chests;
    private List<IntVector2> chestPositions;

    private List<GameObject> apples;
    private List<IntVector2> applePositions;

    private int playerX;
    private int playerY;
    private int playerSpawnX;
    private int playerSpawnY;

    private bool hasKey;

    public bool IsGameOver()
    {
        return gameOver;
    }

    public float GetTotalEpisodeReward()
    {
        return totalEpisodeReward;
    }

    public IntVector2 GetMapSize()
    {
        return new IntVector2(map.GetLength(0), map.GetLength(1));
    }

    public IntVector2 GetPlayerPosition()
    {
        return new IntVector2(playerX, playerY);
    }

    public bool HasKey()
    {
        return hasKey;
    }

    public bool IsBlocked(int x, int y)
    {
        if (x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1))
        {
            return true;
        }

        return map[x, y] == '#';
    }

    public void Initialise()
    {
        Dictionary<char, GameObject> tileSet = new Dictionary<char, GameObject>();
        tileSet.Add('-', floorTile);
        tileSet.Add('#', wallTile);
        tileSet.Add('*', fireTile);
        tileSet.Add('a', applesTile);
        tileSet.Add('k', keyTile);
        tileSet.Add('c', chestTile);
        tileSet.Add('m', monsterTile);

        Transform tilesParent = GameObject.Find("Tiles").transform;

        monsters = new List<GameObject>();
        monsterPositions = new List<IntVector2>();
        monsterSpawnPositions = new List<IntVector2>();

        key = null;
        keyPosition = null;

        chests = new List<GameObject>();
        chestPositions = new List<IntVector2>();

        apples = new List<GameObject>();
        applePositions = new List<IntVector2>();

        TextAsset mytxtData = (TextAsset)Resources.Load("map" + mapNum);

        string[] lines = mytxtData.text.Split(
            new string[] { Environment.NewLine, "" + (char)10, "" + (char)13 },
            StringSplitOptions.None
        );

        bool playerPlaced = false;

        for (int l = 0; l < lines.Length; l++)
        {
            char[] charArray = lines[l].ToCharArray();
            if (l == 0)
            {
                map = new char[charArray.Length, lines.Length];
            }

            int y = lines.Length - l - 1;
            for (int x = 0; x < charArray.Length; x++)
            {
                Instantiate(tileSet['-'], new Vector3(x, y, 0), Quaternion.identity, tilesParent);

                if (charArray[x] == 'p')
                {
                    if (playerPlaced)
                    {
                        throw new Exception("Map cannot contain more than one player!");
                    }

                    player.transform.position = new Vector3(x, y, 0);
                    map[x, y] = '-';
                    playerX = x;
                    playerY = y;
                    playerSpawnX = x;
                    playerSpawnY = y;
                    playerPlaced = true;
                }
                else if (charArray[x] == 'm')
                {
                    map[x, y] = '-';
                    monsters.Add(
                        Instantiate(
                            tileSet[charArray[x]],
                            new Vector3(x, y, 0),
                            Quaternion.identity,
                            tilesParent
                        )
                    );
                    monsterPositions.Add(new IntVector2(x, y));
                    monsterSpawnPositions.Add(new IntVector2(x, y));
                }
                else if (charArray[x] == 'a')
                {
                    map[x, y] = '-';
                    apples.Add(
                        Instantiate(
                            tileSet[charArray[x]],
                            new Vector3(x, y, 0),
                            Quaternion.identity,
                            tilesParent
                        )
                    );
                    applePositions.Add(new IntVector2(x, y));
                }
                else if (charArray[x] == 'k')
                {
                    map[x, y] = '-';

                    if (key != null)
                    {
                        throw new Exception("Map cannot contain more than one key!");
                    }

                    key = Instantiate(
                        tileSet[charArray[x]],
                        new Vector3(x, y, 0),
                        Quaternion.identity,
                        tilesParent
                    );
                    keyPosition = new IntVector2(x, y);
                }
                else if (charArray[x] == 'c')
                {
                    map[x, y] = '-';
                    chests.Add(
                        Instantiate(
                            tileSet[charArray[x]],
                            new Vector3(x, y, 0),
                            Quaternion.identity,
                            tilesParent
                        )
                    );
                    chestPositions.Add(new IntVector2(x, y));
                }
                else if (charArray[x] != '-')
                {
                    map[x, y] = charArray[x];
                    Instantiate(
                        tileSet[charArray[x]],
                        new Vector3(x, y, 0),
                        Quaternion.identity,
                        tilesParent
                    );
                }
            }
        }

        GameObject.Find("Camera").transform.position = new Vector3(
            map.GetLength(0) / 2.0f,
            map.GetLength(1) / 2.0f,
            -10f
        );

        Reset();
    }

    public void Reset()
    {
        gameOver = false;
        totalEpisodeReward = 0.0f;

        playerX = playerSpawnX;
        playerY = playerSpawnY;
        player.transform.position = new Vector3(playerX, playerY, player.transform.position.z);

        for (int i = 0; i < monsters.Count; i++)
        {
            monsterPositions[i] = new IntVector2(
                monsterSpawnPositions[i].x,
                monsterSpawnPositions[i].y
            );

            monsters[i].transform.position = new Vector3(
                monsterPositions[i].x,
                monsterPositions[i].y,
                monsters[i].transform.position.z
            );
        }

        foreach (GameObject apple in apples)
        {
            apple.SetActive(true);
        }

        if (key != null)
        {
            key.SetActive(true);
        }

        foreach (GameObject chest in chests)
        {
            chest.SetActive(true);
        }

        hasKey = false;
    }

    // Returns a tuple (reward, gameOver)
    public (float, bool) Act(int action)
    {
        if (gameOver)
        {
            return (0.0f, true);
        }

        // Handle monster movement first
        List<IntVector2> newMonsterPositions = new List<IntVector2>();
        for (int i = 0; i < monsters.Count; i++)
        {
            if (UnityEngine.Random.value < MONSTER_MOVE_PROB)
            {
                int moveDir = 0;

                IntVector2 currentPos = monsterPositions[i];

                bool canMoveUp =
                    (currentPos.y < monsterSpawnPositions[i].y)
                    && !IsBlocked(currentPos.x, currentPos.y + 1);

                bool canMoveDown =
                    (currentPos.y > (monsterSpawnPositions[i].y - 3))
                    && !IsBlocked(currentPos.x, currentPos.y - 1);

                if (canMoveUp && canMoveDown)
                {
                    moveDir = 2 * UnityEngine.Random.Range(0, 2) - 1;
                }
                else if (canMoveUp)
                {
                    moveDir = 1;
                }
                else if (canMoveDown)
                {
                    moveDir = -1;
                }

                newMonsterPositions.Add(
                    new IntVector2(monsterPositions[i].x, monsterPositions[i].y + moveDir)
                );
            }
            else
            {
                newMonsterPositions.Add(monsterPositions[i]);
            }
        }

        int newX = playerX;
        int newY = playerY;

        if (action == 0) // Up
        {
            newY++;
        }
        else if (action == 1) // Down
        {
            newY--;
        }
        else if (action == 2) // Left
        {
            newX--;
        }
        else if (action == 3) // Right
        {
            newX++;
        }

        if (IsBlocked(newX, newY))
        {
            newX = playerX;
            newY = playerY;
        }

        // Check if the player has hit a monster.
        // Counts as a hit if the player and monster have moved
        // to the same square, or if they've swapped squares.
        bool hitMonster = false;
        for (int i = 0; i < newMonsterPositions.Count; i++)
        {
            bool swappedSquares = (
                playerX == newMonsterPositions[i].x
                && playerY == newMonsterPositions[i].y
                && newX == monsterPositions[i].x
                && newY == monsterPositions[i].y
            );

            if (swappedSquares)
            {
                // Don't move the monster in this situation since it will look confusing visually.
                newMonsterPositions[i] = monsterPositions[i];
            }

            if (
                swappedSquares
                || (newX == newMonsterPositions[i].x && newY == newMonsterPositions[i].y)
            )
            {
                hitMonster = true;
                break;
            }
        }

        monsterPositions = newMonsterPositions;
        for (int i = 0; i < monsterPositions.Count; i++)
        {
            monsters[i].transform.position = new Vector3(
                monsterPositions[i].x,
                monsterPositions[i].y,
                monsters[i].transform.position.z
            );
        }

        playerX = newX;
        playerY = newY;
        player.transform.position = new Vector3(playerX, playerY, transform.position.z);

        float reward = 0.0f;

        // Check if the player has collected an apple.
        for (int i = 0; i < apples.Count; i++)
        {
            if (
                apples[i].activeSelf
                && playerX == applePositions[i].x
                && playerY == applePositions[i].y
            )
            {
                reward += APPLE_REWARD;
                totalEpisodeReward += APPLE_REWARD;
                apples[i].SetActive(false);
                break;
            }
        }

        // Check if the player has got a key.
        if (key != null)
        {
            if (!hasKey && playerX == keyPosition.x && playerY == keyPosition.y)
            {
                hasKey = true;
                reward += KEY_REWARD;
                totalEpisodeReward += KEY_REWARD;
                key.SetActive(false);
            }
        }

        // Check if the player has opened a chest.
        for (int i = 0; i < chests.Count; i++)
        {
            if (
                hasKey
                && chests[i].activeSelf
                && playerX == chestPositions[i].x
                && playerY == chestPositions[i].y
            )
            {
                reward += CHEST_REWARD;
                totalEpisodeReward += CHEST_REWARD;
                chests[i].SetActive(false);
                break;
            }
        }

        if (hitMonster || map[playerX, playerY] == '*') // Hit monster or spikes
        {
            gameOver = true;
            reward += DIED_REWARD;
            totalEpisodeReward += DIED_REWARD;
            return (reward, true);
        }

        // Consider the episode to be over if there are no rewards left.
        gameOver = true;
        foreach (List<GameObject> l in new List<GameObject>[] { chests, apples })
        {
            foreach (GameObject o in l)
            {
                if (o.activeSelf)
                {
                    gameOver = false;
                    break;
                }
            }
        }

        return (reward, gameOver);
    }
}
