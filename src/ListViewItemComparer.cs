// ListViewItemComparer.cs
using System.Collections;
using System.Windows.Forms;

namespace MinimalFirewall
{
    public class ListViewItemComparer(int column, SortOrder order) : IComparer
    {
        public int Compare(object? x, object? y)
        {
            if (x is not ListViewItem itemX || y is not ListViewItem itemY)
            {
                return 0;
            }

            string textX = itemX.SubItems.Count > column ? itemX.SubItems[column].Text : string.Empty;
            string textY = itemY.SubItems.Count > column ? itemY.SubItems[column].Text : string.Empty;

            int compareResult;
            if (int.TryParse(textX, out int intX) && int.TryParse(textY, out int intY))
            {
                compareResult = intX.CompareTo(intY);
            }
            else
            {
                compareResult = string.Compare(textX, textY);
            }

            if (order == SortOrder.Descending)
            {
                return -compareResult;
            }
            else
            {
                return compareResult;
            }
        }
    }
}