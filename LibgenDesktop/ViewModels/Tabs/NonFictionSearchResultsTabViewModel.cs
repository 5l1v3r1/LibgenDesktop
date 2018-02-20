﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using LibgenDesktop.Infrastructure;
using LibgenDesktop.Models;
using LibgenDesktop.Models.Entities;
using LibgenDesktop.Models.Localization;
using LibgenDesktop.Models.Localization.Localizators;
using LibgenDesktop.Models.ProgressArgs;
using LibgenDesktop.ViewModels.EventArguments;
using LibgenDesktop.ViewModels.Panels;
using LibgenDesktop.ViewModels.SearchResultItems;
using static LibgenDesktop.Models.Settings.AppSettings;

namespace LibgenDesktop.ViewModels.Tabs
{
    internal class NonFictionSearchResultsTabViewModel : SearchResultsTabViewModel
    {
        private readonly NonFictionColumnSettings columnSettings;
        private ObservableCollection<NonFictionSearchResultItemViewModel> books;
        private NonFictionSearchResultsTabLocalizator localization;
        private string searchQuery;
        private string bookCount;
        private bool isBookGridVisible;
        private bool isSearchProgressPanelVisible;
        private string searchProgressStatus;
        private bool isStatusBarVisible;
        private bool isExportPanelVisible;

        public NonFictionSearchResultsTabViewModel(MainModel mainModel, IWindowContext parentWindowContext, string searchQuery,
            List<NonFictionBook> searchResults)
            : base(mainModel, parentWindowContext, searchQuery)
        {
            columnSettings = mainModel.AppSettings.NonFiction.Columns;
            this.searchQuery = searchQuery;
            LanguageFormatter formatter = MainModel.Localization.CurrentLanguage.Formatter;
            books = new ObservableCollection<NonFictionSearchResultItemViewModel>(searchResults.Select(book =>
                new NonFictionSearchResultItemViewModel(book, formatter)));
            ExportPanelViewModel = new ExportPanelViewModel(mainModel, LibgenObjectType.NON_FICTION_BOOK, parentWindowContext);
            ExportPanelViewModel.ClosePanel += CloseExportPanel;
            OpenDetailsCommand = new Command(param => OpenDetails((param as NonFictionSearchResultItemViewModel)?.Book));
            SearchCommand = new Command(Search);
            ExportCommand = new Command(ShowExportPanel);
            BookDataGridEnterKeyCommand = new Command(BookDataGridEnterKeyPressed);
            Initialize();
            mainModel.Localization.LanguageChanged += LocalizationLanguageChanged;
        }

        public NonFictionSearchResultsTabLocalizator Localization
        {
            get
            {
                return localization;
            }
            set
            {
                localization = value;
                NotifyPropertyChanged();
            }
        }

        public string SearchQuery
        {
            get
            {
                return searchQuery;
            }
            set
            {
                searchQuery = value;
                NotifyPropertyChanged();
                if (IsExportPanelVisible)
                {
                    ExportPanelViewModel.UpdateSearchQuery(value);
                }
            }
        }

