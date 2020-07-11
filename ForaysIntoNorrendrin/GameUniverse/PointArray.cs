using System;
using GameComponents;

//todo, move this to GameComponents ... (and while doing that, also change some enums like dir4/8 to smaller int types)
namespace Forays{
    public class PointArray<T>{
        private T[][] items;
        public readonly int Width;
        public readonly int Height;
        public PointArray(int width, int height){
            if(width <= 0 || height <= 0) throw new ArgumentException("Width and height must be positive");
            Width = width;
            Height = height;
            items = new T[width][];
            for(int x=0;x<width;++x){
                items[x] = new T[height];
            }
        }
        public T this[Point p]{
            get{
                return items[p.X][p.Y];
            }
            set{
                items[p.X][p.Y] = value;
            }
        }
        public T this[int x, int y]{
            get{
                return items[x][y];
            }
            set{
                items[x][y] = value;
            }
        }
    }
}
