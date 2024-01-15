using Godot;

namespace YAT.Scenes.YatWindow
{
	public partial class YatWindow : Window
	{
		[Export(PropertyHint.Range, "0, 128, 1")]
		public int ViewportEdgeOffset = 48;

		[Export] public EWindowPosition DefaultWindowPosition = EWindowPosition.Center;
		[Export] public bool AllowToGoOffscreen = true; // TODO: Implement this

		public Vector2I InitialSize { get; set; }

		private YAT _yat;
		private Viewport _viewport;

		public enum EWindowPosition
		{
			TopLeft,
			TopRight,
			BottomLeft,
			BottomRight,
			Center
		}

		public override void _Ready()
		{
			_yat = GetNode<YAT>("/root/YAT");
			_viewport = _yat.GetTree().Root.GetViewport();
			_viewport.SizeChanged += OnViewportSizeChanged;

			InitialSize = Size;

			Move(DefaultWindowPosition, (uint)ViewportEdgeOffset);
			OnViewportSizeChanged();
		}

		public void ResetPosition()
		{
			Move(DefaultWindowPosition, (uint)ViewportEdgeOffset);
		}

		private void OnViewportSizeChanged()
		{
			var viewportSize = (Vector2I)_viewport.GetVisibleRect().Size;

			MaxSize = MaxSize with
			{
				X = viewportSize.X - ViewportEdgeOffset,
				Y = viewportSize.Y - ViewportEdgeOffset
			};
		}

		public void Move(EWindowPosition position, uint offset = 0)
		{
			switch (position)
			{
				case EWindowPosition.TopLeft:
					MoveTopLeft(offset);
					break;
				case EWindowPosition.TopRight:
					MoveTopRight(offset);
					break;
				case EWindowPosition.BottomRight:
					MoveBottomRight(offset);
					break;
				case EWindowPosition.BottomLeft:
					MoveBottomLeft(offset);
					break;
				case EWindowPosition.Center:
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
