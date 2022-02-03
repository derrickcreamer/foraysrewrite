using System;
using System.Collections.Generic;

namespace Forays{
	public static class ExtensionsForEnumerable{
		public static List<T> WhereLeast<T>(this IEnumerable<T> source, Func<T, int> getValue){
			List<T> result = new List<T>();
			int least = int.MaxValue;
			foreach(T item in source){
				int value = getValue(item);
				if(value < least){
					least = value;
					result.Clear();
					result.Add(item);
				}
				else if(value == least){
					result.Add(item);
				}
			}
			return result;
		}
		public static List<T> WhereGreatest<T>(this IEnumerable<T> source, Func<T, int> getValue){
			List<T> result = new List<T>();
			int greatest = int.MinValue;
			foreach(T item in source){
				int value = getValue(item);
				if(value > greatest){
					greatest = value;
					result.Clear();
					result.Add(item);
				}
				else if(value == greatest){
					result.Add(item);
				}
			}
			return result;
		}
	}
}
