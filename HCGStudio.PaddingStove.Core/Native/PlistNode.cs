using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace HCGStudio.PaddingStove.Core.Native;

[MustDisposeResource]
internal readonly partial struct PlistNode(nint node) : IDisposable
{
    public void Dispose()
    {
        PlistFree(node);
    }

    public string StringValue
    {
        get
        {
            PlistGetStringVal(node, out var value);
            return value;
        }
    }

    [LibraryImport("libplist-2.0", EntryPoint = "plist_free")]
    private static unsafe partial void PlistFree(nint node);

    [LibraryImport(
        "libplist-2.0",
        EntryPoint = "plist_get_string_val",
        StringMarshalling = StringMarshalling.Utf8)]
    private static unsafe partial void PlistGetStringVal(nint node, out string value);
}