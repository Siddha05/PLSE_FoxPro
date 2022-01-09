using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace PLSE_FoxPro.Models
{
    interface IPagesService
    {
        void AddPage(Page p, bool root = false);
        void RemovePage();
        void RemoveAllPages();
        int CurrentPageStackCount();
    }
    internal class PagesService : IPagesService
    {
        public void AddPage(Page p, bool root = false) => App.MainViewModel.AddPage(p, root);

        public void RemoveAllPages() => App.MainViewModel.RemoveAllPages();

        public void RemovePage() => App.MainViewModel.RemovePage();

        public int CurrentPageStackCount() => App.MainViewModel.PageStackDepth();
    }
}
