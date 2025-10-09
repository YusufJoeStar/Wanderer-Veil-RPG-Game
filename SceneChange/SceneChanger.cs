using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad;
    public string teleporterID; // Give each teleporter a unique name

    [Header("Animation")]
    public Animator fadeAnim;
    public float fadeTime = 0.5f;

    [Header("Player Position")]
    public Vector3 newPlayerPosition;

    private Transform player;
    public static string targetTeleporter = ""; // Which teleporter to spawn at

    private void Start()
    {
        // Wait a frame to ensure scene is fully loaded, then position player
        StartCoroutine(CheckPlayerPosition());
    }

    IEnumerator CheckPlayerPosition()
    {
        // Wait a bit longer for everything to initialize properly
        yield return new WaitForSeconds(0.1f);

        if (targetTeleporter == teleporterID)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                // Force the position more aggressively
                playerObj.transform.position = newPlayerPosition;

                // If player has a Rigidbody2D, stop any movement
                Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = Vector2.zero;
                }

                Debug.Log($"Player positioned at: {playerObj.transform.position} using teleporter: {teleporterID}");
            }
            targetTeleporter = ""; // Clear it
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            player = collision.transform;

            // Tell the system which teleporter to spawn at in the next scene
            SetTargetTeleporter();

            if (fadeAnim != null)
            {
                fadeAnim.Play("FadeOut");
            }

            StartCoroutine(DelayFade());
        }
    }

    void SetTargetTeleporter()
    {
        // Based on THIS teleporter's ID, decide where to spawn in the next scene
        if (teleporterID == "teleporter1") // Scene 1 -> Scene 2
        {
            targetTeleporter = "scene2_spawn";
        }
        else if (teleporterID == "teleporter2") // Scene 1 -> Scene 3  
        {
            targetTeleporter = "scene3_spawn";
        }
        else if (teleporterID == "scene2_spawn") // Scene 2 -> Scene 1
        {
            targetTeleporter = "teleporter1"; // Go back to teleporter 1
        }
        else if (teleporterID == "scene3_spawn") // Scene 3 -> Scene 1
        {
            targetTeleporter = "teleporter2"; // Go back to teleporter 2
        }
    }

    IEnumerator DelayFade()
    {
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene(sceneToLoad);
    }
}