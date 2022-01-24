using System;
using System.Collections.Generic;
using Forays;
using GameComponents;
using GameComponents.DirectionUtility;
using static ForaysUI.ScreenUI.StaticScreen; //todo check

namespace ForaysUI.ScreenUI.MapRendering{
	// BasicMapRenderer makes use of Screen to draw the map
	public class BasicMapRenderer : MapRenderer{

		public BasicMapRenderer(GameRunUI ui) : base(ui) {}

		/*public override void DrawMap(){
			for(int i = 0; i < GameUniverse.MapHeight; i++) { //todo, cache all LOS + lighting for player turn... conditionally? for certain commands?
				for(int j = 0; j < GameUniverse.MapWidth; j++) {
					Point p = new Point(j, i);
					Creature creature = CreatureAt(p);
					ItemType? item = ItemAt(p)?.Type; //todo check ID
					if(drawEnemies && creature != null && Player.CanSee(creature)){ //todo, any optimizations for this CanSee check?
						DrawToMap(i, j, DetermineCreatureColorGlyph(creature.OriginalType, TileTypeAt(p), FeaturesAt(p), item));
						//todo, need to consider inverting colors when colors are the same.
						MapUI.RecordMapMemory(p); // todo, should map memory always get recorded here?
					}
					else if(Player.Position.HasLOS(p, Map.Tiles)){
						Map.Seen[p] = true; //todo!!! This one does NOT stay here. Temporary hack to get map memory working. Should be done in the player turn action or similar.
						ColorGlyph cg = DetermineVisibleColorGlyph(TileTypeAt(p), FeaturesAt(p), item);
						if(!Map.Light.CellAppearsLitToObserver(p, Player.Position)){
							DrawToMap(i, j, cg.GlyphIndex, Color.DarkCyan, cg.BackgroundColor); //todo, only some tiles get darkened this way, right?
						}
						else{
							DrawToMap(i, j, cg);
						}
						MapUI.RecordMapMemory(p);
					}
					else if(false){ // todo, this is where the dcss-style option for seeing previous monster locations will be added
					}
					else if(Map.Seen[p]){
						DrawToMap(i, j, GetLastSeenColorGlyph(p, true));
					}
					else{
						DrawToMap(i, j, ' ', Color.White);
					}
				}
			}
			if(cursor != null) SetCursorPositionOnMap(cursor.Value.Y, cursor.Value.X);
		}*/
		public override void DrawMap(){
			for(int i = 0; i < GameUniverse.MapHeight; i++) { //todo, cache all LOS + lighting for player turn... conditionally? for certain commands?
				for(int j = 0; j < GameUniverse.MapWidth; j++) {
					Point p = new Point(j, i);
					Creature creature = CreatureAt(p);
					ItemType? item = ItemAt(p)?.Type; //todo check ID
					if(drawEnemies && creature != null && visibleCreatures.Contains(creature)){
						DrawToMap(i, j, DetermineCreatureColorGlyph(creature.OriginalType, TileTypeAt(p), FeaturesAt(p), item));
						//todo, need to consider inverting colors when colors are the same.
					}
					else if(playerTurnLos[p]){
						ColorGlyph cg = DetermineVisibleColorGlyph(TileTypeAt(p), FeaturesAt(p), item);
						if(!playerTurnLit[p]){
							DrawToMap(i, j, cg.GlyphIndex, Color.DarkCyan, cg.BackgroundColor); //todo, only some tiles get darkened this way, right?
						}
						else{
							DrawToMap(i, j, cg);
						}
					}
					else if(false){ // todo, this is where the dcss-style option for seeing previous monster locations will be added
					}
					else if(Map.Seen[p]){
						if(memoryFullyVisible){
							if(false){
								//todo, if this tile type is unknown until seen in light...
							}
							else{
								ColorGlyph cg = DetermineVisibleColorGlyph(TileTypeAt(p), FeaturesAt(p), item);
								DrawToMap(i, j, cg);
							}
						}
						DrawToMap(i, j, GetLastSeenColorGlyph(p, true));
					}
					else{
						DrawToMap(i, j, ' ', Color.White);
					}
				}
			}
			if(highlight != null) DrawHighlight(highlight);
			if(cursor != null) SetCursorPositionOnMap(cursor.Value.Y, cursor.Value.X);
		}
		private void DrawHighlight(Highlight highlight){
			ColorGlyph[][] drawn = Screen.GetCurrent(MapUI.RowOffset, MapUI.ColOffset, GameUniverse.MapHeight, GameUniverse.MapWidth);
			if(highlight.Type == MapHighlightType.SinglePoint){
				ColorGlyph currentGlyph = drawn[GameUniverse.MapHeight-1-highlight.Destination.Y][highlight.Destination.X];
				ColorGlyph highlighted = Screen.GetHighlighted(currentGlyph, HighlightType.TargetingValidFocused);
				DrawToMap(highlight.Destination.Y, highlight.Destination.X, highlighted);
			}
			else{ // path or line
				IList<Point> valid = highlight.LineOrPath;
				IList<Point> invalid = null;
				if(highlight.BlockedPoint != null){
					valid = new List<Point>();
					invalid = new List<Point>();
					bool blocked = false;
					for(int i=0;i<highlight.LineOrPath.Count;++i){
						if(blocked){
							invalid.Add(highlight.LineOrPath[i]);
						}
						else{
							valid.Add(highlight.LineOrPath[i]);
							if(highlight.LineOrPath[i] == highlight.BlockedPoint.Value){
								blocked = true;
							}
						}
					}
				}
				if(highlight.Radius != null){
					if(highlight.BlockedPoint != null){
						foreach(Point p in highlight.CellsInRadius){
							if(!valid.Contains(p)) invalid.Add(p); // don't overwrite valid with invalid
						}
					}
					else{
						foreach(Point p in highlight.CellsInRadius){
							if(!valid.Contains(p)) valid.Add(p);
						}
					}
				}
				foreach(Point p in valid){
					ColorGlyph currentGlyph = drawn[GameUniverse.MapHeight-1-p.Y][p.X];
					HighlightType highlightType = highlight.Destination == p ? HighlightType.TargetingValidFocused : HighlightType.TargetingValid;
					ColorGlyph highlighted = Screen.GetHighlighted(currentGlyph, highlightType);
					DrawToMap(p.Y, p.X, highlighted);
				}
				if(invalid != null){
					foreach(Point p in invalid){
						ColorGlyph currentGlyph = drawn[GameUniverse.MapHeight-1-p.Y][p.X];
						HighlightType highlightType = highlight.Destination == p ? HighlightType.TargetingInvalidFocused : HighlightType.TargetingInvalid;
						ColorGlyph highlighted = Screen.GetHighlighted(currentGlyph, highlightType);
						DrawToMap(p.Y, p.X, highlighted);
					}
				}
			}
		}
		public override void HideMap(){

		}

