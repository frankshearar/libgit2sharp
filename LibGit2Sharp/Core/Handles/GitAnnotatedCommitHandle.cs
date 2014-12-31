using System.Runtime.InteropServices;

namespace LibGit2Sharp.Core.Handles
{
    internal class GitAnnotatedCommitHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_annotated_commit_free(handle);
            return true;
        }
    }
}
