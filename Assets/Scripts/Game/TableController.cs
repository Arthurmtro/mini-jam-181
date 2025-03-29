using System;
using UnityEngine;

namespace BunnyCoffee
{
    public class TableController : MonoBehaviour
    {
        [Header("Position")]
        public Transform CustomerPosition;

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
            Gizmos.DrawSphere(CustomerPosition.position, 0.5f);
        }
    }
}