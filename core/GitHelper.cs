using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace BIDSHelper.Core
{
    //from https://github.com/deadlydog/VS.DiffAllFiles/blob/master/VS.DiffAllFiles/GitHelper.cs with some additions by the BI Developer Extensions team
    public static class GitHelper
    {
        // Had to manually sign the LibGit2Sharp.dll myself as per http://www.codeproject.com/Tips/341645/Referenced-assembly-does-not-have-a-strong-name

        /// <summary>
        /// Returns true if the given path is in a Git repository; false if not.
        /// </summary>
        /// <param name="path">The full path to the file or directory.</param>
        /// <returns></returns>
        public static bool IsPathInGitRepository(string path)
        {
            return GetGitRepositoryPath(path) != null;
        }

        /// <summary>
        /// Gets the git repository path based on the path of the given file or directory inside of the repository.
        /// <para>Returns null if the given path is not inside of a Git repository.</para>
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static string GetGitRepositoryPath(string path)
        {
            // If an invalid path was given, just return null.
            if (!Directory.Exists(path) && !File.Exists(path))
                return null;

            // Return the path of the Git repository, if this path is actually within one.
            // https://github.com/libgit2/libgit2sharp/issues/818#issuecomment-54760613
            return Repository.Discover(Path.GetFullPath(path));
        }

        public static Repository GetGitRepository(string repositoryPath)
        {
            try
            {
                return new Repository(repositoryPath);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("There was a problem initializing a Git Repository instance from the path '{0}'. There may be a problem with one of your git .config files, such as invalid syntax or not properly escaping a file path.", repositoryPath), ex);
            }
        }

        /// <summary>
        /// Copies the specific version of a file to the specified path.
        /// <para>Returns true if the version was found and the file copy performed; false if not.</para>
        /// </summary>
        /// <param name="gitRepositoryPath">The git repository path.</param>
        /// <param name="filePathInRepository">The full file path of the file in the repository.</param>
        /// <param name="filePathToCopyFileTo">The file path to copy the specific version of the file to.</param>
        /// <param name="commitSha">The commit containing the file version to obtain. If this is null, the most recent commit of the file will be used. If "Staged" the staged version of the file will used, and if there are no Staged changed for the file, then the most recent commit of the file will be used.</param>
        public static bool GetSpecificVersionOfFile(string gitRepositoryPath, string filePathInRepository, string filePathToCopyFileTo, string commitSha = null)
        {
            // Get the path to the Git repository from the file path.
            var repositoryPath = GetGitRepositoryPath(gitRepositoryPath);
            if (repositoryPath == null)
                return false;

            if (filePathInRepository.StartsWith("/")) filePathInRepository = filePathInRepository.Substring(1);
            if (filePathInRepository.StartsWith("$/")) filePathInRepository = filePathInRepository.Substring(2);

            // Remove the Git repository path from the file path to get the file's path relative to the Git repository.
            var repoRootDirectory = repositoryPath.Replace(@".git\", string.Empty);
            var relativeFilePath = filePathInRepository.Replace(repoRootDirectory, string.Empty);

            // An update to LibGit2Sharp removed support for backslashes, so file paths must use forward slashes: https://github.com/libgit2/libgit2sharp/issues/1075
            relativeFilePath = ReplaceBackslashesWithForwardSlashes(relativeFilePath);

            // Connect to the Git repository.
            using (var repository = GetGitRepository(repositoryPath))
            {
                Blob blob = null;
                if (string.Equals(commitSha, "Staged", StringComparison.OrdinalIgnoreCase))
                {
                    // Get the Staged version of the file if it exists. If it doesn't, the latest (i.e. HEAD) version will be retrieved instead.
                    var blobIndex = repository.Index.FirstOrDefault(b => b.Path.Equals(relativeFilePath));
                    if (blobIndex == null) return false;
                    blob = repository.Lookup<Blob>(blobIndex.Id);
                }
                else
                {
                    // Find the specific commit containing the version of the file to obtain.
                    var commit = (commitSha == null) ? GetPreviousCommitOfFile(repository, relativeFilePath) : repository.Lookup<Commit>(commitSha);
                    if (commit == null) return false;

                    // Get the file from the commit.
                    var treeEntry = commit[relativeFilePath];
                    if (treeEntry == null) return false;
                    blob = treeEntry.Target as Blob;
                }
                if (blob == null) return false;

                // Write a copy of the file to the specified file path.
                File.WriteAllText(filePathToCopyFileTo, blob.GetContentText());
            }

            return true;
        }

        /// <summary>
        /// Copies the previous version of a file to the specified path.
        /// <para>Returns true if the version was found and the file copy performed; false if not.</para>
        /// </summary>
        /// <param name="gitRepositoryPath">The git repository path.</param>
        /// <param name="filePathInRepository">The full file path of the file in the repository.</param>
        /// <param name="filePathToCopyFileTo">The file path to copy the specific version of the file to.</param>
        /// <param name="commitSha">The file as it existed previous to this commit will be obtained. If this is null, the most recent commit of the file will be used.</param>
        public static bool GetPreviousVersionOfFile(string gitRepositoryPath, string filePathInRepository, string previousVersionsFilePathInRepository, string filePathToCopyFileTo, string commitSha)
        {
            // Get the path to the Git repository from the file path.
            var repositoryPath = GetGitRepositoryPath(gitRepositoryPath);
            if (repositoryPath == null)
                return false;

            // Remove the Git repository path from the file path to get the file's path relative to the Git repository.
            var repoRootDirectory = repositoryPath.Replace(@".git\", string.Empty);
            var relativeFilePath = filePathInRepository.Replace(repoRootDirectory, string.Empty);
            var previousVersionsRelativeFilePath = previousVersionsFilePathInRepository.Replace(repoRootDirectory, string.Empty);

            // An update to LibGit2Sharp removed support for backslashes, so file paths must use forward slashes: https://github.com/libgit2/libgit2sharp/issues/1075
            relativeFilePath = ReplaceBackslashesWithForwardSlashes(relativeFilePath);
            previousVersionsRelativeFilePath = ReplaceBackslashesWithForwardSlashes(previousVersionsRelativeFilePath);

            // Connect to the Git repository.
            using (var repository = GetGitRepository(repositoryPath))
            {
                // Find the specific commit containing the version of the file to obtain.
                var commit = GetPreviousCommitOfFile(repository, relativeFilePath, commitSha);
                if (commit == null) return false;

                // Get the file from the commit.
                var treeEntry = commit[previousVersionsRelativeFilePath];
                if (treeEntry == null) return false;

                var blob = treeEntry.Target as Blob;
                if (blob == null) return false;

                // Write a copy of the file to the specified file path.
                File.WriteAllText(filePathToCopyFileTo, blob.GetContentText());
            }

            return true;
        }

        /// <summary>
        /// Gets the previous commit of the file.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="filePathRelativeToRepository">The file path relative to repository.</param>
        /// <param name="commitSha">The commit sha to start the search for the previous version from. If null, the latest commit of the file will be returned.</param>
        /// <returns></returns>
        private static Commit GetPreviousCommitOfFile(Repository repository, string filePathRelativeToRepository, string commitSha = null)
        {
            bool versionMatchesGivenVersion = false;
            // Need to use Topological sort to avoid "Given key not present in dictionary" error: https://github.com/libgit2/libgit2sharp/issues/1520
            var fileHistory = repository.Commits.QueryBy(filePathRelativeToRepository, new FollowFilter() { SortBy = CommitSortStrategies.Topological });
            //new CommitFilter { SortBy = CommitSortStrategies.Topological });
            foreach (var version in fileHistory)
            {
                // If they want the latest commit or we have found the "previous" commit that they were after, return it.
                if (string.IsNullOrWhiteSpace(commitSha) || versionMatchesGivenVersion)
                    return version.Commit;

                // If this commit version matches the version specified, we want to return the next commit in the list, as it will be the previous commit.
                if (version.Commit.Sha.Equals(commitSha))
                    versionMatchesGivenVersion = true;
            }

            return null;
        }

        public static string GetRelativePath(string fileFullPath)
        {
            if (!IsPathInGitRepository(fileFullPath)) return null;
            string sRepositoryPath = GetGitRepositoryPath(fileFullPath);
            sRepositoryPath = sRepositoryPath.Replace(@".git\", string.Empty);
            string sRelativePath = fileFullPath.Replace(sRepositoryPath, string.Empty);
            sRelativePath = ReplaceBackslashesWithForwardSlashes(sRelativePath);
            if (sRelativePath.StartsWith("/")) sRelativePath = sRelativePath.Substring(1);
            return sRelativePath;
        }

        public static string[] GetHistoryOfFile(string fileFullPath)
        {
            if (!IsPathInGitRepository(fileFullPath)) return new string[] { };
            string sRelativePath = GetRelativePath(fileFullPath);
            string sRepositoryPath = GetGitRepositoryPath(fileFullPath);
            var repository = GetGitRepository(sRepositoryPath);
            return GetHistoryOfFile(repository, sRelativePath);
        }

        public static string[] GetHistoryOfFile(Repository repository, string filePathRelativeToRepository)
        {
            if (filePathRelativeToRepository.StartsWith("/")) filePathRelativeToRepository = filePathRelativeToRepository.Substring(1);
            if (filePathRelativeToRepository.StartsWith("$/")) filePathRelativeToRepository = filePathRelativeToRepository.Substring(2);
            List<string> list = new List<string>();
            // Need to use Topological sort to avoid "Given key not present in dictionary" error: https://github.com/libgit2/libgit2sharp/issues/1520
            var fileHistory = repository.Commits.QueryBy(filePathRelativeToRepository, new FollowFilter() { SortBy = CommitSortStrategies.Topological });
            //new CommitFilter { SortBy = CommitSortStrategies.Topological });
            foreach (var version in fileHistory)
            {
                list.Add(version.Commit.Id.Sha + " - " + version.Commit.Author.When.ToString() + " - " + version.Commit.Author.Name);
            }
            return list.ToArray();
        }

        public static string[] GetHistoryOfFile(string sRepositoryPath, string filePathRelativeToRepository)
        {
            List<string> list = new List<string>();
            var repository = GetGitRepository(sRepositoryPath);
            return GetHistoryOfFile(repository, filePathRelativeToRepository);
        }

        /// <summary>
        /// Get if a file at the given path is Staged or not.
        /// </summary>
        /// <param name="gitRepositoryPath">The git repository path.</param>
        /// <param name="filePathInRepository">The file path in repository.</param>
        public static bool IsFileStaged(string gitRepositoryPath, string filePathInRepository)
        {
            // Get the path to the Git repository from the file path.
            var repositoryPath = GetGitRepositoryPath(gitRepositoryPath);
            if (repositoryPath == null)
                return false;

            // Remove the Git repository path from the file path to get the file's path relative to the Git repository.
            var repoRootDirectory = repositoryPath.Replace(@".git\", string.Empty);
            var relativeFilePath = filePathInRepository.Replace(repoRootDirectory, string.Empty);

            // An update to LibGit2Sharp removed support for backslashes, so file paths must use forward slashes: https://github.com/libgit2/libgit2sharp/issues/1075
            relativeFilePath = ReplaceBackslashesWithForwardSlashes(relativeFilePath);

            // Connect to the Git repository.
            using (var repository = GetGitRepository(repositoryPath))
            {
                var stagedFile = repository.RetrieveStatus().Staged.FirstOrDefault(f => string.Equals(f.FilePath, relativeFilePath, StringComparison.OrdinalIgnoreCase));
                return stagedFile != null;
            }
        }

        /// <summary>
        /// Gets the Git configuration entries for the given repository.
        /// </summary>
        /// <param name="gitRepositoryPath">The Git repository path, or the path to a file within the Git repository.</param>
        public static List<ConfigurationEntry<string>> GetGitConfigurationEntries(string gitRepositoryPath)
        {
            // Get the path to the Git repository from the file path.
            var repositoryPath = GetGitRepositoryPath(gitRepositoryPath);
            if (repositoryPath == null)
                return new List<ConfigurationEntry<string>>();

            // Connect to the Git repository.
            using (var repository = GetGitRepository(repositoryPath))
            {
                // Return the configuration information.
                return repository.Config.ToList();
            }
        }

        private static string ReplaceBackslashesWithForwardSlashes(string path) => path.Replace('\\', '/');
    }
}
