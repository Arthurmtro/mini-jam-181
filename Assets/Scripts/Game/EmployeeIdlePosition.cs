using UnityEngine;

namespace BunnyCoffee
{

    public class EmployeeIdlePosition : MonoBehaviour
    {
        [SerializeField] Transform employeeTransform;

        public Vector3 EmployeePosition => employeeTransform.position;

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
                Gizmos.color = IsBusy ? Color.red : Color.magenta;
                Gizmos.DrawSphere(EmployeePosition, 0.25f);
            }
        }
    }
}