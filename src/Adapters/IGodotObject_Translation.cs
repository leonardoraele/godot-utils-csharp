using Godot;

namespace Raele.GodotUtils.Adapters;

public partial interface IGodotObject
{
	public void SetMessageTranslation(bool enable);
	public bool CanTranslateMessages();
	public string Tr(StringName message, StringName? context = null);
	public string TrN(StringName message, StringName pluralMessage, int n, StringName? context = null);
	public StringName GetTranslationDomain();
	public void SetTranslationDomain(StringName domain);
}
