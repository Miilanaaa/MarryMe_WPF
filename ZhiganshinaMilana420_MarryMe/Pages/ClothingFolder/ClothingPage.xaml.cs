using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages.ClothingFolder
{
    public partial class ClothingPage : Page
    {
        private List<Clothing> allClothings;
        private List<Clothing> filteredClothings;
        private List<Clothing> displayedClothings;

        private int currentPage = 1;
        private int itemsPerPage = 6;
        private int totalPages;

        public static List<ClothingType> typees { get; set; }
        public static List<ClothingPhoto> photoClothing = new List<ClothingPhoto>();

        public ClothingPage()
        {
            InitializeComponent();

            allClothings = new List<Clothing>(DbConnection.MarryMe.Clothing.ToList());
            photoClothing = new List<ClothingPhoto>(DbConnection.MarryMe.ClothingPhoto.ToList());
            typees = new List<ClothingType>(DbConnection.MarryMe.ClothingType.ToList());
            typees.Insert(0, new ClothingType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;

            ApplyFiltersAndSort(); // Initialize with all clothings
        }

        public void Refresh()
        {
            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            var category = FilterCb.SelectedItem as ClothingType;

            // Apply filters
            filteredClothings = allClothings
                .Where(a => category == null || category.Id == 0 || a.ClothingTypeId == category.Id)
                .Where(a => SearchTb.Text.Length == 0 || a.Name.ToLower().Contains(SearchTb.Text.Trim().ToLower()))
                .ToList();

            // Update pagination
            currentPage = 1;
            InitializePagination();
        }

        private void InitializePagination()
        {
            // Calculate total pages
            totalPages = (int)Math.Ceiling((double)filteredClothings.Count / itemsPerPage);

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
            displayedClothings = filteredClothings
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            ClothingLV.ItemsSource = displayedClothings;

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
            NavigationService.Navigate(new AddClothingPage());
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Clothing clothing)
            {
                NavigationService.Navigate(new EditClothingPage(clothing));
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Clothing clothing)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить этот товар?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Clothing.Remove(clothing);
                        DbConnection.MarryMe.SaveChanges();
                        allClothings = new List<Clothing>(DbConnection.MarryMe.Clothing.ToList());
                        Refresh();
                        MessageBox.Show("Товар успешно удалён!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о товаре невозможно удалить, он забронирован гостями", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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