using System.Collections.Generic;
using Godot;
using YAT.Attributes;
using YAT.Enums;
using YAT.Helpers;
using YAT.Interfaces;
using YAT.Scenes.Terminal;

namespace YAT.Commands
{
	[Command(
		"cn",
		"Changes the selected node to the specified node path.",
		"[b]Usage[/b]: cn [i]node_path[/i]"
	)]
	[Argument("node_path", "string", "The node path of the new selected node.")]
	public partial class Cn : ICommand
	{
		public YAT Yat { get; set; }

		public Cn(YAT Yat) => this.Yat = Yat;

		private const float DEFAULT_RAY_LENGTH = 256;
		private const char RAY_CAST_PREFIX = '&';

		public CommandResult Execute(Dictionary<string, object> cArgs, params string[] args)
		{
			var path = cArgs["node_path"] as string;
			bool result;

			// If the path starts with RAY_CAST_PREFIX use RayCast to get the node path
			// where the camera is looking at
			if (path.StartsWith(RAY_CAST_PREFIX)) result = ChangeSelectedNode(GetNodePath(path));
			else result = ChangeSelectedNode(path);

			if (!result) Yat.Terminal.Print($"Invalid node path: {path}", Terminal.PrintType.Error);

			return result ? CommandResult.Success : CommandResult.Failure;
		}

		private NodePath GetNodePath(string path)
		{
			var result = World.RayCast(Yat.GetViewport(), GetRayLength(path));

			if (result is null)
			{
				Yat.Terminal.Print("No collider found.", Terminal.PrintType.Error);
				return null;
			}

			Node collider = result.TryGetValue("collider", out Variant value) ? value.As<Node>() : null;

			return collider?.GetPath();
		}

		private static float GetRayLength(string path)
		{
			return NumericHelper.TryConvert(path[1..], out float rayLength)
				? rayLength
				: DEFAULT_RAY_LENGTH;
		}

		private bool ChangeSelectedNode(NodePath path)
		{
			return Yat.Terminal.SelectedNode.ChangeSelectedNode(path);
		}
	}
}
