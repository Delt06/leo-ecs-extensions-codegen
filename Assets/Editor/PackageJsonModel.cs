using System;

// ReSharper disable InconsistentNaming

namespace Editor
{
    [Serializable]
    public class PackageJsonModel
    {
        public string name;
        public string version;
        public string displayName;
        public string description;
        public string unity;
        public string[] keywords;
        public AuthorJsonModel author;
    }
}