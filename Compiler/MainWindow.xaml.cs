using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using Compiler.Models.Lexical;
using Compiler.Models.Parser;
using Compiler.Models;
using Compiler.Models.MRegex;

namespace Compiler;
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

    public void Execute(object parameter) => _execute(parameter);
}

public partial class MainWindow : Window
{
    private const string _aboutPath = @"HtmlSrc\About.html";
    private const string _grammarClassificationPath = @"HtmlSrc\GrammarClassification.html";
    private const string _grammarPath = @"HtmlSrc\Grammar.html";
    private const string _methodOfAnalysisPath = @"HtmlSrc\MethodOfAnalysis.html";
    private const string _neutralizingErrorsPath = @"HtmlSrc\NeutralizingErrors.html";
    private const string _problemStatementPath = @"HtmlSrc\ProblemStatement.html";
    private const string _TestCasePath = @"HtmlSrc\TestCase.html";
    private const string _literaturePath = @"HtmlSrc\ListOfLiterature.html";
    private const string _sourceCode = @"https://github.com/matteoxpo/AbstractCompiler";

    private const string _documentationPath = @"HtmlSrc\Documentation.html";


    public ObservableCollection<Lexeme> Lexemes { get; set; } = new();
    public ObservableCollection<ParsedError> WrongLexemes { get; set; } = new();
    public ObservableCollection<RegexCorrectValue> RegexCorrectValues { get; set; } = new();

    private ParsedError _selectedError;
    public ParsedError SelectedError
    {
        get => _selectedError;
        set
        {
            _selectedError = value;
            if (_selectedError != null)
            {
                try
                {
                    textEditor.Select(value.StartIndex, value.Length);

                }
                catch (ArgumentOutOfRangeException ex) 
                {
                    textEditor.Select(textEditor.Text.Length - 1, 1);
                }
            }
        }
    }
    
    private Lexeme _selectedLexeme;
    public Lexeme SelectedLexeme
    {
        get => _selectedLexeme;
        set
        {
            _selectedLexeme = value;
            if (_selectedLexeme != null)
            {
                textEditor.Select(value.StartIndex, value.Length);
            }
        }
    }

    private RegexCorrectValue _selectedCorrectRegex;
    public RegexCorrectValue SelectedCorrectRegex
    {
        get => _selectedCorrectRegex;
        set
        {
            _selectedCorrectRegex = value;
            if (_selectedCorrectRegex != null)
            {
                textEditor.Select(value.StartIndex, value.Length);
            }
        }
    }
    
    // Файл
    public ICommand CreateButtonClick { get; }
    public ICommand OpenButtonClick { get; }
    public ICommand SaveButtonClick { get; }
    public ICommand SaveAsButtonClick { get; }
    public ICommand ExitButtonClick { get; }

    // Правка
    public ICommand UndoButtonClick { get; }
    public ICommand RepeatButtonClick { get; }
    public ICommand CutButtonClick { get; }
    public ICommand CopyButtonClick { get; }
    public ICommand InsertButtonClick { get; }
    public ICommand DeleteButtonClick { get; }
    public ICommand SelectAllButtonClick { get; }

    // Текст
    public ICommand TaskButtonClick { get; }
    public ICommand GrammaButtonClick { get; }
    public ICommand GrammaClassificationButtonClick { get; }
    public ICommand AnalyzMethodButtonClick { get; }
    public ICommand DiagnosticButtonClick { get; }
    public ICommand TestExampleButtonClick { get; }
    public ICommand LiteratureButtonClick { get; }
    public ICommand SourceCodeButtonClick { get; }

    // Пуск
    public ICommand StartButtonClick { get; }

    // Справка
    public ICommand HelpButtonClick { get; }
    public ICommand AboutButtonClick { get; }


    private string? _filePath = null;

    public MainWindow()
    {

        InitializeComponent();
        DataContext = this;
        Closing += MainWindow_Closing;
        DragEnter += new DragEventHandler(MainWindow_DragEnter);
        Drop += new DragEventHandler(MainWindow_Drop);

        lexerDataGrid.ItemsSource = Lexemes;
        parserDataGrid.ItemsSource = WrongLexemes;

        // Файл
        CreateButtonClick = new RelayCommand(CreateFile);
        OpenButtonClick = new RelayCommand(OpenFile);
        SaveButtonClick = new RelayCommand(SaveFile);
        SaveAsButtonClick = new RelayCommand(SaveFileAs);
        ExitButtonClick = new RelayCommand(ExitApplication);

        // Правка
        UndoButtonClick = new RelayCommand(Undo);
        RepeatButtonClick = new RelayCommand(Repeat);
        CutButtonClick = new RelayCommand(Cut);
        CopyButtonClick = new RelayCommand(Copy);
        InsertButtonClick = new RelayCommand(Insert);
        DeleteButtonClick = new RelayCommand(Delete);
        SelectAllButtonClick = new RelayCommand(SelectAll);

        // Текст
        TaskButtonClick = new RelayCommand(ViewTask);
        GrammaButtonClick = new RelayCommand(ViewGramma);
        GrammaClassificationButtonClick = new RelayCommand(ViewGrammaClassification);
        AnalyzMethodButtonClick = new RelayCommand(ViewAnalyzMethod);
        DiagnosticButtonClick = new RelayCommand(ViewDiagnostic);
        TestExampleButtonClick = new RelayCommand(ViewTestExample);
        LiteratureButtonClick = new RelayCommand(ViewLiterature);
        SourceCodeButtonClick = new RelayCommand(ViewSourceCode);

        // Пуск
        StartButtonClick = new RelayCommand(Start);

        // Справка
        HelpButtonClick = new RelayCommand(ViewHelp);
        AboutButtonClick = new RelayCommand(ViewAbout);
    }

