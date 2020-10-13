// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace System.Diagnostics
{
    public class CorrelationManager
    {
        private readonly AsyncLocal<Guid> _activityId = new AsyncLocal<Guid>();
        private readonly AsyncLocal<StackNode?> _stack = new AsyncLocal<StackNode?>();
        private readonly Stack _stackWrapper;

        internal CorrelationManager()
        {
            _stackWrapper = new AsyncLocalStackWrapper(_stack);
        }

        public Stack LogicalOperationStack => _stackWrapper;

        public void StartLogicalOperation() => StartLogicalOperation(Guid.NewGuid());

        public void StopLogicalOperation() => _stackWrapper.Pop();

        public Guid ActivityId { get { return _activityId.Value; } set { _activityId.Value = value; } }

        public void StartLogicalOperation(object operationId)
        {
            if (operationId is null)
            {
                throw new ArgumentNullException(nameof(operationId));
            }

            _stackWrapper.Push(operationId);
        }

        private sealed class StackNode
        {
            internal StackNode(object? value, StackNode? prev = null)
            {
                Value = value;
                Prev = prev;
                Count = prev is not null ? prev.Count + 1 : 1;
            }

            internal int Count { get; }
            internal object? Value { get; }
            internal StackNode? Prev { get; }
        }

        private sealed class AsyncLocalStackWrapper : Stack
        {
            private readonly AsyncLocal<StackNode?> _stack;

            internal AsyncLocalStackWrapper(AsyncLocal<StackNode?> stack)
            {
                Debug.Assert(stack is not null);
                _stack = stack;
            }

            public override void Clear() => _stack.Value = null;

            public override object Clone() => new AsyncLocalStackWrapper(_stack);

            public override int Count => _stack.Value?.Count ?? 0;

            public override IEnumerator GetEnumerator() => GetEnumerator(_stack.Value);

            public override object? Peek() => _stack.Value?.Value;

            public override bool Contains(object? obj)
            {
                for (StackNode? n = _stack.Value; n is not null; n = n.Prev)
                {
                    if (obj is null)
                    {
                        if (n.Value is null) return true;
                    }
                    else if (obj.Equals(n.Value))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override void CopyTo(Array array, int index)
            {
                for (StackNode? n = _stack.Value; n is not null; n = n.Prev)
                {
                    array.SetValue(n.Value, index++);
                }
            }

            private IEnumerator GetEnumerator(StackNode? n)
            {
                while (n is not null)
                {
                    yield return n.Value;
                    n = n.Prev;
                }
            }

            public override object? Pop()
            {
                StackNode? n = _stack.Value;
                if (n is null)
                {
                    base.Pop(); // used to throw proper exception
                }
                _stack.Value = n!.Prev;
                return n.Value;
            }

            public override void Push(object? obj)
            {
                _stack.Value = new StackNode(obj, _stack.Value);
            }

            public override object?[] ToArray()
            {
                StackNode? n = _stack.Value;
                if (n is null)
                {
                    return Array.Empty<object>();
                }

                var results = new List<object?>();
                do
                {
                    results.Add(n.Value);
                    n = n.Prev;
                }
                while (n is not null);
                return results.ToArray();
            }
        }
    }
}
