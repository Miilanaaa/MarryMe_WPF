using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages.TransferFolder
{
    public partial class TransferMenuPage : Page
    {
        public static List<TransferType> typees { get; set; }
        public static List<Transfer> allTransfer { get; set; }
        private List<Transfer> filteredTransfer;
        private List<Transfer> displayedTransfer;
        Couple contextCouple;
        public static CoupleFavorites coupleFavorites1 = new CoupleFavorites();

        private int currentPage = 1;
        private int itemsPerPage = 8;
        private int totalPages;

        public TransferMenuPage(Couple couple, CoupleFavorites coupleFavorites)
        {
            InitializeComponent();
            contextCouple = couple;
            coupleFavorites1 = coupleFavorites;

            LoadAllTransfers();
            InitializeFilters();
            this.DataContext = this;
        }

        private void LoadAllTransfers()
        {
            allTransfer = DbConnection.MarryMe.Transfer.ToList();
            ApplyFiltersAndSort();
        }

        private void InitializeFilters()
        {
            typees = DbConnection.MarryMe.TransferType.ToList();
            typees.Insert(0, new TransferType() { Name = "Все" });
            FilterCb.SelectedIndex = 0;

            // Инициализация фильтра по количеству машин
            CarCountFilterCb.ItemsSource = new List<string>
            {
                "Любое количество",
                "1 машина", "2 машины", "3 машины", "4 машины", "5 машин",
                "6 машин", "7 машин", "8 машин", "9 машин", "10 машин"
            };
            CarCountFilterCb.SelectedIndex = 0;

            // Инициализация сортировки
            SortCb.SelectedIndex = 0;
        }

        private void ApplyFiltersAndSort()
        {
            try
            {
                // Применяем фильтр по поисковой строке
                string searchText = SearchTb.Text?.ToLower() ?? string.Empty;
                filteredTransfer = allTransfer
                    .Where(t => t.Name.ToLower().Contains(searchText))
                    .ToList();

                // Фильтр по типу транспорта
                if (FilterCb.SelectedItem is TransferType selectedType && selectedType.Name != "Все")
                {
                    filteredTransfer = filteredTransfer
                        .Where(t => t.TransferTypeId == selectedType.Id)
                        .ToList();
                }

                // Фильтр по цене
                if (decimal.TryParse(PriceFromTb.Text, out decimal minPrice))
                {
                    filteredTransfer = filteredTransfer
                        .Where(t => t.Price >= minPrice)
                        .ToList();
                }

                if (decimal.TryParse(PriceToTb.Text, out decimal maxPrice))
                {
                    filteredTransfer = filteredTransfer
                        .Where(t => t.Price <= maxPrice)
                        .ToList();
                }

                // Фильтр по количеству машин
                if (CarCountFilterCb.SelectedIndex > 0)
                {
                    int carsCount = CarCountFilterCb.SelectedIndex; // 1-10
                    filteredTransfer = filteredTransfer
                        .Where(t => t.NumberСars == carsCount)
                        .ToList();
                }

                // Применяем сортировку
                switch (SortCb.SelectedIndex)
                {
                    case 1: // По возрастанию цены
                        filteredTransfer = filteredTransfer
                            .OrderBy(t => t.Price)
                            .ToList();
                        break;
                    case 2: // По убыванию цены
                        filteredTransfer = filteredTransfer
                            .OrderByDescending(t => t.Price)
                            .ToList();
                        break;
                    default: // По умолчанию (без сортировки или по ID)
                        filteredTransfer = filteredTransfer
                            .OrderBy(t => t.Id)
                            .ToList();
                        break;
                }

                // Обновляем пагинацию
                currentPage = 1;
                InitializePagination();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтров: {ex.Message}");
            }
        }

        private bool IsTransferBooked(int transferId, DateTime date)
        {
            return DbConnection.MarryMe.TransferBookingDates
                .Any(b => b.TransferId == transferId &&
                         b.BookingDate == date &&
                         b.Status == true);
        }

        private void InitializePagination()
        {
            PaginationPanel.Children.Clear();
            PaginationPanel.Children.Add(PrevPageBtn);

            totalPages = (int)Math.Ceiling((double)filteredTransfer.Count / itemsPerPage);

            // Создаем кнопки страниц
            int startPage = Math.Max(1, currentPage - 2);
            int endPage = Math.Min(totalPages, currentPage + 2);

            for (int i = startPage; i <= endPage; i++)
            {
                var pageBtn = new Button
                {
                    Content = i.ToString(),
                    Width = 40,
                    Height = 40,
                    FontSize = 15,
                    Margin = new Thickness(5),
                    Tag = i
                };
                pageBtn.Click += PageBtn_Click;

                if (i == currentPage)
                {
                    pageBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909478"));
                    pageBtn.Foreground = Brushes.White;
                }

                PaginationPanel.Children.Add(pageBtn);
            }

            PaginationPanel.Children.Add(NextPageBtn);
            LoadPageData();
        }

        private void LoadPageData()
        {
            displayedTransfer = filteredTransfer
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            TransferLV.ItemsSource = displayedTransfer;
            UpdatePaginationButtons();
        }

        private void UpdatePaginationButtons()
        {
            PrevPageBtn.IsEnabled = currentPage > 1;
            NextPageBtn.IsEnabled = currentPage < totalPages;

            foreach (var child in PaginationPanel.Children)
            {
                if (child is Button btn && btn.Tag is int pageNumber)
                {
                    btn.Background = pageNumber == currentPage ?
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#909478")) :
                        Brushes.Transparent;
                    btn.Foreground = pageNumber == currentPage ?
                        Brushes.White :
                        Brushes.Black;
                }
            }
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

        // Обработчики событий
        private void SearchTb_TextChanged(object sender, TextChangedEventArgs e) => ApplyFiltersAndSort();
        private void FilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFiltersAndSort();
        private void CarCountFilterCb_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFiltersAndSort();
        private void SortCb_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFiltersAndSort();
        private void DateBookingDp_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ApplyFiltersAndSort();
        private void ApplyPriceFilter_Click(object sender, RoutedEventArgs e) => ApplyFiltersAndSort();

        private void TransferLV_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TransferLV.SelectedItem is Transfer transfer)
            {
                NavigationService.Navigate(new CardTransferPage(transfer, contextCouple, coupleFavorites1));
            }
        }

        private void ExitBt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new СoupleСardPage(contextCouple));
        }
    }
}