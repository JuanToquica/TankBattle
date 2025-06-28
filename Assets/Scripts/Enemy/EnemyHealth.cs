using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : HealthBase
{
    [SerializeField] private Transform healthBarCanvas;
    [SerializeField] private float timeOfDeath;
    public EnemyManager enemyManager;
    private EnemyAttack enemyAttack;
    private Outline outline;
    private EnemyAI enemyAI;  
    private TankMaterialHandlerBase changeTankPaint;

    private void Start()
    {
        health = maxHealth;
        enemyAI = GetComponent<EnemyAI>();
        changeTankPaint = GetComponent<TankMaterialHandlerBase>();
        enemyAttack = GetComponent<EnemyAttack>();
        outline = GetComponent<Outline>();
    }

    protected override void Update()
    {
        base.Update();
        LookAtThePlayer();
    }

    private void LookAtThePlayer()
    {
        healthBarCanvas.transform.forward = -Camera.main.transform.forward;
    }

    protected override void Die()
    {
        base.Die(); 
        changeTankPaint.OnTankDead();
        healthBarCanvas.gameObject.SetActive(false);

        GameObject vfx = Instantiate(deathVfx, transform.position + new Vector3(0,1.3f,0) + enemyAI.directionToPlayer, transform.rotation);
        vfx.transform.parent = transform;

        enemyAI.desiredMovement = 0;
        enemyAI.desiredRotation = 0;
        enemyAI.OnTankDead();

        outline.enabled = false;
        enemyAttack.OnTankDead();        
        enemyAttack.enabled = false;

        int area = enemyAI.enemyArea;
        if (enemyAI.enemyArea == 13)
            area = 7;
        if (enemyAI.enemyArea == 14)
        {
            enemyManager.chasingInArea14 = false;
            area = enemyAI.oldArea;
        }
        enemyManager.DeadEnemy(area);
        Invoke("DestroyTank", timeOfDeath);
    }

    private void DestroyTank()
    {
        Destroy(gameObject);
    }
}
