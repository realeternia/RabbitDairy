using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

public class CustomTextBox : Panel
{
    private string _text = "";
    private int _cursorPosition = 0;
    private bool _cursorVisible = true;
    private Timer _cursorTimer;
    private Font _font;
    
    // 翻页相关变量
    private int _charsPerPage; // 每页显示的字符数
    private string _previousText = ""; // 用于检测文本是否变化

    // 导入Windows API用于光标和输入法
    [DllImport("user32.dll")]
    static extern bool CreateCaret(IntPtr hWnd, IntPtr hBitmap, int nWidth, int nHeight);

    [DllImport("user32.dll")]
    static extern bool ShowCaret(IntPtr hWnd);

    [DllImport("user32.dll")]
    static extern bool SetCaretPos(int X, int Y);

    [DllImport("user32.dll")]
    static extern bool DestroyCaret();

    [DllImport("imm32.dll")]
    public static extern IntPtr ImmGetContext(IntPtr hWnd);

    [DllImport("imm32.dll")]
    public static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM form);

    [DllImport("imm32.dll")]
    public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

    public struct COMPOSITIONFORM
    {
        public int dwStyle;
        public Point ptCurrentPos;
        public Rectangle rcArea;
    }

    public const int CFS_POINT = 0x0002;

    public CustomTextBox()
    {
        this.BackColor = Color.White;
        this.BorderStyle = BorderStyle.FixedSingle;
        this.Padding = new Padding(3);
        this._font = new Font("Microsoft YaHei", 9);

        // 设置光标定时器
        _cursorTimer = new Timer();
        _cursorTimer.Interval = 500; // 500ms闪烁间隔
        _cursorTimer.Tick += (s, e) =>
        {
            _cursorVisible = !_cursorVisible;
            this.Invalidate(); // 重绘控件
        };
        _cursorTimer.Start();

        // 启用双缓冲减少闪烁
        this.DoubleBuffered = true;

        // 设置控件样式支持键盘输入
        this.SetStyle(ControlStyles.Selectable, true);
        this.TabStop = true;
        
        // 初始化每页字符数
        UpdateCharsPerPage();
    }
    
    // 更新每页显示的字符数
    private void UpdateCharsPerPage()
    {
        Size cellSize = GetCellSize();
        int cellsPerRow = this.Width / cellSize.Width;
        int cellsPerColumn = this.Height / cellSize.Height;
        _charsPerPage = cellsPerRow * cellsPerColumn;
    }
    
    // 确保光标在当前页范围内
    private void CheckCursorPagePosition()
    {
        int pageStart = 0;
        int pageEnd = Math.Min(pageStart + _charsPerPage, _text.Length);
        
        // 如果光标不在当前页范围内，将其移动到页首
        if (_cursorPosition < pageStart || _cursorPosition > pageEnd)
        {
            _cursorPosition = Math.Max(pageStart, Math.Min(_cursorPosition, _text.Length));
        }
    }

    // 文本属性
    public string TextContent
    {
        get { return _text; }
        set
        {
            string oldValue = _text;
            _text = value ?? "";
            _cursorPosition = Math.Min(_cursorPosition, _text.Length);
            
            this.Invalidate();

            // 新增：文本变化时更新统计显示
            UpdatePageInfo();
        }
    }

    // 字体属性
    public Font TextFont
    {
        get { return _font; }
        set
        {
            _font = value;
            this.Invalidate();
        }
    }

    // 新增：外部用于显示页信息的 Label（可为 null）
    public Label LabelPageInfo { get; set; }

    // 新增：更新统计信息并写入 LabelPageInfo（字符数：不计回车/换行/空格；行数：按 '\n' 计）
    private void UpdatePageInfo()
    {
        if (LabelPageInfo == null) return;

        int charCount = 0;
        int lineCount = 0;

        if (string.IsNullOrEmpty(_text))
        {
            charCount = 0;
            lineCount = 0;
        }
        else
        {
            foreach (char c in _text)
            {
                if (c != '\r' && c != '\n' && c != ' ')
                    charCount++;
            }

            // 统计行数：按换行符分割，空文本视为0行
            lineCount = 1;
            foreach (char c in _text)
            {
                if (c == '\n') lineCount++;
            }
        }

        LabelPageInfo.Text = $"字数: {charCount}, 行数: {lineCount}";
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        Graphics g = e.Graphics;
        
        // 加载背景图像
        Image bgImage = null;
        try
        {
            if (File.Exists("cell.png"))
                bgImage = Image.FromFile("cell.png");

            var cellSize = new Size(bgImage != null ? bgImage.Width : 40, bgImage != null ? bgImage.Height : 40);
            var cellsPerRow = Width / cellSize.Width;
            var yCount = Height / cellSize.Height;

            // 绘制网格背景
            if(bgImage != null)
            for (int j = 0; j < yCount; j++)
            {
                for (int i = 0; i < cellsPerRow; i++)
                {
                    g.DrawImage(bgImage, i * cellSize.Width + 5, j * cellSize.Height + 5, cellSize.Width, cellSize.Height);
                }
            }

            // 计算当前页的起始和结束位置
            int pageStart = 0;
            int pageEnd = _text.Length;

            // 绘制文本（处理换行符）
            int currentRow = 0;
            int currentCol = 0;
            int charCountOnPage = 0;
            
            for (int i = pageStart; i < pageEnd; i++)
            {
                // 如果已达到页面字符上限，停止绘制
                if (charCountOnPage >= _charsPerPage)
                    break;
                
                char c = _text[i];
                
                if (c == '\n')
                {
                    // 处理换行符，移动到下一行开头
                    currentRow++;
                    currentCol = 0;
                    charCountOnPage++;
                }
                else
                {
                    // 绘制非换行符字符
                    if (currentRow < yCount && currentCol < cellsPerRow)
                    {
                        g.DrawString(c.ToString(), _font, Brushes.Black, currentCol * cellSize.Width + 5 + 7, currentRow * cellSize.Height + 20 + 5);
                    }
                    
                    // 移动到下一列
                    currentCol++;
                    charCountOnPage++;
                    if (currentCol >= cellsPerRow)
                    {
                        currentRow++;
                        currentCol = 0;
                    }
                }
            }

            // 绘制光标（当控件获得焦点且光标可见时）
            if (this.Focused && _cursorVisible)
            {
                var cursorPos = GetCursorPosition();
                int textHeight = GetTextHeight(g);
                
                using (Pen cursorPen = new Pen(Color.Black, 1))
                {
                    g.DrawLine(cursorPen,
                        cursorPos.X - 4, cursorPos.Y,
                        cursorPos.X - 4, cursorPos.Y + textHeight);
                }
            }
        }
        finally
        {
            // 释放图像资源
            bgImage?.Dispose();
        }
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        _cursorVisible = true;
        this.Invalidate();

        // 创建系统光标
        CreateNativeCaret();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        _cursorVisible = false;
        this.Invalidate();

        // 销毁系统光标
        DestroyCaret();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (this.Focused)
        {
            UpdateCaretPosition();
            UpdateInputMethodPosition();
        }
        
        // 更新每页字符数
        UpdateCharsPerPage();
        
        // 确保光标在当前页范围内
        CheckCursorPagePosition();
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        if (this.Focused)
        {
            UpdateCaretPosition();
            UpdateInputMethodPosition();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        this.Focus();

        // 计算相对于当前页的点击位置
        _cursorPosition = GetCursorPositionFromPoint(e.Location);
        
        this.Invalidate();
        UpdateInputMethodPosition();
    }

   
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        Size cellSize = GetCellSize();
        int cellsPerRow = this.Width / cellSize.Width;

        switch (e.KeyCode)
        {         
            case Keys.Left:
                if (_cursorPosition > 0)
                {
                    _cursorPosition--;
                    UpdateCaretPosition();
                }
                break;
            case Keys.Right:
                if (_cursorPosition < _text.Length)
                {
                    _cursorPosition++;
                    UpdateCaretPosition();
                }
                break;
            case Keys.Up:
                // 向上移动光标到上一行：在向上 cellsPerRow 范围内寻找最后一个 '\n'，若找到则移到该 '\n' 的前一格；否则减去 cellsPerRow。
                {
                    int cellsPerRowLocal = cellsPerRow;
                    int startRange = Math.Max(0, _cursorPosition - cellsPerRowLocal);
                    int endRange = Math.Max(0, _cursorPosition - 1);
                    int lastNewline = -1;
                    if (endRange >= 0)
                    {
                        lastNewline = _text.LastIndexOf('\n', endRange);
                        // 确保找到的换行符在我们的范围内
                        if (lastNewline < startRange)
                            lastNewline = -1;
                    }

                    if (lastNewline != -1)
                    {
                        // 移到该换行符的前一格（如果存在），否则到文本开头
                        int target = lastNewline - 1;
                        _cursorPosition = Math.Max(0, target);
                    }
                    else
                    {
                        // 没有换行符，按 cellsPerRow 向上移动
                        _cursorPosition = Math.Max(0, _cursorPosition - cellsPerRowLocal);
                    }

                    UpdateCaretPosition();
                }
                break;
            case Keys.Down:
                // 向下移动光标到下一行：在向下 cellsPerRow 范围内寻找第一个 '\n'，若找到则移到该 '\n' 的后一格；否则加上 cellsPerRow。
                {
                    int cellsPerRowLocal = cellsPerRow;
                    int startRange = _cursorPosition;
                    int endRange = Math.Min(_text.Length - 1, _cursorPosition + cellsPerRowLocal - 1);
                    int nextNewline = -1;
                    if (startRange <= endRange)
                    {
                        nextNewline = _text.IndexOf('\n', startRange);
                        // 确保找到的换行符在我们的范围内
                        if (nextNewline == -1 || nextNewline > endRange)
                            nextNewline = -1;
                    }

                    if (nextNewline != -1)
                    {
                        // 移到该换行符的后一格（即下一行开头）
                        int target = nextNewline + 1;
                        _cursorPosition = Math.Min(_text.Length, target);
                    }
                    else
                    {
                        // 没有换行符，按 cellsPerRow 向下移动
                        _cursorPosition = Math.Min(_text.Length, _cursorPosition + cellsPerRowLocal);
                    }

                    UpdateCaretPosition();
                }
                break;
            case Keys.Home:
                // 移动到当前行首
                {
                    // 向前查找最近的换行符或文本开头
                    int pos = _cursorPosition - 1;
                    while (pos >= 0 && _text[pos] != '\n')
                    {
                        pos--;
                    }
                    _cursorPosition = pos + 1;
                }
                break;
            case Keys.End:
                // 移动到当前行尾或文本末尾
                {
                    // 向后查找最近的换行符或文本结尾
                    int pos = _cursorPosition;
                    while (pos < _text.Length && _text[pos] != '\n')
                    {
                        pos++;
                    }
                    _cursorPosition = pos;
                }
                break;
            case Keys.Back:
                if (_cursorPosition > 0)
                {
                    _text = _text.Remove(_cursorPosition - 1, 1);
                    _cursorPosition--;
                }
                break;
            case Keys.Delete:
                if (_cursorPosition < _text.Length)
                {
                    _text = _text.Remove(_cursorPosition, 1);
                }
                break;
            case Keys.Enter:
                // 处理Enter键，支持长按持续输入
                _text = _text.Insert(_cursorPosition, "\n");
                _cursorPosition++;
                break;
        }

        // 重置光标可见性
        _cursorVisible = true;
        
        // 新增：文本可能已变化，更新统计信息
        UpdatePageInfo();

        this.Invalidate();
        UpdateInputMethodPosition();

        e.Handled = true;
    }

    // 新增：将方向键（以及需要的导航键）视为输入键，防止被父容器当作焦点导航键拦截。
    protected override bool IsInputKey(Keys keyData)
    {
        Keys key = keyData & Keys.KeyCode;
        if (key == Keys.Up || key == Keys.Down || key == Keys.Left || key == Keys.Right
            || key == Keys.Home || key == Keys.End || key == Keys.PageUp || key == Keys.PageDown)
        {
            return true;
        }
        return base.IsInputKey(keyData);
    }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        base.OnKeyPress(e);

        // 处理可打印字符
        if (!char.IsControl(e.KeyChar))
        {
            _text = _text.Insert(_cursorPosition, e.KeyChar.ToString());
            _cursorPosition++;
            
            this.Invalidate();
            UpdateInputMethodPosition();

            // 新增：文本已变化，刷新统计
            UpdatePageInfo();
        }

        e.Handled = true;
    }

    // 获取网格单元格大小
    private Size GetCellSize()
    {
        try
        {
            using (Image bgImage = Image.FromFile("cell.png"))
            {
                return new Size(bgImage.Width, bgImage.Height);
            }
        }
        catch
        {
            // 如果无法加载图像，返回默认大小
            return new Size(30, 30);
        }
    }

    // 将光标位置转换为网格坐标
    private Point GetGridPositionFromCursor()
    {
        Size cellSize = GetCellSize();
        int cellsPerRow = this.Width / cellSize.Width;
        
        // 计算当前页的起始位置
        int pageStart = 0;
        
        // 确保光标位置相对于当前页
        int relativeCursorPos = Math.Max(0, _cursorPosition - pageStart);
        
        // 计算实际字符位置（跳过换行符）
        int actualPosition = 0;
        int displayRow = 0;
        int displayCol = 0;
        
        // 只计算当前页内的字符
        int startIndex = Math.Max(0, pageStart);
        int endIndex = Math.Min(_cursorPosition, pageStart + _charsPerPage);
        
        for (int i = startIndex; i < endIndex; i++)
        {
            if (i >= _text.Length) break;
            
            if (_text[i] == '\n')
            {
                // 遇到换行符，行数加1，列数重置
                displayRow++;
                displayCol = 0;
            }
            else
            {
                // 正常字符，计算网格位置
                actualPosition++;
                displayCol++;
                if (displayCol >= cellsPerRow)
                {
                    displayRow++;
                    displayCol = 0;
                }
            }
        }
        
        return new Point(displayCol, displayRow);
    }

    // 获取光标Y坐标位置
    private Point GetCursorPosition()
    {
        Size cellSize = GetCellSize();
        Point gridPos = GetGridPositionFromCursor();
        // 文本绘制在单元格内偏下的位置，光标也应该跟随
        return new Point(gridPos.X * cellSize.Width + 5, gridPos.Y * cellSize.Height + 20 + 5);
    }

    // 根据点击位置计算光标位置
    private int GetCursorPositionFromPoint(Point point)
    {
        Size cellSize = GetCellSize();
        int cellsPerRow = this.Width / cellSize.Width;
        
        // 计算点击位置对应的网格坐标
        int targetCol = (point.X - 5) / cellSize.Width;
        int targetRow = (point.Y - 5) / cellSize.Height;
        
        // 计算当前页的起始位置
        int pageStart = 0;
        
        // 遍历当前页内的文本，找到对应的光标位置
        int currentRow = 0;
        int currentCol = 0;
        int charCountOnPage = 0;
        
        for (int i = pageStart; i < _text.Length; i++)
        {
            // 如果已达到页面字符上限，停止搜索
            if (charCountOnPage >= _charsPerPage)
                break;
            
            if (_text[i] == '\n')
            {
                // 遇到换行符，行数加1，列数重置
                currentRow++;
                currentCol = 0;
                charCountOnPage++;
            }
            else
            {
                // 如果到达目标位置，返回当前索引
                if (currentRow == targetRow && currentCol == targetCol)
                {
                    return i;
                }
                
                // 正常字符，移动到下一列
                currentCol++;
                charCountOnPage++;
                if (currentCol >= cellsPerRow)
                {
                    currentRow++;
                    currentCol = 0;
                }
            }
        }
        
        // 如果未找到精确位置，返回当前页的末尾位置
        return Math.Min(pageStart + _charsPerPage, _text.Length);
    }

    // 获取文本高度（用于光标）
    private int GetTextHeight(Graphics g)
    {
        return (int)g.MeasureString("A", _font).Height;
    }

    // 创建系统原生光标
    private void CreateNativeCaret()
    {
        try
        {
            int caretHeight = GetTextHeight(this.CreateGraphics());
            CreateCaret(this.Handle, IntPtr.Zero, 1, caretHeight);
            ShowCaret(this.Handle);
            UpdateCaretPosition();
            // 同时更新输入法位置
            UpdateInputMethodPosition();
        }
        catch (Exception ex)
        {
            // 系统光标创建失败，使用自绘光标
            System.Diagnostics.Debug.WriteLine("Native caret creation failed: " + ex.Message);
        }
    }

    // 更新系统光标位置
    private void UpdateCaretPosition()
    {
        try
        {
            using (Graphics g = this.CreateGraphics())
            {
                var cursorPos = GetCursorPosition();
                // 直接使用相对于控件的坐标
                SetCaretPos(cursorPos.X, cursorPos.Y);
            }
        }
        catch
        {
            // 忽略光标位置更新错误
        }
    }

    // 更新输入法位置
    private void UpdateInputMethodPosition()
    {
        try
        {
            // 确保控件已创建句柄
            if (!this.IsHandleCreated) return;

            using (Graphics g = this.CreateGraphics())
            {
                // 获取光标在控件内的位置（基于网格）
                Size cellSize = GetCellSize();
                Point gridPos = GetGridPositionFromCursor();
                
                // 直接基于网格位置计算光标位置，与OnPaint和UpdateCaretPosition保持一致
                int cursorX = gridPos.X * cellSize.Width;
                int cursorY = gridPos.Y * cellSize.Height - 60; // 与GetCursorYPosition中的20偏移保持一致
                
                // 设置输入法窗口的位置在光标右侧上方，更符合用户习惯
                Point cursorPos = new Point(cursorX, cursorY - 5);
                
                // 转换为屏幕坐标
                Point screenPos = this.PointToScreen(cursorPos);

                // 获取输入法上下文并设置组合窗口位置
                IntPtr hIMC = ImmGetContext(this.Handle);
                if (hIMC != IntPtr.Zero)
                {
                    COMPOSITIONFORM compForm = new COMPOSITIONFORM();
                    compForm.dwStyle = CFS_POINT; // 指定位置模式
                    compForm.ptCurrentPos = cursorPos; // 使用屏幕坐标

                    // 应用设置
                    ImmSetCompositionWindow(hIMC, ref compForm);
                    
                    // 释放上下文
                    ImmReleaseContext(this.Handle, hIMC);
                }
            }
        }
        catch (Exception ex)
        {
            // 记录错误但不抛出异常
            System.Diagnostics.Debug.WriteLine("更新输入法位置失败: " + ex.Message);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cursorTimer?.Stop();
            _cursorTimer?.Dispose();
            _font?.Dispose();
        }
        base.Dispose(disposing);
    }
}