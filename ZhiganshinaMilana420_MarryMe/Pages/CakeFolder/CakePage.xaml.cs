using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages.CakeFolder
{
    public partial class CakePage : Page
    {
        private List<Cake> allCakes;
        private List<Cake> filteredCakes;
        private List<Cake> displayedCakes;

        private int currentPage = 1;
        private int itemsPerPage = 6;
        private int totalPages;

        public static List<CakeType> typees { get; set; }

        public CakePage()
        {
            InitializeComponent();

            allCakes = new List<Cake>(DbConnection.MarryMe.Cake.ToList());
            typees = new List<CakeType>(DbConnection.MarryMe.CakeType.ToList());
            typees.Insert(0, new CakeType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;

            ApplyFiltersAndSort(); // Initialize with all cakes
        }

        public void Refresh()
        {
            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            var category = FilterCb.SelectedItem as CakeType;

            // Apply filters
            filteredCakes = allCakes
                .Where(a => category == null || category.Id == 0 || a.CakeTypeId == category.Id)
                .Where(a => SearchTb.Text.Length == 0 || a.Name.ToLower().Contains(SearchTb.Text.Trim().ToLower()))
                .ToList();

            // Update pagination
            currentPage = 1;
            InitializePagination();
        }

        private void InitializePagination()
        {
            // Calculate total pages
            totalPages = (int)Math.Ceiling((double)filteredCakes.Count / itemsPerPage);

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
            displayedCakes = filteredCakes
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            CakeLV.ItemsSource = displayedCakes;

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

        private void FilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Refresh();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddCakePage());
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Cake cake)
            {
                NavigationService.Navigate(new EditCakePage(cake));
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Cake cake)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Cake.Remove(cake);
                        DbConnection.MarryMe.SaveChanges();
                        allCakes = new List<Cake>(DbConnection.MarryMe.Cake.ToList());
                        Refresh();
                        MessageBox.Show("Товар успешно удален!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Товар невозможно удалить, он забронирован клиентами", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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