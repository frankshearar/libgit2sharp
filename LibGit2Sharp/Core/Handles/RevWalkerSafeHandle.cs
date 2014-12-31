namespace LibGit2Sharp.Core.Handles
{
    internal class RevWalkerSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_revwalk_free(handle);
            return true;
        }
    }
}
