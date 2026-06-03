using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace CompanionAgent.Core;

public sealed class AcSharedMemoryReader : IDisposable
{
    private const string PhysicsFile  = "Local\\acpmf_physics";
    private const string GraphicsFile = "Local\\acpmf_graphics";
    private const string StaticFile   = "Local\\acpmf_static";

    private MemoryMappedFile? _physicsMap, _graphicsMap, _staticMap;
    private MemoryMappedViewAccessor? _physicsView, _graphicsView, _staticView;

    public bool TryConnect()
    {
        try
        {
            _physicsMap   = MemoryMappedFile.OpenExisting(PhysicsFile);
            _graphicsMap  = MemoryMappedFile.OpenExisting(GraphicsFile);
            _staticMap    = MemoryMappedFile.OpenExisting(StaticFile);
            _physicsView  = _physicsMap.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            _graphicsView = _graphicsMap.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            _staticView   = _staticMap.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            return true;
        }
        catch (FileNotFoundException) { return false; }
        catch { return false; }
    }

    public AcPhysics?  ReadPhysics()  => TryRead<AcPhysics>(_physicsView);
    public AcGraphics? ReadGraphics() => TryRead<AcGraphics>(_graphicsView);
    public AcStatic?   ReadStatic()   => TryRead<AcStatic>(_staticView);

    // Read<T> does not initialize MarshalAs arrays inside structs.
    // Use Marshal.PtrToStructure via a pinned byte array instead.
    private static T? TryRead<T>(MemoryMappedViewAccessor? view) where T : struct
    {
        if (view is null) return null;
        try
        {
            var size  = Marshal.SizeOf<T>();
            var bytes = new byte[size];
            view.ReadArray(0, bytes, 0, size);
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try   { return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject()); }
            finally { handle.Free(); }
        }
        catch { return null; }
    }

    public void Dispose()
    {
        _physicsView?.Dispose();
        _graphicsView?.Dispose();
        _staticView?.Dispose();
        _physicsMap?.Dispose();
        _graphicsMap?.Dispose();
        _staticMap?.Dispose();
    }
}