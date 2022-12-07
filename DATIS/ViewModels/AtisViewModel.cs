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
using CommunityToolkit.Mvvm.ComponentModel;

namespace DATIS.ViewModels
{
    public partial class AtisViewModel : ObservableObject
    {
        public DispatcherQueue Dispatcher { get; set; }
        public AtisFetchService AtisFetcher { get; set; }
        public ObservableCollection<string> AirportNames { get; private set; }
        
        private List<Atis> FetchedAtisList { get; set; }
        private string _currentCombinedAtisText = "";
        private string _currentDepartureAtisText = "";
        private string _currentArrivalAtisText = "";

        [ObservableProperty]
        private string dropdownPlaceholderText = "Loading airports...";

        [ObservableProperty]
        private bool dropdownEnabled = false;

        [ObservableProperty]
        private string atisText = "";

        [ObservableProperty]
        private string atisTitle = "D-ATIS";

        [ObservableProperty]
        private bool btnDepartureVisibility = false;

        [ObservableProperty]
        private bool btnArrivalVisibility = false;

        [ObservableProperty]
        private bool btnDepartureEnabled = false;

        [ObservableProperty]
        private bool btnArrivalEnabled = true;

        private string _selectedAirport;
        public string SelectedAirport
        {
            get => _selectedAirport;
            set => UpdateVisibleAtis(value);
        }

        public AtisViewModel(DispatcherQueue dispatcher)
        {
            AtisFetcher = new AtisFetchService();
            Dispatcher = dispatcher;
            AirportNames = new ObservableCollection<string>();
            FetchedAtisList = new List<Atis>();
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

                    // If the list of airports has changed, update the dropdown on the UI Thread
                    // so that notifications are captured by UI
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
                        });
                    }
                    else
                    {
                        // If the list of airports hasn't changed, we still need to update the displayed
                        // ATIS title and text in case that has changed
                        Dispatcher.TryEnqueue(() => { UpdateVisibleAtis(); });
                    }

                    await Task.Delay(Constants.atisUpdateDelay);
                }
            });
        }

        private void UpdateVisibleAtis(string? selectedAirport = null)
        {
            _selectedAirport = selectedAirport != null ? selectedAirport : _selectedAirport;

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

        [RelayCommand]
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
    }
}
