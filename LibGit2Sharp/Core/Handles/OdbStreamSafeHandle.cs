namespace LibGit2Sharp.Core.Handles
{
    internal class OdbStreamSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_odb_stream_free(handle);
            return true;
        }
    }
}
