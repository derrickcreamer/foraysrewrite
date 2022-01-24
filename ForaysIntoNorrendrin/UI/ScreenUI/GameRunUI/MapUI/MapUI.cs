using System;
using System.Collections.Generic;
using Forays;
using GameComponents;
using static ForaysUI.ScreenUI.StaticScreen; //todo check

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// LookMode.cs
	// MapDescription.cs
	// MapMemory.cs
	// MapUI.cs (has constructor)
	public partial class MapUI : GameUIObject {
		// Track display height/width separately to make it easier to change later:
		public const int MapDisplayHeight = GameUniverse.MapHeight;
		public const int MapDisplayWidth = GameUniverse.MapWidth;

		public static int RowOffset;
		public static int ColOffset;

		private PointArray<Color> colorVariations; // for terrain variations that don't change after creation

		public MapUI(GameRunUI ui) : base(ui) {
			tilesLastSeen = new PointArray<TileType>(GameUniverse.MapWidth, GameUniverse.MapHeight);
			featuresLastSeen = new PointArray<FeatureType>(GameUniverse.MapWidth, GameUniverse.MapHeight);
			trapsLastSeen = new Dictionary<Point, TrapType>();
			itemsLastSeen = new Dictionary<Point, ItemType>();
			colorVariations = new PointArray<Color>(GameUniverse.MapWidth, GameUniverse.MapHeight);
		}
	}
}
