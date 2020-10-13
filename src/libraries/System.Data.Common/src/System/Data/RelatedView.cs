// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;

namespace System.Data
{
    internal sealed class RelatedView : DataView, IFilter
    {
        private readonly DataKey? _parentKey;
        private readonly DataKey _childKey;
        private readonly DataRowView? _parentRowView;
        private readonly object[]? _filterValues;

        public RelatedView(DataColumn[] columns, object[] values) : base(columns[0].Table, false)
        {
            if (values is null)
            {
                throw ExceptionBuilder.ArgumentNull(nameof(values));
            }

            _parentRowView = null;
            _parentKey = null;
            _childKey = new DataKey(columns, true);
            _filterValues = values;
            Debug.Assert(Table == _childKey.Table, "Key.Table Must be equal to Current Table");
            base.ResetRowViewCache();
        }

        public RelatedView(DataRowView parentRowView, DataKey parentKey, DataColumn[] childKeyColumns) : base(childKeyColumns[0].Table, false)
        {
            _filterValues = null;
            _parentRowView = parentRowView;
            _parentKey = parentKey;
            _childKey = new DataKey(childKeyColumns, true);
            Debug.Assert(Table == _childKey.Table, "Key.Table Must be equal to Current Table");
            base.ResetRowViewCache();
        }

        private object[]? GetParentValues()
        {
            if (_filterValues is not null)
            {
                return _filterValues;
            }

            if (!_parentRowView!.HasRecord())
            {
                return null;
            }
            return _parentKey!.Value.GetKeyValues(_parentRowView.GetRecord());
        }


        public bool Invoke(DataRow row, DataRowVersion version)
        {
            object[]? parentValues = GetParentValues();
            if (parentValues is null)
            {
                return false;
            }

            object[] childValues = row.GetKeyValues(_childKey, version);

            bool allow = true;
            if (childValues.Length != parentValues.Length)
            {
                allow = false;
            }
            else
            {
                for (int i = 0; i < childValues.Length; i++)
                {
                    if (!childValues[i].Equals(parentValues[i]))
                    {
                        allow = false;
                        break;
                    }
                }
            }

            IFilter? baseFilter = base.GetFilter();
            if (baseFilter is not null)
            {
                allow &= baseFilter.Invoke(row, version);
            }

            return allow;
        }

        internal override IFilter GetFilter() => this;

        // move to OnModeChanged
        public override DataRowView AddNew()
        {
            DataRowView addNewRowView = base.AddNew();
            addNewRowView.Row.SetKeyValues(_childKey, GetParentValues()!);
            return addNewRowView;
        }

        internal override void SetIndex(string newSort, DataViewRowState newRowStates, IFilter? newRowFilter)
        {
            SetIndex2(newSort, newRowStates, newRowFilter, false);
            Reset();
        }

        public override bool Equals(DataView? dv)
        {
            RelatedView? other = dv as RelatedView;
            if (other is null)
            {
                return false;
            }
            if (!base.Equals(dv))
            {
                return false;
            }
            if (_filterValues is not null)
            {
                return (CompareArray(_childKey.ColumnsReference, other._childKey.ColumnsReference) && CompareArray(_filterValues, other._filterValues));
            }
            else
            {
                if (other._filterValues is not null)
                {
                    return false;
                }

                return (CompareArray(_childKey.ColumnsReference, other._childKey.ColumnsReference) &&
                        CompareArray(_parentKey!.Value.ColumnsReference, _parentKey.Value.ColumnsReference) &&
                        _parentRowView!.Equals(other._parentRowView));
            }
        }

        private bool CompareArray(object?[]? value1, object?[]? value2)
        {
            if (value1 is null || value2 is null)
            {
                return value1 == value2;
            }
            if (value1.Length != value2.Length)
            {
                return false;
            }
            for (int i = 0; i < value1.Length; i++)
            {
                if (value1[i] != value2[i])
                    return false;
            }
            return true;
        }
    }
}
