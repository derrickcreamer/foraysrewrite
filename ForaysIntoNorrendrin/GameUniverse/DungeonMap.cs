using System;
using System.Collections.Generic;
using GameComponents;
using UtilityCollections;

namespace Forays {
	public enum MapLight { MagicalDarkness = -1, Normal = 0, MagicalLight = 1 };
	public class DungeonMap : GameObject {
		new public Grid<Creature, Point> Creatures;
		public List<CreatureGroup> CreatureGroups;
		new public Grid<Tile, Point> Tiles;
		new public Grid<Item, Point> Items;
		//todo: features here
		public MapLight MapLight;
		public int DangerModifier;
		public EasyHashSet<Point> Seen; // todo: hashset isn't bad BUT a 2d array might be better..not sure.

		public DungeonMap(GameUniverse g) : base(g) {
			const int width = 30; //todo
			const int height = 20;
			Func<Point, bool> isInBounds = p => p.X >= 0 && p.X < 30 && p.Y >= 0 && p.Y < 20; //todo
			Creatures = new Grid<Creature, Point>(isInBounds);
			Tile floor = new Tile(g){ Type = TileType.Floor }; //todo, this'll change
			Tiles = new Grid<Tile, Point>(isInBounds) { GetDefaultElement = () => floor };
			Items = new Grid<Item, Point>(isInBounds);
			Seen = new EasyHashSet<Point>();
			//need to create a map, and populate it...
			// tiles and creatures, at least.
			// floors & walls, and a few goblins, then?
			// walls at edges, plus a 5% chance for others?
			//
			// great, now, does any of that happen in the ctor? prob not, right?
			// this should be a gameobject, right?
			// Unless it's going to be populated by something else...

			//todo, figure out tile prototypes eventually

			for(int i=0;i<width;++i)
				for(int j=0;j<height;++j)
					if(i == 0 || j == 0 || i == width-1 || j == height-1 || R.OneIn(20))
						Tiles.Add(new Tile(g){ Type = TileType.Wall }, new Point(i, j));
					else if(R.OneIn(50))
						Creatures.Add(new Creature(g){ Type = CreatureType.Goblin }, new Point(i, j)); //todo
		}

		/*
								all groups
								all features
								all burning objects
								all aesthetic features
								current footsteps
								locations seen
								light map
								direction exited (AI helper)

										 */
	}
}
