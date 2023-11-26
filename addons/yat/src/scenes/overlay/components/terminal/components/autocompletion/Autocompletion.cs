using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using YAT.Attributes;
using YAT.Helpers;

public partial class Autocompletion : PanelContainer
{
	private YAT.YAT _yat;
	private RichTextLabel _text;
	private MarginContainer _container;
	private YAT.Scenes.Overlay.Components.Terminal.Input _input;

	private string cachedInput = string.Empty;
	private string[] suggestions = Array.Empty<string>();
	private uint suggestionIndex = 0;

	public override void _Ready()
	{
		_yat = GetNode<YAT.YAT>("/root/YAT");
		_text = GetNode<RichTextLabel>("%Text");
		_container = GetNode<MarginContainer>("./MarginContainer");
		_input = GetNode<YAT.Scenes.Overlay.Components.Terminal.Input>("../HBoxContainer/Input");
		_input.TextChanged += UpdateCommandInfo;

		_container.Visible = false;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("yat_terminal_autocompletion") && _input.HasFocus())
		{
			Autocomplete();
			_input.CallDeferred("grab_focus"); // Prevent toggling the input focus
		}
	}

	/// <summary>
	/// Updates the command information based on the provided text.
	/// </summary>
	/// <param name="text">The text to update the command information with.</param>
	private void UpdateCommandInfo(string text)
	{
		var tokens = TextHelper.SanitizeText(text);

		if (!AreTokensValid(tokens)) return;

		DisplayCommandInfo(GenerateCommandInfo(tokens));
	}

	/// <summary>
	/// Generates the command information based on the given tokens.
	/// </summary>
	/// <param name="tokens">The tokens representing the command and its arguments.</param>
	/// <returns>The generated command information.</returns>
	private string GenerateCommandInfo(string[] tokens)
	{
		var command = _yat.Commands[tokens[0]];
		var commandAttribute = command.GetAttribute<CommandAttribute>();
		var commandArguments = command.GetAttribute<ArgumentsAttribute>();

		StringBuilder commandInfo = new();
		commandInfo.Append(commandAttribute.Name);

		if (commandArguments != null)
		{
			var keys = commandArguments.Args.Keys;
			for (int i = 0; i < keys.Count; i++)
			{
				string key = keys.ElementAt(i);
				var arg = commandArguments.Args[key];
				bool current = tokens.Length - 1 == i;
				bool valid = CommandHelper.ValidateCommandArgument(
					commandAttribute.Name,
					arg,
					new Dictionary<string, object>() { { key, arg } },
					(tokens.Length - 1 >= i + 1) ? tokens[i + 1] : string.Empty,
					false
				);

				string argument = string.Format(
					" {0}{1}<{2}:{3}>{4}{5}",
					valid ? string.Empty : $"[color={_yat.Options.ErrorColor.ToHtml()}]",
					current ? "[b]" : string.Empty,
					key,
					(arg is string[]) ? "options" : arg,
					current ? "[/b]" : string.Empty,
					valid ? string.Empty : "[/color]"
				);

				commandInfo.Append(argument);

				if (i < keys.Count - 1) commandInfo.Append(' ');
			}
		}

		return commandInfo.ToString();
	}

	/// <summary>
	/// Displays the command information in the terminal.
	/// </summary>
	/// <param name="commandInfo">The command information to display.</param>
	private void DisplayCommandInfo(string commandInfo)
	{
		_text.Clear();
		_text.AppendText(commandInfo);
	}

	/// <summary>
	/// Checks if the given tokens are valid.
	/// </summary>
	/// <param name="tokens">The tokens to be checked.</param>
	/// <returns>True if the tokens are valid, false otherwise.</returns>
	private bool AreTokensValid(string[] tokens)
	{
		if (tokens.Length == 0 || !_yat.Commands.ContainsKey(tokens[0]))
		{
			_container.Visible = false;
			return false;
		}

		_container.Visible = true;
		return true;
	}

	/// <summary>
	/// Provides suggestions for autocompletion of user input in the terminal.
	/// </summary>
	private void Autocomplete()
	{
		if (suggestions.Length > 0 &&
			(_input.Text == cachedInput || suggestions.Contains(_input.Text))
		)
		{
			UseNextSuggestion();
			return;
		}

		cachedInput = _input.Text;
		suggestions = Array.Empty<string>();
		suggestionIndex = 0;

		var tokens = TextHelper.SanitizeText(_input.Text);

		if (tokens.Length == 1)
		{
			suggestions = GenerateCommandSuggestions(tokens[0]);

			if (suggestions.Length > 0) UseNextSuggestion();

			return;
		}
	}

	/// <summary>
	/// Selects the next suggestion from the list of suggestions
	/// and updates the text input with it.
	/// </summary>
	private void UseNextSuggestion()
	{
		if (suggestions.Length == 0) return;

		suggestionIndex = (uint)((suggestionIndex + 1) % suggestions.Length);
		_input.Text = suggestions[suggestionIndex];

		_input.MoveCaretToEnd();
	}

	/// <summary>
	/// Generates an array of command suggestions based on the input state.
	/// </summary>
	/// <param name="token">The current input state.</param>
	/// <returns>An array of command suggestions.</returns>
	private string[] GenerateCommandSuggestions(string token)
	{
		return _yat.Commands
			.Where(x => x.Value.GetAttribute<CommandAttribute>()?.Name?.StartsWith(token) == true)
			.Select(x => x.Value.GetAttribute<CommandAttribute>()?.Name)
			.Where(name => !string.IsNullOrEmpty(name))
			.Distinct()
			.ToArray();
	}
}