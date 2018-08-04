using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SerializationUtility {
	//todo, needs tests
	public static class SerializationHelper {
		private static class DynamicTypeInfo<T> {
			public static int TypeIndex;
			public static Action<T, SaveContext> Save;
			public static Func<LoadContext, T> Load;
		}

		private static List<Type> types = new List<Type>();
		private static HashSet<Type> typeIsRegistered = new HashSet<Type>();

		//todo: make sure the xml & docs reflect that ALL object types should be registered, not just the subclassed ones.
		public static void RegisterType<T>(Action<T, SaveContext> save, Func<LoadContext, T> load) where T : class {
			// Check the hashset first, to achieve idempotence (assuming equivalent save/load methods):
			if(typeIsRegistered.Add(typeof(T))) {
				types.Add(typeof(T));
				DynamicTypeInfo<T>.TypeIndex = types.IndexOf(typeof(T));
			}
			DynamicTypeInfo<T>.Save = save;
			DynamicTypeInfo<T>.Load = load;
		}

		private static class DynamicStructTypeInfo<T> {
			public static Action<T, SaveContext> Save;
			public static Func<LoadContext, T> Load;
		}

		// No 'struct' constraint on this one; ref types are explicitly okay here (but must do their own null checks).
		public static void RegisterStructType<T>(Action<T, SaveContext> save, Func<LoadContext, T> load) {
			DynamicStructTypeInfo<T>.Save = save;
			DynamicStructTypeInfo<T>.Load = load;
		}

		private static void SerializeDynamic(object o, SaveContext context) {
			SerializeDynamicInternal((dynamic)o, context);
		}

		private static void SerializeDynamicInternal<T>(T item, SaveContext context) {
			int typeIdx = DynamicTypeInfo<T>.TypeIndex;
			var save = DynamicTypeInfo<T>.Save;
			context.Write(typeIdx);
			save(item, context);
		}

		private static T DeserializeDynamic<T>(LoadContext context) {
			return (T)DeserializeDynamicInternal(context);
		}

		private static object DeserializeDynamicInternal(LoadContext context) {
			int typeIdx = context.ReadInt();
			Type itemType = types[typeIdx];
			Type staticType = typeof(DynamicTypeInfo<>).MakeGenericType(itemType);
			// If there are perf problems, see whether caching fieldinfo could help:
			FieldInfo loadField = staticType.GetField("Load", BindingFlags.Static | BindingFlags.Public);
			object loadFunc = loadField.GetValue(null);
			object item = (loadFunc as Delegate).DynamicInvoke(context);
			return item;
		}

		public class SaveContext {
			public SaveContext(Stream s) { stream = new BinaryWriter(s); }

			protected BinaryWriter stream;

			public BinaryWriter Writer => stream;

			protected Dictionary<object, int> objIds = new Dictionary<object, int>();

			protected int nextId = 1;

			public void WriteObject<T>(T obj) where T : class {
				if(obj == null) {
					// id 0 is reserved for null:
					stream.Write(0);
					return;
				}

				if(objIds.TryGetValue(obj, out int existingId)) {
					// Object already has an ID and has already been (or is currently being) written,
					// so just write the ID this time:
					stream.Write(existingId);
					return;
				}

				int newId = nextId;
				nextId++;
				objIds.Add(obj, newId);
				stream.Write(newId);
				SerializeDynamic(obj, this);
			}

			//todo, xml note here about no strict struct req?
			public void WriteStruct<T>(T value) {
				// No null check, no ref comparison, and no dynamic subtype resolution:
				DynamicStructTypeInfo<T>.Save(value, this);
			}

			public void Write(bool value) => stream.Write(value);
			public void Write(byte value) => stream.Write(value);
			public void Write(char value) => stream.Write(value);
			public void Write(decimal value) => stream.Write(value);
			public void Write(double value) => stream.Write(value);
			public void Write(float value) => stream.Write(value);
			public void Write(int value) => stream.Write(value);
			public void Write(long value) => stream.Write(value);
			public void Write(sbyte value) => stream.Write(value);
			public void Write(short value) => stream.Write(value);
			public void Write(uint value) => stream.Write(value);
			public void Write(ulong value) => stream.Write(value);
			public void Write(ushort value) => stream.Write(value);

			//todo, add xml note that null will be saved correctly here
			public void Write(string value) {
				if(WriteIsNotNull(value)) {
					stream.Write(value);
				}
			}

			public void WriteNullable(bool? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(byte? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(char? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(decimal? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(double? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(float? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(int? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(long? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(sbyte? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(short? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(uint? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(ulong? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }
			public void WriteNullable(ushort? value) { if(WriteIsNotNull(value)) { Write(value.Value); } }

			public void WriteNullableStruct<T>(T? value) where T : struct {
				if(WriteIsNotNull(value)) {
					DynamicStructTypeInfo<T>.Save(value.Value, this);
				}
			}

			public void WriteArray(bool[] values) => WriteOtherArray(values, Write);
			public void WriteArray(decimal[] values) => WriteOtherArray(values, Write);
			public void WriteArray(double[] values) => WriteOtherArray(values, Write);
			public void WriteArray(float[] values) => WriteOtherArray(values, Write);
			public void WriteArray(int[] values) => WriteOtherArray(values, Write);
			public void WriteArray(long[] values) => WriteOtherArray(values, Write);
			public void WriteArray(sbyte[] values) => WriteOtherArray(values, Write);
			public void WriteArray(short[] values) => WriteOtherArray(values, Write);
			public void WriteArray(uint[] values) => WriteOtherArray(values, Write);
			public void WriteArray(ulong[] values) => WriteOtherArray(values, Write);
			public void WriteArray(ushort[] values) => WriteOtherArray(values, Write);
			public void WriteArray(string[] values) => WriteOtherArray(values, Write);
			public void WriteArray(byte[] values) {
				if(WriteIsNotNull(values)) {
					stream.Write(values.Length);
					stream.Write(values);
				}
			}
			public void WriteArray(char[] values) {
				if(WriteIsNotNull(values)) {
					stream.Write(values.Length);
					stream.Write(values);
				}
			}

			public void WriteArray(IEnumerable<bool> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<decimal> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<double> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<float> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<int> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<long> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<sbyte> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<short> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<uint> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<ulong> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<ushort> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<string> values) => WriteOtherArray(values?.ToArray(), Write);
			public void WriteArray(IEnumerable<byte> values) => WriteArray(values?.ToArray());
			public void WriteArray(IEnumerable<char> values) => WriteArray(values?.ToArray());

			public void WriteObjectArray<T>(T[] values) where T : class => WriteOtherArray(values, WriteObject);
			public void WriteObjectArray<T>(IEnumerable<T> values) where T : class => WriteOtherArray(values?.ToArray(), WriteObject);

			public void WriteStructArray<T>(T[] values) => WriteOtherArray(values, WriteStruct);
			public void WriteStructArray<T>(IEnumerable<T> values) => WriteOtherArray(values?.ToArray(), WriteStruct);

			public void WriteNullableArray(bool?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(byte?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(char?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(decimal?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(double?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(float?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(int?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(long?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(sbyte?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(short?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(uint?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(ulong?[] values) => WriteOtherArray(values, WriteNullable);
			public void WriteNullableArray(ushort?[] values) => WriteOtherArray(values, WriteNullable);

			public void WriteNullableArray(IEnumerable<bool?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<byte?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<char?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<decimal?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<double?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<float?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<int?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<long?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<sbyte?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<short?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<uint?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<ulong?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);
			public void WriteNullableArray(IEnumerable<ushort?> values) => WriteOtherArray(values?.ToArray(), WriteNullable);

			public void WriteNullableStructArray<T>(T?[] values) where T : struct => WriteOtherArray(values, WriteNullableStruct);
			public void WriteNullableStructArray<T>(IEnumerable<T?> values) where T : struct => WriteOtherArray(values?.ToArray(), WriteNullableStruct);

			public void WriteOtherArray<T>(IEnumerable<T> values, Action<T> save) => WriteOtherArray(values?.ToArray(), save);
			public void WriteOtherArray<T>(T[] values, Action<T> save) {
				if(WriteIsNotNull(values)) {
					stream.Write(values.Length);
					foreach(var value in values) {
						save(value);
					}
				}
			}
			public void WriteDictionary<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> dict, Action<TKey> saveKey, Action<TValue> saveValue) {
				if(WriteIsNotNull(dict)) {
					KeyValuePair<TKey, TValue>[] pairs = dict.ToArray();
					stream.Write(pairs.Length);
					foreach(var pair in pairs) {
						saveKey(pair.Key);
						saveValue(pair.Value);
					}
				}
			}
			// Simple types & structs that can be null are handled by writing a bool, not an id of 0:
			protected bool WriteIsNotNull<T>(T value) {
				bool notNull = value != null;
				stream.Write(notNull);
				return notNull;
			}

		}

		public class LoadContext {
			public LoadContext(Stream s) { stream = new BinaryReader(s); }

			protected BinaryReader stream;

			public BinaryReader Reader => stream;

			protected Dictionary<int, object> objectsById = new Dictionary<int, object>();

			// For now I'm assuming that the tree won't be very deep, and there won't be many ids
			// on this stack at a time. If that changes, there's room for improvement here.
			protected Stack<int> idsBeingLoaded = new Stack<int>();

			//todo, add xml note about usage
			public T RegisterPartial<T>(T obj) {
				// Associate this object with the top ID of the stack:
				int currentId = idsBeingLoaded.Peek();
				objectsById[currentId] = obj;
				return obj;
			}

			//todo, xml note somewhere that mentions that T here needn't be registered if it's an abstract base type / interface / etc.
			public T ReadObject<T>() where T : class {
				int id = stream.ReadInt32();
				if(id == 0) return null;
				// If present in the dictionary, it's already loaded:
				if(objectsById.TryGetValue(id, out object loadedObj)) return (T)loadedObj;

				if(idsBeingLoaded.Contains(id)) {
					// If we've started loading this id already, but there is no object reference yet,
					// then an unhandled cycle is present:
					throw new InvalidOperationException($"Unhandled cycle detected: id '{id}' of type '{typeof(T).Name}'.");
				}

				idsBeingLoaded.Push(id);
				T obj = DeserializeDynamic<T>(this);
				idsBeingLoaded.Pop();
				objectsById[id] = obj;
				return obj;
			}

			public T ReadStruct<T>() {
				return DynamicStructTypeInfo<T>.Load(this);
			}

			public bool ReadBool() => stream.ReadBoolean();
			public byte ReadByte() => stream.ReadByte();
			public char ReadChar() => stream.ReadChar();
			public decimal ReadDecimal() => stream.ReadDecimal();
			public double ReadDouble() => stream.ReadDouble();
			public float ReadFloat() => stream.ReadSingle();
			public int ReadInt() => stream.ReadInt32();
			public long ReadLong() => stream.ReadInt64();
			public sbyte ReadSByte() => stream.ReadSByte();
			public short ReadShort() => stream.ReadInt16();
			public uint ReadUInt() => stream.ReadUInt32();
			public ulong ReadULong() => stream.ReadUInt64();
			public ushort ReadUShort() => stream.ReadUInt16();

			public string ReadString() {
				if(ReadIsNotNull()) return stream.ReadString();
				else return null;
			}

			public bool? ReadNullableBool() { if(ReadIsNotNull()) { return stream.ReadBoolean(); } else return null; }
			public byte? ReadNullableByte() { if(ReadIsNotNull()) { return stream.ReadByte(); } else return null; }
			public char? ReadNullableChar() { if(ReadIsNotNull()) { return stream.ReadChar(); } else return null; }
			public decimal? ReadNullableDecimal() { if(ReadIsNotNull()) { return stream.ReadDecimal(); } else return null; }
			public double? ReadNullableDouble() { if(ReadIsNotNull()) { return stream.ReadDouble(); } else return null; }
			public float? ReadNullableFloat() { if(ReadIsNotNull()) { return stream.ReadSingle(); } else return null; }
			public int? ReadNullableInt() { if(ReadIsNotNull()) { return stream.ReadInt32(); } else return null; }
			public long? ReadNullableLong() { if(ReadIsNotNull()) { return stream.ReadInt64(); } else return null; }
			public sbyte? ReadNullableSByte() { if(ReadIsNotNull()) { return stream.ReadSByte(); } else return null; }
			public short? ReadNullableShort() { if(ReadIsNotNull()) { return stream.ReadInt16(); } else return null; }
			public uint? ReadNullableUInt() { if(ReadIsNotNull()) { return stream.ReadUInt32(); } else return null; }
			public ulong? ReadNullableULong() { if(ReadIsNotNull()) { return stream.ReadUInt64(); } else return null; }
			public ushort? ReadNullableUShort() { if(ReadIsNotNull()) { return stream.ReadUInt16(); } else return null; }

			public T? ReadNullableStruct<T>() where T : struct {
				if(ReadIsNotNull()) {
					return DynamicStructTypeInfo<T>.Load(this);
				}
				else return null;
			}

			public bool[] ReadBoolArray() => ReadOtherArray(ReadBool);
			public decimal[] ReadDecimalArray() => ReadOtherArray(ReadDecimal);
			public double[] ReadDoubleArray() => ReadOtherArray(ReadDouble);
			public float[] ReadFloatArray() => ReadOtherArray(ReadFloat);
			public int[] ReadIntArray() => ReadOtherArray(ReadInt);
			public long[] ReadLongArray() => ReadOtherArray(ReadLong);
			public sbyte[] ReadSByteArray() => ReadOtherArray(ReadSByte);
			public short[] ReadShortArray() => ReadOtherArray(ReadShort);
			public string[] ReadStringArray() => ReadOtherArray(ReadString);
			public uint[] ReadUIntArray() => ReadOtherArray(ReadUInt);
			public ulong[] ReadULongArray() => ReadOtherArray(ReadULong);
			public ushort[] ReadUShortArray() => ReadOtherArray(ReadUShort);
			public byte[] ReadByteArray() {
				if(ReadIsNotNull()) {
					int count = stream.ReadInt32();
					return stream.ReadBytes(count);
				}
				else return null;
			}
			public char[] ReadCharArray() {
				if(ReadIsNotNull()) {
					int count = stream.ReadInt32();
					return stream.ReadChars(count);
				}
				else return null;
			}

			public T[] ReadObjectArray<T>() where T : class => ReadOtherArray(ReadObject<T>);

			public T[] ReadStructArray<T>() => ReadOtherArray(ReadStruct<T>);

			public bool?[] ReadNullableBoolArray() => ReadOtherArray(ReadNullableBool);
			public byte?[] ReadNullableByteArray() => ReadOtherArray(ReadNullableByte);
			public char?[] ReadNullableCharArray() => ReadOtherArray(ReadNullableChar);
			public decimal?[] ReadNullableDecimalArray() => ReadOtherArray(ReadNullableDecimal);
			public double?[] ReadNullableDoubleArray() => ReadOtherArray(ReadNullableDouble);
			public float?[] ReadNullableFloatArray() => ReadOtherArray(ReadNullableFloat);
			public int?[] ReadNullableIntArray() => ReadOtherArray(ReadNullableInt);
			public long?[] ReadNullableLongArray() => ReadOtherArray(ReadNullableLong);
			public sbyte?[] ReadNullableSByteArray() => ReadOtherArray(ReadNullableSByte);
			public short?[] ReadNullableShortArray() => ReadOtherArray(ReadNullableShort);
			public uint?[] ReadNullableUIntArray() => ReadOtherArray(ReadNullableUInt);
			public ulong?[] ReadNullableULongArray() => ReadOtherArray(ReadNullableULong);
			public ushort?[] ReadNullableUShortArray() => ReadOtherArray(ReadNullableUShort);

			public T?[] ReadNullableStructArray<T>() where T : struct => ReadOtherArray(ReadNullableStruct<T>);

			public T[] ReadOtherArray<T>(Func<T> load) {
				if(ReadIsNotNull()) {
					int count = stream.ReadInt32();
					T[] result = new T[count];
					for(int i = 0; i<count; ++i) {
						result[i] = load();
					}
					return result;
				}
				else return null;
			}
			public Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(Func<TKey> loadKey, Func<TValue> loadValue) {
				if(ReadIsNotNull()) {
					var result = new Dictionary<TKey, TValue>();
					int count = stream.ReadInt32();
					for(int i = 0; i<count; ++i) {
						TKey key = loadKey();
						TValue value = loadValue();
						result.Add(key, value);
					}
					return result;
				}
				else return null;
			}
			protected bool ReadIsNotNull() => stream.ReadBoolean();
		}
	}
}
