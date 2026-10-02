using System.Collections;
using UnityEngine;


public class BoxColliderClickHandler : MonoBehaviour
{
    // Example operation: Set a target GameObject active
    public GameObject targetObject;
    public GameObject targetObject1;
    public GameObject targetObject2;
    public GameObject targetObject3;
    public GameObject targetObject4;
    public GameObject targetObject5;
    public GameObject targetObject6;
    public GameObject targetObject7; // Assign the object to activate in the Inspector
    
    public GameObject targetObject8; // Assign the object to activate in the Inspector
    
    public GameObject targetObject9; // Assign the object to activate in the Inspector
    
    public GameObject targetObject10; // Assign the object to activate in the Inspector
    
    public GameObject targetObject11;
    
    public GameObject targetObject12; // Assign the object to activate in the Inspector

    public GameObject targetObject13; // Assign the object to activate in the Inspector

    public GameObject Victory;

    [SerializeField] private float delay = 0.5f;



    private IEnumerator OnMouseDown()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {

            if (targetObject != null)
            {
                targetObject.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject1.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject2.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject3.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject4.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject5.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject6.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject7.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject8.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject9.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject10.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject11.SetActive(true);
                yield return new WaitForSeconds(delay);
                targetObject12.SetActive(true);

                yield return new WaitForSeconds(2f);
                targetObject13.SetActive(true);
            

            }
        }
    }
}

