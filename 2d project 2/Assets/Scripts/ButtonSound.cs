using UnityEngine;

public class ButtonSound : MonoBehaviour
{

    public AudioClip clip;

    public void PlaySound()
    {
        AudioManager.instance.PlaySound(clip);
    }

}
