using Godot;
using YAT.Helpers;

namespace YAT.Scenes.Overlay.Components.Terminal
{
	public partial class Input : LineEdit
	{
		private YAT _yat;
		private CommandManager _commandManager;

		public override void _Ready()
		{
			_yat = GetNode<YAT>("/root/YAT");
			_commandManager = _yat.GetNode<CommandManager>("CommandManager");

			TextSubmitted += OnTextSubmitted;
		}

		/// <summary>
		/// Handles the submission of a command by sanitizing the input,
		/// executing the command, and clearing the input buffer.
		/// </summary>
		/// <param name="command">The command to be submitted.</param>
		private void OnTextSubmitted(string command)
		{
			// If the input string starts with a specified character,
			// treat it as a method call on the selected node
			if (command.StartsWith('$'))
			{
				_yat.Terminal.SelectedNode.ParseAndCallMethods(command[1..]);
				AddToTheHistory(command);
				Clear();
				return;
			}

			var input = TextHelper.SanitizeText(command);
			input = TextHelper.ConcatenateSentence(input);

			if (input.Length == 0 || _commandManager.Locked) return;

			AddToTheHistory(command);

			_commandManager.Run(input);
			Clear();
		}

		private void AddToTheHistory(string command)
		{
			_yat.HistoryNode = null;
			_yat.History.AddLast(command);
			if (_yat.History.Count > _yat.Options.HistoryLimit) _yat.History.RemoveFirst();
		}

		/// <summary>
		/// Moves the caret to the end of the input text.
		/// </summary>
		public void MoveCaretToEnd()
		{
			CaretColumn = Text.Length;
		}
	}
}
