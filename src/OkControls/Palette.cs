namespace OkControls;

/// <summary>
/// Default colours of the OkControls (light look with a blue-grey accent taken from the OK logo).
/// The controls only use these as defaults – every colour can be overridden through the control's properties.
/// Kept separate from the app's Theme so the library has no dependency on the app.
/// </summary>
internal static class Palette
{
    public static readonly Color Accent = Color.FromArgb(62, 99, 119);        // #3E6377
    public static readonly Color HoldFill = Color.FromArgb(196, 215, 228);    // #C4D7E4
    public static readonly Color HoldText = Color.FromArgb(47, 77, 94);       // #2F4D5E
    public static readonly Color Selection = Color.FromArgb(220, 231, 238);   // #DCE7EE
    public static readonly Color Border = Color.FromArgb(198, 208, 214);      // #C6D0D6
    public static readonly Color Text = Color.FromArgb(31, 42, 48);           // #1F2A30
    public static readonly Color TextMuted = Color.FromArgb(90, 105, 112);    // #5A6970
    public static readonly Color DisabledBack = Color.FromArgb(233, 237, 237);// #E9EDED
    public static readonly Color DisabledText = Color.FromArgb(154, 166, 171);// #9AA6AB
    public static readonly Color Warning = Color.FromArgb(232, 115, 12);      // #E8730C
}
