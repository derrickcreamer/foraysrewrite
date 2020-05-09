using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UtilityCollections;

namespace GameComponents {
	//todo: xml: maybe list the main assumptions made by this class:
	// one element per position, one position per element, moves need explicit calls, uhhh...
	// maybe mention the default values returned for empty positions.
	public class Grid<T, TPosition> : IEnumerable<T> where T : class {
		protected BimapOneToOne<T, TPosition> map;
		protected Func<T> getDefault;
		protected Func<TPosition, bool> isInBounds;
		public Grid() : this(null) { }
		public Grid(Func<TPosition, bool> isInBounds) { //todo: add a constructor that takes a rectangle, or at least a helper method.
			map = new BimapOneToOne<T, TPosition>();
			if(isInBounds == null) isInBounds = x => true;
			this.isInBounds = isInBounds;
		}
		//todo: xml: "if set, ..." just like defaultvaluedict.
		public Func<T> GetDefaultElement {
			get => getDefault;
			set => getDefault = value;
		}
		public bool InBounds(TPosition position) => isInBounds(position);
		public TPosition GetPositionOf(T element) => map[element];
		//todo, added but not tested:
		public bool TryGetPositionOf(T element, out TPosition position) => map.TryGetValue(element, out position);
		public T this[TPosition position] {
			get {
				if(map.TryGetValue(position, out T result) || getDefault == null) return result;
				else return getDefault(); // TryGetValue sets 'result' to default(T) if not found.
			}
		}
		public IEnumerable<T> this[IEnumerable<TPosition> positions] {
			get {
				if(positions == null) throw new ArgumentNullException(nameof(positions));
				var returned = new HashSet<T>();
				foreach(TPosition p in positions) {
					if(map.TryGetValue(p, out T result)){
						if(returned.Add(result)) yield return result;
					}
				}
			}
		}
		public bool Contains(T element) => map.Contains(element); // todo: names for these 2 methods?
		public bool HasContents(TPosition position) => map.Contains(position);
		/// <summary>
		/// Add an element to the grid at the given position.
		/// Fails if the element already exists on the grid, or if the destination is out of bounds or already occupied.
		/// Returns true if the element is at the destination (regardless of whether it was already there).
		/// </summary>
		public bool Add(T element, TPosition destination) {
			if(map.TryGetValue(element, out TPosition source)) {
				if(destination == null) throw new ArgumentNullException(nameof(destination)); // Throw instead of returning false if null.
				return source.Equals(destination); // If the element is on the grid already, return false unless already at the destination.
			}
			if(!map.Contains(destination) && isInBounds(destination)) {
				map.Add(element, destination);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Move an element from one position to another.
		/// Fails if the element is not on the grid, or if the destination is out of bounds or already occupied.
		/// Returns true if the element is at the destination (regardless of whether it was already there).
		/// </summary>
		public bool Move(T element, TPosition destination) {
			if(!map.TryGetValue(element, out TPosition source)){
				if(destination == null) throw new ArgumentNullException(nameof(destination));
				return false; // Not on grid, return false.
			}
			if(!map.Contains(destination) && isInBounds(destination)) {
				map[destination] = element;
				return true;
			}
			if(source.Equals(destination)) return true; // Already at destination
			return false;
		}
		/// <summary>
		/// Move any contents of the source position to the destination position.
		/// Fails if source has an element to move and destination is OOB or occupied, preventing the move.
		/// Returns true if no element remains at the source (i.e., if successfully moved or if source was empty) or if source equals destination.
		/// </summary>
		public bool MoveContents(TPosition source, TPosition destination) {
			if(!map.TryGetValue(source, out T element)){
				if(destination == null) throw new ArgumentNullException(nameof(destination));
				return true; // Nothing to move, return true.
			}
			if(map.Contains(destination)) {
				return source.Equals(destination); // Return true if moving to the same position.
			}
			if(!isInBounds(destination)) return false; // Can't move there, return false.
			map[destination] = element;
			return true;
		}
		/// <summary>
		/// Swap the positions of two elements on the grid.
		/// Fails if either element is not on the grid.
		/// </summary>
		public bool Swap(T element1, T element2) {
			if(element2 == null) throw new ArgumentNullException(nameof(element2)); // Check this arg before TryGetValue checks the other.
			if(map.TryGetValue(element1, out TPosition source1) && map.TryGetValue(element2, out TPosition source2)) {
				map[source1] = element2;
				map[source2] = element1;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Swap any contents of two positions.
		/// Fails if it tries to swap an element to a position out of bounds.
		/// </summary>
		public bool SwapContents(TPosition source1, TPosition source2) {
			bool found1 = map.TryGetValue(source1, out T element1);
			if(map.TryGetValue(source2, out T element2)) {
				if(found1) { // Both present: swap
					map[source1] = element2;
					map[source2] = element1;
					return true;
				}
				else { // One present: move
					if(!isInBounds(source1)) return false;
					map[source1] = element2;
					return true;
				}
			}
			else {
				if(found1) { // One present: move
					if(!isInBounds(source2)) return false;
					map[source2] = element1;
					return true;
				}
				else return true; // None present
			}
		}
		/// <summary>
		/// Swap the positions of the given element and any contents of the given position.
		/// Fails if the element is not on the grid, or if the destination is out of bounds.
		/// </summary>
		public bool SwapContents(TPosition source1, T element2) {
			if(!map.TryGetValue(element2, out TPosition source2)){
				if(source1 == null) throw new ArgumentNullException(nameof(source1));
				return false;
			}
			if(map.TryGetValue(source1, out T element1)) { // Both present: swap
				map[source1] = element2;
				map[source2] = element1;
				return true;
			}
			else { // One present: move
				if(!isInBounds(source1)) return false;
				map[source1] = element2;
				return true;
			}
		}
		/// <summary>
		/// Replace an element on the grid with a new element in the same position.
		/// Fails if the old element is not on the grid, or if the new element is already on the grid.
		/// </summary>
		public bool Replace(T replacedElement, T newElement) {
			if(newElement == null) throw new ArgumentNullException(nameof(newElement)); // Check this arg before TryGetValue checks the other.
			if(map.TryGetValue(replacedElement, out TPosition position) && !map.Contains(newElement)) {
				map[position] = newElement;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Replace any contents of the given position with a new element.
		/// Fails if the position is out of bounds or if the new element is already on the grid.
		/// </summary>
		public bool ReplaceContents(TPosition replacedPosition, T newElement) {
			if(replacedPosition == null) throw new ArgumentNullException(nameof(replacedPosition));
			if(!map.Contains(newElement) && isInBounds(replacedPosition)) {
				map[replacedPosition] = newElement;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Remove an element from the grid.
		/// </summary>
		public void Remove(T element) => map.Remove(element);
		/// <summary>
		/// Remove any contents of the given position from the grid.
		/// </summary>
		public void RemoveContents(TPosition position) => map.Remove(position);
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		public IEnumerator<T> GetEnumerator() => map.KeysT1.GetEnumerator();
	}
	/*public class MultiGrid<T, TPosition> : IEnumerable<T> where T : class {
		protected BimapOneToMany<TPosition, T> map;
		protected Func<TPosition, bool> isInBounds;
		public MultiGrid() : this(null) { }
		public MultiGrid(Func<TPosition, bool> isInBounds) {
			map = new BimapOneToMany<TPosition, T>();
			if(isInBounds == null) isInBounds = x => true;
			this.isInBounds = isInBounds;
		}
		public bool InBounds(TPosition position) => isInBounds(position);
		public TPosition GetPositionOf(T element) => map[element];
		public IEnumerable<T> this[TPosition position] {
			get {
				map.TryGetValues(position, out IEnumerable<T> values);
				return values; // Empty, not null, if no values are present. //todo: xml note for this?
			}
		}
		//todo: xml note that this one is flattened.
		public IEnumerable<T> this[IEnumerable<TPosition> positions] {
			get {
				if(positions == null) throw new ArgumentNullException(nameof(positions));
				var returned = new HashSet<TPosition>();
				foreach(TPosition p in positions) {
					if(!returned.Add(p)) continue; // If this position has already been considered, skip it.
					if(map.TryGetValues(p, out IEnumerable<T> values)) {
						foreach(var result in values) {
							yield return result;
						}
					}
				}
			}
		}
		public IEnumerable<IGrouping<TPosition, T>> GetGroupedElements(IEnumerable<TPosition> positions) { //todo: name?
			var returned = new HashSet<TPosition>();
			foreach(TPosition p in positions) {
				if(!returned.Add(p)) continue; // If this position has already been considered, skip it.
				if(map.TryGetValues(p, out IEnumerable<T> values)) {
					yield return new Grouping<TPosition, T>(p, values);
				}
			}
		}
		public bool Contains(T element) => map.Contains(element); // todo: names?
		public bool HasContents(TPosition position) => map.Contains(position);
		/// <summary>
		/// Add an element to the grid at the given position.
		/// Fails if the element already exists on the grid, or if the destination is out of bounds.
		/// Returns true if the element is at the destination (regardless of whether it was already there).
		/// </summary>
		public bool Add(T element, TPosition destination) {
			if(destination == null) throw new ArgumentNullException(nameof(destination));
			if(map.TryGetValue(element, out TPosition source)) {
				return source.Equals(destination); // If the element is on the grid already, return false unless already at the destination.
			}
			if(isInBounds(destination)) {
				map.Add(destination, element);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Move an element from one position to another.
		/// Fails if the element is not on the grid, or if the destination is out of bounds.
		/// Returns true if the element is at the destination (regardless of whether it was already there).
		/// </summary>
		public bool Move(T element, TPosition destination) {
			if(destination == null) throw new ArgumentNullException(nameof(destination));
			if(!map.TryGetValue(element, out TPosition source)) return false; // Not on grid, return false.
			if(isInBounds(destination)) {
				map[element] = destination;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Move any contents of the source position to the destination position.
		/// Fails if source has element(s) to move and destination is OOB, preventing the move.
		/// Returns true if no element remains at the source. (i.e., returns true if successfully moved or if source was empty.)
		/// </summary>
		public bool MoveContents(TPosition source, TPosition destination) {
			if(destination == null) throw new ArgumentNullException(nameof(destination));
			if(!map.TryGetValues(source, out IEnumerable<T> contents)) return true; // Nothing to move, return true.
			if(!isInBounds(destination)) return false; // Can't move there, return false.
			foreach(var element in contents.ToArray()) map[element] = destination;
			return true;
		}
		/// <summary>
		/// Swap the positions of two elements on the grid.
		/// Fails if either element is not on the grid.
		/// </summary>
		public bool Swap(T element1, T element2) {
			if(element2 == null) throw new ArgumentNullException(nameof(element2));
			if(map.TryGetValue(element1, out TPosition source1) && map.TryGetValue(element2, out TPosition source2)) {
				map[element1] = source2;
				map[element2] = source1;
				return true;
			}
			return false;
		}
		/// <summary>
		/// Swap any contents of two positions.
		/// Fails if it tries to swap any element(s) to a position out of bounds.
		/// </summary>
		public bool SwapContents(TPosition source1, TPosition source2) {
			bool found1 = map.TryGetValues(source1, out IEnumerable<T> contents1);
			if(map.TryGetValues(source2, out IEnumerable<T> contents2)) {
				if(found1) { // Both present: swap
					contents1 = contents1.ToArray();
					contents2 = contents2.ToArray();
					foreach(var element1 in contents1) map[element1] = source2;
					foreach(var element2 in contents2) map[element2] = source1;
					return true;
				}
				else { // One present: move
					if(!isInBounds(source1)) return false;
					foreach(var element2 in contents2.ToArray()) map[element2] = source1;
					return true;
				}
			}
			else {
				if(found1) { // One present: move
					if(!isInBounds(source2)) return false;
					foreach(var element1 in contents1.ToArray()) map[element1] = source2;
					return true;
				}
				else return true; // None present
			}
		}
		/// <summary>
		/// Replace an element on the grid with a new element in the same position.
		/// Fails if the old element is not on the grid, or if the new element is already on the grid.
		/// </summary>
		public bool Replace(T replacedElement, T newElement) {
			if(newElement == null) throw new ArgumentNullException(nameof(newElement));
			if(map.TryGetValue(replacedElement, out TPosition position) && !map.Contains(newElement)) {
				map[newElement] = position;
				map.Remove(replacedElement);
				return true;
			}
			return false;
		}
		/// <summary>
		/// Remove an element from the grid.
		/// </summary>
		public void Remove(T element) => map.Remove(element);
		/// <summary>
		/// Remove any contents of the given position from the grid.
		/// </summary>
		public void RemoveContents(TPosition position) => map.Remove(position);
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		public IEnumerator<T> GetEnumerator() => map.KeysTMany.GetEnumerator();
	}*/
}
