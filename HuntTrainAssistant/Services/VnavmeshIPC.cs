using ECommons.EzIpcManager;

namespace HuntTrainAssistant.Services;

public class VnavmeshIPC
{
    [EzIPC("Nav.IsReady")] public Func<bool> IsReady;

    private VnavmeshIPC()
    {
        EzIPC.Init(this, "vnavmesh", SafeWrapper.AnyException);
    }
}
