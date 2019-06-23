using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    //是否在移动
    protected bool isMoving = false;
    //移动目标点
    private Vector3 targetPosition;
    //移动速度
    public float speed = 1.2f;
    private Animator animator;
    //是否正在攻击
    internal bool isAttacking = false;
    internal float attackTime = float.MinValue;
    //描述
    public string desc = "";
    internal void Start()
    {
        animator = GetComponent<Animator>();
    }
    /// <summary>
    /// 移动到某处
    /// </summary>
    /// <param name="pos">移动目标点</param>
    public void MoveTo(Vector3 pos)
    {
        targetPosition = pos;
        isMoving = true;
        animator.SetBool("isMoving", isMoving);
    }
    /// <summary>
    /// 更新移动
    /// </summary>
    public void MoveUpdate()
    {
        if (!isMoving) return;
        Vector3 pos = transform.position;
        transform.LookAt(targetPosition);
        transform.position = Vector3.MoveTowards(pos, targetPosition, speed * Time.deltaTime);
        //if (Vector3.Distance(pos, targetPosition) < 0.05f)
        if ((pos - targetPosition).magnitude < 0.05f)
        {
            isMoving = false;
            animator.SetBool("isMoving", isMoving);
        }
    }
    //攻击动作
    public void Attack()
    {
        isAttacking = true;
        attackTime = Time.time;
        animator.SetBool("isAttacking", true);
    }
    //攻击Update
    public void AttackUpdate()
    {
        if (!isAttacking) return;
        if (Time.time - attackTime < 1.2f) return;
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }
    internal void Update()
    {
        MoveUpdate();
        AttackUpdate();
    }
}
