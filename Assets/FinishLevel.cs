using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLevel : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;
    [SerializeField] PauseMenu pauseMenu;
    // Start is called before the first frame update

    private void Awake()
    {
        pauseMenu = GameObject.Find("Canvas").GetComponent<PauseMenu>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
            _particleSystem.Play();
        pauseMenu.LevelComplete();
    }
}
