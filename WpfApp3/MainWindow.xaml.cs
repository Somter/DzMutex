using System.IO;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Task[] arraytasks = new Task[3];
        public SynchronizationContext uiContext;
        Random random = new Random();   
        public MainWindow()
        {
            InitializeComponent();
            uiContext = SynchronizationContext.Current; 
        }

        void ThreadFunction1() 
        { 
            int[] numbers = new int[100];
            bool CreatedNew;    

            Mutex mutex = new Mutex(false, "DB744E26-72C1-4F2A-8BF8-5C31980953C7", out CreatedNew);
            mutex.WaitOne();
            for (int  i = 0; i < numbers.Length; i++) 
            {
                numbers[i] = random.Next(i, 100);   
            }

            File.WriteAllLines("number.txt", numbers.Select(n => n.ToString()));
            uiContext.Send(d => txtStatus1.Text = "Поток 1: Поток записал числа в файл", null);
            Thread.Sleep(1000);
            mutex.ReleaseMutex();   
        }

        void ThreadFunction2(Task t)
        {
            try
            {
                if (t.Exception == null)
                {
                    bool CreatedNew;
                    Mutex mutex = new Mutex(false, "DB744E26-72C1-4F2A-8BF8-5C31980953C7", out CreatedNew);
                    mutex.WaitOne();
                    uiContext.Send(d => txtStatus2.Text = "Поток 2: Мьютекс свободен! Начинаем поиск простых чисел из первого файла.", null);
                    Thread.Sleep(1000);


                    if (!File.Exists("number.txt"))
                    {
                        uiContext.Send(d => txtStatus2.Text = "Поток 2: Файл с числами не найден!", null);
                        mutex.ReleaseMutex();
                        return;
                    }

                    string[] lines = File.ReadAllLines("number.txt");
                    int[] numbers = lines.Select(line => int.Parse(line)).ToArray();

                    var primeNumbers = numbers.Where(IsPrime).ToArray();

                 
                    File.WriteAllLines("primeNumbers.txt", primeNumbers.Select(n => n.ToString()));

                    Thread.Sleep(1000);
                    uiContext.Send(d => txtStatus2.Text = "Поток 2: Файл с простыми числами создан.", null);

                    mutex.ReleaseMutex();
                }
                else
                {
                    // Обработка исключений из первого потока
                    uiContext.Send(d => txtStatus2.Text = "Поток 2: Ошибка в первом потоке!", null);
                }
            }
            catch (Exception ex)
            {
                uiContext.Send(d => txtStatus2.Text = $"Поток 2: Ошибка при обработке: {ex.Message}", null);
            }
        }


        void ThreadFunction3(Task t)
        {
            try
            {
                if (t.Exception == null)
                {
                    bool CreatedNew;
                    Mutex mutex = new Mutex(false, "DB744E26-72C1-4F2A-8BF8-5C31980953C7", out CreatedNew);
                    mutex.WaitOne();
                    uiContext.Send(d => txtStatus3.Text = "Поток 3: Мьютекс свободен! Начинаем поиск простых чисел с последней цифрой 7.", null);
                    Thread.Sleep(1000);

                    if (!File.Exists("primeNumbers.txt"))
                    {
                        uiContext.Send(d => txtStatus3.Text = "Поток 3: Файл с простыми числами не найден!", null);
                        mutex.ReleaseMutex();
                        return;
                    }

                    string[] lines = File.ReadAllLines("primeNumbers.txt");
                    int[] primeNumbers = lines.Select(line => int.Parse(line)).ToArray();

                    var numbersEndingIn7 = primeNumbers.Where(n => n % 10 == 7).ToArray();

                    File.WriteAllLines("primeNumbersEndingIn7.txt", numbersEndingIn7.Select(n => n.ToString()));

                    Thread.Sleep(1000);
                    uiContext.Send(d => txtStatus3.Text = "Поток 3: Файл с простыми числами, заканчивающимися на 7, создан.", null);

                    mutex.ReleaseMutex();
                }
                else
                {
                    uiContext.Send(d => txtStatus3.Text = "Поток 3: Ошибка в предыдущем потоке!", null);
                }
            }
            catch (Exception ex)
            {
                uiContext.Send(d => txtStatus2.Text = $"Поток 3: Ошибка при обработке: {ex.Message}", null);
            }
        }

        private bool IsPrime(int number)
        {
            if (number < 2)
                return false; 

            for (int i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0) 
                    return false;
            }

            return true; 
        }


        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                arraytasks[0] = Task.Factory.StartNew(ThreadFunction1);
                arraytasks[1] = arraytasks[0].ContinueWith(ThreadFunction2);
                arraytasks[2] = arraytasks[1].ContinueWith(ThreadFunction3); 

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}