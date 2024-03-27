using System;
using Forays;
using GameComponents;

namespace ForaysUI.ScreenUI.MapRendering{
	//todo, pretty sure that the maprenderer also needs to provide a method for telling us the map position of a mouse coord, eventually.
	public abstract class MapRenderer : GameUIObject{
		protected Point? cursor;
		protected Highlight highlight;
		protected bool memoryFullyVisible;
		protected bool drawEnemies;
		//todo - will i need a caching option here?
		// monster info popups probably go here

		private static Func<GameRunUI,MapRenderer> createMapRenderer;
		protected MapRenderer(GameRunUI ui) : base(ui){}
		public static void SetFactoryMethod(Func<GameRunUI,MapRenderer> createMapRenderer){
			MapRenderer.createMapRenderer = createMapRenderer;
		}
		public static MapRenderer Create(GameRunUI ui) => createMapRenderer.Invoke(ui);

		public abstract void DrawMap();
		public abstract void HideMap();
		// The Update methods should be overridden if there's any unique update logic (such as setting dirty flags etc.):
		public virtual void UpdateCursorPosition(Point? cursor){ this.cursor = cursor; }
		public virtual void UpdateHighlight(Highlight highlight){ this.highlight = highlight; }
		public virtual void UpdateMapMemoryVisibility(bool mapMemoryFullyVisible){ memoryFullyVisible = mapMemoryFullyVisible; }
		public virtual void UpdateDrawEnemies(bool drawEnemies){ this.drawEnemies = drawEnemies; }
		public virtual void UpdateAllSettings(
			Point? activeCursor,
			Highlight highlight = null,
			bool mapMemoryFullyVisible = false,
			bool drawEnemies = true)
		{
			UpdateCursorPosition(activeCursor);
			UpdateHighlight(highlight);
			UpdateMapMemoryVisibility(mapMemoryFullyVisible);
			UpdateDrawEnemies(drawEnemies);
		}

	}
}
