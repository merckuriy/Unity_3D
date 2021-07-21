using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Rigidbody playerRb;
    public float jumpForce;
    public float gravityModifier;
    public ParticleSystem explosionParticle;
    public ParticleSystem dirtParticle;
    private AudioSource playerAudio;
    public AudioClip jumpSound; 
    public AudioClip crashSound;

    public bool gameOver;
    private bool isOnGround = true;
    private Animator playerAnim;

    // Start is called before the first frame update
    void Start()
    {
        playerRb = GetComponent<Rigidbody>();
        Physics.gravity *= gravityModifier;
        playerAnim = GetComponent<Animator>();
        playerAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isOnGround && !gameOver)
        {
            playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isOnGround = false;
            dirtParticle.Stop();
            playerAnim.SetTrigger("Jump_trig");
            playerAudio.PlayOneShot(jumpSound, 1.0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isOnGround = true;
            dirtParticle.Play();
        }else if (collision.gameObject.CompareTag("Obstacle")){
            gameOver = true;
            Debug.Log("Game Over!");

            playerAnim.SetBool("Death_b", true);
            playerAnim.SetInteger("DeathType_int", 1);

            dirtParticle.Stop();
            explosionParticle.Play();
            playerAudio.PlayOneShot(crashSound, 1.0f);
            GameObject.Find("Main Camera").GetComponent<AudioSource>().Stop();

            StartCoroutine(NewGame());
        }
    }

    IEnumerator NewGame()
    {
        yield return new WaitForSeconds(5);
        gameOver = false;
        Debug.Log("New game");
        playerAnim.SetBool("Death_b", false);
        GameObject.Find("Main Camera").GetComponent<AudioSource>().Play();
    }
}
