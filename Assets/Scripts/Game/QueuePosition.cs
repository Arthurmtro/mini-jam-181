using UnityEditor;
using UnityEngine;

public class QueuePosition : MonoBehaviour
{
    [SerializeField] Transform customerTransform;
    public int Index;

    public Vector3 CustomerPosition => customerTransform.position;

    public bool IsBusy { get; private set; }

    public void Reserve()
    {
        IsBusy = true;
    }

    public void Free()
    {
        IsBusy = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Handles.Label(transform.position + 1.25f * Vector3.up, Index.ToString());
        if (customerTransform != null)
        {
            Gizmos.color = IsBusy ? Color.red : Color.yellow;
            Gizmos.DrawSphere(CustomerPosition, 1f);
        }
    }
}