		public void DrawToMap(int row, int col, int glyphIndex, Color color, Color bgColor = Color.Black)
			=> Screen.Write(GameUniverse.MapHeight-1-row+MapUI.RowOffset, col+MapUI.ColOffset, glyphIndex, color, bgColor);
		public void DrawToMap(int row, int col, ColorGlyph cg)
			=> Screen.Write(GameUniverse.MapHeight-1-row+MapUI.RowOffset, col+MapUI.ColOffset, cg);
		//todo public ColorGlyph GetCachedAtMapPosition(Point p) => cachedMapDisplay[GameUniverse.MapHeight-1-p.Y][p.X];
		private void SetCursorPositionOnMap(int row, int col)
			=> Screen.SetCursorPosition(GameUniverse.MapHeight-1-row+MapUI.RowOffset, col+MapUI.ColOffset);

		private static ColorGlyph DetermineCreatureColorGlyph(CreatureType creature, TileType tile, FeatureType features, ItemType? item){
			//todo, add trap, shrine, etc.
			//todo features
			Color bgColor = Color.Black;
			if(features.HasFeature(FeatureType.Water)) bgColor = Color.DarkCyan;
			//else if(features.HasFeature(FeatureType.Lava)) bgColor = Color.DarkRed;
			else if(tile == TileType.ThickIce) bgColor = Color.Gray;
			else if(tile == TileType.DeepWater){
				if(features.HasFeature(FeatureType.Ice) || features.HasFeature(FeatureType.CrackedIce)) bgColor = Color.Gray;
				else bgColor = Color.DarkBlue;
			}
			ColorGlyph cg = GameObjectGlyphs.Get(creature);
			return new ColorGlyph(cg.GlyphIndex, cg.ForegroundColor, bgColor);
		}
		private static ColorGlyph DetermineVisibleColorGlyph(TileType tile, FeatureType features, ItemType? item){ //todo, add trap, shrine, etc.
			//todo features
			if(item != null) return GameObjectGlyphs.Get(item.Value);
			if(features.HasFeature(FeatureType.Ice)) return GameObjectGlyphs.Get(FeatureType.Ice);// new ColorGlyph('~', Color.Cyan, Color.Gray);
			if(features.HasFeature(FeatureType.CrackedIce)) return new ColorGlyph('~', Color.Red, Color.Gray);
			if(features.HasFeature(FeatureType.BrokenIce)) return new ColorGlyph('~', Color.Gray, Color.DarkBlue);
			if(features.HasFeature(FeatureType.Water)) return new ColorGlyph('~', Color.Cyan, Color.DarkCyan);
			//todo, should change color for creatures based on background, right? do I need rules for which colors to modify/invert?
			//so... if background color... then return a modified version.
			//bg colors are currently black, gray, dark blue... plus targeting/mouseover stuff.

			return GameObjectGlyphs.Get(tile);
		}
		private ColorGlyph GetLastSeenColorGlyph(Point p, bool useOutOfSightColor){
			ColorGlyph cg = DetermineVisibleColorGlyph(MapUI.LastKnownTile(p), MapUI.LastKnownFeatures(p), MapUI.LastKnownItem(p));
			if(useOutOfSightColor){
				if(cg.BackgroundColor != Color.Black)
					return new ColorGlyph(cg.GlyphIndex, Color.Black, Color.OutOfSight);
				else
					return new ColorGlyph(cg.GlyphIndex, Color.OutOfSight, Color.Black);
			}
			else return cg;
		}
	}
}
