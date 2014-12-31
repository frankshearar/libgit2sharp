﻿namespace LibGit2Sharp.Core.Handles
{
    internal class ReferenceSafeHandle : SafeHandleBase
    {
        protected override bool ReleaseHandleImpl()
        {
            Proxy.Std.git_reference_free(handle);
            return true;
        }
    }
}
