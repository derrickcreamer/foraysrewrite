using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UtilityCollections { // Updated 2017-09-02
	using CircularLinkedListExtensions;
	/// <summary>
	/// A hashset with an indexer for convenience.
	/// </summary>
	public class EasyHashSet<T> : HashSet<T> {
		/// <summary>
		/// GET: Returns true if the given element is present.
		/// SET: If 'true', add the given element. If 'false', remove the given element.
		/// </summary>
		public bool this[T t] {
			get { return Contains(t); }
			set {
				if(value) Add(t);
				else Remove(t);
			}
		}
		public EasyHashSet() { }
		public EasyHashSet(IEqualityComparer<T> comparer) : base(comparer) { }
		public EasyHashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer = null) : base(collection, comparer) { }
	}
	/// <summary>
	/// A dictionary that returns a default value if the given key isn't present.
	/// </summary>
	public class DefaultValueDictionary<TKey, TValue> : Dictionary<TKey, TValue> {
		new public TValue this[TKey key] {
			get {
				TValue v;
				if(TryGetValue(key, out v) || getDefaultValue == null) { // TryGetValue sets 'v' to default(TValue) if not found.
					return v;
				}
				else return getDefaultValue();
			}
			set {
				base[key] = value;
			}
		}
		private Func<TValue> getDefaultValue = null;
		/// <summary>
		/// If defined, the result of this method will be used instead of default(TValue).
		/// </summary>
		public Func<TValue> GetDefaultValue {
			get {
				if(getDefaultValue == null) return () => default(TValue);
				else return getDefaultValue;
			}
			set { getDefaultValue = value; }
		}
		public DefaultValueDictionary() { }
		public DefaultValueDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
		public DefaultValueDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer = null)
			: base(dictionary, comparer) { }
	}

	public class Grouping<TKey, TValue> : IGrouping<TKey, TValue> {
		public TKey Key { get; protected set; }
		private readonly IEnumerable<TValue> sequence;
		public Grouping(TKey key, IEnumerable<TValue> sequence) {
			Key = key;
			this.sequence = sequence ?? throw new ArgumentNullException(nameof(sequence));
		}
		public IEnumerator<TValue> GetEnumerator() => sequence.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
	}

	/// <summary>
	/// A collection for mapping one key to any number of values
	/// </summary>
	public class MultiValueDictionary<TKey, TValue> : IEnumerable<IGrouping<TKey, TValue>> {
		private Dictionary<TKey, ICollection<TValue>> d;
		private readonly Func<ICollection<TValue>> createCollection;
		public MultiValueDictionary() : this(null, null, null) { }
		public MultiValueDictionary(IEqualityComparer<TKey> comparer) : this(null, null, comparer) { }
		/// <param name="collection">The existing collection of keys & values with which to initialize this collection</param>
		public MultiValueDictionary(IEnumerable<IGrouping<TKey, TValue>> collection,
			IEqualityComparer<TKey> comparer) : this(null, collection, comparer) { }
		private MultiValueDictionary(Func<ICollection<TValue>> createCollection,
			IEnumerable<IGrouping<TKey, TValue>> existingCollection,
			IEqualityComparer<TKey> comparer = null)
		{
			d = new Dictionary<TKey, ICollection<TValue>>(comparer);
			if(createCollection == null) createCollection = () => new List<TValue>();
			this.createCollection = createCollection;
			if(existingCollection != null) {
				foreach(var group in existingCollection) {
					this[group.Key] = group;
				}
			}
		}
		/// <summary>
		/// Create a MultiValueDictionary that uses a specific type of ICollection, instead of the default List.
		/// </summary>
		/// <typeparam name="TCollection">The type of ICollection to use instead of List</typeparam>
		public static MultiValueDictionary<TKey, TValue> Create<TCollection>() where TCollection : ICollection<TValue>, new() {
			return new MultiValueDictionary<TKey, TValue>(() => new TCollection(), null, null);
		}
		/// <summary>
		/// Create a MultiValueDictionary that uses a specific type of ICollection, instead of the default List.
		/// </summary>
		/// <typeparam name="TCollection">The type of ICollection to use instead of List</typeparam>
		public static MultiValueDictionary<TKey, TValue> Create<TCollection>(IEqualityComparer<TKey> comparer)
			where TCollection : ICollection<TValue>, new()
		{
			return new MultiValueDictionary<TKey, TValue>(() => new TCollection(), null, comparer);
		}
		/// <summary>
		/// Create a MultiValueDictionary that uses a specific type of ICollection, instead of the default List.
		/// </summary>
		/// <typeparam name="TCollection">The type of ICollection to use instead of List</typeparam>
		/// <param name="collection">The existing collection of keys & values with which to initialize this collection</param>
		public static MultiValueDictionary<TKey, TValue> Create<TCollection>(
			IEnumerable<IGrouping<TKey, TValue>> collection,
			IEqualityComparer<TKey> comparer)
			where TCollection : ICollection<TValue>, new()
		{
			return new MultiValueDictionary<TKey, TValue>(() => new TCollection(), collection, comparer);
		}
		public IEqualityComparer<TKey> Comparer => d.Comparer;
		/// <summary>
		/// Returns all groupings of one key to some (nonzero) number of values
		/// </summary>
		public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() {
			foreach(var pair in d) if(pair.Value.Count > 0) yield return new Grouping<TKey, TValue>(pair.Key, pair.Value.ToArray());
		}
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		/// <summary>
		/// Returns all pairs of one key to one value
		/// </summary>
		public IEnumerable<KeyValuePair<TKey, TValue>> GetAllKeyValuePairs() {
			foreach(var pair in d) {
				foreach(var v in pair.Value) {
					yield return new KeyValuePair<TKey, TValue>(pair.Key, v);
				}
			}
		}
		/// <summary>
		/// Returns all keys with at least one value
		/// </summary>
		public ICollection<TKey> GetAllKeys() => d.Where(pair => pair.Value.Count > 0).Select(pair => pair.Key).ToArray();
		/// <summary>
		/// Returns all values in the collection
		/// </summary>
		public IEnumerable<TValue> GetAllValues() {
			foreach(var collection in d.Values) {
				foreach(var v in collection) {
					yield return v;
				}
			}
		}
		/// <summary>
		/// Retrieves the values for a given key. Returns true if the key has at least one value. (If no values are present, 'values' is empty, NOT null.)
		/// </summary>
		public bool TryGetValues(TKey key, out IEnumerable<TValue> values) {
			if(d.TryGetValue(key, out ICollection<TValue> collection)) {
				values = collection.ToArray();
				return collection.Count > 0;
			}
			else {
				values = Enumerable.Empty<TValue>();
				return false;
			}
		}
		/// <summary>
		/// GET: Retrieves the values for a given key.  SET: Replaces the given key's values with a new collection.
		/// </summary>
		public IEnumerable<TValue> this[TKey key] {
			get {
				ICollection<TValue> values;
				if(d.TryGetValue(key, out values)) return values.ToArray();
				else return Enumerable.Empty<TValue>();
			}
			set {
				if(value == null) {
					Clear(key);
				}
				else {
					ICollection<TValue> coll = createCollection();
					foreach(TValue v in value) {
						coll.Add(v);
					}
					d[key] = coll;
				}
			}
		}
		/// <summary>
		/// Returns the number of keys that have at least 1 value
		/// </summary>
		public int GetKeyCount() => d.Count(pair => pair.Value.Count > 0);
		/// <summary>
		/// Returns the number of values in the collection
		/// </summary>
		public int GetValueCount() => d.Sum(pair => pair.Value.Count);
		//possible xml note: "if you want duplicate values to be ignored, consider creating this collection with HashSet"
		public void Add(TKey key, TValue value) {
			ICollection<TValue> values;
			if(!d.TryGetValue(key, out values)) {
				values = createCollection();
				d.Add(key, values);
			}
			values.Add(value);
		}
		public bool Remove(TKey key, TValue value) {
			ICollection<TValue> values;
			if(d.TryGetValue(key, out values)) return values.Remove(value);
			else return false;
		}
		public void Clear() => d.Clear();
		public void Clear(TKey key) => d.Remove(key);
		public bool Contains(TKey key, TValue value) {
			ICollection<TValue> values;
			return d.TryGetValue(key, out values) && values.Contains(value);
		}
		public bool Contains(TValue value) {
			foreach(var collection in d.Values) {
				if(collection.Contains(value)) return true;
			}
			return false;
		}
		//possible xml note: suggest HashSet here too, "if you're using AddUnique exclusively".
		public bool AddUnique(TKey key, TValue value) {
			ICollection<TValue> values;
			if(d.TryGetValue(key, out values)) {
				if(values.Contains(value)) return false;
			}
			else {
				values = createCollection();
				d.Add(key, values);
			}
			values.Add(value);
			return true;
		}
		public bool AnyValues(TKey key) {
			ICollection<TValue> values;
			return d.TryGetValue(key, out values) && values.Count > 0;
		}
	}
	public class BimapOneToOne<T1, T2> : IEnumerable<(T1 one, T2 two)> {
		protected Dictionary<T1, T2> d1;
		protected Dictionary<T2, T1> d2;
		public BimapOneToOne() : this(null, null) { }
		public BimapOneToOne(IEqualityComparer<T1> comparer1, IEqualityComparer<T2> comparer2) {
			if(comparer1 == null) comparer1 = EqualityComparer<T1>.Default;
			if(comparer2 == null) comparer2 = EqualityComparer<T2>.Default;
			d1 = new Dictionary<T1, T2>(comparer1);
			d2 = new Dictionary<T2, T1>(comparer2);
		}
		public T1 this[T2 key] {
			get => d2[key];
			set {
				if(key == null) throw new ArgumentNullException(nameof(key));
				if(d1.TryGetValue(value, out T2 result2)) { // If 'value' currently has any association...
					if(d2.Comparer.Equals(result2, key)) return; // Already linked; just return.
					else d2.Remove(result2); // 'value' has a new association, so break the previous one.
				}
				if(d2.TryGetValue(key, out T1 result1)) { // Same check, for the other half.
					d1.Remove(result1);
				}
				d1[value] = key;
				d2[key] = value;
			}
		}
		public T2 this[T1 key] {
			get => d1[key];
			set {
				if(key == null) throw new ArgumentNullException(nameof(key));
				if(d2.TryGetValue(value, out T1 result1)) {
					if(d1.Comparer.Equals(result1, key)) return;
					else d1.Remove(result1);
				}
				if(d1.TryGetValue(key, out T2 result2)) {
					d2.Remove(result2);
				}
				d2[value] = key;
				d1[key] = value;
			}
		}
		public bool TryGetValue(T2 key, out T1 value) => d2.TryGetValue(key, out value);
		public bool TryGetValue(T1 key, out T2 value) => d1.TryGetValue(key, out value);
		public bool Contains(T1 key) => d1.ContainsKey(key);
		public bool Contains(T2 key) => d2.ContainsKey(key);
		public bool Contains(T1 key1, T2 key2) {
			if(key2 == null) throw new ArgumentNullException(nameof(key2));
			if(!d1.TryGetValue(key1, out T2 result)) return false;
			return d2.Comparer.Equals(result, key2);
		}
		public void Add(T1 key1, T2 key2) {
			if(key2 == null) throw new ArgumentNullException(nameof(key2)); // Check first, to prevent inconsistent state if the
			if(d2.ContainsKey(key2)) throw new ArgumentException("Key '" + nameof(key2) + "' already present."); // 2nd Add() would throw.
			d1.Add(key1, key2);
			d2.Add(key2, key1);
		}
		public bool Remove(T1 key) {
			if(!d1.TryGetValue(key, out T2 result)) return false;
			d2.Remove(result);
			d1.Remove(key);
			return true;
		}
		public bool Remove(T2 key) {
			if(!d2.TryGetValue(key, out T1 result)) return false;
			d1.Remove(result);
			d2.Remove(key);
			return true;
		}
		public bool Remove(T1 key1, T2 key2) {
			if(!Contains(key1, key2)) return false;
			d1.Remove(key1);
			d2.Remove(key2);
			return true;
		}
		public IEnumerator<(T1 one, T2 two)> GetEnumerator() {
			foreach(var pair in d1) {
				yield return (pair.Key, pair.Value);
			}
		}
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		public IEqualityComparer<T1> ComparerT1 => d1.Comparer;
		public IEqualityComparer<T2> ComparerT2 => d2.Comparer;
		public ICollection<T1> KeysT1 => d1.Keys;
		public ICollection<T2> KeysT2 => d2.Keys;
		public void Clear() {
			d1.Clear();
			d2.Clear();
		}
	}
	public class BimapOneToMany<T1, TMany> : IEnumerable<(T1 one, TMany two)> {
		protected MultiValueDictionary<T1, TMany> d1;
		protected Dictionary<TMany, T1> d2;
		public BimapOneToMany() : this(null, null) { }
		public BimapOneToMany(IEqualityComparer<T1> comparer1, IEqualityComparer<TMany> comparer2) {
			if(comparer1 == null) comparer1 = EqualityComparer<T1>.Default;
			if(comparer2 == null) comparer2 = EqualityComparer<TMany>.Default;
			d1 = MultiValueDictionary<T1, TMany>.Create<HashSet<TMany>>(comparer1); // Using a hashset to automatically ignore duplicates
			d2 = new Dictionary<TMany, T1>(comparer2);
		}
		public T1 this[TMany key] {
			get => d2[key];
			set {
				if(value == null) throw new ArgumentNullException("value");
				if(d2.TryGetValue(key, out T1 result1)) { // If 'obj' was already linked to a T1, remove its link with 'obj'.
					if(d1.Comparer.Equals(result1, value)) return; // (If it's the same link, just return.)
					else d1.Remove(result1, key);
				}
				d1.Add(value, key);
				d2[key] = value;
			}
		}
		public IEnumerable<TMany> this[T1 key] {
			get => d1[key];
			set {
				if(value == null) throw new ArgumentNullException("value");
				foreach(var tmany in d1[key]) {
					d2.Remove(tmany); // Break any links between 'obj' and the replaced collection.
				}
				foreach(var tmany in value) {
					d1.Remove(d2[tmany], tmany); // Break any links the new collection may already have.
				}
				d1[key] = value;
				foreach(var tmany in value) {
					d2[tmany] = key;
				}
			}
		}
		public bool TryGetValue(TMany key, out T1 value) => d2.TryGetValue(key, out value);
		public bool TryGetValues(T1 key, out IEnumerable<TMany> values) => d1.TryGetValues(key, out values);
		public bool Contains(T1 key) => d1.AnyValues(key);
		public bool Contains(TMany key) => d2.ContainsKey(key);
		public bool Contains(T1 key1, TMany key2) {
			if(key1 == null) throw new ArgumentNullException(nameof(key1));
			if(!d2.TryGetValue(key2, out T1 result)) return false;
			return d1.Comparer.Equals(result, key1);
		}
		public void Add(T1 key1, TMany key2) {
			if(key2 == null) throw new ArgumentNullException(nameof(key2)); // Check first, to prevent inconsistent state if the
			if(d2.ContainsKey(key2)) throw new ArgumentException("Key '" + nameof(key2) + "' already present."); // 2nd Add() would throw.
			d1.Add(key1, key2);
			d2.Add(key2, key1);
		}
		public bool Remove(T1 key) {
			if(!d1.AnyValues(key)) return false;
			foreach(var tmany in d1[key]) {
				d2.Remove(tmany);
			}
			d1.Clear(key);
			return true;
		}
		public bool Remove(TMany key) {
			if(!d2.TryGetValue(key, out T1 result)) return false;
			d1.Remove(result, key);
			d2.Remove(key);
			return true;
		}
		public bool Remove(T1 key1, TMany key2) {
			if(!Contains(key1, key2)) return false;
			return Remove(key2);
		}
		//todo: needs testing:
		public IEnumerator<(T1 one, TMany two)> GetEnumerator() {
			foreach(var pair in d1.GetAllKeyValuePairs()) yield return (pair.Key, pair.Value);
		}
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		public IEnumerable<IGrouping<T1, TMany>> GroupsByT1 => d1;
		public IEqualityComparer<T1> ComparerT1 => d1.Comparer;
		public IEqualityComparer<TMany> ComparerTMany => d2.Comparer;
		public ICollection<T1> KeysT1 => d1.GetAllKeys();
		public ICollection<TMany> KeysTMany => d2.Keys;
		public void Clear() {
			d1.Clear();
			d2.Clear();
		}
	}
	public class BimapManyToMany<T1, T2> : IEnumerable<(T1 one, T2 two)> {
		protected MultiValueDictionary<T1, T2> d1;
		protected MultiValueDictionary<T2, T1> d2;
		public BimapManyToMany() : this(null, null) { }
		public BimapManyToMany(IEqualityComparer<T1> comparer1, IEqualityComparer<T2> comparer2) {
			if(comparer1 == null) comparer1 = EqualityComparer<T1>.Default;
			if(comparer2 == null) comparer2 = EqualityComparer<T2>.Default;
			d1 = MultiValueDictionary<T1, T2>.Create<HashSet<T2>>(comparer1); // Using a hashset to automatically ignore duplicates
			d2 = MultiValueDictionary<T2, T1>.Create<HashSet<T1>>(comparer2);
		}
		public IEnumerable<T1> this[T2 key] {
			get => d2[key];
			set {
				if(value == null) throw new ArgumentNullException("value");
				foreach(var t1 in d2[key]) {
					d1.Remove(t1, key); // Break any links between 'obj' and the replaced collection.
				}
				d2[key] = value;
				foreach(var t1 in value) {
					d1.Add(t1, key);
				}
			}
		}
		public IEnumerable<T2> this[T1 key] {
			get => d1[key];
			set {
				if(value == null) throw new ArgumentNullException("value");
				foreach(var t2 in d1[key]) {
					d2.Remove(t2, key);
				}
				d1[key] = value;
				foreach(var t2 in value) {
					d2.Add(t2, key);
				}
			}
		}
		public bool TryGetValues(T2 key, out IEnumerable<T1> values) => d2.TryGetValues(key, out values);
		public bool TryGetValues(T1 key, out IEnumerable<T2> values) => d1.TryGetValues(key, out values);
		public bool Contains(T1 key) => d1.AnyValues(key);
		public bool Contains(T2 key) => d2.AnyValues(key);
		public bool Contains(T1 key1, T2 key2) {
			if(key2 == null) throw new ArgumentNullException(nameof(key2));
			return d1.Contains(key1, key2); // (This could be optimized, esp. if I knew the counts.)
		}
		public void Add(T1 key1, T2 key2) {
			if(key2 == null) throw new ArgumentNullException(nameof(key2)); // Check first, to prevent inconsistent state if the 2nd Add() would throw.
			d1.Add(key1, key2);
			d2.Add(key2, key1);
		}
		public bool Remove(T1 key) {
			if(!d1.AnyValues(key)) return false;
			foreach(var t2 in d1[key]) {
				d2.Remove(t2, key);
			}
			d1.Clear(key);
			return true;
		}
		public bool Remove(T2 key) {
			if(!d2.AnyValues(key)) return false;
			foreach(var t1 in d2[key]) {
				d1.Remove(t1, key);
			}
			d2.Clear(key);
			return true;
		}
		public bool Remove(T1 key1, T2 key2) => d1.Remove(key1, key2) | d2.Remove(key2, key1); // Note the non-short-circuiting operator!
		//todo: needs testing:
		public IEnumerator<(T1 one, T2 two)> GetEnumerator() {
			foreach(var pair in d1.GetAllKeyValuePairs()) {
				yield return (pair.Key, pair.Value);
			}
		}
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		public IEqualityComparer<T1> ComparerT1 => d1.Comparer;
		public IEqualityComparer<T2> ComparerT2 => d2.Comparer;
		public ICollection<T1> KeysT1 => d1.GetAllKeys();
		public ICollection<T2> KeysT2 => d2.GetAllKeys();
		public IEnumerable<IGrouping<T1, T2>> GroupsByT1 => d1;
		public IEnumerable<IGrouping<T2, T1>> GroupsByT2 => d2;
		public void Clear() {
			d1.Clear();
			d2.Clear();
		}
	}
	/// <summary>
	/// A priority queue with stable sorting (i.e. preservation of insertion order)
	/// </summary>
	/// <typeparam name="T">The type of element in this priority queue</typeparam>
	/// <typeparam name="TSortKey">The type of the sort key that will be used to order the elements</typeparam>
	public class PriorityQueue<T, TSortKey> : IEnumerable<T>, IReadOnlyCollection<T> {
		/// <param name="keySelector">The function by which the sort key is found, for each element</param>
		/// <param name="descending">If set to true, the elements with the highest sort order will be dequeued first, instead of last</param>
		public PriorityQueue(Func<T, TSortKey> keySelector, bool descending = false)
			: this(keySelector, Comparer<TSortKey>.Default.Compare, descending) { }
		/// <param name="keySelector">The function by which the sort key is found, for each element</param>
		/// <param name="compare">Defines a custom comparer for sort keys</param>
		/// <param name="descending">If set to true, the elements with the highest sort order will be dequeued first, instead of last</param>
		public PriorityQueue(Func<T, TSortKey> keySelector, Func<TSortKey, TSortKey, int> compare, bool descending = false) {
			if(keySelector == null) throw new ArgumentNullException(nameof(keySelector));
			if(compare == null) compare = Comparer<TSortKey>.Default.Compare;
			set = new SortedSet<PQElement>(new PriorityQueueComparer(descending, keySelector, compare));
			DescendingOrder = descending;
		}
		/// <param name="keySelector">The function by which the sort key is found, for each element</param>
		/// <param name="collection">The priority queue will be initialized with this collection</param>
		/// <param name="descending">If set to true, the elements with the highest sort order will be dequeued first, instead of last</param>
		public PriorityQueue(Func<T, TSortKey> keySelector, IEnumerable<T> collection, bool descending = false)
			: this(keySelector, (Func<TSortKey, TSortKey, int>)null, descending) {
			if(collection != null) foreach(T item in collection) Enqueue(item);
		}
		/// <param name="keySelector">The function by which the sort key is found, for each element</param>
		/// <param name="collection">The priority queue will be initialized with this collection</param>
		/// <param name="compare">Defines a custom comparer for sort keys</param>
		/// <param name="descending">If set to true, the elements with the highest sort order will be dequeued first, instead of last</param>
		public PriorityQueue(Func<T, TSortKey> keySelector, IEnumerable<T> collection, Func<TSortKey, TSortKey, int> compare = null, bool descending = false)
			: this(keySelector, compare, descending) {
			if(collection != null) foreach(T item in collection) Enqueue(item);
		}
		/// <param name="keySelector">The function by which the sort key is found, for each element</param>
		/// <param name="compare">Defines a custom comparer for sort keys</param>
		/// <param name="descending">If set to true, the elements with the highest sort order will be dequeued first, instead of last</param>
		public PriorityQueue(Func<T, TSortKey> keySelector, IComparer<TSortKey> comparer, bool descending = false)
			: this(keySelector, (comparer == null)? (Func<TSortKey, TSortKey, int>)null : comparer.Compare, descending) { }
		/// <param name="keySelector">The function by which the sort key is found, for each element</param>
		/// <param name="collection">The priority queue will be initialized with this collection</param>
		/// <param name="compare">Defines a custom comparer for sort keys</param>
		/// <param name="descending">If set to true, the elements with the highest sort order will be dequeued first, instead of last</param>
		public PriorityQueue(Func<T, TSortKey> keySelector, IEnumerable<T> collection, IComparer<TSortKey> comparer, bool descending = false)
			: this(keySelector, collection, (comparer == null) ? (Func<TSortKey,TSortKey,int>)null : comparer.Compare, descending) { }
		private SortedSet<PQElement> set;
		private struct PQElement {
			public readonly int idx;
			public readonly T item;
			public PQElement(int idx, T item) {
				this.idx = idx;
				this.item = item;
			}
		}
		private class PriorityQueueComparer : Comparer<PQElement> {
			private readonly bool descending;
			private readonly Func<T, TSortKey> getSortKey;
			private readonly Func<TSortKey, TSortKey, int> compare;
			public int SortKeyCompare(TSortKey x, TSortKey y) => compare(x, y);
			public TSortKey GetSortKey(T item) => getSortKey(item);
			public PriorityQueueComparer(bool descending, Func<T, TSortKey> keySelector, Func<TSortKey, TSortKey, int> compare) {
				this.descending = descending;
				getSortKey = keySelector;
				this.compare = compare;
			}
			public override int Compare(PQElement x, PQElement y) {
				int primarySort;
				if(descending) primarySort = compare(getSortKey(y.item), getSortKey(x.item)); // Flip x & y for descending order.
				else primarySort = compare(getSortKey(x.item), getSortKey(y.item));
				if(primarySort != 0) return primarySort;
				else return Comparer<int>.Default.Compare(x.idx, y.idx); // Use insertion order as the final tiebreaker.
			}
		}
		/// <summary>
		/// If true, the elements with the highest sort order will be dequeued first, instead of last
		/// </summary>
		public bool DescendingOrder { get; protected set; }
		private static int nextIdx = 0;
		/// <summary>
		/// Inserts a new element into the queue according to its sort order
		/// </summary>
		public void Enqueue(T item) {
			if(item == null) throw new ArgumentNullException("item");
			set.Add(new PQElement(nextIdx++, item));
		}
		/// <summary>
		/// Removes and returns the element with the lowest or highest sort order (depending on DescendingOrder)
		/// </summary>
		public T Dequeue() {
			if(set.Count == 0) throw new InvalidOperationException("The PriorityQueue is empty.");
			PQElement next = set.Min;
			set.Remove(next);
			return next.item;
		}
		/// <summary>
		/// The number of elements in the collection
		/// </summary>
		public int Count => set.Count;
		/// <summary>
		/// Remove all elements from the collection
		/// </summary>
		public void Clear() => set.Clear();
		/// <summary>
		/// Returns true if the given element is present in the collection
		/// </summary>
		public bool Contains(T item) {
			if(item == null) throw new ArgumentNullException("item");
			return set.Any(x => x.item.Equals(item));
		}
		/// <summary>
		/// Returns (but does not remove) the element with the lowest or highest sort order (depending on DescendingOrder)
		/// </summary>
		public T Peek() {
			if(set.Count == 0) throw new InvalidOperationException("The PriorityQueue is empty.");
			return set.Min.item;
		}
		/// <summary>
		/// Removes the first instance of the given element from the collection. This is an O(n) operation.
		/// </summary>
		public bool Remove(T item) {
			if(item == null) throw new ArgumentNullException("item");
			PQElement? found = null;
			foreach(var element in set) { // Linear search is the best we can do, given our constraints.
				if(element.item.Equals(item)) {
					found = element;
					break;
				}
			}
			if(found == null) return false;
			set.Remove(found.Value);
			return true;

		}
		/// <summary>
		/// Removes all instances of the given element from the collection. Returns the number of instances removed. This is an O(n) operation.
		/// </summary>
		public int RemoveAll(T item) {
			if(item == null) throw new ArgumentNullException("item");
			List<PQElement> found = GetAllEntries(item);
			foreach(var pqElement in found) set.Remove(pqElement);
			return found.Count;
		}
		/// <summary>
		/// Change the priority of an element (without changing its insertion order). Returns false if not found, but the sort key will be updated regardless. This is an O(n) operation.
		/// </summary>
		/// <param name="change">The delegate that will actually change the element's sort key</param>
		public bool ChangePriority(T item, Action<T> change) {
			if(change == null) throw new ArgumentNullException("change");
			return ChangePriority(item, () => change(item));
		}
		/// <summary>
		/// Change the priority of an element (without changing its insertion order). Returns false if not found, but the sort key will be updated regardless. This is an O(n) operation.
		/// </summary>
		/// <param name="change">The delegate that will actually change the element's sort key</param>
		public bool ChangePriority(T item, Action change) {
			if(item == null) throw new ArgumentNullException("item");
			if(change == null) throw new ArgumentNullException("change");
			List<PQElement> found = GetAllEntries(item);
			// Remove the elements before changing - otherwise, the set can't find them:
			foreach(var pqElement in found) set.Remove(pqElement);
			change();
			// Add the elements again with new priorities:
			foreach(var pqElement in found) set.Add(pqElement);
			return found.Count > 0;
		}
		private List<PQElement> GetAllEntries(T item) {
			List<PQElement> found = new List<PQElement>();
			PriorityQueueComparer comparer = null;
			TSortKey itemSortKey = default(TSortKey);
			foreach(var element in set) {
				// After the first one is found, any duplicate entries must have the same SortKeyCompare result:
				if(found.Count > 0 && comparer.SortKeyCompare(comparer.GetSortKey(element.item),itemSortKey) != 0) {
					break; //todo, test this (for both changepriority and removeall)
				}
				if(element.item.Equals(item)) {
					found.Add(element);
					if(comparer == null) {
						comparer = set.Comparer as PriorityQueueComparer;
						itemSortKey = comparer.GetSortKey(item);
					}
				}
			}
			return found;
		}
		public IEnumerable<T> GetAllInInsertionOrder() {
			foreach(var pqelement in set.OrderBy(x => x.idx)) yield return pqelement.item;
		}
		public IEnumerator<T> GetEnumerator() {
			foreach(var x in set) yield return x.item;
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	/// <summary>
	/// Maintains a totally ordered set, offering constant time comparison of the relative order of its elements.
	/// </summary>
	public class OrderingCollection<T> : IEnumerable<T> {
		// Based on "Two Algorithms for Maintaining Order in a List", Sleator and Dietz, 1988
		private class OrderingNode {
			public T Element;
			public ulong Label;
			public OrderingNode(T element, ulong label) {
				Element = element;
				Label = label;
			}
		}

		private LinkedListNode<OrderingNode> BaseRecord;

		private LinkedList<OrderingNode> list;

		private DefaultValueDictionary<T, LinkedListNode<OrderingNode>> dict;

		public OrderingCollection() : this(null, null) { }
		public OrderingCollection(IEqualityComparer<T> comparer) : this(null, comparer) { }
		public OrderingCollection(IEnumerable<T> collection, IEqualityComparer<T> comparer) {
			list = new LinkedList<OrderingNode>();
			BaseRecord = new LinkedListNode<OrderingNode>(new OrderingNode(default(T), 0UL));
			list.AddFirst(BaseRecord);
			dict = new DefaultValueDictionary<T, LinkedListNode<OrderingNode>>(comparer);
			if(collection != null) {
				foreach(var element in collection) InsertAtEnd(element);
			}
		}

		public IEnumerator<T> GetEnumerator() {
			OrderingNode baseRecordNode = BaseRecord.Value;
			foreach(var node in list) {
				if(node != baseRecordNode) yield return node.Element;
			}
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public bool Contains(T element) => dict.ContainsKey(element);

		public int Compare(T first, T second) {
			var firstNode = dict[first];
			var secondNode = dict[second];
			if(firstNode == null) throw new KeyNotFoundException("Element 'first' not present in collection.");
			if(secondNode == null) throw new KeyNotFoundException("Element 'second' not present in collection.");
			return LabelRelativeToBase(firstNode).CompareTo(LabelRelativeToBase(secondNode));
		}

		public int Count => dict.Count;

		public bool Remove(T element) {
			var node = dict[element];
			if(node == null) return false;
			list.Remove(node);
			dict.Remove(element);
			return true;
		}

		/// <summary>
		/// Inserts a new element before (with a lower value than) all other elements in the ordering.
		/// </summary>
		public void InsertAtStart(T newElement) => InsertAfter(BaseRecord, newElement);

		/// <summary>
		/// Inserts a new element after (with a higher value than) all other elements in the ordering.
		/// </summary>
		public void InsertAtEnd(T newElement) => InsertAfter(list.Last, newElement);

		/// <summary>
		/// Insert a new element directly before an existing element. (If beforeElement is null, the new element is inserted at the end.)
		/// </summary>
		/// <param name="beforeElement">The existing element already in the collection</param>
		public void InsertBefore(T beforeElement, T newElement) {
			if(beforeElement == null) {
				InsertAfter(list.Last, newElement);
			}
			else {
				var node = dict[beforeElement];
				if(node == null) throw new KeyNotFoundException("Can't insert before an element that isn't in the collection.");
				InsertAfter(node.Previous, newElement);
			}
		}

		/// <summary>
		/// Insert a new element directly after an existing element. (If afterElement is null, the new element is inserted at the start.)
		/// </summary>
		/// <param name="afterElement">The existing element already in the collection</param>
		public void InsertAfter(T afterElement, T newElement) {
			if(afterElement == null) {
				InsertAfter(BaseRecord, newElement);
			}
			else {
				var node = dict[afterElement];
				if(node == null) throw new KeyNotFoundException("Can't insert after an element that isn't in the collection.");
				InsertAfter(node, newElement);
			}
		}

		private ulong LabelRelativeToBase(LinkedListNode<OrderingNode> record) {
			unchecked { return record.Value.Label - BaseRecord.Value.Label; }
		}

		private ulong? NextLabelRelativeToBase(LinkedListNode<OrderingNode> record) {
			var next = record.GetNextCircular();
			if(next == BaseRecord) return null; // return M
			else return LabelRelativeToBase(next);
		}

		/// <summary>
		/// Returns w_i for values of i greater than zero. If start and finish are the same, returns null (for M) rather than zero.
		/// </summary>
		private ulong? LabelDistance(LinkedListNode<OrderingNode> start, LinkedListNode<OrderingNode> finish) {
			if(start == finish) return null; // return M

			unchecked {
				return finish.Value.Label - start.Value.Label;
			}
		}

		// Here we want to find the ulong closest to x * (MAX_ULONG + 1) for a double x (0.0 - 1.0).
		// So we'll do x*MAX_ULONG + x.
		//ulong GetPortionOfM(double x) => (ulong)(x*ulong.MaxValue + x);

		private ulong GetFractionOfM(ulong n, ulong d) {
			double x = (double)n / (double)d;
			return (ulong)(x*(double)ulong.MaxValue + x);
		}

		// Max for decimal is (ballpark) around 10 million times higher than max for ulong.
		// Therefore, overflow could occur if n exceeds 10 million.
		private ulong GetFraction(ulong x, ulong n, ulong d) {
			decimal decX = x, decN = n, decD = d;
			return (ulong)(decX * decN / decD); // n is expected to be fairly small here.
		}

		private void InsertAfter(LinkedListNode<OrderingNode> afterNode, T newElement) {
			if(newElement == null) throw new ArgumentNullException("newElement", "Collection does not support null entries.");
			if(dict.ContainsKey(newElement)) throw new InvalidOperationException("Element is already present in collection.");
			// Find the starting point for our possible relabeling.

			ulong j = 1UL;
			LinkedListNode<OrderingNode> current = afterNode.GetNextCircular();

			while(LabelDistance(afterNode, current) != null && LabelDistance(afterNode, current).Value <= j * j) {
				++j;
				current = current.GetNextCircular();
			}

			ulong? finalLabel = LabelDistance(afterNode, current);

			// Now, relabel (j-1) records:
			current = afterNode.GetNextCircular();
			for(ulong k = 1UL;k<j;++k) {
				ulong relabel;
				if(finalLabel == null) {
					unchecked {
						relabel = GetFractionOfM(k, j) + afterNode.Value.Label;
					}
				}
				else {
					unchecked {
						relabel = GetFraction(finalLabel.Value, k, j) + afterNode.Value.Label;
					}
				}
				current.Value.Label = relabel;
				current = current.GetNextCircular();
			}

			// Insert the new element:
			const ulong halfM = 1UL << 63; // 2^63, half of 2^64

			ulong newLabelRelativeToBase;
			if(NextLabelRelativeToBase(afterNode) == null) {
				// Averaging with M, so use halfM:
				newLabelRelativeToBase = (LabelRelativeToBase(afterNode)/2UL) + halfM;
			}
			else {
				// Carefully avoid off-by-one errors here...
				newLabelRelativeToBase = (NextLabelRelativeToBase(afterNode).Value - LabelRelativeToBase(afterNode))/2UL
					+ LabelRelativeToBase(afterNode);
			}
			ulong newLabel;
			unchecked {
				newLabel = newLabelRelativeToBase + BaseRecord.Value.Label;
			}
			var newNode = new LinkedListNode<OrderingNode>(new OrderingNode(newElement, newLabel));
			list.AddAfter(afterNode, newNode);
			dict[newElement] = newNode;
		}
	}
}

namespace UtilityCollectionsExtensions {
	using UtilityCollections;
	public static class Extensions {
		public static T1 GetT1<T1, T2>(this BimapOneToOne<T1, T2> bimap, T2 obj) => bimap[obj];
		public static T2 GetT2<T1, T2>(this BimapOneToOne<T1, T2> bimap, T1 obj) => bimap[obj];
		public static bool ContainsT1<T1, T2>(this BimapOneToOne<T1, T2> bimap, T1 obj) => bimap.Contains(obj);
		public static bool ContainsT2<T1, T2>(this BimapOneToOne<T1, T2> bimap, T2 obj) => bimap.Contains(obj);
		public static bool RemoveT1<T1, T2>(this BimapOneToOne<T1, T2> bimap, T1 obj) => bimap.Remove(obj);
		public static bool RemoveT2<T1, T2>(this BimapOneToOne<T1, T2> bimap, T2 obj) => bimap.Remove(obj);
		public static bool TryGetValueT1<T1, T2>(this BimapOneToOne<T1, T2> bimap, T1 key, out T2 value) => bimap.TryGetValue(key, out value);
		public static bool TryGetValueT2<T1, T2>(this BimapOneToOne<T1, T2> bimap, T2 key, out T1 value) => bimap.TryGetValue(key, out value);

		public static T1 GetT1<T1, TMany>(this BimapOneToMany<T1, TMany> bimap, TMany obj) => bimap[obj];
		public static IEnumerable<TMany> GetTMany<T1, TMany>(this BimapOneToMany<T1, TMany> bimap, T1 obj) => bimap[obj];
		public static bool ContainsT1<T1, TMany>(this BimapOneToMany<T1, TMany> bimap, T1 obj) => bimap.Contains(obj);
		public static bool ContainsTMany<T1, TMany>(this BimapOneToMany<T1, TMany> bimap, TMany obj) => bimap.Contains(obj);
		public static bool RemoveT1<T1, TMany>(this BimapOneToMany<T1, TMany> bimap, T1 obj) => bimap.Remove(obj);
		public static bool RemoveTMany<T1, TMany>(this BimapOneToMany<T1, TMany> bimap, TMany obj) => bimap.Remove(obj);

		public static IEnumerable<T1> GetT1<T1, T2>(this BimapManyToMany<T1, T2> bimap, T2 obj) => bimap[obj];
		public static IEnumerable<T2> GetT2<T1, T2>(this BimapManyToMany<T1, T2> bimap, T1 obj) => bimap[obj];
		public static bool ContainsT1<T1, T2>(this BimapManyToMany<T1, T2> bimap, T1 obj) => bimap.Contains(obj);
		public static bool ContainsT2<T1, T2>(this BimapManyToMany<T1, T2> bimap, T2 obj) => bimap.Contains(obj);
		public static bool RemoveT1<T1, T2>(this BimapManyToMany<T1, T2> bimap, T1 obj) => bimap.Remove(obj);
		public static bool RemoveT2<T1, T2>(this BimapManyToMany<T1, T2> bimap, T2 obj) => bimap.Remove(obj);
		public static bool TryGetValuesT1<T1, T2>(this BimapManyToMany<T1, T2> bimap, T1 key, out IEnumerable<T2> values) => bimap.TryGetValues(key, out values);
		public static bool TryGetValuesT2<T1, T2>(this BimapManyToMany<T1, T2> bimap, T2 key, out IEnumerable<T1> values) => bimap.TryGetValues(key, out values);
	}
}

namespace CircularLinkedListExtensions {
	public static class Extensions {
		public static LinkedListNode<T> GetNextCircular<T>(this LinkedListNode<T> node) {
			return node.Next ?? node.List.First;
		}
		public static LinkedListNode<T> GetPreviousCircular<T>(this LinkedListNode<T> node) {
			return node.Previous ?? node.List.Last;
		}
	}
}
