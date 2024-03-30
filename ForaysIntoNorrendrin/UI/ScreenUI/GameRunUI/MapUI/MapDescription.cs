using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Forays;
using GameComponents;
using GrammarUtility;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// LookMode.cs
	// MapDescription.cs
	// MapMemory.cs
	// MapUI.cs (has constructor)
	public partial class MapUI : GameUIObject {
		private string GetDescriptionAtCell(Point p){
			List<string> items = new List<string>();
			bool includeMonsters = true; //todo
			if(includeMonsters){
				Creature creature = CreatureAt(p);
				if(creature != null && creature != Player && Player.CanSee(creature)){
					string creatureStatus = "(unhurt, unaware)"; //todo
					items.Add(Grammar.Get(Determinative.AAn, Names.Get(creature.OriginalType), extraText: creatureStatus));
				}
			}
			Item item = ItemAt(p);
			if(item != null){
				string itemExtra = "";
				//check item ID here, todo
				ItemType finalType = item.Type; //todo, check ID for this too
				items.Add(Grammar.Get(Determinative.AAn, Names.Get(finalType), extraText: itemExtra));
			}
			TileType tileType = TileTypeAt(p);
			//todo, check tile known status?
			FeatureType features = FeaturesAt(p);
			if(features != FeatureType.None){
				items.AddRange(Names.GetAllFeatures(features).Select(name => Grammar.Get(Determinative.AAn, name)));
			}
			//todo, traps, shrines, idols, etc.
			return GetDescriptionInternal(items, tileType, "You see ");
		}
		private string GetLastKnownDescriptionAtCell(Point p){
			List<string> items = new List<string>();
			if(false){ //todo, last known enemy position option
				items.Add("todo name");
			}
			ItemType itemType;
			if(itemsLastSeen.TryGetValue(p, out itemType)){
				//todo, ID?
				//todo, extra info?
				string itemExtra = "";
				items.Add(Grammar.Get(Determinative.AAn, Names.Get(itemType), extraText: itemExtra));
			}
			TileType tileType = tilesLastSeen[p];
			//todo, check tile known status?
			//todo, features here
			//todo, traps, shrines, idols, etc.
			return GetDescriptionInternal(items, tileType, "You remember seeing ");
		}
		//todo, this is going to need to be reworked a bit once everything is present, because
		// of cases like flying monsters, items under water, ice chunks in water, etc.,
		// possibly all at the same time, so there will be some special cases.
		// "You see a red potion, a vragling, and a layer of ice over deep water."
		// "You see a bat-thing over deep water, chunks of ice on the surface,
		// and a red potion under the surface."
		private string GetDescriptionInternal(List<string> items, TileType tileType, string initialText){
			string tileConnector = GetConnectingWordForTile(tileType);
			string tileName = Grammar.Get(Determinative.AAn, Names.Get(tileType));
			if(items.Count > 0 && tileConnector == "and"){ // If it's "and", just handle it like the others:
				items.Add(tileName);
			}
			StringBuilder sb = new StringBuilder();
			sb.Append(initialText);
			if(items.Count == 0){
				sb.Append(tileName);
			}
			else if(items.Count == 1){
				sb.Append(items[0]);
				sb.Append(" ");
				sb.Append(tileConnector);
				sb.Append(" ");
				sb.Append(tileName);
			}
			else{
				AppendWordListWithCommas(sb, items);
				if(tileType != TileType.Floor && tileConnector != "and"){
					sb.Append(tileConnector);
					sb.Append(" ");
					sb.Append(tileName);
				}
			}
			sb.Append(".");
			string result = sb.ToString();
			sb.Clear();
			return result;
		}
		private string GetConnectingWordForTile(TileType type){
			//todo
			switch(type){
				case TileType.Floor:
				case TileType.Staircase:
					return "on";
				//case TileType.: todo door
				default: return "and";
			}
		}
		private void AppendWordListWithCommas(StringBuilder sb, IList<string> words){
			if(words.Count == 0) return;
			else if(words.Count == 1) sb.Append(words[0]); // "one"
			else if(words.Count == 2){ // "one and two"
				sb.Append(words[0]);
				sb.Append(" and ");
				sb.Append(words[1]);
			}
			else{ // "one, two, and three"
				for(int i=0;i<words.Count - 1;++i){
					sb.Append(words[i]);
					sb.Append(", ");
				}
				sb.Append(" and ");
				sb.Append(words[words.Count - 1]);
			}
		}
	}
}
