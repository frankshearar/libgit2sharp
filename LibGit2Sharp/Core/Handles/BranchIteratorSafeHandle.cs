namespace LibGit2Sharp.Core.Handles
{
    internal class BranchIteratorSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_branch_iterator_free(handle);
            return true;
        }
    }
}
