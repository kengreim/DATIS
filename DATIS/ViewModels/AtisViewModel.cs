using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DATIS.Services;
using DATIS.Models;
using Microsoft.UI.Dispatching;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Text.RegularExpressions;

namespace DATIS.ViewModels
{
    public class AtisViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public DispatcherQueue Dispatcher { get; set; }
        public AtisFetchService AtisFetcher { get; set; }
        public ObservableCollection<String> AirportNames { get; private set; }
        
        private List<Atis> FetchedAtisList { get; set; }
        private string _currentCombinedAtisText = "";
        private string _currentDepartureAtisText = "";
        private string _currentArrivalAtisText = "";

        private string _dropdownPlaceholderText = "Loading airports...";
        public string DropdownPlaceholderText
        {
            get => _dropdownPlaceholderText;
            set => SetField(ref _dropdownPlaceholderText, value);
        }

        private bool _dropdownEnabled = false;
        public bool DropdownEnabled
        {
            get => _dropdownEnabled;
            set => SetField(ref _dropdownEnabled, value);
        }

        private string _selectedAirport;
        public string SelectedAirport
        {
            get => _selectedAirport;
            set => UpdateVisibleAtis(value);
        }

        private string _atisText = "";
        public string AtisText
        {
            get => _atisText;
            set => SetField(ref _atisText, value);
        }

        private string _atisTitle = "D-ATIS";
        public string AtisTitle
        {
            get => _atisTitle;
            set => SetField(ref _atisTitle, value);
        }

        private bool _btnDepartureVisibility = false;
        public bool BtnDepartureVisibility
        {
            get => _btnDepartureVisibility;
            set => SetField(ref _btnDepartureVisibility, value);
        }

        private bool _btnArrivalVisibility = false;
        public bool BtnArrivalVisibility
        {
            get => _btnArrivalVisibility;
            set => SetField(ref _btnArrivalVisibility, value);
        }

        private bool _btnDepartureEnabled = false;
        public bool BtnDepartureEnabled
        {
            get => _btnDepartureEnabled;
            set => SetField(ref _btnDepartureEnabled, value);
        }

        private bool _btnArrivalEnabled = true;
        public bool BtnArrivalEnabled
        {
            get => _btnArrivalEnabled;
            set => SetField(ref _btnArrivalEnabled, value);
        }

        public ICommand DepArrToggleCommand { get; }

        public AtisViewModel(DispatcherQueue dispatcher)
        {
            AtisFetcher = new AtisFetchService();
            Dispatcher = dispatcher;
            AirportNames = new ObservableCollection<string>();
            FetchedAtisList = new List<Atis>();
            DepArrToggleCommand = new RelayCommand(DepArrToggle);
            StartUpdateLoop();
        }

        public async void StartUpdateLoop()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    List<Atis> newAtis = await AtisFetcher.GetAllAsync();
                    var tempFetchedAirports = new List<string>();

                    // Store all of the ATIS internally
                    FetchedAtisList.Clear();
                    foreach (var item in newAtis)
                    {
                        FetchedAtisList.Add(item);
                        if (!tempFetchedAirports.Contains(item.Airport))
                        {
                            tempFetchedAirports.Add(item.Airport);
                        }
                    }

                    // Update the airport list for dropdown on the UI Thread so that notifications are captured by UI
                    // But only do it if the list has changed
                    if (!AirportNames.SequenceEqual(tempFetchedAirports))
                    {
                        Dispatcher.TryEnqueue(() =>
                        {
                            AirportNames.Clear();
                            foreach (var airport in tempFetchedAirports)
                            {
                                AirportNames.Add(airport);
                            }
                            DropdownPlaceholderText = "Select an Airport";
                            DropdownEnabled = true;

                            // TODO: need to update the displayed texts here too, not just airport list

                        });
                    }

                    await Task.Delay(Constants.atisUpdateDelay);
                }
            });
        }

        private void UpdateVisibleAtis(string selectedAirport)
        {
            _selectedAirport = selectedAirport;

            var filteredAtis = new List<Atis>();
            foreach (var item in FetchedAtisList)
            {
                if (item.Airport == selectedAirport)
                {
                    filteredAtis.Add(item);
                }
            }

            if (filteredAtis.Count == 1)
            {               
                _currentCombinedAtisText = filteredAtis[0].Datis;
                AtisText = AtisTextBodyFromFullAtis(_currentCombinedAtisText);
                AtisTitle = AtisTitleFromFullAtis(_currentCombinedAtisText);

                // Hide Dep/Arr buttons
                BtnDepartureVisibility = false;
                BtnArrivalVisibility = false;
            }
            else if (filteredAtis.Count == 2)
            {
                // If we just changed to this airport, default to showing the Departure
                if (filteredAtis[0].Type == "departure")
                {
                    _currentCombinedAtisText = "";
                    _currentDepartureAtisText = filteredAtis[0].Datis;
                    _currentArrivalAtisText = filteredAtis[1].Datis;
                }
                else
                {
                    _currentCombinedAtisText = "";
                    _currentDepartureAtisText = filteredAtis[1].Datis;
                    _currentArrivalAtisText = filteredAtis[0].Datis;
                }

                AtisText = AtisTextBodyFromFullAtis(_currentDepartureAtisText);
                AtisTitle = AtisTitleFromFullAtis(_currentDepartureAtisText);

                // Show Dep/Arr buttons
                BtnDepartureVisibility = true;
                BtnArrivalVisibility = true;

                // Make Dep button disabled (selected) and Arr button enabled
                BtnDepartureEnabled = false;
                BtnArrivalEnabled = true;
            }
        }

        private void DepArrToggle()
        {
            // If Departure button is disabled, that means we're currently showing the departure ATIS, so change to arrival
            AtisText = !BtnDepartureEnabled ? AtisTextBodyFromFullAtis(_currentArrivalAtisText) : AtisTextBodyFromFullAtis(_currentDepartureAtisText);
            
            // Update ATIS title
            AtisTitle = !BtnDepartureEnabled ? AtisTitleFromFullAtis(_currentArrivalAtisText) : AtisTitleFromFullAtis(_currentDepartureAtisText);

            // Swap button states
            BtnDepartureEnabled = !BtnDepartureEnabled;
            BtnArrivalEnabled = !BtnArrivalEnabled;
        }

        private static String AtisTextBodyFromFullAtis(string fullAtisText)
        {
            var atisSplits = fullAtisText.Split(". ").ToList();
            var noTitle = string.Join(". ", atisSplits.GetRange(1, atisSplits.Count - 1));
            var regex = new Regex(Regex.Escape(". "));
            return regex.Replace(noTitle, ".\n\n", 1);
        }

        private static String AtisTitleFromFullAtis(string fullAtisText)
        {
            var atisSplits = fullAtisText.Split(". ").ToList();
            return atisSplits[0].Replace("ARR/DEP ", "");
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

    }
}
