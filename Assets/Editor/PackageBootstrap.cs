using UnityEditor;

namespace Editor
{
    public static class PackageBootstrap
    {
        [MenuItem("Package Template/Set Up Package")]
        public static void SetUpPackage()
        {
            PackageSetUpWindow.Open();
        }
    }
}