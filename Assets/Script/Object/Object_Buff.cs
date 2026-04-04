using System.Collections;
using System.Collections.Generic;
using UnityEngine;









public class Object_Buff : MonoBehaviour
{
    
    private PlayerStats statsToModify;

    [Header("Buff details")]
    [SerializeField]private BuffEffectData[] buffs;
    [SerializeField]private string buffName;
    
    [SerializeField]private float buffDuration = 4f;
   

    [Header("float details")]
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float floatRange = .1f;
    private Vector3 startPos;

    private void Awake()
    {
        
        startPos = transform.position;
    }

    private void Update()
    {
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.position = startPos + new Vector3(0, yOffset);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        

        statsToModify = collision.GetComponent<PlayerStats>();

        if (statsToModify.canApplyBuffOf(buffName))
        {
            statsToModify.ApplyBuff(buffs, buffDuration, buffName);
            Destroy(gameObject);
        }
    }
   
}
