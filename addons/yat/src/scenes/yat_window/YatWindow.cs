using Godot;

namespace YAT.Scenes.YatWindow
{
	public partial class YatWindow : Window
	{
		public enum WindowPosition
		{
			TopLeft,
			TopRight,
			BottomLeft,
			BottomRight,
			Center
		}

		public void Move(WindowPosition position, uint offset = 0)
		{
			switch (position)
			{
				case WindowPosition.TopLeft:
					MoveTopLeft(offset);
					break;
				case WindowPosition.TopRight:
					MoveTopRight(offset);
					break;
				case WindowPosition.BottomRight:
					MoveBottomRight(offset);
					break;
				case WindowPosition.BottomLeft:
					MoveBottomLeft(offset);
					break;
				case WindowPosition.Center:
					MoveToTheCenter();
					break;
			}
		}

		protected void MoveTopLeft(uint offset)
		{
			Position = new(
				(int)(0 + offset),
				(int)(0 + offset)
			);
		}

		protected void MoveTopRight(uint offset)
		{
			var viewportRect = GetTree().Root.GetViewport().GetVisibleRect();
			var bottomLeft = viewportRect.Position + viewportRect.Size;
			var rect = GetVisibleRect();

			Position = new(
				(int)(bottomLeft.X - rect.Size.X - offset),
				(int)(0 + offset)
			);
		}

		protected void MoveBottomRight(uint offset)
		{
			var viewportRect = GetTree().Root.GetViewport().GetVisibleRect();
			var topRight = viewportRect.Position + viewportRect.Size;
			var rect = GetVisibleRect();

			Position = new(
				(int)(topRight.X - rect.Size.X - offset),
				(int)(topRight.Y - rect.Size.Y - offset)
			);
		}

		protected void MoveBottomLeft(uint offset)
		{
			var viewportRect = GetTree().Root.GetViewport().GetVisibleRect();
			var bottomLeft = viewportRect.Position + viewportRect.Size;
			var rect = GetVisibleRect();

			Position = new(
				(int)(0 + offset),
				(int)(bottomLeft.Y - rect.Size.Y - offset)
			);
		}

		protected void MoveToTheCenter()
		{
			MoveToCenter();
		}
	}
}
