using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System;
using System.Collections.ObjectModel;
using Compiler.Models.Lexical;
using Compiler.Models.Parser;

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
    public ObservableCollection<Lexeme> Lexemes { get; set; } = new ();
    public ObservableCollection<ParsedError> WrongLexemes { get; set; } = new();

    private ParsedError _selectedError;
    public ParsedError SelectedError
    {
        get => _selectedError;
        set
        {
            _selectedError = value;
            if (_selectedError != null)
            {
                var start = value.StartIndex;
                var len = value.EndIndex - value.StartIndex > 0 ? value.EndIndex - value.StartIndex : 0;
                textEditor.Select(start, len);
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
                textEditor.Select(value.StartIndex, value.EndIndex - value.StartIndex > 0 ? value.EndIndex - value.StartIndex : 0);
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
                _filePath = saveFileDialog.FileName; // Обновляем путь к файлу
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
        // Если есть несохраненные изменения, запрашиваем подтверждение пользователя
        if (HasUnsavedChanges())
        {
            MessageBoxResult result = MessageBox.Show("Есть несохраненные изменения. Хотите сохранить перед выходом?", "Подтверждение выхода", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveFile(parameter); // Сохраняем файл
                    break;
                case MessageBoxResult.No:
                    // Просто выходим
                    break;
                case MessageBoxResult.Cancel:
                    return; // Отменяем выход
            }
        }

        Application.Current.Shutdown();
    }

    private bool HasUnsavedChanges()
    {
        // Если файл не был открыт, либо содержимое файла отличается от содержимого текстового редактора,
        // значит, есть несохраненные изменения
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
    public void ViewTask(object parameter) { }
    public void ViewGramma(object parameter) { }
    public void ViewGrammaClassification(object parameter) { }
    public void ViewAnalyzMethod(object parameter) { }
    public void ViewDiagnostic(object parameter) { }
    public void ViewTestExample(object parameter) { }
    public void ViewLiterature(object parameter) { }
    public void ViewSourceCode(object parameter) { }

    // Пуск
    public void Start(object parameter) 
    {
        LexicalAnalysis();
        LexiaclParse();
    }
   

    // Справка
    public void ViewHelp(object parameter) 
    {
        try
        {
            var openBrowserProcess = new Process()
            {
                StartInfo = new ProcessStartInfo(GenerateHelpHtml())
                {
                    UseShellExecute = true
                }
            };
            openBrowserProcess.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при открытии файла справки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    public void ViewAbout(object parameter)
    {
        MessageBox.Show("" +
            "Compiler - текстовый редактор для языкового процессора\n" +
            "Название приложения: Compiler\n" +
            "Версия: 1.0\n" +
            "Автор: Хромин Сергей Константинович\n" +
            "Дата создания: 15.02.2024\n",
            "О приложении", MessageBoxButton.OK, MessageBoxImage.Information);
    }




    public string GenerateHelpHtml()
    {
        // Путь к исполняемому файлу приложения
        string executablePath = Assembly.GetExecutingAssembly().Location;

        // Путь к папке, в которой находится исполняемый файл
        string directoryPath = System.IO.Path.GetDirectoryName(executablePath);

        // Путь к HTML-файлу справки
        string helpFilePath = System.IO.Path.Combine(directoryPath, "documentation.html");

        if (!File.Exists(helpFilePath))
        {
            // Содержимое HTML-файла справки
            string htmlContent = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Справка по приложению</title>
</head>
<body>
    <h1>Справка по приложению</h1>
    <h2>Меню ""Файл""</h2>
    <ul>
        <li><strong>Создать:</strong> Создает новый документ.</li>
        <li><strong>Открыть:</strong> Открывает существующий документ.</li>
        <li><strong>Сохранить:</strong> Сохраняет текущий документ.</li>
        <li><strong>Сохранить как:</strong> Сохраняет текущий документ с новым именем.</li>
        <li><strong>Выход:</strong> Завершает работу приложения.</li>
    </ul>
    <h2>Меню ""Правка""</h2>
    <ul>
        <li><strong>Отменить:</strong> Отменяет последнее действие.</li>
        <li><strong>Повторить:</strong> Повторяет последнее отмененное действие.</li>
        <li><strong>Вырезать:</strong> Вырезает выбранный текст.</li>
        <li><strong>Копировать:</strong> Копирует выбранный текст.</li>
        <li><strong>Вставить:</strong> Вставляет скопированный или вырезанный текст.</li>
        <li><strong>Удалить:</strong> Удаляет выбранный текст.</li>
        <li><strong>Выделить все:</strong> Выделяет весь текст в редакторе.</li>
    </ul>
    <h2>Меню ""Текст""</h2>
    <p>В данном разделе будет содержаться информация о функциях из меню ""Текст"".</p>
</body>
</html>
";
        // Запись содержимого HTML-файла в файл
       
            File.WriteAllText(helpFilePath, htmlContent);
        }
        return helpFilePath;
    }


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
        foreach(var lexeme in LexicalAnalyzer.Analyze(textEditor.Text)) 
        { 
            Lexemes.Add(lexeme);
        }
    }
    private void LexiaclParse()
    {
        WrongLexemes.Clear();
        foreach(var error in Parser.Parse(Lexemes))
        {
            WrongLexemes.Add(error);
        }
    }

}
