using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameComponents;

namespace Forays {
	public enum ItemType { PotionOfHealth, PotionOfSpeed, PotionOfStrength };
	public class Item : GameObject, IPhysicalObject {
		public Point? Position => Items.TryGetPositionOf(this, out Point p)? p : (Point?)null;
		public ItemType Type;
		//qty, charges, other data(?), ignored (UI only?), do not stack, revealed by light, 

		public Item(GameUniverse g) : base(g) { }

	}
}
