using System.Text.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;


namespace alarm_vs_preview61825at113pm
{
    public class User : INotifyPropertyChanged
    {
        private string message;
        private DateTime date;

        public string Message
        {
            get { return message; }
            set
            {
                if (message != value)
                {
                    message = value;
                    OnPropertyChanged("Message");
                }
            }
        }

        public DateTime Date
        {
            get { return date; }
            set
            {
                if (date != value)
                {
                    date = value;
                    OnPropertyChanged("Date");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    //
    [ValueConversion(typeof(object), typeof(string))]
    public class FormattingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            string formatString = parameter as string;
            if (formatString != null)
            {
                return string.Format(culture, formatString, value);
            }
            else
            {
                return value.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public partial class MainWindow : Window
    {
        private DispatcherTimer dispatcherTimer;
        private TimeSpan remainingTime;
        public ObservableCollection<User> _users { get; set; }
        public DateTime Date { get; private set; }

        private List<User> selectedItems = new List<User>();
        public string myText;
        private DateTime dateTimeFormattedDate;
        //
        private string appDirectory;
        private string filePath;


        public MainWindow()
        {
            InitializeComponent();
            // App location
            this.WindowStartupLocation = WindowStartupLocation.Manual;
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
            // Textbox design
            tbTime.Background = Brushes.Transparent;
            tbTime.BorderThickness = new Thickness(0);
            tbTime.BorderBrush = Brushes.Transparent;
            tbTime.Foreground = new SolidColorBrush(Colors.White);
            // Initialize path
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appDirectoryName = "BreakNow Alarm Clock";
            appDirectory = Path.Combine(documentsPath, appDirectoryName);
            Directory.CreateDirectory(appDirectory);

            // Current time
            InitializeTimeDisplay();
            // Timer
            InitializeTimer();
            //
            LoadUsers();
            DeleteDatetimeThatHasPassed();
            //
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

        }


        // Save and load
        private void SaveItemsToFile(List<User> users, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(users);
            File.WriteAllText(filePath, jsonString);
        }

        private void SaveUsers()
        {
            string filePath = Path.Combine(appDirectory, "users1.5.json");
            SaveItemsToFile(_users.ToList(), filePath);
        }


        private List<User> LoadItemsFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<User>>(json);

            }
            return new List<User>();
        }


        private void LoadUsers()
        {
            string filePath = Path.Combine(appDirectory, "users1.5.json");
            List<User> users = LoadItemsFromFile(filePath);
            _users = new ObservableCollection<User>(users);
            AlarmListBox.ItemsSource = _users;
            SortListBoxItems();
        }

                
        // ScrollView
        private ScrollViewer FindScrollViewer(DependencyObject d)
        {
            if (d is ScrollViewer)
                return d as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var sw = FindScrollViewer(VisualTreeHelper.GetChild(d, i));
                if (sw != null) return sw;
            }
            return null;
        }


        private void InitializeScrollView()
        {
            ScrollViewer scrollViewer = FindScrollViewer(AlarmListBox);
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(0);
            }
        }


        private void InitializeTimeDisplay()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick2;
            timer.Start();
        }


        private void Timer_Tick2(object sender, EventArgs e)
        {
            MyTime.Text = DateTime.Now.ToString("dddd, MMM dd yyyy, hh:mm:ss");
        }


        private void HomeMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }


        private void HomeCloseButton_Click(object sender, RoutedEventArgs e)
        {
            SaveUsers();
            System.Windows.Application.Current.Shutdown();
        }


        private void FAQButton_Click(object sender, RoutedEventArgs e)
        {
            WindowFaq newWindow = new();
            newWindow.Show();
        }


