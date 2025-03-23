using System.Collections;
using UnityEngine;

public class WeaponAssaultRifle : MonoBehaviour
{
    [Header("Fire Effects")]
    [SerializeField]
    private GameObject muzzleFlashEffect;   // �ѱ� ����Ʈ on / off

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip audioClipTakeOutWeapon;   // ���� ���� ����
    [SerializeField]
    private AudioClip audioClipFire;   // ���� ����


    [Header("Weapon Setting")]
    [SerializeField]
    private WeaponSet weaponSet; // ���� ����

    private float lastAttackTime = 0; // ������ �߻� �ð� ü�Ѥ�

    private AudioSource audioSource;
    private PlayerAnimateController animator;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInParent<PlayerAnimateController>();
    }

    private void OnEnable()
    {
        // ���� ���� ����
        PlaySound(audioClipTakeOutWeapon);
        // �ѱ� ����Ʈ ������Ʈ ��Ȱ��ȭ
        muzzleFlashEffect.SetActive(false);
    }

    private void PlaySound(AudioClip clip) // ���� ���� ���� ��, ���� clip ��ü�� ���
    {
        audioSource.Stop();         // 
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StartWeaponAction(int type = 0)
    {
        // ���� ���콺 Ŭ�� (���� ����
        if (type == 0)
        {
            // �ѿ��� ����
            if( weaponSet.isAutomaticAttack == true)
            {
                StartCoroutine("OnAttackLoop");
            }
            // �ܹ� ����
            else
            {
                OnAttack();
            }
        }
        
    }

    public void StopWeaponAction(int type = 0)
    {
        if(type == 0)
        {
            StopCoroutine("OnAttackLoop");
        }
    }

    private IEnumerator OnAttackLoop()
    {
        while (true)
        {
            OnAttack();

            yield return null;
        }
    }

    private void OnAttack()
    {
        if (Time.time - lastAttackTime > weaponSet.attakRate)
        {
            if (animator.MoveSpeed > 0.5f)
            {
                return;
            }

            lastAttackTime = Time.time;

            animator.Play("Fire", -1, 0); // ���� �ִϸ��̼�
            StartCoroutine("OnMuzzleFlashEffect"); // �ѱ� ����Ʈ
            PlaySound(audioClipFire); // �ѱ� �߻���
        }
    }

    public IEnumerator OnMuzzleFlashEffect()
    {
        muzzleFlashEffect.SetActive(true);

        yield return new WaitForSeconds(weaponSet.attakRate * 0.3f);

        muzzleFlashEffect.SetActive(false);
    }
}
