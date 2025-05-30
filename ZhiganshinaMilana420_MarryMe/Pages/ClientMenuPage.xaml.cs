using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZhiganshinaMilana420_MarryMe.DB;
using ZhiganshinaMilana420_MarryMe.Windows;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    public partial class ClientMenuPage : Page
    {
        public bool IsAdmin => UserInfo.User?.RoleId == 1;
        public static List<Couple> couples { get; set; }
        public static List<Gromm> gromms { get; set; }
        public static List<Bride> brides { get; set; }
        public static List<WeddingStatus> statuses { get; set; }

        private int currentCouplePage = 1;
        private const int couplesPerPage = 4; // Количество пар на странице
        private int totalCouplePages;
        private List<Couple> allCouples = new List<Couple>();

        public ClientMenuPage()
        {
            InitializeComponent();
            LoadData();
            this.DataContext = this;

            // Установка фильтра по умолчанию (Id = 1)
            FilterCb.SelectedItem = statuses.FirstOrDefault(s => s.Id == 1);

            // Подписка на события для обновления данных при изменении фильтров
            SearchTb.TextChanged += UpdateData;
            FilterCb.SelectionChanged += UpdateData;
            DateTaskDp.SelectedDateChanged += UpdateData;
            

        }


        private void LoadData()
        {
            allCouples = DbConnection.MarryMe.Couple.ToList();
            statuses = DbConnection.MarryMe.WeddingStatus.ToList();
            gromms = DbConnection.MarryMe.Gromm.ToList();
            brides = DbConnection.MarryMe.Bride.ToList();

            UpdateData(null, null);
        }

        private void UpdateData(object sender, EventArgs e)
        {
            var query = DbConnection.MarryMe.Couple.AsQueryable();

            // Фильтрация по поиску
            if (!string.IsNullOrWhiteSpace(SearchTb.Text))
            {
                string searchText = SearchTb.Text.ToLower();
                query = query.Where(c =>
                    c.Gromm.Surname.ToLower().Contains(searchText) ||
                    c.Gromm.Name.ToLower().Contains(searchText) ||
                    c.Bride.Surname.ToLower().Contains(searchText) ||
                    c.Bride.Name.ToLower().Contains(searchText));
            }

            // Фильтрация по статусу
            if (FilterCb.SelectedItem is WeddingStatus status)
            {
                query = query.Where(c => c.WeddingStatusId == status.Id);
            }

            // Фильтрация по дате
            if (DateTaskDp.SelectedDate.HasValue)
            {
                query = query.Where(c => c.WeddingDate == DateTaskDp.SelectedDate.Value);
            }

            // Применяем фильтры и обновляем список
            allCouples = query.OrderBy(c => c.WeddingDate).ToList();
            currentCouplePage = 1;
            InitializeCouplePagination();
        }
        private void InitializeCouplePagination()
        {
            // Вычисляем общее количество страниц
            totalCouplePages = (int)Math.Ceiling((double)allCouples.Count / couplesPerPage);

            // Если нет данных, скрываем пагинацию
            if (totalCouplePages == 0)
            {
                CouplePaginationPanel.Visibility = Visibility.Collapsed;
                return;
            }
            CouplePaginationPanel.Visibility = Visibility.Visible;

            // Очищаем панель пагинации
            CouplePaginationPanel.Children.Clear();
            CouplePaginationPanel.Children.Add(CouplePrevPageBtn);

            // Определяем диапазон отображаемых страниц (максимум 5)
            int startPage = Math.Max(1, currentCouplePage - 2);
            int endPage = Math.Min(totalCouplePages, startPage + 4);
            startPage = Math.Max(1, endPage - 4);

            // Добавляем первую страницу и многоточие при необходимости
            if (startPage > 1)
            {
                AddPageButton(1);
                if (startPage > 2)
                {
                    var ellipsis = new TextBlock { Text = "...", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };
                    CouplePaginationPanel.Children.Add(ellipsis);
                }
            }

            // Создаем кнопки для страниц в диапазоне
            for (int i = startPage; i <= endPage; i++)
            {
                AddPageButton(i);
            }

            // Добавляем последнюю страницу и многоточие при необходимости
            if (endPage < totalCouplePages)
            {
                if (endPage < totalCouplePages - 1)
                {
                    var ellipsis = new TextBlock { Text = "...", Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = VerticalAlignment.Center };
                    CouplePaginationPanel.Children.Add(ellipsis);
                }
                AddPageButton(totalCouplePages);
            }

            CouplePaginationPanel.Children.Add(CoupleNextPageBtn);

            // Загружаем данные для текущей страницы
            LoadCouplePageData();
        }
        private void AddPageButton(int pageNumber)
        {
            var pageBtn = new Button
            {
                Content = pageNumber.ToString(),
                Width = 30,
                Height = 30,
                FontSize = 12,
                Margin = new Thickness(2, 0, 2, 0),
                Tag = pageNumber,
                Style = (Style)FindResource("ActionButton")
            };
            pageBtn.Click += CouplePageBtn_Click;

            if (pageNumber == currentCouplePage)
            {
                pageBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909478"));
                pageBtn.Foreground = Brushes.White;
            }
            else
            {
                pageBtn.Background = Brushes.Transparent;
                pageBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909478"));
            }

            CouplePaginationPanel.Children.Add(pageBtn);
        }
        private void LoadCouplePageData()
        {
            if (allCouples == null || !allCouples.Any())
            {
                CoupleLV.ItemsSource = null;
                return;
            }

            // Получаем пары для текущей страницы
            var displayedCouples = allCouples
                .Skip((currentCouplePage - 1) * couplesPerPage)
                .Take(couplesPerPage)
                .ToList();

            CoupleLV.ItemsSource = displayedCouples;

            // Обновляем состояние кнопок
            UpdateCouplePaginationButtons();
        }

        private void UpdateCouplePaginationButtons()
        {
            CouplePrevPageBtn.IsEnabled = currentCouplePage > 1;
            CoupleNextPageBtn.IsEnabled = currentCouplePage < totalCouplePages;

            foreach (var child in CouplePaginationPanel.Children)
            {
                if (child is Button btn && btn.Tag is int pageNumber)
                {
                    if (pageNumber == currentCouplePage)
                    {
                        btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909478"));
                        btn.Foreground = Brushes.White;
                    }
                    else
                    {
                        btn.Background = Brushes.Transparent;
                        btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909478"));
                    }
                }
            }
        }

        private void CouplePageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int pageNumber)
            {
                currentCouplePage = pageNumber;
                LoadCouplePageData();
            }
        }

        private void CouplePrevPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentCouplePage > 1)
            {
                currentCouplePage--;
                LoadCouplePageData();
            }
        }

        private void CoupleNextPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentCouplePage < totalCouplePages)
            {
                currentCouplePage++;
                LoadCouplePageData();
            }
        }

        private void CoupleLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CoupleLV.SelectedItem is Couple couple && couple.WeddingStatusId == 1)
            {
                NavigationService.Navigate(new EditClientPage(couple));
            }
        }

        private void FinishWeddingBtt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var couple = button.DataContext as Couple;

            var result = MessageBox.Show($"Вы уверены, что хотите завершить свадьбу пары {couple.Gromm.Surname} {couple.Gromm.Name} и {couple.Bride.Surname} {couple.Bride.Name}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    couple.WeddingStatusId = 2; // 2 - Завершена

                    var gromm = DbConnection.MarryMe.Gromm.FirstOrDefault(g => g.Id == couple.GroomId);
                    if (gromm != null)
                    {
                        gromm.PassportNumber = null;
                        gromm.PassportSeries = null;
                        gromm.PassportAddress = null;
                        gromm.Addresss = null;
                    }

                    var bride = DbConnection.MarryMe.Bride.FirstOrDefault(b => b.Id == couple.BrideId);
                    if (bride != null)
                    {
                        bride.PassportNumber = null;
                        bride.PassportSeries = null;
                        bride.PassportAddress = null;
                        bride.Addresss = null;
                    }

                    DbConnection.MarryMe.SaveChanges();
                    LoadData();
                    CoupleLV.Items.Refresh();
                    MessageBox.Show("Свадьба успешно завершена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CancelWeddingBtt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var couple = button.DataContext as Couple;

            var result = MessageBox.Show($"Вы уверены, что хотите отменить свадьбу пары {couple.Gromm.Surname} {couple.Gromm.Name} и {couple.Bride.Surname} {couple.Bride.Name}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    couple.WeddingStatusId = 3; // 3 - Отменена

                    var gromm = DbConnection.MarryMe.Gromm.FirstOrDefault(g => g.Id == couple.GroomId);
                    if (gromm != null)
                    {
                        gromm.PassportNumber = null;
                        gromm.PassportSeries = null;
                        gromm.PassportAddress = null;
                        gromm.Addresss = null;
                    }

                    var bride = DbConnection.MarryMe.Bride.FirstOrDefault(b => b.Id == couple.BrideId);
                    if (bride != null)
                    {
                        bride.PassportNumber = null;
                        bride.PassportSeries = null;
                        bride.PassportAddress = null;
                        bride.Addresss = null;
                    }

                    DbConnection.MarryMe.SaveChanges();
                    LoadData();
                    MessageBox.Show("Свадьба успешно отменена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AssignManagerBtt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var couple = button.DataContext as Couple;

            if (couple.WeddingDate == null)
            {
                MessageBox.Show("У пары не указана дата свадьбы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AssignManagerWindow assignManagerWindow = new AssignManagerWindow(couple.WeddingDate.Value, couple);
            assignManagerWindow.ShowDialog();

            // Обновляем данные после закрытия окна
            if (assignManagerWindow.DialogResult == true)
            {
                LoadData();
            }
        }
        private void ChangeManagerBtt_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var couple = button.DataContext as Couple;

            if (couple.WeddingDate == null)
            {
                MessageBox.Show("У пары не указана дата свадьбы!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AssignManagerWindow assignManagerWindow = new AssignManagerWindow(couple.WeddingDate.Value, couple);
            assignManagerWindow.ShowDialog();

            // Обновляем данные после закрытия окна
            if (assignManagerWindow.DialogResult == true)
            {
                LoadData();
            }
        }
    }
}