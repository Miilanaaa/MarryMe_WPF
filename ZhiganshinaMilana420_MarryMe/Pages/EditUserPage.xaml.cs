﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZhiganshinaMilana420_MarryMe.DB;

namespace ZhiganshinaMilana420_MarryMe.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditUserPage.xaml
    /// </summary>
    public partial class EditUserPage : Page
    {
        private bool isPasswordVisible = false;
        private TextBox visiblePasswordTextBox;
        private Users contextUser;
        private byte[] photoBytes;

        public EditUserPage(Users user)
        {
            InitializeComponent();
            contextUser = user;
            LoadUserData();
            LoadGender();
            VisiblePasswordTb.Text = PasswordTb.Password;
            this.DataContext = this;
            ValidateFields();
        }

        

        private void LoadUserData()
        {
            // Основные данные
            SurnameTb.Text = contextUser.Surname;
            NameTb.Text = contextUser.Name;
            PatronymicTb.Text = contextUser.Patronymic;
            LoginTb.Text = contextUser.Login;
            PasswordTb.Password = contextUser.Password;
            EmailTb.Text = contextUser.Email;

            // Даты и финансы
            BirthDateDp.SelectedDate = contextUser.BirthDate;

            // Фото
            if (contextUser.Photo != null)
            {
                photoBytes = contextUser.Photo;
                TestImg.Source = LoadImage(photoBytes);
            }
        }

        private void LoadGender()
        {
            if (contextUser.IdGender == 1) // Мужской
            {
                GenderMen.IsChecked = true;
                GenderGirl.IsChecked = false;
            }
            else if (contextUser.IdGender == 2) // Женский
            {
                GenderMen.IsChecked = false;
                GenderGirl.IsChecked = true;
            }
        }

        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        private bool ValidateFields()
        {
            bool isValid = true;

            // Список всех обязательных полей
            var requiredFields = new List<Control>
            {
                SurnameTb, NameTb, PatronymicTb,
                LoginTb, EmailTb, 
                BirthDateDp
            };

            foreach (var field in requiredFields)
            {
                if (field is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
                {
                    ApplyErrorStyle(field);
                    isValid = false;
                }
                else if (field is ComboBox comboBox && comboBox.SelectedItem == null)
                {
                    ApplyErrorStyle(field);
                    isValid = false;
                }
                else if (field is DatePicker datePicker && datePicker.SelectedDate == null)
                {
                    ApplyErrorStyle(field);
                    isValid = false;
                }
                else
                {
                    ClearErrorStyle(field);
                }
            }

            // Проверка пароля
            if (string.IsNullOrEmpty(isPasswordVisible ? VisiblePasswordTb.Text : PasswordTb.Password))
            {
                ApplyErrorStyle(isPasswordVisible ? (Control)VisiblePasswordTb : PasswordTb);
                isValid = false;
            }
            else
            {
                ClearErrorStyle(isPasswordVisible ? (Control)VisiblePasswordTb : PasswordTb);
            }

            // Проверка пола
            if (!GenderMen.IsChecked.GetValueOrDefault() && !GenderGirl.IsChecked.GetValueOrDefault())
            {
                ApplyErrorStyle(GenderMen);
                ApplyErrorStyle(GenderGirl);
                isValid = false;
            }
            else
            {
                ClearErrorStyle(GenderMen);
                ClearErrorStyle(GenderGirl);
            }

            // Проверка фото
            if (TestImg.Source == null)
            {
                ApplyErrorStyle(TestImg);
                isValid = false;
            }
            else
            {
                ClearErrorStyle(TestImg);
            }

            return isValid;
        }

        private void ApplyErrorStyle(Control control)
        {
            control.BorderBrush = Brushes.Red;
            control.BorderThickness = new Thickness(1);
            control.ToolTip = "Обязательное поле";
        }

        private void ApplyErrorStyle(Image image)
        {
            var parentBorder = image.Parent as Border;
            if (parentBorder != null)
            {
                parentBorder.BorderBrush = Brushes.Red;
                parentBorder.BorderThickness = new Thickness(2);
                parentBorder.ToolTip = "Добавьте фото";
            }
        }

        private void ClearErrorStyle(Control control)
        {
            control.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFABAdB3"));
            control.BorderThickness = new Thickness(1);
            control.ToolTip = null;
        }

        private void ClearErrorStyle(Image image)
        {
            var parentBorder = image.Parent as Border;
            if (parentBorder != null)
            {
                parentBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E4C8BF"));
                parentBorder.BorderThickness = new Thickness(5);
                parentBorder.ToolTip = null;
            }
        }

        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                // Показываем текст пароля
                VisiblePasswordTb.Text = PasswordTb.Password;
                VisiblePasswordTb.Visibility = Visibility.Visible;
                PasswordTb.Visibility = Visibility.Collapsed;
                TogglePasswordButton.Content = new MaterialDesignThemes.Wpf.PackIcon
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.EyeOff
                };
            }
            else
            {
                // Скрываем текст пароля
                PasswordTb.Password = VisiblePasswordTb.Text;
                PasswordTb.Visibility = Visibility.Visible;
                VisiblePasswordTb.Visibility = Visibility.Collapsed;
                TogglePasswordButton.Content = new MaterialDesignThemes.Wpf.PackIcon
                {
                    Kind = MaterialDesignThemes.Wpf.PackIconKind.Eye
                };
            }
        }

        private void changeBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "*.png|*.png|*.jpeg|*.jpeg|*.jpg|*.jpg"
            };

            if (openFileDialog.ShowDialog().GetValueOrDefault())
            {
                photoBytes = File.ReadAllBytes(openFileDialog.FileName);
                TestImg.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                ClearErrorStyle(TestImg);
            }
        }

        private void AddEmployyeBt_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateFields())
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Проверяем, существует ли пользователь с таким логином (кроме текущего)
                var existingUser = DbConnection.MarryMe.Users
                    .FirstOrDefault(u => u.Login == LoginTb.Text && u.Id != contextUser.Id);

                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь под таким логином уже существует, введите другой логин!",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Обновление основных данных
                contextUser.Surname = SurnameTb.Text;
                contextUser.Name = NameTb.Text;
                contextUser.Patronymic = PatronymicTb.Text;
                contextUser.Login = LoginTb.Text;
                contextUser.Password = isPasswordVisible ? VisiblePasswordTb.Text : PasswordTb.Password;
                contextUser.Email = EmailTb.Text;

                // Обновление дат и финансов
                contextUser.BirthDate = BirthDateDp.SelectedDate.Value;

                // Обновление пола
                contextUser.IdGender = GenderMen.IsChecked.GetValueOrDefault() ? 1 : 2;

                // Обновление фото
                if (photoBytes != null)
                {
                    contextUser.Photo = photoBytes;
                }

                DbConnection.MarryMe.SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновляем данные в UserInfo, если редактируется текущий пользователь
                if (UserInfo.User != null && UserInfo.User.Id == contextUser.Id)
                {
                    UserInfo.User = contextUser;
                }

                NavigationService.Navigate(new MenuPage(UserInfo.User));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoBackBtt_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

