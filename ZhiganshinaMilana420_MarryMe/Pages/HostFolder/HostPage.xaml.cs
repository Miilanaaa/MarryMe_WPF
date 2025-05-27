using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages.HostFolder
{
    public partial class HostPage : Page
    {
        private List<Host> allHosts;
        private List<Host> filteredHosts;
        private List<Host> displayedHosts;

        private int currentPage = 1;
        private int itemsPerPage = 6;
        private int totalPages;

        public static List<HostPhoto> hostPhotos = new List<HostPhoto>();
        public static List<Host> hosts { get; set; }

        public HostPage()
        {
            InitializeComponent();

            allHosts = new List<Host>(DbConnection.MarryMe.Host.ToList());
            hosts = new List<Host>(allHosts);
            hostPhotos = new List<HostPhoto>(DbConnection.MarryMe.HostPhoto.ToList());

            this.DataContext = this;

            ApplyFiltersAndSort(); // Initialize with all hosts
        }

        public void Refresh()
        {
            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            // Apply filters
            filteredHosts = allHosts
                .Where(h => SearchTb.Text.Length == 0 ||
                       h.Surname.ToLower().Contains(SearchTb.Text.Trim().ToLower()) ||
                       h.Name.ToLower().Contains(SearchTb.Text.Trim().ToLower()))
                .ToList();

            // Update pagination
            currentPage = 1;
            InitializePagination();
        }

        private void InitializePagination()
        {
            // Calculate total pages
            totalPages = (int)Math.Ceiling((double)filteredHosts.Count / itemsPerPage);

            // Clear pagination panel
            PaginationPanel.Children.Clear();
            PaginationPanel.Children.Add(PrevPageBtn);

            // Create page buttons
            for (int i = 1; i <= totalPages; i++)
            {
                var pageBtn = new Button
                {
                    Content = i.ToString(),
                    Width = 40,
                    Height = 40,
                    FontSize = 15,
                    Margin = new Thickness(5, 0, 5, 0),
                    Tag = i
                };
                pageBtn.Click += PageBtn_Click;

                if (i == currentPage)
                {
                    pageBtn.Background = Brushes.LightGray;
                }

                PaginationPanel.Children.Add(pageBtn);
            }

            PaginationPanel.Children.Add(NextPageBtn);

            LoadPageData();
        }

        private void LoadPageData()
        {
            displayedHosts = filteredHosts
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            HostLV.ItemsSource = displayedHosts;

            UpdatePaginationButtons();
        }

        private void UpdatePaginationButtons()
        {
            foreach (var child in PaginationPanel.Children)
            {
                if (child is Button btn && btn.Tag is int pageNumber)
                {
                    btn.Background = pageNumber == currentPage ? Brushes.LightGray : Brushes.Transparent;
                }
            }

            PrevPageBtn.IsEnabled = currentPage > 1;
            NextPageBtn.IsEnabled = currentPage < totalPages;
        }

        private void PageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int pageNumber)
            {
                currentPage = pageNumber;
                LoadPageData();
            }
        }

        private void PrevPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                LoadPageData();
            }
        }

        private void NextPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                LoadPageData();
            }
        }

        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddHostPage());
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Host host)
            {
                NavigationService.Navigate(new EditHostPage(host));
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Host host)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить данные этого ведущего?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Host.Remove(host);
                        DbConnection.MarryMe.SaveChanges();
                        allHosts = new List<Host>(DbConnection.MarryMe.Host.ToList());
                        Refresh();
                        MessageBox.Show("Данные о ведущем успешно удалены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о ведущем невозможно удалить, он забронирован гостями", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CollectionPage());
        }
    }
}