using System;
using System.Collections.Generic;
using GTAFullTrainer.UI;
using GTAFullTrainer.CoreUI;

namespace GTAFullTrainer.API
{
    public static class MenuAPI
    {
        // Add an entire category
        public static int AddPage(string name)
        {
            int index = UICore.Categories.Count;
            UICore.RegisterCategory(name, new List<UIControl>());
            return index;
        }

        // Add controls to a page
        public static void AddButton(int page, string label, Action callback)
        {
            UICore.CategoryItems[page].Add(new UIButton(label, callback));
        }

        public static void AddToggle(int page, string label, bool defaultState, Action<bool> onToggle)
        {
            UICore.CategoryItems[page].Add(new UIToggle(label, defaultState)
            {
                OnToggleLogic = onToggle
            });
        }

        public static void AddSlider(int page, string label, float min, float max, float def, Action<float> onSlide)
        {
            UICore.CategoryItems[page].Add(new UISlider(label, min, max, def)
            {
                OnSlideLogic = onSlide
            });
        }

        public static void AddList(int page, string label, string[] entries, Action<int> onChange)
        {
            UICore.CategoryItems[page].Add(new UIList(label, entries)
            {
                OnListChangeLogic = onChange
            });
        }
    }
}
