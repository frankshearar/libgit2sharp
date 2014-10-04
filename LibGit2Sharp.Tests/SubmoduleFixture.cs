﻿using System;
using System.IO;
using System.Linq;
using LibGit2Sharp.Tests.TestHelpers;
using Xunit;
using Xunit.Extensions;

namespace LibGit2Sharp.Tests
{
    public class SubmoduleFixture : BaseFixture
    {
        [Fact]
        public void RetrievingSubmoduleForNormalDirectoryReturnsNull()
        {
            var path = CloneSubmoduleTestRepo();
            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules["just_a_dir"];
                Assert.Null(submodule);
            }
        }

        [Theory]
        [InlineData("sm_added_and_uncommited", SubmoduleStatus.InConfig | SubmoduleStatus.InIndex | SubmoduleStatus.InWorkDir | SubmoduleStatus.IndexAdded)]
        [InlineData("sm_changed_file", SubmoduleStatus.InConfig | SubmoduleStatus.InHead | SubmoduleStatus.InIndex | SubmoduleStatus.InWorkDir | SubmoduleStatus.WorkDirFilesModified)]
        [InlineData("sm_changed_head", SubmoduleStatus.InConfig | SubmoduleStatus.InHead | SubmoduleStatus.InIndex | SubmoduleStatus.InWorkDir | SubmoduleStatus.WorkDirModified)]
        [InlineData("sm_changed_index", SubmoduleStatus.InConfig | SubmoduleStatus.InHead | SubmoduleStatus.InIndex | SubmoduleStatus.InWorkDir | SubmoduleStatus.WorkDirFilesIndexDirty)]
        [InlineData("sm_changed_untracked_file", SubmoduleStatus.InConfig | SubmoduleStatus.InHead | SubmoduleStatus.InIndex | SubmoduleStatus.InWorkDir | SubmoduleStatus.WorkDirFilesUntracked)]
        [InlineData("sm_gitmodules_only", SubmoduleStatus.InConfig)]
        [InlineData("sm_missing_commits", SubmoduleStatus.InConfig | SubmoduleStatus.InHead | SubmoduleStatus.InIndex | SubmoduleStatus.InWorkDir | SubmoduleStatus.WorkDirModified)]
        [InlineData("sm_unchanged", SubmoduleStatus.InConfig | SubmoduleStatus.InHead | SubmoduleStatus.InIndex | SubmoduleStatus.InWorkDir)]
        public void CanRetrieveTheStatusOfASubmodule(string name, SubmoduleStatus expectedStatus)
        {
            var path = CloneSubmoduleTestRepo();
            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules[name];
                Assert.NotNull(submodule);
                Assert.Equal(name, submodule.Name);
                Assert.Equal(name, submodule.Path);

                var status = submodule.RetrieveStatus();
                Assert.Equal(expectedStatus, status);
            }
        }

        [Theory]
        [InlineData("sm_added_and_uncommited", null, "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0")]
        [InlineData("sm_changed_file", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0")]
        [InlineData("sm_changed_head", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0", "3d9386c507f6b093471a3e324085657a3c2b4247")]
        [InlineData("sm_changed_index", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0")]
        [InlineData("sm_changed_untracked_file", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0")]
        [InlineData("sm_gitmodules_only", null, null, null)]
        [InlineData("sm_missing_commits", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0", "5e4963595a9774b90524d35a807169049de8ccad")]
        [InlineData("sm_unchanged", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0", "480095882d281ed676fe5b863569520e54a7d5c0")]
        public void CanRetrieveTheCommitIdsOfASubmodule(string name, string headId, string indexId, string workDirId)
        {
            var path = CloneSubmoduleTestRepo();
            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules[name];
                Assert.NotNull(submodule);
                AssertBelongsToARepository(repo, submodule);
                Assert.Equal(name, submodule.Name);

                Assert.Equal((ObjectId)headId, submodule.HeadCommitId);
                Assert.Equal((ObjectId)indexId, submodule.IndexCommitId);
                Assert.Equal((ObjectId)workDirId, submodule.WorkDirCommitId);

                AssertEntryId((ObjectId)headId, repo.Head[name], c => c.Target.Id);
                AssertEntryId((ObjectId)indexId, repo.Index[name], i => i.Id);
            }
        }

        private static void AssertEntryId<T>(ObjectId expected, T entry, Func<T, ObjectId> selector)
        {
            Assert.Equal(expected, ReferenceEquals(entry, null) ? null : selector(entry));
        }

        [Fact]
        public void CanEnumerateRepositorySubmodules()
        {
            var expectedSubmodules = new[]
            {
                "sm_added_and_uncommited",
                "sm_changed_file",
                "sm_changed_head",
                "sm_changed_index",
                "sm_changed_untracked_file",
                "sm_gitmodules_only",
                "sm_missing_commits",
                "sm_unchanged",
            };

            var path = CloneSubmoduleTestRepo();
            using (var repo = new Repository(path))
            {
                var submodules = repo.Submodules.OrderBy(s => s.Name, StringComparer.Ordinal);

                Assert.Equal(expectedSubmodules, submodules.Select(s => s.Name).ToArray());
                Assert.Equal(expectedSubmodules, submodules.Select(s => s.Path).ToArray());
                Assert.Equal(Enumerable.Repeat("../submodule_target_wd", expectedSubmodules.Length).ToArray(),
                             submodules.Select(s => s.Url).ToArray());
            }
        }

        [Theory]
        [InlineData("sm_changed_head", false)]
        [InlineData("sm_changed_head", true)]
        public void CanStageChangeInSubmoduleViaIndexStage(string submodulePath, bool appendPathSeparator)
        {
            submodulePath += appendPathSeparator ? Path.DirectorySeparatorChar : default(char?);

            var path = CloneSubmoduleTestRepo();
            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules[submodulePath];
                Assert.NotNull(submodule);

                var statusBefore = submodule.RetrieveStatus();
                Assert.Equal(SubmoduleStatus.WorkDirModified, statusBefore & SubmoduleStatus.WorkDirModified);

                repo.Index.Stage(submodulePath);

                var statusAfter = submodule.RetrieveStatus();
                Assert.Equal(SubmoduleStatus.IndexModified, statusAfter & SubmoduleStatus.IndexModified);
            }
        }

        [Theory]
        [InlineData("sm_changed_head", false)]
        [InlineData("sm_changed_head", true)]
        public void CanStageChangeInSubmoduleViaIndexStageWithOtherPaths(string submodulePath, bool appendPathSeparator)
        {
            submodulePath += appendPathSeparator ? Path.DirectorySeparatorChar : default(char?);

            var path = CloneSubmoduleTestRepo();
            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules[submodulePath];
                Assert.NotNull(submodule);

                var statusBefore = submodule.RetrieveStatus();
                Assert.Equal(SubmoduleStatus.WorkDirModified, statusBefore & SubmoduleStatus.WorkDirModified);

                Touch(repo.Info.WorkingDirectory, "new-file.txt");

                repo.Index.Stage(new[] { "new-file.txt", submodulePath, "does-not-exist.txt" });

                var statusAfter = submodule.RetrieveStatus();
                Assert.Equal(SubmoduleStatus.IndexModified, statusAfter & SubmoduleStatus.IndexModified);
            }
        }

        [Fact]
        public void CanInitSubmodule()
        {
            var path = CloneSubmoduleSmallTestRepo();
            string submoduleName = "submodule_target_wd";

            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules[submoduleName];

                Assert.NotNull(submodule);
                Assert.True((submodule.RetrieveStatus() & SubmoduleStatus.WorkDirUninitialized) == SubmoduleStatus.WorkDirUninitialized);

                var configEntryBeforeInit = repo.Config.Get<string>(string.Format("submodule.{0}.url", submoduleName));
                Assert.Null(configEntryBeforeInit);

                submodule.Init(false);

                var configEntryAfterInit = repo.Config.Get<string>(string.Format("submodule.{0}.url", submoduleName));
                Assert.NotNull(configEntryAfterInit);

                // TODO: The relative path should probably be expanded in libgit2
                // as part of init...
                Assert.Equal("../submodule_target_wd", configEntryAfterInit.Value);
            }
        }

        [Fact]
        public void UpdatingUninitializedSubmoduleThrows()
        {
            var path = CloneSubmoduleSmallTestRepo();
            string submoduleName = "submodule_target_wd";

            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules[submoduleName];

                Assert.NotNull(submodule);
                Assert.True((submodule.RetrieveStatus() & SubmoduleStatus.WorkDirUninitialized) == SubmoduleStatus.WorkDirUninitialized);

                Assert.Throws<UninitializedSubmoduleException>(() => submodule.Update(false));
            }
        }

        [Fact]
        public void CanUpdateSubmodule()
        {
            var path = CloneSubmoduleSmallTestRepo();
            string submoduleName = "submodule_target_wd";

            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules[submoduleName];

                Assert.NotNull(submodule);
                Assert.True((submodule.RetrieveStatus() & SubmoduleStatus.WorkDirUninitialized) == SubmoduleStatus.WorkDirUninitialized);

                submodule.Init(false);

                // Manually expand the relative submodule url here - lg2 complains that "../submodule_target_wd" is an unsupported URL protocol.
                // Need to investigate further...
                string submoduleUrl = repo.Config.Get<string>(string.Format("submodule.{0}.url", submoduleName), ConfigurationLevel.Local).Value;
                repo.Config.Set<string>(string.Format("submodule.{0}.url", submoduleName), Path.Combine(path, submoduleUrl));

                submodule.Update(false);

                Assert.True((submodule.RetrieveStatus() & SubmoduleStatus.InWorkDir) == SubmoduleStatus.InWorkDir);
                Assert.Equal((ObjectId)"480095882d281ed676fe5b863569520e54a7d5c0", submodule.HeadCommitId);
                Assert.Equal((ObjectId)"480095882d281ed676fe5b863569520e54a7d5c0", submodule.IndexCommitId);
                Assert.Equal((ObjectId)"480095882d281ed676fe5b863569520e54a7d5c0", submodule.WorkDirCommitId);
            }
        }

        [Fact(Skip = "Skipping due issue with relative local urls.")]
        public void CanUpdateUnitializedSubmodule()
        {
            var path = CloneSubmoduleSmallTestRepo();
            string submoduleName = "submodule_target_wd";

            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules[submoduleName];

                Assert.NotNull(submodule);
                Assert.True((submodule.RetrieveStatus() & SubmoduleStatus.WorkDirUninitialized) == SubmoduleStatus.WorkDirUninitialized);

                submodule.Update(true);

                Assert.True((submodule.RetrieveStatus() & SubmoduleStatus.InWorkDir) == SubmoduleStatus.InWorkDir);
                Assert.Equal((ObjectId)"480095882d281ed676fe5b863569520e54a7d5c0", submodule.HeadCommitId);
                Assert.Equal((ObjectId)"480095882d281ed676fe5b863569520e54a7d5c0", submodule.IndexCommitId);
                Assert.Equal((ObjectId)"480095882d281ed676fe5b863569520e54a7d5c0", submodule.WorkDirCommitId);
            }
        }

        [Fact]
        public void CanUpdateSubmoduleAfterCheckout()
        {
            var path = CloneSubmoduleSmallTestRepo();
            string submoduleName = "submodule_target_wd";

            using (var repo = new Repository(path))
            {
                var submodule = repo.Submodules[submoduleName];

                Assert.NotNull(submodule);
                Assert.True((submodule.RetrieveStatus() & SubmoduleStatus.WorkDirUninitialized) == SubmoduleStatus.WorkDirUninitialized);

                submodule.Init(false);

                // Manually expand the relative submodule url here - lg2 complains that "../submodule_target_wd" is an unsupported URL protocol.
                // Need to investigate further...
                string submoduleUrl = repo.Config.Get<string>(string.Format("submodule.{0}.url", submoduleName), ConfigurationLevel.Local).Value;
                repo.Config.Set<string>(string.Format("submodule.{0}.url", submoduleName), Path.Combine(path, submoduleUrl));

                submodule.Update(false);

                Assert.True((submodule.RetrieveStatus() & SubmoduleStatus.InWorkDir) == SubmoduleStatus.InWorkDir);

                repo.Checkout("alternate");
                Assert.True((submodule.RetrieveStatus() & SubmoduleStatus.WorkDirModified) == SubmoduleStatus.WorkDirModified);

                submodule = repo.Submodules[submoduleName];

                Assert.Equal((ObjectId)"5e4963595a9774b90524d35a807169049de8ccad", submodule.HeadCommitId);
                Assert.Equal((ObjectId)"5e4963595a9774b90524d35a807169049de8ccad", submodule.IndexCommitId);
                Assert.Equal((ObjectId)"480095882d281ed676fe5b863569520e54a7d5c0", submodule.WorkDirCommitId);

                submodule.Update(false);
                submodule = repo.Submodules[submoduleName];

                Assert.Equal((ObjectId)"5e4963595a9774b90524d35a807169049de8ccad", submodule.HeadCommitId);
                Assert.Equal((ObjectId)"5e4963595a9774b90524d35a807169049de8ccad", submodule.IndexCommitId);
                Assert.Equal((ObjectId)"5e4963595a9774b90524d35a807169049de8ccad", submodule.WorkDirCommitId);
            }
        }
    }
}
