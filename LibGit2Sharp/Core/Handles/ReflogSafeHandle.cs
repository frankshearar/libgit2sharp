namespace LibGit2Sharp.Core.Handles
{
    internal class ReflogSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_reflog_free(handle);
            return true;
        }
    }
}
