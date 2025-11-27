using System;
using System.Collections.Generic;
using NinnyTrainer.CoreUI;
using NinnyTrainer.UI;

namespace NinnyTrainer.API
{
    public static class MenuAPI
    {
        // Add a page
        public static int AddPage(string name)
        {
            int index = UICore.Categories.Count;
            UICore.RegisterCategory(name, new List<UIControl>());
            return index;
        }

        // Add a button
        public static void AddButton(int page, string label, Action callback)
        {
            UICore.CategoryItems[page].Add(new UIButton(label, callback));
        }

        // Add a toggle
        public static void AddToggle(int page, string label, bool defaultState, Action<bool> callback)
        {
            UICore.CategoryItems[page].Add(new UIToggle(label, defaultState, callback));
        }

        // Add a slider
        public static void AddSlider(int page, string label, float min, float max, float def, Action<float> callback)
        {
            UICore.CategoryItems[page].Add(new UISlider(label, min, max, def, callback));
        }

        // Add a list selector
        public static void AddList(int page, string label, string[] items, Action<int> callback)
        {
            UICore.CategoryItems[page].Add(new UIList(label, items, callback));
        }
    }
}
