using System.IO.MemoryMappedFiles;

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

    public AcPhysics?  ReadPhysics()
    {
        if (_physicsView is null) return null;
        try { _physicsView.Read(0, out AcPhysics r); return r; }
        catch { return null; }
    }

    public AcGraphics? ReadGraphics()
    {
        if (_graphicsView is null) return null;
        try { _graphicsView.Read(0, out AcGraphics r); return r; }
        catch { return null; }
    }

    public AcStatic? ReadStatic()
    {
        if (_staticView is null) return null;
        try { _staticView.Read(0, out AcStatic r); return r; }
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