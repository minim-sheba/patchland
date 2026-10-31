using UnityEngine;

public class EnvironmentalSound : MonoBehaviour
{
    public GameObject CubeOfSilence;
    public AudioSource EnvironmentSound;
    public Animator InsectSoundsAnimator;
    private int CubeOfSilence_count = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EnvironmentSound.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Entering Cube of Silence");
    }
    private void OnTriggerExit(Collider other)
    {
        
        // Debug.Log("Exiting Cube of Silence");
        // Debug.Log("CubeofSilence_count is " + CubeOfSilence_count.ToString());
        if (CubeOfSilence_count < 1) { 
            // Start environmental sound, play animation that fades in the sound
            EnvironmentSound.Play();
            InsectSoundsAnimator.Play("Fade_PenNibInsects", 0);
            CubeOfSilence_count += 1;
        }
    }
}
