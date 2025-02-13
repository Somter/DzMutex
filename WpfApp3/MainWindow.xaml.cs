using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp3
{
    public partial class MainWindow : Window
    {
        private static Semaphore semaphore = new Semaphore(3, 3, "GlobalAppSemaphore");
        public SynchronizationContext uiContext;
        Random random = new Random();
        Mutex mutex = new Mutex();

        public MainWindow()
        {
            if (!semaphore.WaitOne(0))
            {
                MessageBox.Show("Программа не может запустить более 3 потоков.");   
                Close();
                return;
            }

            InitializeComponent();
            uiContext = SynchronizationContext.Current;
            this.Closed += (s, e) => semaphore.Release();
        }

        void ThreadFunction1()
        {
            int[] numbers = new int[100];

            mutex.WaitOne();
            for (int i = 0; i < numbers.Length; i++)
            {
                numbers[i] = random.Next(i, 100);
            }

            File.WriteAllLines("number.txt", numbers.Select(n => n.ToString()));
            Thread.Sleep(1000);
            uiContext.Send(d => txtStatus1.Text = "Поток 1: Поток записал числа в файл", null);
            mutex.ReleaseMutex();

            ThreadFunction2();
        }

        void ThreadFunction2()
        {
            try
            {
                mutex.WaitOne();
                Thread.Sleep(1000);
                uiContext.Send(d => txtStatus2.Text = "Поток 2: Мьютекс свободен! Начинаем поиск простых чисел из первого файла.", null);

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

                ThreadFunction3();
            }
            catch (Exception ex)
            {
                uiContext.Send(d => txtStatus2.Text = $"Поток 2: Ошибка при обработке: {ex.Message}", null);
            }
        }

        void ThreadFunction3()
        {
            try
            {
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
            catch (Exception ex)
            {
                uiContext.Send(d => txtStatus3.Text = $"Поток 3: Ошибка при обработке: {ex.Message}", null);
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
                Task.Factory.StartNew(ThreadFunction1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
