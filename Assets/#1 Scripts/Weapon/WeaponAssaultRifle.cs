using UnityEngine;

public class WeaponAssaultRifle : MonoBehaviour
{
    [Header("AudioBehaviour Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;   // ���� ���� ����

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        PlaySound(audioClipTakeOutWeapon);
    }

    private void PlaySound(AudioClip clip) // ���� ���� ���� ��, ���� clip ��ü�� ���
    {
        audioSource.Stop();         // 
        audioSource.clip = clip;
        audioSource.Play();
    }
}
