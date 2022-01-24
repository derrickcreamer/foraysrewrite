using System;
using System.Collections.Generic;
using Forays;
using GameComponents;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// LookMode.cs
	// MapDescription.cs
	// MapMemory.cs
	// MapUI.cs (has constructor)
	public partial class MapUI : GameUIObject {
		private PointArray<TileType> tilesLastSeen;
		private PointArray<FeatureType> featuresLastSeen;
		private Dictionary<Point, TrapType> trapsLastSeen;
		//todo, shrines, idols?
		private Dictionary<Point, ItemType> itemsLastSeen;

		public void RecordMapMemory(Point p){
			tilesLastSeen[p] = TileTypeAt(p);
			featuresLastSeen[p] = FeaturesAt(p);
			Item item = ItemAt(p);
			if(item != null) itemsLastSeen[p] = item.Type; //todo ID
			else itemsLastSeen.Remove(p);
			//todo traps etc.
		}

		public TileType LastKnownTile(Point p) => tilesLastSeen[p];
		public FeatureType LastKnownFeatures(Point p) => featuresLastSeen[p];
		public TrapType? LastKnownTrap(Point p){
			if(trapsLastSeen.TryGetValue(p, out TrapType trap)) return trap;
			else return null;
			//todo, ID?
		}
		public ItemType? LastKnownItem(Point p){
			if(itemsLastSeen.TryGetValue(p, out ItemType item)) return item;
			else return null;
			//todo, ID?
		}
	}
}
