using System;
using System.Windows.Forms;


namespace WeekReport
{
    public partial class Form1 : Form
    {

        private readonly string _otchetFileName = System.Environment.MachineName + " otchet.txt"; //задаем индивидуальное имя производное от имени машины для файла отчета
        private readonly string _controlFileName = System.Environment.MachineName + " control.txt"; //задаем индивидуальное имя производное от имени машины для файла контроля
        private readonly string _dataFileName = System.Environment.MachineName + " data.bin"; //задаем индивидуальное имя производное от имени машины для файла данных
        private const byte FirstDayOfWeekShift = 4; //смещение начала недели, 4 - первый день четверг

        public Form1()
        {
            InitializeComponent();
            saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S; //задали горячую клавишу для сохранения CTRL-S
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (!ReadSave.FirstLaunchCheck(_otchetFileName, _controlFileName)) Environment.Exit(0); //проверка на первыз запуск программы и отсутвие файлов с отчетами
            FormBorderStyle = FormBorderStyle.Fixed3D; //Запрет на изменение размера окна
            InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(System.Globalization.CultureInfo.CreateSpecificCulture("Ru")); // переключаемся на русский
            Text = Environment.MachineName; // задаем заголовок окна 

            dateNowStripStatus.Text = @"Сегодня " + DateTime.Now.ToLongDateString(); //выводим сегодняшнюю дату
            weekNowStripStatusLabel.Text = Convert.ToString(((DateTime.Now.DayOfYear + FirstDayOfWeekShift) / 7) + 1) + @"-я неделя"; // выводим номер недели (+firstDayOfWeekShift для начала недели со смещением)

            otchetTextBox.Text = ReadSave.ReadText(_otchetFileName); //читаем файл отчета
            controlTextBox.Text = ReadSave.ReadText(_controlFileName); // читаем файл на контроле
            ReadData(); //Читаем данные настроек

        }
        private void ReadData()
        {
            try
            {
                var readerData = new System.IO.BinaryReader(System.IO.File.OpenRead(_dataFileName));
                string weekNumber = readerData.ReadString();  //возвращаем номер недели
                string numberPp = readerData.ReadString();     //возвращаем номер дела
                string weeklyDeal = readerData.ReadString();   // возвращаем текст повторяемой задачи
                bool orfoChek = readerData.ReadBoolean();
                readerData.Close();
                weeklyDealTextBox.Text = weeklyDeal;
                countWeekDealNumericUpDown.Value = Convert.ToDecimal(numberPp);
                orfoCheckBox.Checked = orfoChek;
                if (weekNowStripStatusLabel.Text != Convert.ToString(weekNumber)) Reset(); //если номер текущей недели не совпадает с последней запомненной то делаем сброс 
            }
            catch (System.IO.FileNotFoundException)
            {
                ReadSave.SaveData(_dataFileName, weekNowStripStatusLabel.Text, Convert.ToString(countWeekDealNumericUpDown.Value), weeklyDealTextBox.Text, Convert.ToBoolean(orfoCheckBox.CheckState));
            }
        }

        private void Reset() //переход на новую неделю
        {
            string firstLine = Environment.NewLine + Environment.NewLine + weekNowStripStatusLabel.Text;
            otchetTextBox.Text += firstLine;
            ReadSave.SaveText(_otchetFileName, firstLine);
            if (weeklyDealTextBox.Text != String.Empty)  //Если есть повторяемые задачи то выводим их в бокс
            {
                otchetTextBox.Text += "\r\n" + weeklyDealTextBox.Text;
                ReadSave.SaveText(_otchetFileName, weeklyDealTextBox.Text);
            }
            countWeekDealNumericUpDown.Value = 0;
            RollDown(); //Перемотка в конец
        }


        private void KeyPressInTextBox1(object sender, KeyEventArgs e) //нажатие кнопок и реакция на них в приложении
        {
            if (e.KeyCode == Keys.Enter)  //нажатие Enter в строке ввода
            {
                string newDeal = inputTextBox.Text;
                newDeal = newDeal.Substring(0, 1).ToUpper() + (newDeal.Length > 1 ? newDeal.Substring(1) : ""); //делаем первую букву заглавноей

                if (orfoCheckBox.Checked) //если стоит галочка провеки орфографии то проверяем
                    newDeal = Environment.NewLine + @"- " + Orfo(newDeal) + @";";
                //otchetTextBox.Text += Environment.NewLine+ "- " + ReadSave.retransText(s) + ";"; //текст с перевдом
                else
                    newDeal = Environment.NewLine + @"- " + newDeal + @";";

                otchetTextBox.Text += newDeal;
                RollDown();
                countWeekDealNumericUpDown.Value++; //Увеличиваем счетчик дел на +1
                ReadSave.SaveText(_otchetFileName, newDeal); //Сохраняем строку в файл отчета
            }
            if (e.KeyCode == Keys.Escape) //по нажатию Escape выходим из приложения
                Application.Exit();
        }


        private void Button_OnControl_Click(object sender, EventArgs e) //Нажатие кнопки взять на котроль
        {
            string newDeal = inputTextBox.Text;
            if (orfoCheckBox.Checked) //если стоит галочка провеки орфографии то проверяем
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
            var w = new Microsoft.Office.Interop.Word.Application {Visible = false };
            w.Documents.Add();
            w.Selection.Text = inputText;
            w.ActiveDocument.CheckSpelling();
            string chekSpellOk = w.Selection.Text;
            w.ActiveDocument.Close(Microsoft.Office.Interop.Word.WdSaveOptions.wdDoNotSaveChanges);
            w.Quit();
            return chekSpellOk;
        }
        //Сохраняем настройки при выходе
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) => ReadSave.SaveData(_dataFileName, weekNowStripStatusLabel.Text, Convert.ToString(countWeekDealNumericUpDown.Value), weeklyDealTextBox.Text, Convert.ToBoolean(orfoCheckBox.CheckState));
        private void Form1_Shown(object sender, EventArgs e) => RollDown(); //Перемотка в конец при запуске
        private void NewWeekMenuItem_Click(object sender, EventArgs e) => Reset(); //Начать новую неделю из меню
        public void AboutMenuItem_Click(object sender, EventArgs e) => MessageBox.Show("Программа для ведения списка дел\n2665603@gmail.com\nВ.В.Юрасов\n2019 г.", "О программе");

    }
}