    // Файл
    public void CreateFile(object parameter)
    {
        string text = textEditor.Text;

        if (!string.IsNullOrEmpty(_filePath))
        {
            MessageBoxResult result = MessageBox.Show("У вас уже открыт файл. Хотите создать новый файл на основе содержимого открытого файла?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.WriteAllText(_filePath, text);
                    MessageBox.Show("Новый файл успешно создан на основе содержимого открытого файла.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        else
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _filePath = saveFileDialog.FileName;
                    File.WriteAllText(_filePath, text);
                    MessageBox.Show("Файл успешно создан.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public void OpenFile(object parameter)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                _filePath = openFileDialog.FileName;
                string text = File.ReadAllText(_filePath);
                textEditor.Text = text;
                MessageBox.Show($"Файл {_filePath} успешно открыт.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    public void SaveFile(object parameter)
    {
        if (string.IsNullOrEmpty(_filePath))
        {
            SaveFileAs(parameter);
            return;
        }

        string text = textEditor.Text;

        try
        {
            File.WriteAllText(_filePath, text);
            MessageBox.Show("Файл успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (IOException ex)
        {
            MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public void SaveFileAs(object parameter)
    {
        string text = textEditor.Text;

        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                _filePath = saveFileDialog.FileName;
                File.WriteAllText(_filePath, text);
                MessageBox.Show("Файл успешно сохранен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    public void ExitApplication(object parameter)
    {
        if (HasUnsavedChanges())
        {
            MessageBoxResult result = MessageBox.Show("Есть несохраненные изменения. Хотите сохранить перед выходом?", "Подтверждение выхода", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveFile(parameter);
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    return;
            }
        }

        Application.Current.Shutdown();
    }

    private bool HasUnsavedChanges()
    {
        return string.IsNullOrEmpty(_filePath) || File.ReadAllText(_filePath) != textEditor.Text;
    }
    public void Undo(object parameter)
    {
        if (textEditor.CanUndo)
        {
            textEditor.Undo();
        }
        else
        {
            MessageBox.Show("Невозможно выполнить отмену действия.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Правка
    public void Repeat(object parameter) => textEditor.Redo();
    public void Cut(object parameter) => textEditor.Cut();
    public void Copy(object parameter) => textEditor.Copy();
    public void Insert(object parameter) => textEditor.Paste();
    public void Delete(object parameter) => textEditor.SelectedText = String.Empty;
    public void SelectAll(object parameter) => textEditor.SelectAll();


    // Текст
    public void ViewTask(object parameter) => Open(_problemStatementPath);
    public void ViewGramma(object parameter) => Open(_grammarPath);
    public void ViewGrammaClassification(object parameter) => Open(_grammarClassificationPath);
    public void ViewAnalyzMethod(object parameter) => Open(_methodOfAnalysisPath);
    public void ViewDiagnostic(object parameter) => Open(_neutralizingErrorsPath);
    public void ViewTestExample(object parameter) => Open(_TestCasePath);
    public void ViewLiterature(object parameter) => Open(_literaturePath);
    public void ViewSourceCode(object parameter) => HtmlService.Open(_sourceCode);



    // Пуск
    public void Start(object parameter)
    {
        LexicalAnalysis();
        LexiaclParse();
        RegexParser();
    }


    // Справка
    public void ViewHelp(object parameter) => Open(_documentationPath);

    public void ViewAbout(object parameter) => Open(_aboutPath);

    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        MessageBoxResult result = MessageBox.Show("Хотите сохранить изменения перед закрытием?", "Подтверждение", MessageBoxButton.YesNoCancel);

        switch (result)
        {
            case MessageBoxResult.Yes:
                SaveFile(_filePath);
                break;
            case MessageBoxResult.No:
                break;
            case MessageBoxResult.Cancel:
                e.Cancel = true;
                break;
        }
    }
    private void MainWindow_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void MainWindow_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                textEditor.Text += LoadFileContent(file);
            }
        }
    }
    private string LoadFileContent(string filePath) => File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem != null)
        {
            if (comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string fontSizeString = selectedItem.Content.ToString();
                if (int.TryParse(fontSizeString, out int fontSize))
                {
                    if (textEditor != null)
                    {
                        textEditor.FontSize = fontSize;
                        textEditor.InvalidateVisual(); // Вызываем переотрисовку textEditor
                    }
                }
            }
        }
    }
    private void Open(string path)
    {
        try
        {
            HtmlService.Open(GetFullPath(path));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при открытии url: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string GetFullPath(string path) => System.IO.Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, path);

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            if (_filePath == null)
            {
                CreateFile(null);
            }
            else
            {
                SaveFile(null);
            }
            e.Handled = true;
        }
    }
    private void LexicalAnalysis()
    {
        Lexemes.Clear();
        foreach (var lexeme in LexicalAnalyzer.Analyze(textEditor.Text))
        {
            Lexemes.Add(lexeme);
        }
    }
    private void LexiaclParse()
    {
        WrongLexemes.Clear();
        foreach (var error in Parser.Parse(Lexemes.ToList()))
        {
            WrongLexemes.Add(error);
        }
    }
    private void RegexParser() 
    {
        RegexCorrectValues.Clear();
        foreach (var regCorrectValue in Models.MRegex.RegexParser.Parse(textEditor.Text)) 
        {
            RegexCorrectValues.Add(regCorrectValue);
        }
    }

}