        private void InitializeTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Tick += DispatcherTimer_Tick2;
        }


        private void Start_Click(object sender, RoutedEventArgs e)
        {
            string input = tbTime.Text;
            if (input == "00:00:00")
            {
                MessageBox.Show("Please set timer. Timer format: Hours:Minutes:Seconds");
            }


            else if (TimeSpan.TryParse(input, out remainingTime))
            {
                dispatcherTimer.Start();
            }

            else
            {
                MessageBox.Show("Invalid time format");
            }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            tbTime.Text = "00:00:00";
        }


        private void DispatcherTimer_Tick2(object sender, EventArgs e)
        {
            remainingTime -= TimeSpan.FromSeconds(1);
            if (remainingTime <= TimeSpan.Zero)
            {
                dispatcherTimer.Stop();
                tbTime.Text = "00:00:00";
                TimerKani aw = new TimerKani();
                aw.Show();
            }

            else
            {
                tbTime.Text = remainingTime.ToString(@"hh\:mm\:ss");
            }
        }


        


        // Select 7 days only
        private void DatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            MyDatePicker.BlackoutDates.AddDatesInPast();
            DateTime futureDate = DateTime.Today.AddDays(6);
            MyDatePicker.BlackoutDates.Add(new CalendarDateRange(futureDate.AddDays(1), DateTime.MaxValue));
        }



        private void SaveAlarmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string inputTime = HoursMinutesTb.Text;
                string inputDate = MyDatePicker.Text;
                string inputMessage = AlarmMessageTb.Text;
                DateTime inputDateTime = DateTime.Parse(inputDate + " " + inputTime);
                string formattedDate = inputDateTime.ToString("g", CultureInfo.CreateSpecificCulture("en-us"));
                DateTime now = DateTime.Now;
                DateTime dateTimeFormattedDate = Convert.ToDateTime(formattedDate);
                var duplicates = _users.Where(item => item.Date == dateTimeFormattedDate);

                if (MyDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Please choose a date within the next 7 days.");
                }

                else if (string.IsNullOrEmpty(inputTime))
                {
                    MessageBox.Show("Please set time. Format sample: 9:30 AM");
                }

                else if (inputTime.Length < 6)
                {
                    MessageBox.Show("Please enter correct time format. Example: 9:30 AM or 9:30 PM.");
                }

                else if (inputTime.Length > 10)
                {
                    MessageBox.Show("Please enter correct time format. Example: 9:30 AM or 9:30 PM.");
                }

                else if (dateTimeFormattedDate < now)
                {
                    MessageBox.Show("Time you entered has passed. Please enter future time.");
                }

                else if (inputMessage.Length > 25)
                {
                    MessageBox.Show("Message must not be more than 25 characters, including spaces.");
                }

                else if (duplicates.Any())
                {
                    MessageBox.Show("Chosen date plus time already saved. Please remove existing saved date + time or choose another date + time.");
                }

                else if (Regex.IsMatch(inputTime, @"/((1[0-2]|0?[1-9]):([0-5][0-9]) ?([AaPp][Mm]))/"))
                {
                    _users.Add(new User() { Date = dateTimeFormattedDate });
                    MyDatePicker.Text = string.Empty;
                    HoursMinutesTb.Text = string.Empty;
                    AlarmMessageTb.Text = string.Empty;
                    HoursMinutesTb.Clear();
                    AlarmMessageTb.Clear();
                    SortListBoxItems();
                    InitializeScrollView();
                }

                else if (inputMessage.Length <= 25)
                {
                    _users.Add(new User() { Date = dateTimeFormattedDate, Message = inputMessage });
                    MyDatePicker.Text = string.Empty;
                    HoursMinutesTb.Text = string.Empty;
                    AlarmMessageTb.Text = string.Empty;
                    HoursMinutesTb.Clear();
                    AlarmMessageTb.Clear();
                    SortListBoxItems();
                    InitializeScrollView();
                }

                else
                {
                    MessageBox.Show("Something is wrong. Please contact the developers.");
                }
            }

            catch (Exception)
            {
                MessageBox.Show("Please enter correct time format. Example: 9:30 AM or 9:30 PM.");
            }

        }


        private void SortListBoxItems()
        {
            AlarmListBox.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Date", System.ComponentModel.ListSortDirection.Ascending));

        }


        private void Chck_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            User newVal = (User)chk.Tag;
            if (chk.IsChecked.HasValue && chk.IsChecked.Value)
            {
                selectedItems.Add(newVal);
            }
            else
            {
                selectedItems.Remove(newVal);
            }
        }


        private void DeleteDatetimeThatHasPassed()
        {
            if (_users != null)
            {
                foreach (var item in _users.ToList())
                {
                    if (item.Date < DateTime.Now)
                    {
                        _users.Remove(item);
                    }
                }
            }
        }


        private void CheckRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in selectedItems)
            {
                _users.Remove(item);
            }
            selectedItems.Clear();
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_users != null)
            {
                foreach (var item in _users)
                {
                    if (item.Date <= DateTime.Now)
                    {
                        string myText = $" Take a break! \n It's now {item.Date}. \n Scheduled alarm has arrived. \n\n {item.Message} ";
                        AlarmKaniWindow sw = new AlarmKaniWindow(myText);
                        sw.Show();
                        DeleteDatetimeThatHasPassed();
                        break;
                    }
                }
            }
        }
    }
}
