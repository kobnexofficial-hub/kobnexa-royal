namespace KobnexaRoyal
{
    using System.Collections.Generic;

    /// <summary>
    /// Pure C# navigation history for the lobby menus. Unit-testable, no Unity UI dependency.
    /// Home is the root: Back from Home stays on Home.
    /// </summary>
    public sealed class MenuNavigationStack
    {
        public const string HomePanelId = "Home";
        private readonly Stack<string> history = new Stack<string>();

        public string Current { get; private set; }

        public MenuNavigationStack()
        {
            Current = HomePanelId;
        }

        public int Depth
        {
            get { return history.Count; }
        }

        public bool CanGoBack
        {
            get { return history.Count > 0; }
        }

        public string GoTo(string panelId)
        {
            if (string.IsNullOrEmpty(panelId) || panelId == Current) return Current;
            history.Push(Current);
            Current = panelId;
            return Current;
        }

        public string GoBack()
        {
            if (history.Count == 0)
            {
                Current = HomePanelId;
                return Current;
            }
            Current = history.Pop();
            return Current;
        }

        public string GoHome()
        {
            history.Clear();
            Current = HomePanelId;
            return Current;
        }
    }
}
