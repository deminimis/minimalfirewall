// File: SortableBindingList.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

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

        private static object? GetNestedPropertyValue(object? obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName)) return null;

            object? currentObject = obj;
            foreach (string part in propertyName.Split('.'))
            {
                if (currentObject == null) return null;
                Type type = currentObject.GetType();
                PropertyInfo? info = type.GetProperty(part);
                if (info == null) return null;
                currentObject = info.GetValue(currentObject, null);
            }
            return currentObject;
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            _sortProperty = prop;
            _sortDirection = direction;

            if (Items is List<T> items)
            {
                items.Sort((a, b) =>
                {
                    var valueA = GetNestedPropertyValue(a, prop.Name);
                    var valueB = GetNestedPropertyValue(b, prop.Name);

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