// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DATIS.Models;
using DATIS.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DATIS
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        //private static AtisFetcher _atisFetcher;
        //private static List<Atis> _lastAtis;
        //private static Task _atisLoopTask;

        //public ObservableCollection<String>  Airports { get; private set; }

        AtisViewModel AtisViewModel { get; set; }
        
        public MainWindow()
        {
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            AtisViewModel = new AtisViewModel(dispatcherQueue);
            this.Title = "D-ATIS";
            this.InitializeComponent();
        }

        //async void StartAtisWorker() // Probably should add some cancellation tokens in this method?
        //{
        //    _atisFetcher = new AtisFetcher();
        //    _atisLoopTask = Task.Run(async () =>
        //    {
        //        while (true)
        //        {
        //            _lastAtis = await _atisFetcher.GetAllAsync();
        //            var newAirportList = new List<string>();
        //            Airports.Clear();
        //            foreach (var item in _lastAtis)
        //            {
        //                Airports.Add(item.Airport);
        //            }
        //            await Task.Delay(Constants.atisUpdateDelay);
        //        }
        //    });
        //}

    }

}
