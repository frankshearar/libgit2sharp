using System;
using System.Runtime.InteropServices;

namespace LibGit2Sharp.Core.Handles
{
    [StructLayout(LayoutKind.Sequential)]
    internal class GitBuf : IDisposable
    {
        public IntPtr ptr;
        public UIntPtr asize;
        public UIntPtr size;

        public void Dispose()
        {
            Proxy.Std.git_buf_free(this);
        }
    }
}
