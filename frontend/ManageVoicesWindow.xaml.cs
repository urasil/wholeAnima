﻿using System;
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
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using System.Security.Policy;
using dotnetAnima.Core;


namespace dotnetAnima
{
    /// <summary>
    /// Interaction logic for ManageVoicesWindow.xaml
    /// </summary>
    public partial class ManageVoicesWindow : Page
    {
        // Back end
        string backendJsonFilePath = @"../../../backend.json";
        private string backendJsonContent = File.ReadAllText("../../../backend.json");
        private Dictionary<string, string> backendJsonObject;

        // Front end
        string frontendJsonFilePath = @"../../../frontend.json";
        private string frontendJsonContent = File.ReadAllText("../../../frontend.json");
        private Dictionary<string, string> frontendJsonObject;

        // Profile Languages
        string profileLanguagesJsonFilePath = @"../../../profileLanguages.json";
        private string profileLanguagesJsonContent = File.ReadAllText("../../../profileLanguages.json");
        private Dictionary<string, List<string>> profileLanguagesObject;


        public ManageVoicesWindow()
        {
            InitializeComponent();
            frontendJsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(frontendJsonContent);
            backendJsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(backendJsonContent);
            updateVoices();

        }

        private void readingBackendJson()
        {
            string updatedJsonContent = File.ReadAllText("../../../backend.json");
            backendJsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(updatedJsonContent);
        }

        private void readingProfileLanguageJson()
        {
            string updatedJsonContent = File.ReadAllText("../../../profileLanguages.json");
            profileLanguagesObject = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(updatedJsonContent);
        }

        private void readingFrontendJson()
        {
            string frontendJsonContent = File.ReadAllText(frontendJsonFilePath);
            frontendJsonObject = JsonConvert.DeserializeObject<Dictionary<string, string>>(frontendJsonContent);
        }

        private void updateVoices()
        {
            string[] listOfNames;
            listOfNames = ExtractNames();
            string currentUser = frontendJsonObject["nameOfCurrentUser"];
            yourVoiceRadioButton.Content = currentUser;


            foreach (string name in listOfNames)
            {
                if (name != currentUser)
                {
                    RadioButton radioButton = new RadioButton();
                    radioButton.Content = name;
                    radioButton.Name = name;
                    radioButton.Foreground = Brushes.AntiqueWhite;
                    radioButton.Margin = new Thickness(0, 0, 0, 10);
                    radioButton.FontSize = 25;
                    radioButton.FontWeight = FontWeights.DemiBold;
                    radioButton.FontStyle = FontStyles.Italic;
                    radioButton.FontFamily = new FontFamily("Times New Roman");
                    radioButton.GroupName = "VoiceSelection";
                    groupPanel.Children.Add(radioButton);
                }

            }
        }

        private string[] ExtractNames()
        {
            string[] filePaths = Directory.GetFiles("../../../animaProfiles", "*.animaprofile");  //only get files with extension of 'animeprofile'
            string[] namesList = new string[filePaths.Length];

            for (int i = 0; i < filePaths.Length; i++)
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(filePaths[i]);
                namesList[i] = fileName;
            }

            return namesList;
        }


