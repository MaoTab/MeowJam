using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Jam;

public partial class TipBuilder : RichTextLabel
{
    // 避免在切换状态的空隙再次执行操作
    public bool Freeze;
    [Export]public bool LockPos;
    public override void _Ready()
    {
        MetaHoverStarted += meta =>
        {
            if (Freeze) return;
            var tipList = new List<string>();
            var isLink = false;
            if (ParseExpression(meta.ToString(),out var datas))
            {
                isLink = true;
                foreach (var data in datas)
                {
                    if (ParseKeyValue(data, out var param))
                    {
                        switch (param.Key)
                        {
                            case "log":
                                tipList.Add("点击此Tip将跳转到详情界面。");
                                continue;
                            case "show":
                                isLink = false;
                                tipList.Insert(0,Tr(param.Value)); 
                                continue;
                        }
                    }
                }
            }
            else
            {
                tipList.Insert(0,Tr(meta.ToString())); 
            }

            if (!isLink)
            {
                Game.Gui.CreatListTip(tipList,true);
            }
            
        };

        MetaClicked += meta =>
        {
            return;
            if (Freeze) return;
    
            var isLink = false;
            if (ParseExpression(meta.ToString(),out var datas))
            {
                isLink = true;
                foreach (var data in datas)
                {
                    if (ParseKeyValue(data, out var param))
                    {
                        switch (param.Key)
                        {
                            case "log":
                                Game.Gui.RemoveAllTip();
                                // Game.Gui.ShowLogBox();
                                continue;
                            case "show":
                                isLink = false;
                                continue;
                        }
                    }
                }
            }
            
            if(!isLink)
                Game.Gui.FreezeTip(this);
            
        };

        MetaHoverEnded += meta =>
        {
            Game.Gui.RemoveAllTip();
            return;
            
            if (Freeze) return;
            var isLink = false;
            if (ParseExpression(meta.ToString(),out var datas))
            {
                isLink = true;
                foreach (var data in datas)
                {
                    if (ParseKeyValue(data, out var param))
                    {
                        switch (param.Key)
                        {
                            case "show":
                                isLink = false;
                                continue;
                        }
                    }
                }
            }
           
            if(!isLink)
                Game.Gui.RemoveLastTip(this);
        };
    }
    
    private static bool ParseKeyValue(string element,out KeyValuePair<string, string> param)
    {
        param = new KeyValuePair<string, string>(); 
        
        // 检查是否包含 '=' 符号
        if (element.Contains('='))
        {
            var parts = element.Split(new[] { '=' }, 2); // 只分割成两部分
            if (parts.Length == 2)
            {
                string key = parts[0].Trim();
                string value = parts[1].Trim();
                param = new KeyValuePair<string, string>(key, value); // 返回键值对
                return true;
            }
        }

        // 如果没有 '='，返回原始元素
        return false;
    }
    
    public static bool ParseExpression(string input,out List<string> array)
    {
        array = new List<string>();
        
        // 检查是否是被括号包围的表达式
        if (input.StartsWith("(") && input.EndsWith(")"))
        {
            // 去掉外层括号
            string content = input[1..^1].Trim(); // C# 8.0 及以上版本的字符串切片

            // 按逗号分割并返回数组
            array = content.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return true;
        }

        // 视为普通字符串
        return false;
    }
}