using System;
using UnityEngine;

namespace BunnyCoffee
{
    public enum TableDirection
    {
        Left,
        Right
    }

    public class TableController : MonoBehaviour
    {
        [Header("Position")]
        public Transform CustomerPosition;
        public TableDirection Direction;

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
            if (CustomerPosition == null)
            {
                return;
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, 2f * Vector3.one);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(CustomerPosition.position, 1.5f);

            if (Direction == TableDirection.Left)
            {
                Gizmos.DrawLine(CustomerPosition.position, CustomerPosition.position + 2f * Vector3.left);
            }
            if (Direction == TableDirection.Right)
            {
                Gizmos.DrawLine(CustomerPosition.position, CustomerPosition.position + 2f * Vector3.right);
            }
        }
    }
}