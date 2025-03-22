using UnityEngine;

public class WeaponAssaultRifle : MonoBehaviour
{
    [Header("AudioBehaviour Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;   // 무기 장착 사운드

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        PlaySound(audioClipTakeOutWeapon);
    }

    private void PlaySound(AudioClip clip) // 기존 사운드 정지 후, 사운드 clip 교체후 재생
    {
        audioSource.Stop();         // 
        audioSource.clip = clip;
        audioSource.Play();
    }
}
