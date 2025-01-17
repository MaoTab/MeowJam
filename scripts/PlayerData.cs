using Godot;
using Jam.data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jam;

public class PlayerData
{
    /// <summary>
    /// 生物棱镜 肉体
    /// </summary>
    public PrismData BioPrism;
    
    /// <summary>
    /// 心理棱镜 理性
    /// </summary>
    public PrismData PsyPrism;
    
    /// <summary>
    /// 社会棱镜 情感
    /// </summary>
    public PrismData SocPrism;
    
    /// <summary>
    /// 玩家棱镜（自身）
    /// </summary>
    public PrismData SelfPrism;
    
    /// <summary>
    /// 饥饿值
    /// </summary>
    public int Hunger { get; set; }
    
    /// <summary>
    /// 生命值
    /// </summary>
    public int Hp { get; set; }
    
    public void Save()
    {
        // 读取存档文件
        var load = FileAccess.Open("user://prism.txt", FileAccess.ModeFlags.Write);
        if(load == null) return;
        
        var save = new JObject()
        {
            {nameof(BioPrism) , BioPrism.Level},
            {nameof(PsyPrism) , PsyPrism.Level},
            {nameof(SocPrism) , SocPrism.Level},
            {nameof(SelfPrism), SelfPrism.Level},
            {nameof(Hunger) , Hunger },
            {nameof(Hp) , Hp },
        };
        
        load.StoreLine(JsonConvert.SerializeObject(save));
        load.Close();
    }
    
    public PlayerData Init()
    {
        // 读取配置文件
        var json = FileAccess.GetFileAsString("res://data/prism.txt");
        var jsonToken = JsonConvert.DeserializeObject<PlayerData>(json);
        
        // 数据写入
        BioPrism.Init(jsonToken.BioPrism);
        PsyPrism.Init(jsonToken.PsyPrism);
        SocPrism.Init(jsonToken.SocPrism);
        SelfPrism.Init(jsonToken.SelfPrism);
        
        // 读取存档文件
        var loadData = FileAccess.GetFileAsString("user://prism.txt");
        var load = FileAccess.Open("user://prism.txt", FileAccess.ModeFlags.WriteRead);
        
       
        if (loadData.Length > 0)
        {
            var loadToken = JsonConvert.DeserializeObject<JObject>(loadData);
            if (loadToken != null)
            {
                // 读取存档数据
                BioPrism.Level = loadToken[nameof(BioPrism) ]!.Value<int>();
                PsyPrism.Level = loadToken[nameof(PsyPrism)]!.Value<int>();
                SocPrism.Level = loadToken[nameof(SocPrism)]!.Value<int>();
                SelfPrism.Level = loadToken[nameof(SelfPrism)]!.Value<int>();
                Hunger = loadToken[nameof(Hunger)]!.Value<int>();
                Hp = loadToken[nameof(Hp)]!.Value<int>();
            }
        }
        
        var save = new JObject
        {
            {nameof(BioPrism) , BioPrism.Level},
            {nameof(PsyPrism) , PsyPrism.Level},
            {nameof(SocPrism) , SocPrism.Level},
            {nameof(SelfPrism), SelfPrism.Level},
            {nameof(Hunger) , Hunger },
            {nameof(Hp) , Hp },
        };

        if (load != null)
        {
            load.StoreLine(JsonConvert.SerializeObject(save));
            load.Close();
        }
        
        return this;
    }
}