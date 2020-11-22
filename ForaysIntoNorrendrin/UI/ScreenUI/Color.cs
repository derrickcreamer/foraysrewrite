using System;
using System.Runtime.InteropServices;

namespace ForaysUI.ScreenUI{
	[StructLayout(LayoutKind.Sequential)] // should be the default but make it explicit. Should be exactly 64 bits.
	public struct ColorGlyph : IEquatable<ColorGlyph>{
		public readonly int GlyphIndex;
		public readonly Color ForegroundColor;
		public readonly Color BackgroundColor;

		public ColorGlyph(int glyphIndex, Color color, Color bgColor = Color.Black){
			GlyphIndex = glyphIndex;
			ForegroundColor = color;
			BackgroundColor = bgColor;
		}
		public override bool Equals(object obj){
			if(obj is ColorGlyph) return Equals((ColorGlyph)obj);
			else return false;
		}
		public bool Equals(ColorGlyph other){
			return GlyphIndex == other.GlyphIndex && ForegroundColor == other.ForegroundColor && BackgroundColor == other.BackgroundColor;
		}
		public bool Equals(int glyphIndex, Color color, Color bgColor){
			return GlyphIndex == glyphIndex && ForegroundColor == color && BackgroundColor == bgColor;
		}
		public override int GetHashCode(){
			// If I assume that the sign bit will always end up 0 and I don't bother fixing that, then there are 31 bits to work with.
			// Since there will probably be fewer than 512 colors, let's take only 9 bits from each Color.
			// That leaves 13 bits of the GlyphIndex, so 8192 values for the glyph. That should be plenty!
			int result = GlyphIndex; // Lower 13
			result |= (ushort)ForegroundColor << 13; // Next 9
			result |= (ushort)BackgroundColor << 22; // Final 9
			return result;
		}
	}
	public enum Color : ushort{ // 2 bytes
		Black, Gray, Cyan, White, DarkGray, Yellow, Todo  //todo reorder
	};
}
