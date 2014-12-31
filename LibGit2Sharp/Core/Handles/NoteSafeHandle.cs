namespace LibGit2Sharp.Core.Handles
{
    internal class NoteSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_note_free(handle);
            return true;
        }
    }
}
