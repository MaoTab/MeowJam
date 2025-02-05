using Godot;
using Steamworks;

namespace SnowBlindness.scripts.system;

public class SteamMgr
{
    protected static bool s_EverInitialized = false;
    protected bool m_bInitialized = false;
    protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;
    protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText) 
    {
        GD.PushError(pchDebugText);
    }

    public SteamMgr()
    {
        if (!DllCheck.Test()) 
        {
            GD.PushError("[Steamworks.NET] DllCheck测试返回false，某个或多个Steamworks二进制文件似乎是错误版本。", this);
        }
        
        try 
        {
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(3136080)))
            {
                GD.Print("[Steamworks.NET] 关闭，因为 RestartAppIfNecessary 返回 true。Steam 将重新启动应用程序。");
            }
        }
        catch (System.DllNotFoundException e) 
        { 
            // 在这里捕捉这个异常，因为它将是第一次出现。
            GD.PushError("[Steamworks.NET] 无法加载 [lib]steam_api.dll/so/dylib。它可能不在正确的位置。有关更多详细信息，请参阅 README。\n" + e, this);
        }
        
        m_bInitialized = SteamAPI.Init();
        
        if (!m_bInitialized)
        {
            GD.PushError("[Steamworks.NET] SteamAPI_Init() 失败。请参考Valve的文档或此行上方的注释以获取更多信息。", this);
        }

        s_EverInitialized = true;
    }

    public void Init()
    {
        if (!m_bInitialized) {
            return;
        }

        if (m_SteamAPIWarningMessageHook == null) 
        {
            // 设置回调以接收来自 Steam 的警告消息。
            // 必须在启动参数中添加 "-debug_steamapi" 才能接收警告。
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
        SteamFriends.SetRichPresence("steam_display", "#Status_AtMainMenu");
    }

    public void Shutdown()
    {
        if (!m_bInitialized) 
        {
            return;
        }

        SteamAPI.Shutdown();
    }
    
    public void Process() 
    {
        if (!m_bInitialized) 
        {
            return;
        }

        // 运行 Steam 客户端回调
        SteamAPI.RunCallbacks();
    }
    
}