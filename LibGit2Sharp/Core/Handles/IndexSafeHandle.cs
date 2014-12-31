namespace LibGit2Sharp.Core.Handles
{
    internal class IndexSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_index_free(handle);
            return true;
        }
    }
}
