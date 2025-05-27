using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ZhiganshinaMilana420_MarryMe.DB;
using ZhiganshinaMilana420_MarryMe.Windows;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    public partial class TaskPage : Page
    {
        public static List<TaskUsers> taskUsers { get; set; }
        Users contextUsers;

        public TaskPage(Users users)
        {
            InitializeComponent();
            contextUsers = users;
            DateTime today = DateTime.Today;
            taskUsers = DbConnection.MarryMe.TaskUsers
                .Where(i => i.UserId == users.Id && i.DateEnd == today)
                .ToList();
            TaskUserLV.ItemsSource = taskUsers;
            DateTaskDp.Text = today.ToString();
            this.DataContext = this;

            UpdateEmptyTaskImageVisibility();
        }

        private void UpdateEmptyTaskImageVisibility()
        {
            if (TaskUserLV.ItemsSource == null || !TaskUserLV.ItemsSource.Cast<object>().Any())
            {
                EmptyTaskImage.Visibility = Visibility.Visible;
                TaskUserLV.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyTaskImage.Visibility = Visibility.Collapsed;
                TaskUserLV.Visibility = Visibility.Visible;
            }
        }

        private void AddTaskUserBt_Click(object sender, RoutedEventArgs e)
        {
            AddTaskUserWindow addTaskUserWindow = new AddTaskUserWindow(contextUsers);
            addTaskUserWindow.Closed += (s, args) =>
            {
                RefreshTaskList();
                UpdateEmptyTaskImageVisibility();
            };
            addTaskUserWindow.ShowDialog();
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TaskUserAddPage());
        }

        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Реализация поиска, если нужно
        }

        private void DateTaskDp_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateTaskDp.SelectedDate == null) return;

            DateTime selectedDate = DateTaskDp.SelectedDate ?? DateTime.MinValue;
            var filteredItems = DbConnection.MarryMe.TaskUsers
                .Where(i => i.DateEnd == selectedDate && i.UserId == contextUsers.Id)
                .ToList();
            TaskUserLV.ItemsSource = filteredItems;

            UpdateEmptyTaskImageVisibility();
        }

        public void RefreshTaskList()
        {
            if (DateTaskDp.SelectedDate != null)
            {
                DateTime selectedDate = DateTaskDp.SelectedDate ?? DateTime.Today;
                taskUsers = DbConnection.MarryMe.TaskUsers
                    .Where(i => i.DateEnd == selectedDate && i.UserId == contextUsers.Id)
                    .ToList();
            }
            else
            {
                DateTime today = DateTime.Today;
                taskUsers = DbConnection.MarryMe.TaskUsers
                    .Where(i => i.DateEnd == today && i.UserId == contextUsers.Id)
                    .ToList();
            }

            TaskUserLV.ItemsSource = taskUsers;
            UpdateEmptyTaskImageVisibility();
        }

        private void EditBt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var task = button.DataContext as TaskUsers;

            if (task != null)
            {
                if (task.AdminId != UserInfo.User.Id)
                {
                    MessageBox.Show("Вы можете редактировать только задачи, которые создали вы.",
                          "Ограничение прав",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                if (task.DateEnd < DateTime.Today)
                {
                    MessageBox.Show("Редактирование задач с прошедшей датой запрещено.",
                          "Ограничение",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                EditTaskUsersWindow editTaskUsersWindow = new EditTaskUsersWindow(contextUsers, task);
                editTaskUsersWindow.Closed += (s, args) =>
                {
                    RefreshTaskList();
                    UpdateEmptyTaskImageVisibility();
                };
                editTaskUsersWindow.ShowDialog();
            }
        }

        private void DelateBt_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is TaskUsers taskToDelete)
            {
                if (taskToDelete.AdminId != UserInfo.User.Id)
                {
                    MessageBox.Show("Вы можете удалять только задачи, которые создали сами.",
                          "Ограничение прав",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                if (taskToDelete.DateEnd < DateTime.Today)
                {
                    MessageBox.Show("Удаление задач с прошедшей датой запрещено.",
                          "Ограничение",
                          MessageBoxButton.OK,
                          MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить эту задачу?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.TaskUsers.Remove(taskToDelete);
                        DbConnection.MarryMe.SaveChanges();
                        RefreshTaskList();
                        MessageBox.Show("Задача успешно удалена!",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не удалось удалить задачу: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }
    }
}