// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace System.Collections.ObjectModel
{
    [Serializable]
    [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    [System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public abstract class KeyedCollection<TKey, TItem> : Collection<TItem> where TKey: notnull
    {
        private const int DefaultThreshold = 0;

        private readonly IEqualityComparer<TKey> comparer; // Do not rename (binary serialization)
        private Dictionary<TKey, TItem>? dict; // Do not rename (binary serialization)
        private int keyCount; // Do not rename (binary serialization)
        private readonly int threshold; // Do not rename (binary serialization)

        protected KeyedCollection() : this(null, DefaultThreshold)
        {
        }

        protected KeyedCollection(IEqualityComparer<TKey>? comparer) : this(comparer, DefaultThreshold)
        {
        }

        protected KeyedCollection(IEqualityComparer<TKey>? comparer, int dictionaryCreationThreshold)
            : base(new List<TItem>()) // Be explicit about the use of List<T> so we can foreach over
                                      // Items internally without enumerator allocations.
        {
            if (dictionaryCreationThreshold < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(dictionaryCreationThreshold), SR.ArgumentOutOfRange_InvalidThreshold);
            }

            this.comparer = comparer ?? EqualityComparer<TKey>.Default;
            threshold = dictionaryCreationThreshold == -1 ? int.MaxValue : dictionaryCreationThreshold;
        }

        /// <summary>
        /// Enables the use of foreach internally without allocations using <see cref="List{T}"/>'s struct enumerator.
        /// </summary>
        private new List<TItem> Items
        {
            get
            {
                Debug.Assert(base.Items is List<TItem>);
                return (List<TItem>)base.Items;
            }
        }

        public IEqualityComparer<TKey> Comparer => comparer;

        public TItem this[TKey key]
        {
            get
            {
                TItem item;
                if (TryGetValue(key, out item!))
                {
                    return item;
                }

                throw new KeyNotFoundException(SR.Format(SR.Arg_KeyNotFoundWithKey, key.ToString()));
            }
        }

        public bool Contains(TKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (dict is not null)
            {
                return dict.ContainsKey(key);
            }

            foreach (TItem item in Items)
            {
                if (comparer.Equals(GetKeyForItem(item), key))
                {
                    return true;
                }
            }

            return false;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TItem item)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (dict is not null)
            {
                return dict.TryGetValue(key, out item!);
            }

            foreach (TItem itemInItems in Items)
            {
                TKey keyInItems = GetKeyForItem(itemInItems);
                if (keyInItems is not null && comparer.Equals(key, keyInItems))
                {
                    item = itemInItems;
                    return true;
                }
            }

            item = default;
            return false;
        }

        private bool ContainsItem(TItem item)
        {
            TKey key;
            if ((dict is null) || ((key = GetKeyForItem(item)) is null))
            {
                return Items.Contains(item);
            }

            TItem itemInDict;
            if (dict.TryGetValue(key, out itemInDict!))
            {
                return EqualityComparer<TItem>.Default.Equals(itemInDict, item);
            }

            return false;
        }

        public bool Remove(TKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (dict is not null)
            {
                TItem item;
                return dict.TryGetValue(key, out item!) && Remove(item);
            }

            for (int i = 0; i < Items.Count; i++)
            {
                if (comparer.Equals(GetKeyForItem(Items[i]), key))
                {
                    RemoveItem(i);
                    return true;
                }
            }

            return false;
        }

        protected IDictionary<TKey, TItem>? Dictionary => dict;

        protected void ChangeItemKey(TItem item, TKey newKey)
        {
            if (!ContainsItem(item))
            {
                throw new ArgumentException(SR.Argument_ItemNotExist, nameof(item));
            }

            TKey oldKey = GetKeyForItem(item);
            if (!comparer.Equals(oldKey, newKey))
            {
                if (newKey is not null)
                {
                    AddKey(newKey, item);
                }
                if (oldKey is not null)
                {
                    RemoveKey(oldKey);
                }
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            dict?.Clear();
            keyCount = 0;
        }

        protected abstract TKey GetKeyForItem(TItem item);

        protected override void InsertItem(int index, TItem item)
        {
            TKey key = GetKeyForItem(item);
            if (key is not null)
            {
                AddKey(key, item);
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            TKey key = GetKeyForItem(Items[index]);
            if (key is not null)
            {
                RemoveKey(key);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TItem item)
        {
            TKey newKey = GetKeyForItem(item);
            TKey oldKey = GetKeyForItem(Items[index]);

            if (comparer.Equals(oldKey, newKey))
            {
                if (newKey is not null && dict is not null)
                {
                    dict[newKey] = item;
                }
            }
            else
            {
                if (newKey is not null)
                {
                    AddKey(newKey, item);
                }

                if (oldKey is not null)
                {
                    RemoveKey(oldKey);
                }
            }

            base.SetItem(index, item);
        }

        private void AddKey(TKey key, TItem item)
        {
            if (dict is not null)
            {
                dict.Add(key, item);
            }
            else if (keyCount == threshold)
            {
                CreateDictionary();
                dict!.Add(key, item);
            }
            else
            {
                if (Contains(key))
                {
                    throw new ArgumentException(SR.Format(SR.Argument_AddingDuplicate, key), nameof(key));
                }

                keyCount++;
            }
        }

        private void CreateDictionary()
        {
            dict = new Dictionary<TKey, TItem>(comparer);
            foreach (TItem item in Items)
            {
                TKey key = GetKeyForItem(item);
                if (key is not null)
                {
                    dict.Add(key, item);
                }
            }
        }

        private void RemoveKey(TKey key)
        {
            Debug.Assert(key is not null, "key shouldn't be null!");
            if (dict is not null)
            {
                dict.Remove(key);
            }
            else
            {
                keyCount--;
            }
        }
    }
}
