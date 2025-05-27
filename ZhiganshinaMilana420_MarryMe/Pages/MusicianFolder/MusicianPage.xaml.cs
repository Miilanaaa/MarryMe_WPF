using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages.MusicianFolder
{
    public partial class MusicianPage : Page
    {
        private List<Musician> allMusicians;
        private List<Musician> filteredMusicians;
        private List<Musician> displayedMusicians;

        private int currentPage = 1;
        private int itemsPerPage = 6;
        private int totalPages;

        public static List<MusicianType> typees { get; set; }

        public MusicianPage()
        {
            InitializeComponent();

            allMusicians = new List<Musician>(DbConnection.MarryMe.Musician.ToList());
            typees = new List<MusicianType>(DbConnection.MarryMe.MusicianType.ToList());
            typees.Insert(0, new MusicianType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;
            this.DataContext = this;

            ApplyFiltersAndSort(); // Initialize with all musicians
        }

        public void Refresh()
        {
            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            var category = FilterCb.SelectedItem as MusicianType;

            // Apply filters
            filteredMusicians = allMusicians
                .Where(a => category == null || category.Id == 0 || a.MusicianTypeId == category.Id)
                .Where(a => SearchTb.Text.Length == 0 || a.TeamName.ToLower().Contains(SearchTb.Text.Trim().ToLower()))
                .ToList();

            // Update pagination
            currentPage = 1;
            InitializePagination();
        }
        private void InitializePagination()
        {
            // Calculate total pages
            totalPages = (int)Math.Ceiling((double)filteredMusicians.Count / itemsPerPage);

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
            displayedMusicians = filteredMusicians
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            MusicianLV.ItemsSource = displayedMusicians;

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
            NavigationService.Navigate(new AddMusicianPage());
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Musician musician)
            {
                NavigationService.Navigate(new EditMusicianPage(musician));
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Musician musician)
            {
                try
                {
                    MessageBoxResult result = MessageBox.Show("Вы точно хотите удалить эту группу?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        DbConnection.MarryMe.Musician.Remove(musician);
                        DbConnection.MarryMe.SaveChanges();
                        allMusicians = new List<Musician>(DbConnection.MarryMe.Musician.ToList());
                        Refresh();
                        MessageBox.Show("Группа успешно удалёна!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("Информацию о группе невозможно удалить, она забронирована клиентами", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
