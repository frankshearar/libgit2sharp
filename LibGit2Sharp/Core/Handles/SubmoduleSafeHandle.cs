namespace LibGit2Sharp.Core.Handles
{
    internal class SubmoduleSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_submodule_free(handle);
            return true;
        }
    }
}