        public ObservableCollection<NonFictionSearchResultItemViewModel> Books
        {
            get
            {
                return books;
            }
            set
            {
                books = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsBookGridVisible
        {
            get
            {
                return isBookGridVisible;
            }
            set
            {
                isBookGridVisible = value;
                NotifyPropertyChanged();
            }
        }

        public int TitleColumnWidth
        {
            get
            {
                return columnSettings.TitleColumnWidth;
            }
            set
            {
                columnSettings.TitleColumnWidth = value;
            }
        }

        public int AuthorsColumnWidth
        {
            get
            {
                return columnSettings.AuthorsColumnWidth;
            }
            set
            {
                columnSettings.AuthorsColumnWidth = value;
            }
        }

        public int SeriesColumnWidth
        {
            get
            {
                return columnSettings.SeriesColumnWidth;
            }
            set
            {
                columnSettings.SeriesColumnWidth = value;
            }
        }

        public int YearColumnWidth
        {
            get
            {
                return columnSettings.YearColumnWidth;
            }
            set
            {
                columnSettings.YearColumnWidth = value;
            }
        }

        public int PublisherColumnWidth
        {
            get
            {
                return columnSettings.PublisherColumnWidth;
            }
            set
            {
                columnSettings.PublisherColumnWidth = value;
            }
        }

        public int FormatColumnWidth
        {
            get
            {
                return columnSettings.FormatColumnWidth;
            }
            set
            {
                columnSettings.FormatColumnWidth = value;
            }
        }

        public int FileSizeColumnWidth
        {
            get
            {
                return columnSettings.FileSizeColumnWidth;
            }
            set
            {
                columnSettings.FileSizeColumnWidth = value;
            }
        }

        public int OcrColumnWidth
        {
            get
            {
                return columnSettings.OcrColumnWidth;
            }
            set
            {
                columnSettings.OcrColumnWidth = value;
            }
        }

        public bool IsSearchProgressPanelVisible
        {
            get
            {
                return isSearchProgressPanelVisible;
            }
            set
            {
                isSearchProgressPanelVisible = value;
                NotifyPropertyChanged();
            }
        }

        public string SearchProgressStatus
        {
            get
            {
                return searchProgressStatus;
            }
            set
            {
                searchProgressStatus = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsStatusBarVisible
        {
            get
            {
                return isStatusBarVisible;
            }
            set
            {
                isStatusBarVisible = value;
                NotifyPropertyChanged();
            }
        }

        public string BookCount
        {
            get
            {
                return bookCount;
            }
            set
            {
                bookCount = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsExportPanelVisible
        {
            get
            {
                return isExportPanelVisible;
            }
            set
            {
                isExportPanelVisible = value;
                NotifyPropertyChanged();
            }
        }

        public ExportPanelViewModel ExportPanelViewModel { get; }

        public NonFictionSearchResultItemViewModel SelectedBook { get; set; }

        public Command OpenDetailsCommand { get; }
        public Command SearchCommand { get; }
        public Command ExportCommand { get; }
        public Command BookDataGridEnterKeyCommand { get; }

        public event EventHandler<OpenNonFictionDetailsEventArgs> OpenNonFictionDetailsRequested;

        public override void ShowExportPanel()
        {
            if (IsBookGridVisible)
            {
                IsBookGridVisible = false;
                IsStatusBarVisible = false;
                IsExportPanelVisible = true;
                ExportPanelViewModel.ShowPanel(SearchQuery);
            }
        }

        public override void HandleTabClosing()
        {
            MainModel.Localization.LanguageChanged -= LocalizationLanguageChanged;
        }

        private void Initialize()
        {
            localization = MainModel.Localization.CurrentLanguage.NonFictionSearchResultsTab;
            isBookGridVisible = true;
            isStatusBarVisible = true;
            isSearchProgressPanelVisible = false;
            isExportPanelVisible = false;
            UpdateBookCount();
            Events.RaiseEvent(ViewModelEvent.RegisteredEventId.FOCUS_SEARCH_TEXT_BOX);
        }

        private void UpdateBookCount()
        {
            BookCount = Localization.GetStatusBarText(Books.Count);
        }

        private void BookDataGridEnterKeyPressed()
        {
            OpenDetails(SelectedBook.Book);
        }

        private void OpenDetails(NonFictionBook book)
        {
            OpenNonFictionDetailsRequested?.Invoke(this, new OpenNonFictionDetailsEventArgs(book));
        }

        private async void Search()
        {
            if (!String.IsNullOrWhiteSpace(SearchQuery) && !IsSearchProgressPanelVisible && !IsExportPanelVisible)
            {
                Title = SearchQuery;
                IsBookGridVisible = false;
                IsStatusBarVisible = false;
                IsSearchProgressPanelVisible = true;
                UpdateSearchProgressStatus(0);
                Progress<SearchProgress> searchProgressHandler = new Progress<SearchProgress>(HandleSearchProgress);
                CancellationToken cancellationToken = new CancellationToken();
                List<NonFictionBook> result = new List<NonFictionBook>();
                try
                {
                    result = await MainModel.SearchNonFictionAsync(SearchQuery, searchProgressHandler, cancellationToken);
                }
                catch (Exception exception)
                {
                    ShowErrorWindow(exception, ParentWindowContext);
                }
                LanguageFormatter formatter = MainModel.Localization.CurrentLanguage.Formatter;
                Books = new ObservableCollection<NonFictionSearchResultItemViewModel>(result.Select(book =>
                    new NonFictionSearchResultItemViewModel(book, formatter)));
                UpdateBookCount();
                IsSearchProgressPanelVisible = false;
                IsBookGridVisible = true;
                IsStatusBarVisible = true;
            }
        }

        private void HandleSearchProgress(SearchProgress searchProgress)
        {
            UpdateSearchProgressStatus(searchProgress.ItemsFound);
        }

        private void UpdateSearchProgressStatus(int booksFound)
        {
            SearchProgressStatus = Localization.GetSearchProgressText(booksFound);
        }

        private void CloseExportPanel(object sender, EventArgs e)
        {
            IsExportPanelVisible = false;
            IsBookGridVisible = true;
            IsStatusBarVisible = true;
        }

        private void LocalizationLanguageChanged(object sender, EventArgs e)
        {
            Language newLanguage = MainModel.Localization.CurrentLanguage;
            Localization = newLanguage.NonFictionSearchResultsTab;
            UpdateBookCount();
            LanguageFormatter newFormatter = newLanguage.Formatter;
            foreach (NonFictionSearchResultItemViewModel book in Books)
            {
                book.UpdateLocalization(newFormatter);
            }
            ExportPanelViewModel.UpdateLocalization(newLanguage);
        }
    }
}
