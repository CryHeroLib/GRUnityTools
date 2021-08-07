using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GRTools.GitPackageResolver
{
    internal class PackageMeta
    {
#if NETSTANDARD
        const RegexOptions k_RegOption = RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture;
#else
        const RegexOptions k_RegOption = RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.ExplicitCapture;
#endif

        private static readonly Regex s_PackageUrlReg =
            new Regex(
                @"^(git\+)?" +
                @"(?<repository>[^#?]*)" +
                @"(\?(?<query>[^#]*))?" +
                @"(#(?<revision>.*))?",
                k_RegOption);

        private static readonly Regex s_IsGitReg =
            new Regex(
                @"^(git\\+)?" +
                @"(git@|git://|http://|https://|ssh://)",
                k_RegOption);

        public string name { get; private set; }
        internal SemVersion version { get; private set; }
        public string revision { get; private set; }
        public string repository { get; private set; }
        public string path { get; private set; }
        public PackageMeta[] dependencies { get; private set; }
        public PackageMeta[] dependencyPackages { get; private set; }
        public string url { get; set; }

        private PackageMeta()
        {
            name = "";
            repository = "";
            revision = "";
            path = "";
            url = "";
            version = new SemVersion(0);
            dependencies = new PackageMeta [0];
            dependencyPackages = new PackageMeta [0];
        }

        public static PackageMeta FromPackageJson(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return null;
                }

                string dir = Path.GetDirectoryName(filePath);
                Dictionary<string, object> dict = Json.Deserialize(File.ReadAllText(filePath)) as Dictionary<string, object>;
                PackageMeta package = new PackageMeta() {repository = dir,};


                object obj;

                if (dict.TryGetValue("name", out obj))
                {
                    package.name = obj as string;
                }

                if (dict.TryGetValue("version", out obj))
                {
                    package.version = obj as string;
                }

                if (dict.TryGetValue("dependencies", out obj))
                {
                    package.dependencies = (obj as Dictionary<string, object>)
                        .Select(x => FromNameAndUrl(x.Key, (string) x.Value))
                        .ToArray();
                }
                else
                {
                    package.dependencies = new PackageMeta[0];
                }

                if (dict.TryGetValue("gitPackages", out obj))
                {
                    package.dependencyPackages = (obj as Dictionary<string, object>)
                        .Select(x => FromNameAndUrl(x.Key, (string) x.Value))
                        .ToArray();
                }
                else
                {
                    package.dependencyPackages = new PackageMeta[0];
                }

                return package;
            }
            catch
            {
                return null;
            }
        }

        public static PackageMeta FromPackageDir(string dir)
        {
            var package = FromPackageJson(dir + "/package.json");
            if (package == null) return null;

            package.SetVersion(package.revision);
            return package;
        }

        public static PackageMeta FromNameAndUrl(string name, string url)
        {
            PackageMeta package = new PackageMeta() {name = name};

            // Non git package.
            var isGit = s_IsGitReg.IsMatch(url);
            if (!isGit)
            {
                package.SetVersion(url);
                return package;
            }

            // No package url
            Match m = s_PackageUrlReg.Match(url);
            if (!m.Success) return package;

            package.repository = m.Groups["repository"].Value;
            package.revision = m.Groups["revision"].Value;
            package.url = url;

            // Get version from revision/branch/tag
            package.SetVersion(package.revision);

            package.ProcessUrlQuery(m.Groups["query"].Value);
            return package;
        }

        public string GetPackagePath(string clonePath)
        {
            return string.IsNullOrEmpty(path)
                ? clonePath
                : (clonePath + "/" + path).Replace("//", "/");
        }

        private void SetVersion(string ver)
        {
            SemVersion v;
            if (SemVersion.TryParse(ver, out v) && version < v)
                version = v;
        }

        private void ProcessUrlQuery(string urlQuery)
        {
            // Process url query.
            var queries = urlQuery.Split('&')
                .Select(q => q.Split('='))
                .Where(q => q.Length == 2)
                .ToDictionary(q => q[0], q => q[1]);

            // path query.
            string value;
            if (queries.TryGetValue("path", out value))
                path = value.Trim('/');

            // version query.
            if (queries.TryGetValue("version", out value))
            {
                SemVersion v;
                SemVersion.TryParse(value, out v);
                version = v;
            }
        }

        public IEnumerable<PackageMeta> GetAllDependencies()
        {
            return dependencyPackages.Concat(dependencies);
        }

        public string GetDirectoryName()
        {
            return string.Format("{0}@{1}", name, version);
        }

        public override string ToString()
        {
            return string.Format("{0}@{1} ({2}) [{3}] <{4}>", name, version, revision, path, url);
        }
    }
}
