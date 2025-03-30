using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{
    public class BarPosition : MonoBehaviour
    {
        [SerializeField] Transform employeeTransform;
        [SerializeField] Transform customerTransform;

        public Vector3 EmployeePosition => employeeTransform.position;
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
            Gizmos.DrawWireCube(transform.position, 2f * Vector3.one);
            if (employeeTransform != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(EmployeePosition, 1f);
            }

            if (customerTransform != null)
            {
                Gizmos.color = IsBusy ? Color.red : Color.yellow;
                Gizmos.DrawSphere(CustomerPosition, 1f);
            }
        }
    }
}