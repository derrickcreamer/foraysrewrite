using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forays {
	public class RNG { //splitmix64
		public ulong RngState;

		public ulong GetNext() {
			unchecked {
				ulong z = (RngState += 0x9e3779b97f4a7c15);
				z = (z ^ (z >> 30)) * 0xbf58476d1ce4e5b9;
				z = (z ^ (z >> 27)) * 0x94d049bb133111eb;
				return z ^ (z >> 31);
			}
		}

		public RNG(ulong seed){ RngState = seed; }
		public int GetNext(int upperExclusiveBound) => (int)(GetNext() % (ulong)upperExclusiveBound); //todo, improve w/128bit mult?
		public bool CoinFlip() => GetNext() % 2 == 0;
		public bool OneIn(int x) => GetNext() % (ulong)x == 0;

	}
}
