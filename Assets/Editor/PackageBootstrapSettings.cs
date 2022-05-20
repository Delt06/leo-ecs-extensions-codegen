using UnityEngine;

namespace Editor
{
    [CreateAssetMenu]
    public class PackageBootstrapSettings : ScriptableObject
    {
        [SerializeField] private TextAsset _packageJsonAsset;
        [SerializeField] private TextAsset _asmdefAsset;

        public TextAsset PackageJsonAsset => _packageJsonAsset;
        public TextAsset AsmdefAsset => _asmdefAsset;
    }
}