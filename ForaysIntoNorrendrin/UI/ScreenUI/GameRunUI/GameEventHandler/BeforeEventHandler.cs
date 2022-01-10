using System;
using Forays;
using GameComponents;
using GrammarUtility;

namespace ForaysUI.ScreenUI{
	// This class uses 'partial' so these large methods can be in separate files at no runtime cost.
	// The files in which this class is defined are:
	// AfterEventHandler.cs (contains the constructor)
	// BeforeEventHandler.cs
	// CancelActionHandler.cs
	// ChooseActionHandler.cs
	// StatusEventHandler.cs
	public partial class GameEventHandler : GameUIObject{
		public void BeforeGameEvent(GameObject gameEvent){
			switch(gameEvent){
				case PlayerTurnEvent e:
					ChooseAction(e);
					break;
				case MeleeHitEvent e:
					Messages.Add(e.Creature, "hit", e.Target);
					break;
				case MeleeMissEvent e:
					Messages.Add(e.Creature, "miss", e.Target);
					break;
				case DieEvent e:
					Messages.AddSimple(e.Creature, "die");
					break;
				case IceCrackingEvent e: //todo
					{
						int width = 66;
						int height = 22;
						var noiseObj = new TestNoise();
						//var noise = noiseObj.Get(22, 22, ScreenRNG);
						//var noise = noiseObj.GetFractal2(22, 22, 4, 2, 0.5m, ScreenRNG);
						var noise = noiseObj.GetPerlin(width, height, 5, ScreenRNG);
						//noise = noiseObj.GetPerlin(noise, 6, 0.5m, ScreenRNG);
						//noise = noiseObj.GetPerlin(noise, 3, 0.25m, ScreenRNG);
						for(int x=0;x<width;++x){
							for(int y=0;y<height;++y){
								Color color;
								decimal value = noise[x,y];
								if(value < -99) color = Color.Black;
								else if(value < -77) color = Color.Grayscale10;
								else if(value < -55) color = Color.Grayscale20;
								else if(value < -33) color = Color.Grayscale30;
								else if(value < -11) color = Color.Grayscale40;
								else if(value <= 11) color = Color.Grayscale50;
								else if(value <= 33) color = Color.Grayscale60;
								else if(value <= 55) color = Color.Grayscale70;
								else if(value <= 77) color = Color.Grayscale80;
								else if(value <= 99) color = Color.Grayscale90;
								else color = Color.White;
								MapUI.DrawToMap(y, x, ' ', Color.Black, color);
							}
						}
						StaticScreen.Screen.WindowUpdate();
						ConsoleKeyInfo cki;
						do{ cki = StaticInput.Input.ReadKey(false);
						} while(cki.Key != ConsoleKey.Escape);
					}
					break;
				case IceBreakingEvent e:
					break;
			}
		}
	}
	public class TestNoise{
		public decimal[,] GetPerlin(int width, int height, int cellsPerUnitSquare, GameComponents.RNG rng){
			decimal[,] result = new decimal[width,height];
			return GetPerlin(result, cellsPerUnitSquare, 1m, rng);
		}
		public decimal[,] GetPerlin(decimal[,] resultArray, int cellsPerUnitSquare, decimal persistence, GameComponents.RNG rng){
			int width = resultArray.GetLength(0);
			int height = resultArray.GetLength(1);
			int vectorWidth = (width / cellsPerUnitSquare) + 2;
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
					decimal lerpX0 = lerpTodo(x0y0Dot, x1y0Dot, xVector0);
					decimal lerpX1 = lerpTodo(x0y1Dot, x1y1Dot, xVector0);
					decimal lerpFinal = lerpTodo(lerpX0, lerpX1, yVector0);
					resultArray[x,y] = lerpFinal * persistence;
				}
			}
			for(int x=0;x<width;++x){
				for(int y=0;y<height;++y){
					resultArray[x,y] = resultArray[x,y] * 256m;
				}
			}

			return resultArray;
		}
		// todo amount is 0-1
		public static decimal lerpTodo(decimal a, decimal b, decimal amount){
			decimal diff = b-a;
			decimal progress = amount * diff;
			return a + progress;
		}
	}
}
