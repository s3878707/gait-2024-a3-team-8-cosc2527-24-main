using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    // 0 = shield, 1 = spreadshot, 2 = health
    GameObject _player;

    [SerializeField]
    private int powerupID;

    [SerializeField]
    private float timeUntilDissolve = 20f; // Disappears after time

    [SerializeField]
    private AudioClip _clip;

    float time = 0f;

    Coroutine timer;

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        StartTimer();
    }

    void FixedUpdate()
    {
        if (time >= timeUntilDissolve)
        {
            StopCoroutine(TimerUntilGoneRoutine());
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Player player = other.GetComponent<Player>();

            if (player != null)
            {
                other.GetComponent<PlayerAgent>().HandlePowerup();
                AudioSource.PlayClipAtPoint(_clip, Camera.main.transform.position, 1f);

                // 0 = shield, 1 = spreadshot, 2 = health
                if (powerupID == 0)
                {
                    player.powerupShield();
                }
                else if (powerupID == 1)
                {
                    player.powerupRapidShot();
                }
                else if (powerupID == 2)
                {
                    player.powerupHealth();
                }
            }

            Destroy(this.gameObject);
        }
    }

    public void StartTimer()
    {
        timer = StartCoroutine(TimerUntilGoneRoutine());
    }

    IEnumerator TimerUntilGoneRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            time += 5;
        }
    }
}
