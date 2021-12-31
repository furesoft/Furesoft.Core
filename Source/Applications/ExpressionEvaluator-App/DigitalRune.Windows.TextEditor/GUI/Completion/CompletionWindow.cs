using DigitalRune.Windows.TextEditor.Document;
using DigitalRune.Windows.TextEditor.Utilities;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DigitalRune.Windows.TextEditor.Completion
{
    /// <summary>
    /// The default code completion window.
    /// </summary>
    public class CompletionWindow : AbstractCompletionWindow
    {
        private const int _maxListLength = 12;
        private const int _scrollbarWidth = 16;
        private readonly bool _closeAutomatically;
        private readonly ICompletionData[] _completionData;
        private readonly ICompletionDataProvider _dataProvider;
        private readonly IDocument _document;
        private readonly bool _fixedListViewWidth = true;
        private readonly MouseWheelHandler _mouseWheelHandler = new MouseWheelHandler();
        private readonly bool _showDeclarationWindow = true;
        private readonly TextLocation _textLocation;
        private readonly VScrollBar _vScrollBar;
        private CompletionListView _completionListView;
        private DeclarationViewWindow _declarationViewWindow;
        private int _endOffset;
        private bool _inScrollUpdate;
        private int _startOffset;
        private Rectangle _workingScreen;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompletionWindow"/> class.
        /// </summary>
        /// <param name="completionDataProvider">The completion data provider.</param>
        /// <param name="completionData">The completion data.</param>
        /// <param name="parentForm">The parent form.</param>
        /// <param name="control">The text editor control.</param>
        /// <param name="showDeclarationWindow"><see langword="true"/> to show declaration window; otherwise <see langword="false"/>.</param>
        /// <param name="fixedListViewWidth"><see langword="true"/> to use a fixed width in the list view.</param>
        /// <param name="closeAutomatically"><see langword="true"/> to close the completion window automatically.</param>
        private CompletionWindow(ICompletionDataProvider completionDataProvider, ICompletionData[] completionData, Form parentForm, TextEditorControl control, bool showDeclarationWindow, bool fixedListViewWidth, bool closeAutomatically)
          : base(parentForm, control)
        {
            _dataProvider = completionDataProvider;
            _completionData = completionData;
            _document = control.Document;
            _showDeclarationWindow = showDeclarationWindow;
            _fixedListViewWidth = fixedListViewWidth;
            _closeAutomatically = closeAutomatically;

            int caretOffset = control.ActiveTextAreaControl.Caret.Offset;
            _startOffset = caretOffset;
            _endOffset = caretOffset;

            // Move start offset if something is pre-selected.
            if (!String.IsNullOrEmpty(completionDataProvider.PreSelection))
                _startOffset -= completionDataProvider.PreSelection.Length;

            _completionListView = new CompletionListView(completionData);
            _completionListView.Dock = DockStyle.Fill;
            _completionListView.FilterList = true;
            _completionListView.ImageList = completionDataProvider.ImageList;
            _completionListView.Click += CompletionListViewClick;
            _completionListView.DoubleClick += CompletionListViewDoubleClick;
            _completionListView.FirstItemChanged += CompletionListViewFirstItemChanged;
            _completionListView.ItemCountChanged += CompletionListViewItemCountChanged;
            _completionListView.SelectedItemChanged += CompletionListViewSelectedItemChanged;
            _completionListView.Width = 250;

            Controls.Add(_completionListView);

            _vScrollBar = new VScrollBar();
            _vScrollBar.SmallChange = 1;
            _vScrollBar.LargeChange = _maxListLength;
            _vScrollBar.Dock = DockStyle.Right;
            Controls.Add(_vScrollBar);
            UpdateScrollBar();

            _workingScreen = Screen.GetWorkingArea(Location);
            DrawingSize = GetListViewSize();
            _textLocation = TextEditorControl.ActiveTextAreaControl.TextArea.Caret.Position;
            SetLocation(_textLocation);

            if (_declarationViewWindow == null)
                _declarationViewWindow = new DeclarationViewWindow(parentForm);

            SetDeclarationViewLocation();
            _declarationViewWindow.Show();
            _declarationViewWindow.MouseMove += ControlMouseMove;
            control.Focus();
            CompletionListViewSelectedItemChanged(this, EventArgs.Empty);

            if (!String.IsNullOrEmpty(completionDataProvider.PreSelection))
            {
                // Select item based on pre-selection.
                CaretOffsetChanged(this, EventArgs.Empty);
            }
            else if (completionDataProvider.DefaultIndex >= 0)
            {
                // Select default item
                _completionListView.SelectItem(completionDataProvider.DefaultIndex);
            }

            _vScrollBar.ValueChanged += VScrollBarValueChanged;
            _document.DocumentAboutToBeChanged += DocumentAboutToBeChanged;
        }

        /// <summary>
        /// Shows the completion window.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="control">The text editor control.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="completionDataProvider">The completion data provider.</param>
        /// <param name="firstChar">The first char.</param>
        /// <param name="showDeclarationWindow"><see langword="true"/> to show declaration window; otherwise <see langword="false"/>.</param>
        /// <param name="fixedListViewWidth"><see langword="true"/> to use a fixed width in the list view.</param>
        /// <param name="closeAutomatically"><see langword="true"/> to close the completion window automatically.</param>
        /// <returns>The code completion window.</returns>
        public static CompletionWindow ShowCompletionWindow(Form parent, TextEditorControl control, string fileName, ICompletionDataProvider completionDataProvider, char firstChar, bool showDeclarationWindow, bool fixedListViewWidth, bool closeAutomatically)
        {
            ICompletionData[] completionData = completionDataProvider.GenerateCompletionData(fileName, control.ActiveTextAreaControl.TextArea, firstChar);
            if (completionData == null || completionData.Length == 0)
                return null;

            CompletionWindow codeCompletionWindow = new CompletionWindow(completionDataProvider, completionData, parent, control, showDeclarationWindow, fixedListViewWidth, closeAutomatically);
            codeCompletionWindow.ShowCompletionWindow();
            return codeCompletionWindow;
        }

        /// <summary>
        /// Handles mouse wheel events.
        /// </summary>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
        public void HandleMouseWheel(MouseEventArgs e)
        {
            int scrollDistance = _mouseWheelHandler.GetScrollAmount(e);
            if (scrollDistance == 0)
                return;
            if (TextEditorControl.TextEditorProperties.MouseWheelScrollDown)
                scrollDistance = -scrollDistance;
            int newValue = _vScrollBar.Value + _vScrollBar.SmallChange * scrollDistance;
            _vScrollBar.Value = Math.Max(_vScrollBar.Minimum, Math.Min(_vScrollBar.Maximum - _vScrollBar.LargeChange + 1, newValue));
        }

        /// <summary>
        /// Processes key-events.
        /// </summary>
        /// <param name="ch">The character pressed.</param>
        /// <returns>
        /// 	<see langword="true"/> if the key has been handled by the completion window;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public override bool ProcessKeyEvent(char ch)
        {
            switch (_dataProvider.ProcessKey(ch))
            {
                case CompletionDataProviderKeyResult.BeforeStartKey:
                    // increment start+end, then process as normal char
                    ++_startOffset;
                    ++_endOffset;
                    return base.ProcessKeyEvent(ch);

                case CompletionDataProviderKeyResult.NormalKey:
                    return base.ProcessKeyEvent(ch);

                case CompletionDataProviderKeyResult.InsertionKey:
                    return InsertSelectedItem(ch);

                default:
                    throw new InvalidOperationException("Invalid return value of dataProvider.ProcessKey");
            }
        }

        /// <summary>
        /// Handles changes of the caret position.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void CaretOffsetChanged(object sender, EventArgs e)
        {
            int caretOffset = TextEditorControl.ActiveTextAreaControl.Caret.Offset;

            if (caretOffset == _startOffset)
            {
                _completionListView.SelectItemWithStart(String.Empty);
                return;
            }
            if (caretOffset < _startOffset || caretOffset > _endOffset)
            {
                Close();
            }
            else
            {
                _completionListView.SelectItemWithStart(TextEditorControl.Document.GetText(_startOffset, caretOffset - _startOffset));
                if (_closeAutomatically && _completionListView.SelectedItem == null)
                {
                    // Text typed in the document does not match any entry of the completion window.
                    // -> Automatically close window.
                    Close();
                }
            }
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the <see cref="T:System.Windows.Forms.Form"/>.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _document.DocumentAboutToBeChanged -= DocumentAboutToBeChanged;
                if (_completionListView != null)
                {
                    _completionListView.Dispose();
                    _completionListView = null;
                }
                if (_declarationViewWindow != null)
                {
                    _declarationViewWindow.Dispose();
                    _declarationViewWindow = null;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Called when <see cref="Form.Opacity"/> is changed.
        /// </summary>
        protected override void OnOpacityChanged()
        {
            base.OnOpacityChanged();
            if (_declarationViewWindow != null)
                _declarationViewWindow.Opacity = Opacity;
        }

        /// <summary>
        /// Processes dialog-key events (escape, tab, etc.).
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="keyEventArgs">
        /// The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing the event data.
        /// </param>
        protected override void ProcessTextAreaKey(object sender, KeyEventArgs keyEventArgs)
        {
            if (!Visible)
                return;

            switch (keyEventArgs.KeyData)
            {
                case Keys.Home:
                    _completionListView.SelectItem(0);
                    keyEventArgs.Handled = true;
                    return;

                case Keys.End:
                    _completionListView.SelectItem(_completionListView.ItemCount - 1);
                    keyEventArgs.Handled = true;
                    return;

                case Keys.PageDown:
                    _completionListView.PageDown();
                    keyEventArgs.Handled = true;
                    return;

                case Keys.PageUp:
                    _completionListView.PageUp();
                    keyEventArgs.Handled = true;
                    return;

                case Keys.Down:
                    _completionListView.SelectNextItem();
                    keyEventArgs.Handled = true;
                    return;

                case Keys.Up:
                    _completionListView.SelectPrevItem();
                    keyEventArgs.Handled = true;
                    return;

                case Keys.Tab:
                    InsertSelectedItem('\t');
                    keyEventArgs.Handled = true;
                    return;

                case Keys.Return:
                    InsertSelectedItem('\n');
                    keyEventArgs.Handled = true;
                    return;
            }
            base.ProcessTextAreaKey(sender, keyEventArgs);
        }

        /// <summary>
        /// Sets the location.
        /// </summary>
        protected override void SetLocation(TextLocation textLocation)
        {
            base.SetLocation(textLocation);
            if (_declarationViewWindow != null)
                SetDeclarationViewLocation();
        }

        private void CompletionListViewClick(object sender, EventArgs e)
        {
            TextEditorControl.ActiveTextAreaControl.TextArea.Focus();
        }

        private void CompletionListViewDoubleClick(object sender, EventArgs e)
        {
            InsertSelectedItem('\0');
        }

        private void CompletionListViewFirstItemChanged(object sender, EventArgs e)
        {
            UpdateScrollBar();
        }

        private void CompletionListViewItemCountChanged(object sender, EventArgs e)
        {
            UpdateScrollBar();
            UpdateSize();
            SetLocation(_textLocation);
        }

        private void CompletionListViewSelectedItemChanged(object sender, EventArgs e)
        {
            ICompletionData data = _completionListView.SelectedItem;
            if (_showDeclarationWindow && data != null && !String.IsNullOrEmpty(data.Description))
            {
                _declarationViewWindow.Description = data.Description;
                SetDeclarationViewLocation();
            }
            else
            {
                _declarationViewWindow.Description = null;
            }
        }

        private void DocumentAboutToBeChanged(object sender, DocumentEventArgs e)
        {
            // => startOffset test required so that this startOffset/endOffset are not incremented again
            //    for BeforeStartKey characters
            if (e.Offset >= _startOffset && e.Offset <= _endOffset)
            {
                if (e.Length > 0)
                {
                    // Text has been removed
                    _endOffset -= e.Length;
                }
                if (!String.IsNullOrEmpty(e.Text))
                {
                    // Text has been added.
                    _endOffset += e.Text.Length;
                }
            }
        }

        private Size GetListViewSize()
        {
            int height = _completionListView.ItemHeight * Math.Min(_maxListLength, _completionListView.ItemCount);
            int width = _completionListView.ItemHeight * 10;
            if (!_fixedListViewWidth)
                width = GetListViewWidth(width, height);
            return new Size(width, height);
        }

        /// <summary>
        /// Gets the list view width large enough to handle the longest completion data
        /// text string.
        /// </summary>
        /// <param name="defaultWidth">The default width of the list view.</param>
        /// <param name="height">The height of the list view.  This is
        /// used to determine if the scrollbar is visible.</param>
        /// <returns>The list view width to accommodate the longest completion
        /// data text string; otherwise the default width.</returns>
        private int GetListViewWidth(int defaultWidth, int height)
        {
            float width = defaultWidth;
            using (Graphics graphics = _completionListView.CreateGraphics())
            {
                int imageWidth = _completionListView.ImageWidth;
                for (int i = 0; i < _completionData.Length; ++i)
                {
                    float itemWidth = imageWidth + graphics.MeasureString(_completionData[i].Text, _completionListView.Font).Width;
                    if (itemWidth > width)
                        width = itemWidth;
                }
            }

            float totalItemsHeight = _completionListView.ItemHeight * _completionData.Length;
            if (totalItemsHeight > height)
            {
                // Compensate for scroll bar.
                width += _scrollbarWidth;
            }
            return (int)width;
        }

        private bool InsertSelectedItem(char ch)
        {
            _document.DocumentAboutToBeChanged -= DocumentAboutToBeChanged;
            ICompletionData data = _completionListView.SelectedItem;
            bool result = false;
            if (data != null)
            {
                TextEditorControl.BeginUpdate();
                try
                {
                    // Remove already typed text
                    if (_endOffset - _startOffset > 0)
                        TextEditorControl.Document.Remove(_startOffset, _endOffset - _startOffset);

                    Debug.Assert(_startOffset <= _document.TextLength);

                    // Insert text from completion data
                    result = _dataProvider.InsertAction(data, TextEditorControl.ActiveTextAreaControl.TextArea, _startOffset, ch);
                }
                finally
                {
                    TextEditorControl.EndUpdate();
                }
            }
            Close();
            return result;
        }

        private void SetDeclarationViewLocation()
        {
            //  This method uses the side with more free space
            int leftSpace = Bounds.Left - _workingScreen.Left;
            int rightSpace = _workingScreen.Right - Bounds.Right;

            Point pos;
            // The declaration view window has better line break when used on
            // the right side, so prefer the right side to the left.
            if (rightSpace * 2 > leftSpace)
            {
                _declarationViewWindow.FixedWidth = false;
                pos = new Point(Bounds.Right, Bounds.Top);
                if (_declarationViewWindow.Location != pos)
                {
                    _declarationViewWindow.Location = pos;
                }
            }
            else
            {
                _declarationViewWindow.Width = _declarationViewWindow.GetRequiredLeftHandSideWidth(new Point(Bounds.Left, Bounds.Top));
                _declarationViewWindow.FixedWidth = true;
                if (Bounds.Left < _declarationViewWindow.Width)
                {
                    pos = new Point(0, Bounds.Top);
                }
                else
                {
                    pos = new Point(Bounds.Left - _declarationViewWindow.Width, Bounds.Top);
                }
                if (_declarationViewWindow.Location != pos)
                {
                    _declarationViewWindow.Location = pos;
                }
                _declarationViewWindow.Refresh();
            }
        }

        private void UpdateScrollBar()
        {
            if (_inScrollUpdate)
                return;

            _inScrollUpdate = true;
            if (_completionListView.ItemCount <= _maxListLength)
            {
                _vScrollBar.Visible = false;
            }
            else
            {
                _vScrollBar.Visible = true;
                _vScrollBar.Minimum = 0;
                _vScrollBar.Maximum = _completionListView.ItemCount - 1;
                _vScrollBar.Value = Math.Min(_vScrollBar.Maximum, _completionListView.FirstVisibleItem);
            }
            _inScrollUpdate = false;
        }

        private void UpdateSize()
        {
            Size newSize = GetListViewSize();
            DrawingSize = newSize;
            Size = newSize;
        }

        private void VScrollBarValueChanged(object sender, EventArgs e)
        {
            if (_inScrollUpdate)
                return;

            _inScrollUpdate = true;
            _completionListView.FirstVisibleItem = _vScrollBar.Value;
            _completionListView.Refresh();
            TextEditorControl.ActiveTextAreaControl.TextArea.Focus();
            _inScrollUpdate = false;
        }
    }
}