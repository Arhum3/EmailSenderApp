using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using EmailSenderApp.EmailService;

namespace EmailSenderApp
{
    public partial class MainWindow : Window
    {
        private List<string> recipientList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadCSV_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                recipientList.Clear();
                var lines = File.ReadAllLines(openFileDialog.FileName);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                        recipientList.Add(trimmed);
                }
                Log($"Loaded {recipientList.Count} recipients.");
            }
        }

        private async void SendEmails_Click(object sender, RoutedEventArgs e)
        {
            var smtpHost = SmtpHostBox.Text;
            var smtpPortText = SmtpPortBox.Text;
            var smtpUser = SmtpUserBox.Text;
            var smtpPass = SmtpPassBox.Password;
            var fromEmail = FromEmailBox.Text;
            var subject = SubjectBox.Text;
            var body = new TextRange(BodyEditor.Document.ContentStart, BodyEditor.Document.ContentEnd).Text;

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpPortText) ||
                string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass) ||
                string.IsNullOrWhiteSpace(fromEmail) || string.IsNullOrWhiteSpace(subject) ||
                string.IsNullOrWhiteSpace(body))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(smtpPortText, out int smtpPort))
            {
                MessageBox.Show("SMTP Port must be a valid number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (recipientList.Count == 0)
            {
                MessageBox.Show("Please upload a recipient CSV file.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var senderService = new EmailSender(smtpHost, smtpPort, smtpUser, smtpPass, fromEmail);
            await senderService.SendEmailsAsync(recipientList, subject, body, Log);
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText($"{DateTime.Now}: {message}\n");
                LogBox.ScrollToEnd();
            });
        }
    }
}