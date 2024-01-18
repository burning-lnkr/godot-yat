using Godot;
using YAT.Attributes;
using YAT.Enums;
using YAT.Helpers;
using YAT.Interfaces;
using YAT.Scenes.BaseTerminal;
using static YAT.Scenes.BaseTerminal.BaseTerminal;

namespace YAT.Commands
{
	[Command(
		"cn",
		"Changes the selected node to the specified node path.",
		"[b]Usage[/b]: cn [i]node_path[/i]"
	)]
	[Argument(
		"node_path",
		"string",
		"The node path of the new selected node.\n" +
		"Use [b]&[/b] to use the RayCast method to get the node path where the camera is looking at.\n" +
		"Use [b]&[/b][i]ray_length[/i] to specify the ray length (default 256).\n" +
		"Example: [b]&[/b] or [b]&[/b][i]256[/i]" +
		"\nUse [b]?[/b] to search the node path in the whole tree.\n" +
		"Use [b]??[/b] to search the node path in the current node children.\n"
	)]
	public partial class Cn : ICommand
	{
		private const char RAY_CAST_PREFIX = '&';
		private const float DEFAULT_RAY_LENGTH = 256;

		private const string TREE_DEEP_SEARCH_PREFIX = "?";
		private const string TREE_SHALLOW_SEARCH_PREFIX = "??";

		private YAT _yat;
		private BaseTerminal _terminal;

		public CommandResult Execute(CommandData args)
		{
			var path = args.ConvertedArgs["node_path"] as string;
			bool result;

			_yat = args.Yat;
			_terminal = args.Terminal;

			if (Text.StartsWith(path, TREE_SHALLOW_SEARCH_PREFIX, TREE_DEEP_SEARCH_PREFIX))
				result = ChangeSelectedNode(GetNodeFromSearch(path));
			else if (path.StartsWith(RAY_CAST_PREFIX)) result = ChangeSelectedNode(GetRayCastedNodePath(path));
			else result = ChangeSelectedNode(path);

			if (!result)
			{
				args.Terminal.Print($"Invalid node path: {path}", PrintType.Error);
				return CommandResult.Failure;
			}

			return CommandResult.Success;
		}

		private NodePath GetNodeFromSearch(string path)
		{
			Node result = path.StartsWith(TREE_SHALLOW_SEARCH_PREFIX)
				? World.SearchNode(_terminal.SelectedNode.Current.GetParent(), path[2..])
				: World.SearchNode(_yat.GetTree().Root, path[1..]);

			if (result is null)
			{
				_terminal.Print("No node found.", PrintType.Error);
				return null;
			}

			return result.GetPath();
		}

		private NodePath GetRayCastedNodePath(string path)
		{
			var result = World.RayCast(_yat.GetViewport(), GetRayLength(path));

			if (result is null)
			{
				_terminal.Print("No collider found.", PrintType.Error);
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
			return _terminal.SelectedNode.ChangeSelectedNode(path);
		}
	}
}
