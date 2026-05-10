using UnityEngine;

public class BabaJeeAI : MonoBehaviour
{
    public Transform targetSpot;
    public float walkSpeed = 2f;
    public Animator animator;
    public LayerMask groundLayer;   // Inspector mein isse 'Default' ya 'Ground' par set karein

    private bool shouldWalk = false;
    private bool reachedDestination = false;

    void Update()
    {
        if (shouldWalk && !reachedDestination)
        {
            // 1. Movement Direction
            Vector3 targetPos = new Vector3(targetSpot.position.x, transform.position.y, targetSpot.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);

            // 2. Rotation
            Vector3 direction = (targetPos - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            // 3. Ground Alignment (Isse Baba jee hawa mein nahi jayenge)
            StickToGround();

            // 4. Check Destination
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                               new Vector2(targetSpot.position.x, targetSpot.position.z)) < 0.2f)
            {
                StopWalking();
            }
        }
    }

    void StickToGround()
    {
        RaycastHit hit;
        // Baba jee ke thora upar se niche ki taraf ray phenkna
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out hit, 10f, groundLayer))
        {
            transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
        }
    }

    public void StartWalking()
    {
        if (reachedDestination) return;
        shouldWalk = true;
        if (animator != null) animator.SetBool("isWalking", true);
    }

    private void StopWalking()
    {
        shouldWalk = false;
        reachedDestination = true;
        if (animator != null) animator.SetBool("isWalking", false);

        // Final position adjustment
        RaycastHit hit;
        if (Physics.Raycast(targetSpot.position + Vector3.up * 2f, Vector3.down, out hit, 10f, groundLayer))
        {
            transform.position = hit.point;
        }
        transform.rotation = targetSpot.rotation;
    }
}