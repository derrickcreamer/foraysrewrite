using System;
using System.Collections.Generic;
using GameComponents;
using GameComponents.DirectionUtility;

namespace Forays{
	public static class FloodFill{
		public enum SourceOption { NeverIncludeSource, AlwaysIncludeSource, ConditionallyIncludeSource, StopScanOnSourceFailure };
		public static IEnumerable<Point> Scan(Point source, Func<Point, bool> condition, SourceOption sourceOption = SourceOption.AlwaysIncludeSource){
			List<Point> frontier = new List<Point>();
			HashSet<Point> visited = new HashSet<Point>{source};
			switch(sourceOption){
				case SourceOption.AlwaysIncludeSource:
					yield return source;
					break;
				case SourceOption.ConditionallyIncludeSource:
				case SourceOption.StopScanOnSourceFailure:
					if(condition.Invoke(source)) yield return source;
					else if(sourceOption == SourceOption.StopScanOnSourceFailure) yield break;
					break;
			}
			frontier.Add(source);
 			while(frontier.Count > 0){
				Point current = frontier[frontier.Count - 1];
				frontier.RemoveAt(frontier.Count - 1);
				foreach(Dir8 dir in EightDirections.Enumerate(true, false, false)){
					Point neighbor = current.PointInDir(dir);
					if(!neighbor.ExistsBetweenMapEdges()) continue;
					if(visited.Contains(neighbor)) continue;
					if(condition.Invoke(neighbor)){
						yield return neighbor;
						frontier.Add(neighbor);
						visited.Add(neighbor);
					}
				}
			}
		}
		public static PointArray<bool> ScanToArray(Point source, Func<Point, bool> condition, SourceOption sourceOption = SourceOption.AlwaysIncludeSource){
			PointArray<bool> result = new PointArray<bool>(GameUniverse.MapWidth, GameUniverse.MapHeight);
			foreach(Point p in Scan(source, condition, sourceOption)){
				result[p] = true;
			}
			return result;
		}
	}
}
