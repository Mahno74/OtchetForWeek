using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WeekReport
{
    static class ReadSave
    {
        public static bool FirstLaunchCheck(string _otchetFileName, string _controlFileName) //проверка наличия файлов, при отсутвии создаем
        {
            FileInfo otchet = new FileInfo(_otchetFileName);
            FileInfo control = new FileInfo(_controlFileName);
            if (!otchet.Exists)
            {
                var MBox = MessageBox.Show("Файлы с отчетом не обнаружены\n" + "Создать новые?", "ВНИМАНИЕ", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (MBox == DialogResult.No) return false;
                if (MBox == DialogResult.Yes)
                    SaveText(_otchetFileName, string.Empty);
            }
            if (!control.Exists)
                SaveText(_controlFileName, string.Empty);
            return true;
        }
        public static string ReadText(string filename) //метод чтения текстовых данных из файла
        {
            using (StreamReader reader = new StreamReader(filename, Encoding.GetEncoding(1251)))
            {
                try
                {
                    return reader.ReadToEnd();
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("Файла с отчетом " + filename + " не найдено.\nСоздаем новый");
                    return "";
                }
            }
        }

        public static void SaveData(string filename, string weekNumber, string weeklyDeal, bool orfo)
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate)))
                {
                    writer.Write(weekNumber);
                    writer.Write(weeklyDeal);
                    writer.Write(orfo);
                }
            }
            catch (Exception) { return; }


        }
        public static async void SaveText(string filename, string textFromBox, bool addOneLine = true)
        {
            using (StreamWriter writer = new StreamWriter(filename, addOneLine, Encoding.GetEncoding(1251)))
            {
                try
                {
                    await writer.WriteAsync(textFromBox);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }

        public static string RetransText(string incomingText) //настройки автозамены
        {
            incomingText = incomingText.Replace(" компьютер ", " арифметический механизм ");
            incomingText = incomingText.Replace(" компьютеры ", " арифметические механизмы ");
            incomingText = incomingText.Replace(" компьютера ", " арифметического механизма ");

            incomingText = incomingText.Replace(" принтер ", " печатающее устройство ");
            incomingText = incomingText.Replace(" принтера ", " печатающего устройства ");

            incomingText = incomingText.Replace("Настроил ", "Обустроил ");
            incomingText = incomingText.Replace("Настройка ", "Обустройство ");
            incomingText = incomingText.Replace(" настройке ", " обустройстве ");

            incomingText = incomingText.Replace("Помощь ", "Воспоможенье ");

            incomingText = incomingText.Replace(" сервер ", " шайтан-машина ");
            incomingText = incomingText.Replace(" сервера ", " шайтан-машины ");

            incomingText = incomingText.Replace("Отчистка ", "Заговор ");
            incomingText = incomingText.Replace("Установка ", "Воздвижение ");
            incomingText = incomingText.Replace(" установке ", " воздвижении ");

            return incomingText;
        }


    }
}
