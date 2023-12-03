using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1_WatchingKeys;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    string previousSequence = string.Empty;

    private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        IEnumerable<Key> downKeys = GetDownKeys();

        bool containsWin = downKeys.Contains(Key.LWin) || downKeys.Contains(Key.RWin);
        bool containsShift = downKeys.Contains(Key.LeftShift) || downKeys.Contains(Key.RightShift);
        bool containsCtrl = downKeys.Contains(Key.LeftCtrl) || downKeys.Contains(Key.RightCtrl);
        bool containsAlt = downKeys.Contains(Key.LeftAlt) || downKeys.Contains(Key.RightAlt);

        IEnumerable<Key> justLetterKeys = RemoveModifierKeys(downKeys);

        if (!justLetterKeys.Any() || !(containsWin || containsShift || containsCtrl || containsAlt))
            return;

        List<string> keyStrings = [];

        if (containsWin)
            keyStrings.Add("Win");

        if (containsShift)
            keyStrings.Add("Shift");

        if (containsCtrl)
            keyStrings.Add("Ctrl");

        if (containsAlt)
            keyStrings.Add("Alt");

        foreach (Key key in justLetterKeys)
            keyStrings.Add(key.ToString());

        string currentSequence = string.Join('+', keyStrings);

        if (string.IsNullOrEmpty(currentSequence))
        {
            MainTextBlock.Text += Environment.NewLine;
            MainTextBlock.Text += "< No Keys Pressed >";
        }

        if (currentSequence.Equals(previousSequence))
            return;

        MainTextBlock.Text += Environment.NewLine;
        MainTextBlock.Text += currentSequence;
        previousSequence = currentSequence;

        Scroller.ScrollToBottom();
    }

    private static IEnumerable<Key> RemoveModifierKeys(IEnumerable<Key> downKeys)
    {
        HashSet<Key> filteredKeys = new(downKeys);

        filteredKeys.Remove(Key.LWin);
        filteredKeys.Remove(Key.RWin);

        filteredKeys.Remove(Key.LeftShift);
        filteredKeys.Remove(Key.RightShift);

        filteredKeys.Remove(Key.LeftCtrl);
        filteredKeys.Remove(Key.RightCtrl);

        filteredKeys.Remove(Key.LeftAlt);
        filteredKeys.Remove(Key.RightAlt);


        return filteredKeys;
    }

    private static readonly byte[] DistinctVirtualKeys = Enumerable
    .Range(0, 256)
    .Select(KeyInterop.KeyFromVirtualKey)
    .Where(item => item != Key.None)
    .Distinct()
    .Select(item => (byte)KeyInterop.VirtualKeyFromKey(item))
    .ToArray();

    public static IEnumerable<Key> GetDownKeys()
    {
        var keyboardState = new byte[256];
        GetKeyboardState(keyboardState);

        List<Key> downKeys = [];
        for (var index = 0; index < DistinctVirtualKeys.Length; index++)
        {
            var virtualKey = DistinctVirtualKeys[index];
            if ((keyboardState[virtualKey] & 0x80) != 0)
                downKeys.Add(KeyInterop.KeyFromVirtualKey(virtualKey));
        }

        return downKeys;
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetKeyboardState(byte[] keyState);

    private void Window_Deactivated(object sender, EventArgs e)
    {
        MainTextBlock.Text += Environment.NewLine;
        MainTextBlock.Text += "Lost focus?";
        MainTextBlock.Text += Environment.NewLine;
        MainTextBlock.Text += "Maybe that shortcut is taken";
    }
}