using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioSource;


    public static AudioManager instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // hier controleren we of er al een instantie is van deze singleton
        // als dit zo is dan hoeven we geen nieuwe aan te maken en verwijderen we deze
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
