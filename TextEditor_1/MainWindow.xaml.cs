using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;


namespace TextEditorWPF
{
    public partial class MainWindow : Window
    {
        private string currentFile = null;
        private bool isModified = false;

        public MainWindow()
        {
            InitializeComponent();
            UpdateWindowTitle();

            
            this.Closing += MainWindow_Closing;

            
            RegisterCommands();
        }

        private void RegisterCommands()
        {
            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, (s, e) => NewMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, (s, e) => OpenMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, (s, e) => SaveMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, (s, e) => UndoMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, (s, e) => RedoMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, (s, e) => CutMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (s, e) => CopyMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (s, e) => PasteMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, (s, e) => SelectAllMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Help, (s, e) => HelpMenuItem_Click(null, null)));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, (s, e) => ExitMenuItem_Click(null, null)));
        }

        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MaybeSave())
            {
                EditorTextBox.Clear();
                currentFile = null;
                isModified = false;
                UpdateWindowTitle();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Новый файл создан";

                if (FileInfoTextBox != null)
                    FileInfoTextBox.Text = "";
            } 
        }



        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (MaybeSave())
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Открыть файл",
                    Multiselect = false
                };

                if (ofd.ShowDialog() == true)
                {
                    try
                    {
                        string content = File.ReadAllText(ofd.FileName);
                        EditorTextBox.Text = content;

                        currentFile = ofd.FileName;
                        isModified = false;
                        UpdateWindowTitle();

                        if (StatusTextBlock != null)
                            StatusTextBlock.Text = $"Открыт: {Path.GetFileName(ofd.FileName)}";

                        if (FileInfoTextBox != null)
                            FileInfoTextBox.Text = $"Размер: {new FileInfo(ofd.FileName).Length} байт";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Не удалось открыть файл:\n{ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFile))
            {
                SaveFile(currentFile);
            }
            else
            {
                SaveFileAs();
            }
        }

        private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveFileAs();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        
        private void UndoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EditorTextBox != null && EditorTextBox.CanUndo)
                EditorTextBox.Undo();
        }

        private void RedoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EditorTextBox != null && EditorTextBox.CanRedo)
                EditorTextBox.Redo();
        }

        private void CutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EditorTextBox != null)
                EditorTextBox.Cut();
        }

        private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EditorTextBox != null)
                EditorTextBox.Copy();
        }

        private void PasteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EditorTextBox != null)
                EditorTextBox.Paste();
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EditorTextBox != null)
                EditorTextBox.SelectedText = "";
        }

        private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (EditorTextBox != null)
                EditorTextBox.SelectAll();
        }



        
        private void TextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            string itemName = menuItem?.Header.ToString() ?? "Информация";

            var dialog = new Window
            {
                Title = itemName,
                Width = 500,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                WindowStyle = WindowStyle.ToolWindow
            };

            var textBox = new TextBox
            {
                Text = $"Здесь будет отображаться информация:\n\n{itemName}\n\nРеализуется на следующем этапе разработки.",
                IsReadOnly = true,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 12,
                Padding = new Thickness(10),
                Margin = new Thickness(10),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            dialog.Content = textBox;
            dialog.ShowDialog();
        }


        
        private void RunAnalyzerMenuItem_Click(object sender, RoutedEventArgs e)
        {
            
            if (ResultAreaTextBox != null)
            {
                string text = EditorTextBox.Text;

                string result = "";
                result += "=== ЗАПУСК АНАЛИЗАТОРА ===\n\n";

                if (!string.IsNullOrWhiteSpace(text))
                {
                    string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    int charCount = text.Length;
                    int wordCount = text.Split(new char[] { ' ', '\n', '\r', '\t' },
                        StringSplitOptions.RemoveEmptyEntries).Length;

                    result += "Статистика текста:\n";
                    result += $"• Символов: {charCount}\n";
                    result += $"• Слов: {wordCount}\n";
                    result += $"• Строк: {lines.Length}\n\n";

                    result += "Результат анализа:\n";

                    if (text.Contains("error") || text.Contains("ошибка") ||
                        text.Contains("fail") || text.Contains("exception") ||
                        text.Contains("Error") || text.Contains("Ошибка"))
                    {
                        result += "Обнаружены потенциальные ошибки в тексте.";
                    }
                    else
                    {
                        result += "Синтаксических ошибок не обнаружено.";
                    }


                    ResultAreaTextBox.MouseDown += (s, args) =>
                    {
                        if (EditorTextBox != null)
                        {
                            EditorTextBox.Focus();
                            EditorTextBox.Select(0, 0);
                        }
                    };
                }
                else
                {
                    result += "Текст отсутствует. Нечего анализировать.";
                }

                ResultAreaTextBox.Text = result;

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = "Анализ завершён";
            }
        }

        
        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string helpText =
         "СПРАВКА ПО ПРОГРАММЕ\n\n" +

         "МЕНЮ \"ФАЙЛ\"\n" +
         "Создать (Ctrl+N) — новый документ\n" +
         "Открыть (Ctrl+O) — открыть существующий файл\n" +
         "Сохранить (Ctrl+S) — сохранить текущий документ\n" +
         "Сохранить как (Ctrl+Shift+S) — сохранить под новым именем\n" +
         "Выход (Alt+F4) — завершить работу\n\n" +

         "МЕНЮ \"ПРАВКА\"\n" +
         "Отменить (Ctrl+Z) — отмена действия\n" +
         "Повторить (Ctrl+Y) — возврат отмены\n" +
         "Вырезать / Копировать / Вставить — работа с буфером\n" +
         "Удалить (Del) — удалить выделенный текст\n" +
         "Выделить всё (Ctrl+A) — выделить весь текст\n\n" +

         "МЕНЮ \"ТЕКСТ\"\n" +
         "Содержит теоретические материалы проекта:\n" +
         "описание задачи, грамматики, метода анализа,\n" +
         "тестовый пример и информацию о программе\n\n" +

         "МЕНЮ \"ПУСК\" (F5)\n" +
         "Запускает синтаксический анализ текста\n" +
         "Результат выводится в нижней области окна\n\n" +

         "МЕНЮ \"СПРАВКА\"\n" +
         "Вызов справки (F1) — текущее окно\n" +
         "О программе — сведения о приложении\n\n" +

         "ОБЛАСТИ ОКНА\n" +
         "Верхняя область — редактирование текста\n" +
         "Нижняя область — отображение результатов\n\n" +

         "СТРОКА СОСТОЯНИЯ\n" +
         "Отображает статус, имя файла и позицию курсора\n" +
         "\"*\" рядом с именем файла означает\n" +
         "наличие несохранённых изменений.";

            MessageBox.Show(helpText, "Справка", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string aboutText =
        "Текстовый Редактор\n" +
        "Языковой Процессор\n" +
        "────────────────────────────────────────\n\n" +

        "Версия: 1.0.1\n\n" +

        "Автор: Комаров Дмитрий Павлович\n" +
        "Группа: АП-326\n\n" +

        "Дисциплина:\n" +
        "Теория формальных языков и компиляторов\n\n" +

        "Основные возможности:\n" +
        "• Текстовый редактор с базовыми функциями\n" +
        "• Работа с форматами TXT и RTF\n" +
        "• Имитация синтаксического анализа\n" +
        "• Современный графический интерфейс\n" +
        "• Отображение позиции курсора в строке состояния\n\n" +

        "Используемые технологии:\n" +
        "C#  |  WPF  |  .NET 8.0\n\n" +

        "2026";


            MessageBox.Show(aboutText, "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        
        private void EditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            isModified = true;
            UpdateWindowTitle();

            
            if (EditorTextBox != null)
            {
                string text = EditorTextBox.Text;
                int charCount = text.Length;
                int wordCount = 0;

                if (!string.IsNullOrWhiteSpace(text))
                {
                    wordCount = text.Split(new char[] { ' ', '\n', '\r', '\t' },
                        StringSplitOptions.RemoveEmptyEntries).Length;
                }

                if (FileInfoTextBox != null)
                    FileInfoTextBox.Text = $"Символов: {charCount} | Слов: {wordCount}";
            }
        }

        private void UpdateWindowTitle()
        {
            string title = "Текстовый редактор";
            if (!string.IsNullOrEmpty(currentFile))
                title += $" - {Path.GetFileName(currentFile)}";
            else
                title += " - [Новый файл]";

            if (isModified)
                title += " *";

            this.Title = title;
        }

        private bool MaybeSave()
        {
            if (isModified)
            {
                var result = MessageBox.Show(
                    "Документ был изменён. Сохранить изменения?",
                    "Сохранение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveMenuItem_Click(null, null);
                    return true;
                }
                else if (result == MessageBoxResult.No)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void SaveFile(string fileName)
        {
            try
            {
                string text = EditorTextBox.Text;
                File.WriteAllText(fileName, text);

                currentFile = fileName;
                isModified = false;
                UpdateWindowTitle();

                if (StatusTextBlock != null)
                    StatusTextBlock.Text = $"Сохранено: {Path.GetFileName(fileName)}";

                var fileInfo = new FileInfo(fileName);
                if (FileInfoTextBox != null)
                    FileInfoTextBox.Text = $"Сохранено: {fileInfo.Length} байт";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось сохранить файл:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveFileAs()
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                Title = "Сохранить файл как",
                DefaultExt = "txt",
                AddExtension = true
            };

            if (sfd.ShowDialog() == true)
            {
                SaveFile(sfd.FileName);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!MaybeSave())
            {
                e.Cancel = true;
            }
        }
    }
}