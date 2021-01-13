using System;
using System.Collections.Generic;//todo
using System.Linq;
using GameComponents;
using GameComponents.DirectionUtility;
using UtilityCollections;

namespace Forays {
	public enum MagicalLightState { MagicalDarkness = -1, Normal = 0, MagicalLight = 1 };

	public class LightMap : GameObject {
		const int Width = GameUniverse.MapWidth;
		const int Height = GameUniverse.MapHeight;

		public MagicalLightState MagicalLightState;

		private MultiValueDictionary<Point, int> lightSources;
		private MultiValueDictionary<Point, int> tempRemovedLights; // Used during terrain opacity changes
		private bool midOpacityUpdate;

		///<summary>Counts the number of light sources currently casting light into each cell.
		/// (Currently, all light sources have equal strength, and cells are either lit or not lit.)</summary>
		private PointArray<int> cellBrightness;

		public LightMap(GameUniverse g) : base(g){
			lightSources = new MultiValueDictionary<Point, int>();
			tempRemovedLights = new MultiValueDictionary<Point, int>();
			cellBrightness = new PointArray<int>(Width, Height);
		}
		///<summary>Returns true if cell is lit from any direction.</summary>
		public bool IsLit(Point cell){
			if(MagicalLightState == MagicalLightState.MagicalLight) return true;
			else if(MagicalLightState == MagicalLightState.MagicalDarkness) return false;
			else{
				if(TileDefinition.IsOpaque(Map.Tiles[cell])){
					foreach(Point neighbor in cell.EnumeratePointsAtChebyshevDistance(1, false, false)){
						if(!neighbor.ExistsBetweenMapEdges()) continue;
						if(cellBrightness[neighbor] > 0 && !TileDefinition.IsOpaque(Map.Tiles[neighbor])) return true;
					}
					return false;
				}
				else{
					return cellBrightness[cell] > 0;
				}
			}
		}
		///<summary>Returns true if cell is lit AND (for opaque cells) if the observer is on the right side to see the light.</summary>
		public bool CellAppearsLitToObserver(Point cell, Point observer){
			if(MagicalLightState == MagicalLightState.MagicalLight) return true;
			else if(!IsLit(cell)) return false; // If it isn't lit at all, stop here.
			else{
				if(TileDefinition.IsOpaque(Map.Tiles[cell])){
					// Light for opaque cells must be done carefully so light sources aren't visible through walls.
					// If the observer has LOS to any adjacent lit nonopaque cell, that observer knows this wall is lit.
					for(int i=0;i<8;++i){
						Dir8 dir = EightDirections.Eight[i];
						Point neighbor = cell.PointInDir(dir);
						if(!neighbor.ExistsBetweenMapEdges()) continue;
						if(cellBrightness[neighbor] == 0) continue;
						if(observer.CheckReciprocalBresenhamLineOfSight(neighbor, Map.Tiles)) return true;
					}
					return false;
				}
				else{
					return true; // Cell is lit and not opaque, so it appears lit from anywhere.
				}
			}
		}
		///<summary>Adds a light source of the given radius at the given point, updating surrounding light values.</summary>
		public void AddLightSource(Point sourceCell, int radius){
			lightSources.Add(sourceCell, radius);
			UpdateBrightnessWithinRadius(sourceCell, radius, 1);
		}
		///<summary>Removes a light source of the given radius at the given point, updating surrounding light values.</summary>
		public void RemoveLightSource(Point sourceCell, int radius){
			if(!lightSources.Remove(sourceCell, radius)) throw new InvalidOperationException("No light source of this radius at this cell");
			UpdateBrightnessWithinRadius(sourceCell, radius, -1);
		}
		///<summary>Temporarily removes light sources around the given cell so that that cell's opacity can be changed.
		/// (Immediately after changing the opacity, UpdateAfterOpacityChange MUST be called.)</summary>
		public void UpdateBeforeOpacityChange(Point cell){
			if(midOpacityUpdate) throw new InvalidOperationException("Already in the middle of an opacity update");
			// (A pair of methods to update the opacity of multiple cells at once could also be useful here...)
			midOpacityUpdate = true;
			foreach(KeyValuePair<Point, int> pair in lightSources.GetAllKeyValuePairs()){
				int dist = cell.ChebyshevDistanceFrom(pair.Key);
				if(pair.Value >= dist){
					tempRemovedLights.Add(pair.Key, pair.Value);
					UpdateBrightnessWithinRadius(pair.Key, pair.Value, -1);
				}
			}
		}
		///<summary>Restores the light sources that were temporarily removed when UpdateBeforeOpacityChange was called.
		/// (UpdateBeforeOpacityChange MUST be called immediately before changing the opacity.)</summary>
		public void UpdateAfterOpacityChange(){
			if(!midOpacityUpdate) throw new InvalidOperationException("This method can only be called to finish an opacity update");
			midOpacityUpdate = false;
			foreach(KeyValuePair<Point, int> pair in tempRemovedLights.GetAllKeyValuePairs()){
				UpdateBrightnessWithinRadius(pair.Key, pair.Value, 1);
			}
			tempRemovedLights.Clear();
		}
		private void UpdateBrightnessWithinRadius(Point sourceCell, int radius, int increment){
			for(int i=sourceCell.Y - radius; i<=sourceCell.Y + radius; ++i){
				for(int j=sourceCell.X - radius; j<=sourceCell.X + radius; ++j){
					Point p = new Point(j, i);
					if(!p.ExistsBetweenMapEdges()) continue;
					if(!TileDefinition.IsOpaque(Map.Tiles[p]) && sourceCell.CheckReciprocalBresenhamLineOfSight(p, Map.Tiles)){
						cellBrightness[p] += increment;
					}
				}
			}
		}
	}

}
