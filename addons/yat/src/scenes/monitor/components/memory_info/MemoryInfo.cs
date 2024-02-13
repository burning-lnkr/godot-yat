using Godot;
using YAT.Helpers;
using YAT.Interfaces;

namespace YAT.Scenes;

public partial class MemoryInfo : PanelContainer, IMonitorComponent
{
	public bool UseColors { get; set; }

	private YAT _yat;
	private RichTextLabel _label;

	public override void _Ready()
	{
		_yat = GetNode<YAT>("/root/YAT");
		_label = GetNode<RichTextLabel>("RichTextLabel");
	}

	public void Update()
	{
		var mem = Godot.OS.GetMemoryInfo();
		var physical = mem["physical"];
		var free = mem["free"];
		var stack = mem["stack"];
		var vram = Performance.GetMonitor(Performance.Monitor.RenderVideoMemUsed);

		var freePercent = (float)free / (float)physical * 100f;

		_label.Clear();
		_label.AppendText(
			$"RAM: {Numeric.SizeToString(physical.AsInt64(), 3)}\n" +
			$"Free: {(
				freePercent < 15 && UseColors
				? $"[color={_yat.PreferencesManager.Preferences.ErrorColor}]{Numeric.SizeToString(free.AsInt64(), 3)}[/color]"
				: Numeric.SizeToString(free.AsInt64(), 3))}\n" +
			$"Stack: {Numeric.SizeToString(stack.AsInt64(), 1)}\n" +
			$"VRAM: {Numeric.SizeToString((long)vram, 2)}"
		);
	}
}
