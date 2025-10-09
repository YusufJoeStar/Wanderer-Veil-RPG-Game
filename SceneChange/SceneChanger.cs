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
        StartCoroutine(CheckPlayerPosition());
    }

    IEnumerator CheckPlayerPosition()
    {
        yield return new WaitForSeconds(0.1f);

        if (targetTeleporter == teleporterID)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerObj.transform.position = newPlayerPosition;

                Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.velocity = Vector2.zero;
                }

         
            }
            targetTeleporter = ""; 
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            player = collision.transform;

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
