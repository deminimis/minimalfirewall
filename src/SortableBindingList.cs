using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MinimalFirewall
{
    internal partial class NativeSort
    {
        [LibraryImport("shlwapi.dll", EntryPoint = "StrCmpLogicalW", StringMarshalling = StringMarshalling.Utf16)]
        public static partial int StrCmpLogicalW(string psz1, string psz2);
    }

    public class SortableBindingList<T> : BindingList<T>
    {
        private bool _isSorted;
        private PropertyDescriptor? _sortProperty;
        private ListSortDirection _sortDirection;

        public SortableBindingList() : base() { }

        public SortableBindingList(IList<T> list) : base(list) { }

        protected override bool SupportsSortingCore => true;
        protected override bool IsSortedCore => _isSorted;
        protected override PropertyDescriptor? SortPropertyCore => _sortProperty;
        protected override ListSortDirection SortDirectionCore => _sortDirection;

        protected override void RemoveSortCore()
        {
            _isSorted = false;
            _sortProperty = null;
            _sortDirection = ListSortDirection.Ascending;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            _sortProperty = prop;
            _sortDirection = direction;

            var itemsList = Items as List<T>;

            if (itemsList != null)
            {
                var comparer = new PropertyComparer<T>(prop, direction);
                itemsList.Sort(comparer);
            }
            else
            {
                var comparer = new PropertyComparer<T>(prop, direction);
                var sortedList = new List<T>(Items);
                sortedList.Sort(comparer);

                bool oldRaise = RaiseListChangedEvents;
                RaiseListChangedEvents = false;
                try
                {
                    Clear();
                    foreach (var item in sortedList)
                    {
                        Add(item);
                    }
                }
                finally
                {
                    RaiseListChangedEvents = oldRaise;
                }
            }

            _isSorted = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
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

    public class PropertyComparer<T> : IComparer<T>
    {
        private readonly ListSortDirection _direction;
        private readonly List<PropertyInfo> _propertyChain = new List<PropertyInfo>();

        public PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
        {
            _direction = direction;

            // Parse the property path once during initialization
            if (prop != null)
            {
                Type currentType = typeof(T);
                foreach (var part in prop.Name.Split('.'))
                {
                    PropertyInfo? info = currentType.GetProperty(part);
                    if (info == null) break;
                    _propertyChain.Add(info);
                    currentType = info.PropertyType;
                }
            }
        }

        public int Compare(T? x, T? y)
        {
            object? valueX = GetValue(x);
            object? valueY = GetValue(y);

            int result;

            if (valueX == null && valueY == null) result = 0;
            else if (valueX == null) result = -1;
            else if (valueY == null) result = 1;
            else if (valueX is string s1 && valueY is string s2)
            {
                result = NativeSort.StrCmpLogicalW(s1, s2);
            }
            else if (valueX is IComparable comparableX)
            {
                try { result = comparableX.CompareTo(valueY); }
                catch { result = NativeSort.StrCmpLogicalW(valueX.ToString() ?? "", valueY.ToString() ?? ""); }
            }
            else
            {
                result = NativeSort.StrCmpLogicalW(valueX.ToString() ?? "", valueY.ToString() ?? "");
            }

            return _direction == ListSortDirection.Ascending ? result : -result;
        }

        private object? GetValue(object? obj)
        {
            if (obj == null || _propertyChain.Count == 0) return null;

            object? current = obj;
            foreach (var propInfo in _propertyChain)
            {
                current = propInfo.GetValue(current, null);
                if (current == null) return null;
            }
            return current;
        }
    }
}