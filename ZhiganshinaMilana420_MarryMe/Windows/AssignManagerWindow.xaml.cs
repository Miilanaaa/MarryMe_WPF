using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ZhiganshinaMilana420_MarryMe.DB;
using ZhiganshinaMilana420_MarryMe.Pages;

namespace ZhiganshinaMilana420_MarryMe.Windows
{
    /// <summary>
    /// Логика взаимодействия для AssignManagerWindow.xaml
    /// </summary>
    public partial class AssignManagerWindow : Window
    {
        public static List<Users> users { get; set; }
        private DateTime weddingDate;

        private Couple currentCouple;

        public AssignManagerWindow(DateTime date, Couple couple)
        {
            InitializeComponent();
            weddingDate = date;
            currentCouple = couple;
            LoadFreeManagers();
            this.DataContext = this;
        }

        private void LoadFreeManagers()
        {
            // Получаем всех НЕ уволенных менеджеров (роль 2)
            var allManagers = DbConnection.MarryMe.Users
                .Where(u => u.RoleId == 2 &&
                           u.Id != UserInfo.User.Id &&
                           (u.Dismissed == false || u.Dismissed == null)) // Фильтр по не уволенным
                .ToList();

            // Получаем менеджеров, которые уже заняты в эту дату
            var busyManagers = DbConnection.MarryMe.Couple
                .Where(c => c.WeddingStatusId == 1 &&
                           c.WeddingDate == weddingDate &&
                           c.IdUser != null)
                .Select(c => c.IdUser)
                .ToList();

            // Фильтруем свободных менеджеров (не занятые и не уволенные)
            users = allManagers
                .Where(m => !busyManagers.Contains(m.Id))
                .ToList();

            ManagersLv.ItemsSource = users;
        }

        private void AssignBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ManagersLv.SelectedItem is Users selectedManager)
            {
                try
                {
                    // Находим текущую пару в базе данных
                    var coupleToUpdate = DbConnection.MarryMe.Couple.FirstOrDefault(c => c.BrideId == currentCouple.BrideId);

                    if (coupleToUpdate != null)
                    {
                        // Назначаем менеджера
                        coupleToUpdate.IdUser = selectedManager.Id;
                        coupleToUpdate.WeddingStatusId = 1;

                        // Сохраняем изменения
                        DbConnection.MarryMe.SaveChanges();

                        MessageBox.Show("Менеджер успешно назначен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        DialogResult = true;
                        Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при назначении менеджера: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите менеджера!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
