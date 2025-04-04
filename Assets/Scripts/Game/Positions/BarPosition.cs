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
            if (employeeTransform != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(EmployeePosition, 0.25f);
            }

            if (customerTransform != null)
            {
                Gizmos.color = IsBusy ? Color.red : Color.yellow;
                Gizmos.DrawSphere(CustomerPosition, 0.25f);
            }
        }
    }
}