namespace LibGit2Sharp.Core.Handles
{
    internal class TreeBuilderSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_treebuilder_free(handle);
            return true;
        }
    }
}
