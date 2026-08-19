using ECommons.EzIpcManager;

namespace HuntTrainAssistant.Services;

public class VnavmeshIPC
{
    [EzIPC("Nav.IsReady")] public Func<bool> IsReady;
    [EzIPC("Query.Mesh.PointOnFloor")] public Func<Vector3, bool, float, Vector3?> PointOnFloor;
    [EzIPC("SimpleMove.PathfindAndMoveTo")] public Func<Vector3, bool, bool> PathfindAndMoveTo;

    private VnavmeshIPC()
    {
        EzIPC.Init(this, "vnavmesh", SafeWrapper.AnyException);
    }
}
