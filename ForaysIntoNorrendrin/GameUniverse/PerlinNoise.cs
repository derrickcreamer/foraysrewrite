using System;
using GameComponents;

namespace Forays{
	public static class PerlinNoise{
		public static decimal[,] Get(int width, int height, int cellsPerUnitSquare, GameComponents.RNG rng){
			decimal[,] result = new decimal[width,height];
			return Get(result, cellsPerUnitSquare, rng);
		}
		public static decimal[,] Get(decimal[,] resultArray, int cellsPerUnitSquare, GameComponents.RNG rng, decimal persistence = 1m){
			int width = resultArray.GetLength(0);
			int height = resultArray.GetLength(1);
			int vectorWidth = (width / cellsPerUnitSquare) + 2; // make sure we have at least enough vectors for all 4 corners
			int vectorHeight = (height / cellsPerUnitSquare) + 2;
			Point[,] vectors = new Point[vectorWidth, vectorHeight];
			for(int vx=0;vx<vectorWidth;++vx){
				for(int vy=0;vy<vectorHeight;++vy){
					int px = rng.GetNext(201) - 100; // store vectors as hundredths, positive or negative
					int py = rng.GetNext(201) - 100;
					vectors[vx,vy] = new Point(px, py);
				}
			}
			decimal distBetweenMidpoints = 1m / (decimal)cellsPerUnitSquare;
			for(int x=0;x<width;++x){
				for(int y=0;y<height;++y){
					// first find gradient vectors:
					int sourceX = x / cellsPerUnitSquare;
					int sourceY = y / cellsPerUnitSquare;
					// also find where this x,y is within the containing square:
					int xInCell = x % cellsPerUnitSquare;
					int yInCell = y % cellsPerUnitSquare;
					decimal xVector0 = (distBetweenMidpoints * 0.5m) + xInCell * distBetweenMidpoints;
					decimal yVector0 = (distBetweenMidpoints * 0.5m) + yInCell * distBetweenMidpoints;
					decimal xVector1 = 1m - xVector0;
					decimal yVector1 = 1m - yVector0;
					decimal xGradientx0y0 = vectors[sourceX,sourceY].X / 100m;
					decimal xGradientx1y0 = vectors[sourceX+1,sourceY].X / 100m;
					decimal xGradientx0y1 = vectors[sourceX,sourceY+1].X / 100m;
					decimal xGradientx1y1 = vectors[sourceX+1,sourceY+1].X / 100m;
					decimal yGradientx0y0 = vectors[sourceX,sourceY].Y / 100m;
					decimal yGradientx1y0 = vectors[sourceX+1,sourceY].Y / 100m;
					decimal yGradientx0y1 = vectors[sourceX,sourceY+1].Y / 100m;
					decimal yGradientx1y1 = vectors[sourceX+1,sourceY+1].Y / 100m;
					decimal x0y0Dot = xGradientx0y0 * xVector0 + yGradientx0y0 * yVector0;
					decimal x1y0Dot = xGradientx1y0 * xVector1 + yGradientx1y0 * yVector0;
					decimal x0y1Dot = xGradientx0y1 * xVector0 + yGradientx0y1 * yVector1;
					decimal x1y1Dot = xGradientx1y1 * xVector1 + yGradientx1y1 * yVector1;
					decimal lerpX0 = Lerp(x0y0Dot, x1y0Dot, xVector0);
					decimal lerpX1 = Lerp(x0y1Dot, x1y1Dot, xVector0);
					decimal lerpFinal = Lerp(lerpX0, lerpX1, yVector0);
					resultArray[x,y] += lerpFinal * 256m * persistence;
				}
			}

			return resultArray;
		}
		///<param name="amount">(0.5 is the halfway point)</param>
		public static decimal Lerp(decimal a, decimal b, decimal amount){
			decimal diff = b-a;
			decimal progress = amount * diff;
			return a + progress;
		}
	}
}
