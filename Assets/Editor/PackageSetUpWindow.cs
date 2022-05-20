using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class PackageSetUpWindow : EditorWindow
    {
        private static readonly string DefaultFullPackageName = CreateFullPackageName("new-package");

        public string PackageName;
        public string PackageDisplayName;
        public string Namespace = "DELTation.NewPackage";
        public bool RepoNameSameAsPackageName = true;
        public string RepoName;
        public string PackageDescription;

        private void OnGUI()
        {
            PackageName = TextField("Package Name", PackageName);
            PackageDisplayName = TextField("Package Display Name", PackageDisplayName);
            Namespace = TextField("Namespace", Namespace, ns => ns.Replace(" ", "_"));
            RepoNameSameAsPackageName =
                EditorGUILayout.Toggle("Repo Name Same As Package Name", RepoNameSameAsPackageName);
            if (!RepoNameSameAsPackageName)
                RepoName = TextField("Repo Name", RepoName);

            GUILayout.Label("Package Description");
            PackageDescription = EditorGUILayout.TextArea(PackageDescription, GUILayout.Height(50));

            if (GUILayout.Button("Create Package"))
                CreatePackage();
        }

        private string TextField(string label, string text, [CanBeNull] Func<string, string> onChange = null)
        {
            EditorGUI.BeginChangeCheck();

            text = EditorGUILayout.TextField(label, text);

            if (!EditorGUI.EndChangeCheck()) return text;

            text = text.Trim();
            if (onChange != null)
                text = onChange(text);

            return text;
        }

        private void CreatePackage()
        {
            if (string.IsNullOrWhiteSpace(PackageName))
            {
                DisplayError("Package Name is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(PackageDisplayName))
            {
                DisplayError("Package Display Name is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(Namespace))
            {
                DisplayError("Namespace is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(GetRepoName()))
            {
                DisplayError("Repo Name is required");
                return;
            }

            if (string.IsNullOrWhiteSpace(PackageDescription))
            {
                DisplayError("Package Description is required");
                return;
            }

            var packageBootstrapSettings = Resources.Load<PackageBootstrapSettings>("Package Bootstrap Settings");
            if (packageBootstrapSettings == null)
            {
                DisplayError("Package Bootstrap Settings not found");
                return;
            }


            var unityVersion = Application.unityVersion;
            PatchPackageJson(packageBootstrapSettings.PackageJsonAsset, unityVersion);
            PatchAsmdef(packageBootstrapSettings.AsmdefAsset);
            PatchReadme(unityVersion);
            PatchProjectSettings();

            Directory.Move(
                PathInPackages(DefaultFullPackageName),
                PathInPackages(CreateFullPackageName(PackageName))
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success", "Package set up finished successfully", "OK");
        }

        private void PatchPackageJson(TextAsset packageJsonAsset, string unityVersion)
        {
            var packageJson = JsonUtility.FromJson<PackageJsonModel>(packageJsonAsset.text);
            packageJson.name = PackageName;
            packageJson.displayName = PackageDisplayName;
            packageJson.description = PackageDescription;
            packageJson.unity = string.Join(".", unityVersion.Split('.').Take(2));
            WriteTextToAsset(packageJsonAsset, JsonUtility.ToJson(packageJson));
        }

        private void PatchAsmdef(TextAsset asmdefAsset)
        {
            var asmdef = JsonUtility.FromJson<AsmdefJsonModel>(asmdefAsset.text);
            asmdef.rootNamespace = Namespace;
            asmdef.name = Namespace;
            WriteTextToAsset(asmdefAsset, JsonUtility.ToJson(asmdef));

            var asmdefPath = AssetDatabase.GetAssetPath(asmdefAsset);
            AssetDatabase.RenameAsset(asmdefPath, $"{Namespace}.asmdef");
        }

        private void PatchProjectSettings()
        {
            PlayerSettings.productName = PackageDisplayName;
        }

        private void PatchReadme(string unityVersion)
        {
            var readmePath = Path.Combine(Application.dataPath, "..", "README.md");
            var readmeText = File.ReadAllText(readmePath);
            readmeText = readmeText.Replace("{package-display-name}", PackageDisplayName);
            readmeText = readmeText.Replace("{repo-name}", GetRepoName());
            readmeText = readmeText.Replace("{unity-version}", unityVersion);
            readmeText = readmeText.Replace("{package-description}", PackageDescription);
            readmeText = readmeText.Replace("{package-name}", PackageName);

            File.WriteAllText(readmePath, readmeText);
        }

        private string GetRepoName() => RepoNameSameAsPackageName ? PackageName : RepoName;

        private void WriteTextToAsset(Object asset, string text)
        {
            File.WriteAllText(AssetDatabase.GetAssetPath(asset), text);
            EditorUtility.SetDirty(asset);
        }

        private static void DisplayError(string message)
        {
            EditorUtility.DisplayDialog("Error", message, "OK");
        }

        private static string CreateFullPackageName(string packageName) =>
            $"com.deltation.{packageName}";

        private static string PathInPackages(string path) =>
            Path.Combine(Application.dataPath, "..", "Packages", path);

        public static void Open()
        {
            CreateWindow<PackageSetUpWindow>().Show();
        }
    }
}