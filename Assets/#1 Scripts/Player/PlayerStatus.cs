using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class HPEvent : UnityEngine.Events.UnityEvent<float, float> { }
public class PlayerStatus : MonoBehaviour
{
    [HideInInspector]
    public HPEvent onHpEvent = new HPEvent();
    
    [Header("Walk, Run Speed")]
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    
    [Header("HP")]
    [SerializeField]
    private int maxHp = 100;
    private int currentHp;
    
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public bool DecreaseHp(int damage)
    {
        int previousHp = currentHp;
        currentHp =currentHp - damage > 0? currentHp - damage : 0;
        onHpEvent.Invoke(previousHp, currentHp);

        if (currentHp == 0)
        {
            return true;
        }
        return false;
    }
}
