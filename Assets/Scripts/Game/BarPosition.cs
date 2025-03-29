using UnityEditor;
using UnityEngine;

namespace BunnyCoffee
{
    public class BarPosition : MonoBehaviour
    {
        [SerializeField] Transform employeeTransform;
        [SerializeField] Transform customerTransform;

        public Vector3 Employee => employeeTransform.position;
        public Vector3 Customer => customerTransform.position;

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
                Gizmos.DrawSphere(Employee, 1.5f);
            }

            if (customerTransform != null)
            {
                Gizmos.color = IsBusy ? Color.red : Color.yellow;
                Gizmos.DrawSphere(Customer, 1.5f);
            }
        }
    }
}