        private void RedoVoice(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new BankVoiceWindow());
        }

        // Needs backend functionality
        private async void ImportVoice(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            bool? response = dialog.ShowDialog();
            if (response == true)
            {
                ButtonHelper.DisableButton(importVoice, false);
                string filePath = dialog.FileName;
                frontendJsonObject["importFilePath"] = filePath;
                string updatedJsonContent = JsonConvert.SerializeObject(frontendJsonObject, Formatting.Indented);
                File.WriteAllText(frontendJsonFilePath, updatedJsonContent);
                await SendFileContentBackToFrontend();
                ButtonHelper.DisableButton(importVoice, true);
                if (backendJsonObject["importSuccess"] == "false")
                {
                    MessageBox.Show("Coudln't create a voice profile from uploaded file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    frontendJsonObject["importFilePath"] = "";
                }
                if (backendJsonObject["importSuccess"] == "true")
                {
                    foreach (var child in groupPanel.Children.OfType<RadioButton>().ToList())
                    {
                        if (child.Name != "yourVoiceRadioButton")
                        {
                            groupPanel.Children.Remove(child);
                        }
                    }
                    frontendJsonObject["importFilePath"] = "";
                    updateVoices();
                    MessageBox.Show("Anima Profile Generated", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                backendJsonObject["importSuccess"] = "";

                UpdateFrontendJsonFile();
                UpdateBackendJsonFile();

            }
        }

        private async Task SendFileContentBackToFrontend()
        {
            while (backendJsonObject["importSuccess"] != "true")
            {
                readingBackendJson();
                await Task.Delay(1000);
                if (backendJsonObject["importSuccess"] == "false")
                {
                    break;
                }
            }
        }

        // Delete Voice Button Functions

        private RadioButton FindCheckedRadioButton()
        {
            RadioButton checkedRadioButton = null;
            foreach (var child in groupPanel.Children)
            {
                if (child is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    checkedRadioButton = radioButton;
                    break;
                }
            }
            return checkedRadioButton;
        }
        private void UpdateFrontendJsonFile()
        {
            string updatedJsonContent = JsonConvert.SerializeObject(frontendJsonObject, Formatting.Indented);
            File.WriteAllText(frontendJsonFilePath, updatedJsonContent);
        }

        private void UpdateBackendJsonFile()
        {
            string updatedJsonContent = JsonConvert.SerializeObject(backendJsonObject, Formatting.Indented);
            File.WriteAllText(backendJsonFilePath, updatedJsonContent);
        }

        private void UpdateProfileLanguagesFile()
        {
            string updatedContent = JsonConvert.SerializeObject(profileLanguagesObject, Formatting.Indented);
            File.WriteAllText(profileLanguagesJsonFilePath, updatedContent);
        }
        private void DeleteVoice(object sender, RoutedEventArgs e)
        {
            RadioButton checkedRadioButton = FindCheckedRadioButton();

            if (checkedRadioButton != null)
            {
                string username = checkedRadioButton.Content.ToString();
                string animaProfileName = username + ".animaprofile";
                int radioButtonCount = groupPanel.Children.OfType<RadioButton>().Count();
                string animaProfilePath = System.IO.Path.Combine("../../../animaProfiles", animaProfileName);
                string valueToDeleteKey = "";
                readingProfileLanguageJson();
                foreach (var kvp in profileLanguagesObject)
                {
                    if (kvp.Value.Contains(username))
                    {
                        valueToDeleteKey = kvp.Key.ToString();
                        break; // Exit loop after removing first occurrence
                    }
                }
                Console.WriteLine("asdasdf" + valueToDeleteKey);
                if (valueToDeleteKey != "")
                {
                    profileLanguagesObject[valueToDeleteKey].Remove(username);
                }

                UpdateProfileLanguagesFile();

                if (File.Exists(animaProfilePath) && radioButtonCount > 1)
                {
                    File.Delete(animaProfilePath);

                    if (checkedRadioButton.Content.ToString() != frontendJsonObject["nameOfCurrentUser"])
                    {
                        groupPanel.Children.Remove(checkedRadioButton);
                        yourVoiceRadioButton.IsChecked = true;
                    }
                    else
                    {

                        RadioButton[] radioButtons = groupPanel.Children.OfType<RadioButton>().ToArray();
                        foreach (RadioButton radioButton in radioButtons)
                        {
                            if (radioButton.Content.ToString() != frontendJsonObject["nameOfCurrentUser"])
                            {
                                frontendJsonObject["nameOfCurrentUser"] = radioButton.Content.ToString();
                                yourVoiceRadioButton.Content = frontendJsonObject["nameOfCurrentUser"];
                                yourVoiceRadioButton.IsChecked = true;
                                groupPanel.Children.Remove(radioButton);
                                break;
                            }
                        }
                        UpdateFrontendJsonFile();

                    }
                }

                else
                {
                    MessageBox.Show("Record one more voice to delete this one!");
                }

            }

        }

        private void SelectVoice(object sender, RoutedEventArgs e)
        {
            string oldUser = frontendJsonObject["nameOfCurrentUser"];
            foreach (var child in groupPanel.Children)
            {
                if (child is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    frontendJsonObject["nameOfCurrentUser"] = radioButton.Content.ToString();
                    break;
                }
            }
            string currentUser = frontendJsonObject["nameOfCurrentUser"];
            foreach (var child in groupPanel.Children)
            {
                if (child is RadioButton radioButton)
                {
                    if (radioButton.Content == currentUser)
                    {
                        radioButton.Content = oldUser;
                        break;
                    }
                }

            }
            UpdateFrontendJsonFile();
            yourVoiceRadioButton.Content = currentUser;
            yourVoiceRadioButton.IsChecked = true;

        }

        private void Speak(object sender, RoutedEventArgs e)
        {

            this.NavigationService.Navigate(new TextToSpeechWindow());
        }
    }
}
