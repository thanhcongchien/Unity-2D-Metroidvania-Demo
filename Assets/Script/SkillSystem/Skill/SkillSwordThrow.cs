using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSwordThrow : SkillBase
{
    private SkillObjectSword currentSword;
    private float currentThrowPower;

    [Header("Regular Sword Upgrade")]
    [SerializeField] private GameObject SwordPrefab;
    [Range(0, 10)]
    [SerializeField] private float regularThrowPower = 5;

    [Header("Pierce Sword Upgrade")]
    [SerializeField] private GameObject SwordPrefabPeirce;
    [Range(0, 10)]
    [SerializeField] private float pierceThrowPower = 5;
    public int peirceCount = 2;
    [Header("Spin Sword Upgrade")]
    [SerializeField] private GameObject SwordPrefabSpin;
    public int maxDistance = 5;
    public float attacksPerSecond = 6;
   public float maxSpinDuration = 3;
    [Range(0, 10)]
    [SerializeField] private float spinThrowPower = 5;
    [Header("Bounce Sword Upgrade" )]
    [SerializeField]private GameObject SwordPrefabBounce;
    public int bounceCount = 5;
    public float bounceSpeed = 12;
    [Range(0, 10)]
    [SerializeField] private float bounceThrowPower = 5;
    [Header("Trajectory predition ")]
    [SerializeField] private GameObject TrajectoryDot;
    [SerializeField] private int numofDots = 20;
    [SerializeField] private float spaceBetweenDots = 0.05f;
    private float swordGravity;
    private Transform[] dots;
    private Vector2 confirmedDirection;


    protected override void Awake()
    {
        base.Awake();
        swordGravity=SwordPrefab.GetComponent<Rigidbody2D>().gravityScale;
       dots = GenerateDots();
    }

    public override bool CanUseSkill()
    {
        UpdateThrowPower();
        if(currentSword!=null)
        {
            currentSword.GetSwordBack();
            return false;
        }


        return base.CanUseSkill();
    }

    public void ThrowSword()
    {
        GameObject swordPre=getSwordPrefab();
        GameObject newSword = Instantiate(swordPre, dots[1].position,Quaternion.identity);
        currentSword =newSword.GetComponent<SkillObjectSword>();
        currentSword.SetUpSword(this, GetThrowVelocity(confirmedDirection));
        SetSkillCooldown();
    }
    private GameObject getSwordPrefab()
    {
        if(Unlocked(SkillUpgradeType.SwordThrow))
            return SwordPrefab;
        if(Unlocked(SkillUpgradeType.SwordThrow_Pierce))
            return SwordPrefabPeirce;
        if(Unlocked(SkillUpgradeType.SwordThrow_Spin))
            return SwordPrefabSpin;
        if(Unlocked(SkillUpgradeType.SwordThrow_Bounce))
            return SwordPrefabBounce;

        Debug.Log("没有合适的剑预制体");
        return null;
    }
    private void UpdateThrowPower()
    {
        switch (upgradeType)
        {
            case SkillUpgradeType.SwordThrow:
                currentThrowPower = regularThrowPower;
                break;
            case SkillUpgradeType.SwordThrow_Pierce:
                currentThrowPower = pierceThrowPower;
                break;
            case SkillUpgradeType.SwordThrow_Spin:
                currentThrowPower = spinThrowPower;
                break;
            case SkillUpgradeType.SwordThrow_Bounce:
                currentThrowPower = bounceThrowPower;
                break;
        }

    }
    public void ClearActiveSword(SkillObjectSword sword)
    {
        if (currentSword == sword)
            currentSword = null;
    }

    private Vector2 GetThrowVelocity(Vector2 direction) => direction.normalized * currentThrowPower * 10;

    public void PredictTrajectory(Vector2 direction)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        for (int i=0;i<dots.Length;i++)
        {
            dots[i].position = GetTrajectoryPoint(direction, i * spaceBetweenDots);
           
        }

    }

    private Vector2 GetTrajectoryPoint(Vector2 direction,float t)
    {
        Vector2 initialVelocity = GetThrowVelocity(direction);
        Vector2 gravityEffect = 0.5f * Physics2D.gravity * swordGravity * t * t;
        Vector2 predictedPoint=(initialVelocity*t)+gravityEffect;
        Vector2 playerPos=transform.root.position;

        return playerPos+predictedPoint;
    }
    public void ConfirmTrajectory(Vector2 direction)=>confirmedDirection = direction.normalized;
    

    public void EnableDots(bool enable)
    {
        foreach(Transform t in dots)
        {
            t.gameObject.SetActive(enable);
        }

    }


    private Transform[] GenerateDots()
    {
        Transform[] newDots= new Transform[numofDots];
        
        for(int i=0;i<numofDots;i++)
        {
            newDots[i]=Instantiate(TrajectoryDot,transform.position,Quaternion.identity,transform).transform;
            newDots[i].gameObject.SetActive(false);
        }
        return newDots;
    }
}

