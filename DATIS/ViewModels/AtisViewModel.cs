using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DATIS.Services;
using DATIS.Models;
using Microsoft.UI.Dispatching;
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
        private Dictionary<string, Airport> AirportDictionary { get; set; }

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

        [ObservableProperty]
        private string selectedAirport;

        public AtisViewModel(DispatcherQueue dispatcher)
        {
            AtisFetcher = new AtisFetchService();
            Dispatcher = dispatcher;
            AirportNames = new ObservableCollection<string>();
            StartUpdateLoop();
        }

        public async void StartUpdateLoop()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    // Fetch all the ATIS and build new lookup dict
                    List<Atis> newAtis = await AtisFetcher.GetAllAsync();
                    Dictionary<string, Airport> newAirportDict = new();
                    foreach (var atis in newAtis)
                    {
                        if (newAirportDict.ContainsKey(atis.Airport))
                        {
                            newAirportDict[atis.Airport].AtisList.Add(atis);
                        }
                        else
                        {
                            var newAirport = new Airport(atis.Airport, new List<Atis> { atis });
                            newAirportDict.Add(newAirport.Name, newAirport);
                        }
                    }

                    // Store new fetched dictionary as a class member
                    AirportDictionary = newAirportDict;

                    // If the list of airports has changed, update the dropdown on the UI Thread
                    // so that notifications are captured by UI. Using hash sets to be order invariant
                    var existingAirportsHash = new HashSet<string>(AirportNames);
                    var newAirportsHash = new HashSet<string>(newAirportDict.Keys);
                    if (!existingAirportsHash.SetEquals(newAirportsHash))
                    {
                        Dispatcher.TryEnqueue(() =>
                        {
                            AirportNames.Clear();
                            foreach (KeyValuePair<string, Airport> airport in AirportDictionary)
                            {
                                AirportNames.Add(airport.Key);
                            }
                            DropdownPlaceholderText = "Select an Airport";
                            DropdownEnabled = true;
                        });
                    }

                    // Update the visible ATIS on the main UI thread every time we get knew data, in case it changed
                    Dispatcher.TryEnqueue(() => { UpdateVisibleAtis(); });

                    // Wait to loop with some delay (default 60 seconds)
                    await Task.Delay(Constants.atisUpdateDelay);
                }
            });
        }

        partial void OnSelectedAirportChanged(string value)
        {
            UpdateVisibleAtis(value);
        }

        private void UpdateVisibleAtis(string? selectedAirport = null)
        {
            // Do nothing if we haven't selected any airport now or in the past
            if (SelectedAirport is null && selectedAirport is null)
            {
                return;
            }

            // Find the ATISes for the selected airport
            List<Atis> atisList = AirportDictionary[SelectedAirport].AtisList;


            if (atisList.Count == 1)
            {               
                AtisText = AtisTextBodyFromFullAtis(atisList[0].Text);
                AtisTitle = AtisTitleFromFullAtis(atisList[0].Text);

                // Hide Dep/Arr buttons
                BtnDepartureVisibility = false;
                BtnArrivalVisibility = false;
            }
            else if (atisList.Count == 2)
            {   
                // If we just changed to this airport, default to showing the Departure
                string fullText = AirportDictionary[SelectedAirport].AtisList
                    .Where(a => a.Type == AtisType.Departure)
                    .First()
                    .Text;

                AtisText = AtisTextBodyFromFullAtis(fullText);
                AtisTitle = AtisTitleFromFullAtis(fullText);

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
            var depFullText = AirportDictionary[SelectedAirport].AtisList
                .Where(a => a.Type == AtisType.Departure)
                .First()
                .Text;

            var arrFullText = AirportDictionary[SelectedAirport].AtisList
                .Where(a => a.Type == AtisType.Arrival)
                .First()
                .Text;

            // If Departure button is disabled, that means we're currently showing the departure ATIS, so change to arrival
            AtisText = !BtnDepartureEnabled ? AtisTextBodyFromFullAtis(arrFullText) : AtisTextBodyFromFullAtis(depFullText);
            
            // Update ATIS title
            AtisTitle = !BtnDepartureEnabled ? AtisTitleFromFullAtis(arrFullText) : AtisTitleFromFullAtis(depFullText);

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
