using System;
using GameComponents;

namespace ForaysUI.ScreenUI.MapRenderer{
	//todo, pretty sure that the maprenderer also needs to provide a method for telling us the map position of a mouse coord, eventually.
	public abstract class MapRenderer : GameUIObject{
		protected Point? cursor; // todo, if this Point is also highlighted, AUTOMATICALLY make it bright green or equivalent.
		protected Highlight highlight;
		protected bool memoryFullyVisible;
		protected bool drawEnemies;
		// monster info popups probably go here

		protected static Func<GameRunUI,MapRenderer> createMapRenderer;

		protected MapRenderer(GameRunUI ui) : base(ui){}
		public static void SetFactoryMethod(Func<GameRunUI,MapRenderer> createMapRenderer){
			MapRenderer.createMapRenderer = createMapRenderer;
		}
		public static MapRenderer Create(
			GameRunUI ui,
			Point? activeCursor,
			Highlight highlight = null,
			bool mapMemoryFullyVisible = false,
			bool drawEnemies = true)
		{
			MapRenderer m = createMapRenderer.Invoke(ui);
			m.cursor = activeCursor;
			m.highlight = highlight;
			m.memoryFullyVisible = mapMemoryFullyVisible;
			m.drawEnemies = drawEnemies;
			return m;
		}

		public abstract void DrawMap();
		// todo, somewhere i need to make available a map of what's visible this turn, right?
		// also need list of visible enemies this turn - is that handled by MapMemory or passed directly in?
		public abstract void HideMap();
		public abstract void UpdateCursorPosition(Point? cursor);
		public abstract void UpdateHighlight(Highlight highlight);
		public abstract void UpdateMapMemoryVisibility(bool mapMemoryFullyVisible);
		public abstract void UpdateDrawEnemies(bool drawEnemies);
	}
}
