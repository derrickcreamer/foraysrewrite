using System;
using System.Collections.Generic;
using GameComponents;

namespace Forays{
	public static class ExtensionsForRNG{ // until I get around to putting these in the RNG class itself
		public static int Roll(this RNG rng, int sides){
			if(sides < 1) return 0;
			return rng.GetNext(sides) + 1;
		}
		public static int Roll(this RNG rng, int dice, int sides){
			if(sides < 1) return 0;
			int total = 0;
			for(int i=0;i<dice;++i)
				total += rng.GetNext(sides) + 1;
			return total;
		}
		public static int Between(this RNG rng, int a, int b){ //inclusive
			int min, max;
			if(a < b){
				min = a;
				max = b;
			}
			else{
				min = b;
				max = a;
			}
			// 'between 2 and 7' can return 6 numbers, so get 0-5 first, then add:
			return rng.GetNext(max - min + 1) + min;
		}
		public static bool PercentChance(this RNG rng, int x){
			return x > rng.GetNext(100);
		}
		public static bool FractionalChance(this RNG rng, int x, int outOfY){
			if(x >= outOfY) return true;
			return x > rng.GetNext(outOfY);
		}
		public static T ChooseFromList<T>(this RNG rng, IList<T> list){
			if(list == null || list.Count == 0) throw new InvalidOperationException("List missing or empty");
			return list[rng.GetNext(list.Count)];
		}
		// Rework Choose to avoid 'params' and array allocations:
		public static T Choose<T>(this RNG rng, T t0, T t1){
			if(rng.CoinFlip()) return t0;
			else return t1;
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2){
			switch(rng.GetNext(3)){
				case 0: return t0;
				case 1: return t1;
				default: return t2;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3){
			switch(rng.GetNext(4)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				default: return t3;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4){
			switch(rng.GetNext(5)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				default: return t4;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5){
			switch(rng.GetNext(6)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				default: return t5;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6){
			switch(rng.GetNext(7)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				default: return t6;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7){
			switch(rng.GetNext(8)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				default: return t7;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8){
			switch(rng.GetNext(9)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				default: return t8;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9){
			switch(rng.GetNext(10)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				default: return t9;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10){
			switch(rng.GetNext(11)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				default: return t10;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11){
			switch(rng.GetNext(12)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				default: return t11;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11, T t12){
			switch(rng.GetNext(13)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				case 11: return t11;
				default: return t12;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11, T t12, T t13){
			switch(rng.GetNext(14)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				case 11: return t11;
				case 12: return t12;
				default: return t13;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11, T t12, T t13, T t14){
			switch(rng.GetNext(15)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				case 11: return t11;
				case 12: return t12;
				case 13: return t13;
				default: return t14;
			}
		}
		public static T Choose<T>(this RNG rng, T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7, T t8, T t9, T t10, T t11, T t12, T t13, T t14, T t15){
			switch(rng.GetNext(16)){
				case 0: return t0;
				case 1: return t1;
				case 2: return t2;
				case 3: return t3;
				case 4: return t4;
				case 5: return t5;
				case 6: return t6;
				case 7: return t7;
				case 8: return t8;
				case 9: return t9;
				case 10: return t10;
				case 11: return t11;
				case 12: return t12;
				case 13: return t13;
				case 14: return t14;
				default: return t15;
			}
		}
	}
}
