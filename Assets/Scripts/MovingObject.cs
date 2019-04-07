using System.Collections;
using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{
  public float moveTime = 0.1f;
  public LayerMask blockingLayer;
  private BoxCollider2D boxCollider;
  private Rigidbody2D rb2D;
  private float inverseMoveTime;
  // Use this for initialization
  protected virtual void Start()
  {
    boxCollider = GetComponent<BoxCollider2D>();
    rb2D = GetComponent<Rigidbody2D>();
    inverseMoveTime = 1f / moveTime;
  }

  protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
  {
    Vector2 start = transform.position;
    Vector2 end = start + new Vector2(xDir, yDir);
    boxCollider.enabled = false;
    hit = Physics2D.Linecast(start, end, blockingLayer);
    boxCollider.enabled = true;
    if (hit.transform == null)
    {
      StartCoroutine(SmoothMovement(end));
      return true;
    }
    return false;
  }
  //Co-routine for moving units from one space to next, takes a parameter end to specify where to move to.
  protected IEnumerator SmoothMovement(Vector3 end)
  {
    //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
    //Square magnitude is used instead of magnitude because it's computationally cheaper.
    float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

    //While that distance is greater than a very small amount (Epsilon, almost zero)
    //Stack exchange questions this use of float.Epsilon https://stackoverflow.com/questions/30216575/why-float-epsilon-and-not-zero
    while (sqrRemainingDistance > float.Epsilon)
    {
      //Find a new position proportionally closer to the end, based on the moveTime
      Vector3 newPostion = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);

      //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
      rb2D.MovePosition(newPostion);

      //Recalculate the remaining distance after moving.
      sqrRemainingDistance = (transform.position - end).sqrMagnitude;

      //Return and loop until sqrRemainingDistance is close enough to zero to end the function
      yield return null;
    }
  }

  protected virtual void AttemptMove<T>(int xDir, int yDir)
    where T : Component
  {
    RaycastHit2D hit;
    bool canMove = Move(xDir, yDir, out hit);
    if (hit.transform == null)
    {
      return;
    }
    T hitComponent = hit.transform.GetComponent<T>();
    //If canMove is false and hitComponent is not equal to null, meaning MovingObject is blocked and has hit something it can interact with.
    if (!canMove && hitComponent != null)
    {
      //Call the OnCantMove function and pass it hitComponent as a parameter.
      OnCantMove(hitComponent);
    }
  }

  protected abstract void OnCantMove<T>(T component)
    where T : Component;
}
