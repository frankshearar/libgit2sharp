namespace LibGit2Sharp.Core.Handles
{
    internal class PatchSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_patch_free(handle);
            return true;
        }
    }
}
