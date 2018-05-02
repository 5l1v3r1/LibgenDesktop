﻿using System;
using System.Threading;
using System.Threading.Tasks;
using LibgenDesktop.Infrastructure;
using LibgenDesktop.Models;
using LibgenDesktop.Models.Entities;
using LibgenDesktop.Models.Localization.Localizators;
using LibgenDesktop.ViewModels.Panels;

namespace LibgenDesktop.ViewModels.Tabs
{
    internal abstract class SearchResultsTabViewModel : TabViewModel
    {
        private CancellationTokenSource searchCancellationTokenSource;
        private string searchQuery;
        private LibgenObjectType libgenObjectType;
        private string lastExecutedSearchQuery;
        private bool isBookmarkSet;
        private bool isExportPanelVisible;
        private bool isSearchProgressPanelVisible;
        private string interruptButtonText;
        private bool isInterruptButtonEnabled;

        protected SearchResultsTabViewModel(MainModel mainModel, IWindowContext parentWindowContext, LibgenObjectType libgenObjectType, string searchQuery)
            : base(mainModel, parentWindowContext, searchQuery)
        {
            this.libgenObjectType = libgenObjectType;
            this.searchQuery = searchQuery;
            lastExecutedSearchQuery = searchQuery;
            UpdateBookmarkedState();
            isExportPanelVisible = false;
            isSearchProgressPanelVisible = false;
            ExportPanelViewModel = new ExportPanelViewModel(mainModel, libgenObjectType, parentWindowContext);
            SearchCommand = new Command(Search);
            InterruptSearchCommand = new Command(InterruptSearch);
            ToggleBookmarkCommand = new Command(ToggleBookmark);
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

        public bool IsBookmarkSet
        {
            get
            {
                return isBookmarkSet;
            }
            set
            {
                isBookmarkSet = value;
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

        public string InterruptButtonText
        {
            get
            {
                return interruptButtonText;
            }
            set
            {
                interruptButtonText = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsInterruptButtonEnabled
        {
            get
            {
                return isInterruptButtonEnabled;
            }
            set
            {
                isInterruptButtonEnabled = value;
                NotifyPropertyChanged();
            }
        }

        public ExportPanelViewModel ExportPanelViewModel { get; }

        public Command SearchCommand { get; }
        public Command InterruptSearchCommand { get; }
        public Command ToggleBookmarkCommand { get; }

        private SearchResultsTabLocalizator Localization
        {
            get
            {
                return GetLocalization();
            }
        }

        public void Search(string searchQuery)
        {
            SearchQuery = searchQuery;
            Search();
        }

        public abstract void ShowExportPanel();
        protected abstract SearchResultsTabLocalizator GetLocalization();
        protected abstract Task SearchAsync(string searchQuery, CancellationToken cancellationToken);

        private async void Search()
        {
            if (!String.IsNullOrWhiteSpace(SearchQuery) && !IsSearchProgressPanelVisible && !IsExportPanelVisible)
            {
                Title = SearchQuery;
                lastExecutedSearchQuery = SearchQuery;
                UpdateBookmarkedState();
                InterruptButtonText = Localization.Interrupt;
                IsInterruptButtonEnabled = true;
                IsSearchProgressPanelVisible = true;
                searchCancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = searchCancellationTokenSource.Token;
                await SearchAsync(SearchQuery, cancellationToken);
                IsSearchProgressPanelVisible = false;
            }
        }

        private void InterruptSearch()
        {
            searchCancellationTokenSource.Cancel();
            InterruptButtonText = Localization.Interrupting;
            IsInterruptButtonEnabled = false;
        }

        private void UpdateBookmarkedState()
        {
            IsBookmarkSet = MainModel.HasBookmark(libgenObjectType, lastExecutedSearchQuery);
        }

        private void ToggleBookmark()
        {
            if (isBookmarkSet)
            {
                MainModel.DeleteBookmark(libgenObjectType, lastExecutedSearchQuery);
            }
            else
            {
                MainModel.AddBookmark(libgenObjectType, lastExecutedSearchQuery, lastExecutedSearchQuery);
            }
            UpdateBookmarkedState();
        }
    }
}
