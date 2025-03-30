using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace BunnyCoffee
{
    public class ResourceManager : MonoBehaviour
    {
        [Header("Resources")]
        [SerializeField] ApplianceTypeCollection applianceTypes;
        [SerializeField] ProductCollection productTypes;
        [SerializeField] CustomerTypeCollection customerTypes;

        public ApplianceTypeCollection ApplianceTypes => applianceTypes;
        public ProductCollection ProductTypes => productTypes;
        public CustomerTypeCollection CustomerTypes => customerTypes;
    }
}