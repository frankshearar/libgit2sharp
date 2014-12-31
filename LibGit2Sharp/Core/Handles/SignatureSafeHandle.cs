namespace LibGit2Sharp.Core.Handles
{
    internal class SignatureSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_signature_free(handle);
            return true;
        }
    }
}
