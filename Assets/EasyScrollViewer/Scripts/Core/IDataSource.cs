// Created by Ashechol
// 2023-11-09

namespace EasyScrollViewer
{
    public interface IDataSource
    {
        void RefreshItem(IScrollViewItem item, int index);
        int Count { get; }
    }
}