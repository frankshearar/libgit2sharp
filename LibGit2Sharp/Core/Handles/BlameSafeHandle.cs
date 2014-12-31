namespace LibGit2Sharp.Core.Handles
{
    internal class BlameSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_blame_free(handle);
            return true;
        }
    }
}
