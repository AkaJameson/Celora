using Microsoft.Win32;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LicenseMaker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const string SecretKey = "your-secret-key";

    public MainWindow()
    {
        InitializeComponent();
        ExpireDatePicker.SelectedDate = DateTime.Now.AddDays(30);
    }

    private void GenerateLicense_Click(object sender, RoutedEventArgs e)
    {
        var userName = UserNameBox.Text;
        var accountName = AccountBox.Text;
        var password = PasswordBox.Password;
        var expireAt = ExpireDatePicker.SelectedDate ?? DateTime.UtcNow.AddDays(30);

        var rawData = $"{accountName}|{userName}|{password}|{expireAt:O}";
        var signature = ComputeHMAC(rawData, SecretKey);

        var license = new UserLicenseModel
        {
            Account = accountName,
            UserName = userName,
            Password = password,
            ExpireAt = expireAt,
            Signature = signature
        };

        var json = JsonSerializer.Serialize(license, new JsonSerializerOptions { WriteIndented = true });

        var dialog = new SaveFileDialog
        {
            Filter = "授权文件 (*.license)|*.license",
            FileName = $"license_{userName}.license"
        };

        if (dialog.ShowDialog() == true)
        {
            File.WriteAllText(dialog.FileName, json);
            MessageBox.Show("授权文件生成成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private string ComputeHMAC(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hash);
    }

    public class UserLicenseModel
    {
        public string Account { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime ExpireAt { get; set; }
        public string Signature { get; set; }
    }
}