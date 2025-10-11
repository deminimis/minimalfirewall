// File: SortableBindingList.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MinimalFirewall
{
    public class SortableBindingList<T> : BindingList<T>
    {
        private PropertyDescriptor? _sortProperty;
        private ListSortDirection _sortDirection;

        public SortableBindingList(IList<T> list) : base(list) { }

        protected override bool SupportsSortingCore => true;
        protected override bool IsSortedCore => _sortProperty != null;
        protected override PropertyDescriptor? SortPropertyCore => _sortProperty;
        protected override ListSortDirection SortDirectionCore => _sortDirection;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            _sortProperty = prop;
            _sortDirection = direction;

            if (Items is List<T> items)
            {
                items.Sort((a, b) =>
                {
                    var valueA = prop.GetValue(a);
                    var valueB = prop.GetValue(b);

                    int result = (valueA as IComparable)?.CompareTo(valueB) ?? 0;
                    return direction == ListSortDirection.Ascending ? result : -result;
                });

                ResetBindings();
            }
        }

        public void Sort(string propertyName, ListSortDirection direction)
        {
            var prop = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, true);
            if (prop != null)
            {
                ApplySortCore(prop, direction);
            }
        }
    }
}