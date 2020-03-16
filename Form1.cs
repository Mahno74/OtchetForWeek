using System;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using WeekReport.Properties;


namespace WeekReport
{
    public partial class Form1 : Form
    {
        int weekNumberStore = Settings.Default.weekNow; //номер недели последнего запуска программы
        public int weekNumberNow = new GregorianCalendar().GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Thursday); //номер текущей недели

        private readonly string _otchetFileName = System.Environment.MachineName + "_otchet.txt"; //задаем индивидуальное имя производное от имени машины для файла отчета
        private readonly string _controlFileName = System.Environment.MachineName + "_control.txt"; //задаем индивидуальное имя производное от имени машины для файла контроля
        //private readonly string _dataFileName = System.Environment.MachineName + " data.bin"; //задаем индивидуальное имя производное от имени машины для файла данных
        public Form1()
        {
            InitializeComponent();
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(System.Globalization.CultureInfo.CreateSpecificCulture("Ru")); // переключаемся на русский
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S; //задали горячую клавишу для сохранения CTRL-S
            if (!ReadSave.FirstLaunchCheck(_otchetFileName, _controlFileName)) Environment.Exit(0); //проверка на первыз запуск программы и отсутвие файлов с отчетами
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            StringBuilder headerText = new StringBuilder(); //делаем шапку
            headerText.Append(Environment.MachineName + " ");
            headerText.Append("Сегодня " + DateTime.Now.ToLongDateString()+ " ");
            headerText.Append(weekNumberNow + @"-я неделя");
            this.Text = headerText.ToString();

            otchetTextBox.Text = ReadSave.ReadText(_otchetFileName); //читаем файл отчета
            controlTextBox.Text = ReadSave.ReadText(_controlFileName); // читаем файл на контроле
            ReadData(); //Читаем данные настроек
        }
        private void ReadData()
        {
            weeklyDealTextBox.Text = Settings.Default.weeklyDeal; //повторяющиеся дела
            ToolStripMenuItemOrphoCheck.Checked = Settings.Default.orfoCheck; //проверка орфографии
            weekNumberStore = Settings.Default.weekNow; //номер недели последнего запуска программы
            if (weekNumberStore != weekNumberNow) Reset(); //если номер текущей недели не совпадает с последней запомненной то делаем переход на новую неделю 
        }
        private void Reset() //переход на новую неделю
        {
            string firstLine = Environment.NewLine + Environment.NewLine + weekNumberNow + @"-я неделя";
            otchetTextBox.Text += firstLine;
            ReadSave.SaveText(_otchetFileName, firstLine);
            if (weeklyDealTextBox.Text != String.Empty)  //Если есть повторяемые задачи то выводим их в бокс
            {
                otchetTextBox.Text += "\r\n" + weeklyDealTextBox.Text;
                ReadSave.SaveText(_otchetFileName, "\r\n" + weeklyDealTextBox.Text);
            }
            RollDown(); //Перемотка в конец
        }
        private void KeyPressInTextBox(object sender, KeyEventArgs e) //нажатие кнопок и реакция на них в приложении
        {
            if (e.KeyCode == Keys.Enter)  //нажатие Enter в строке ввода
            {
                string newDeal = inputTextBox.Text;
                newDeal = newDeal.Substring(0, 1).ToUpper() + (newDeal.Length > 1 ? newDeal.Substring(1) : ""); //делаем первую букву заглавной

                if (ToolStripMenuItemOrphoCheck.Checked) //если стоит галочка провеки орфографии то проверяем
                    newDeal = Environment.NewLine + @"- " + Orfo(newDeal) + @";";
                //otchetTextBox.Text += Environment.NewLine+ "- " + ReadSave.retransText(s) + ";"; //текст с перевдом
                else
                    newDeal = Environment.NewLine + @"- " + newDeal + @";";
                otchetTextBox.Text += newDeal;
                RollDown();
                ReadSave.SaveText(_otchetFileName, newDeal); //Сохраняем строку в файл отчета
            }
            if (e.KeyCode == Keys.Escape) //по нажатию Escape выходим из приложения
                Application.Exit();
        }
        private void Button_OnControl_Click(object sender, EventArgs e) //Нажатие кнопки взять на котроль
        {
            string newDeal = inputTextBox.Text;
            if (ToolStripMenuItemOrphoCheck.Checked) //если стоит галочка провеки орфографии то проверяем
            {
                newDeal = Environment.NewLine + DateTime.Now.ToShortDateString() + @" " + Orfo(newDeal) + @";";
            }
            else
            {
                newDeal = Environment.NewLine + DateTime.Now.ToShortDateString() + @" " + newDeal + @";";
            }
            controlTextBox.Text += newDeal;
            RollDown();
            ReadSave.SaveText(_controlFileName, newDeal); //передаем имя файла и текст из 5 бокса
        }

        private void MenuSave(object sender, EventArgs e)
        {
            ReadSave.SaveText(_controlFileName, controlTextBox.Text, false); //передаем имя файла и текст из 5 бокса
            ReadSave.SaveText(_otchetFileName, otchetTextBox.Text, false); //передаем имя файла и текст из 2 бокса
            MessageBox.Show(@"Сохранено успешно", @"Сохранение");
        }
        private void RollDown() //Перемотка в конец
        {
            otchetTextBox.SelectionStart = otchetTextBox.Text.Length; otchetTextBox.ScrollToCaret();
            controlTextBox.SelectionStart = controlTextBox.Text.Length; controlTextBox.ScrollToCaret();
            inputTextBox.Clear(); inputTextBox.Select();
        }

        private string Orfo(string inputText) //проверка орфографии
        {
            var w = new Microsoft.Office.Interop.Word.Application { Visible = false };
            w.Documents.Add();
            w.Selection.Text = inputText;
            w.ActiveDocument.CheckSpelling();
            string chekSpellOk = w.Selection.Text;
            w.ActiveDocument.Close(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges);
            w.Quit();
            return chekSpellOk;
        }
        //Сохраняем настройки при выходе
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.weeklyDeal = weeklyDealTextBox.Text;
            Settings.Default.orfoCheck= ToolStripMenuItemOrphoCheck.Checked;
            Settings.Default.weekNow = weekNumberNow;
            Settings.Default.Save();
        }
        private void Form1_Shown(object sender, EventArgs e) => RollDown(); //Перемотка в конец при запуске
        private void NewWeekMenuItem_Click(object sender, EventArgs e) => Reset(); //Начать новую неделю из меню
        public void AboutMenuItem_Click(object sender, EventArgs e) => MessageBox.Show("Программа для ведения списка дел\n2665603@gmail.com\nВ.В.Юрасов\n2020 г.", "О программе");

    }
}
