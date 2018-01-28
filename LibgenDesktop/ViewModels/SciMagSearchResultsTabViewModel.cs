﻿using System;
using System.Collections.ObjectModel;
using System.Threading;
using LibgenDesktop.Infrastructure;
using LibgenDesktop.Models;
using LibgenDesktop.Models.Entities;
using LibgenDesktop.Models.ProgressArgs;
using LibgenDesktop.Models.Utils;
using static LibgenDesktop.Models.Settings.AppSettings;

namespace LibgenDesktop.ViewModels
{
    internal class SciMagSearchResultsTabViewModel : SearchResultsTabViewModel
    {
        internal class OpenSciMagDetailsEventArgs : EventArgs
        {
            public OpenSciMagDetailsEventArgs(SciMagArticle sciMagArticle)
            {
                SciMagArticle = sciMagArticle;
            }

            public SciMagArticle SciMagArticle { get; }
        }

        private readonly SciMagColumnSettings columnSettings;
        private ObservableCollection<SciMagArticle> articles;
        private string searchQuery;
        private string articleCount;
        private bool isArticleGridVisible;
        private bool isSearchProgressPanelVisible;
        private string searchProgressStatus;
        private bool isStatusBarVisible;
        private bool isExportPanelVisible;

        public SciMagSearchResultsTabViewModel(MainModel mainModel, IWindowContext parentWindowContext, string searchQuery,
            ObservableCollection<SciMagArticle> searchResults)
            : base(mainModel, parentWindowContext, searchQuery)
        {
            columnSettings = mainModel.AppSettings.SciMag.Columns;
            this.searchQuery = searchQuery;
            articles = searchResults;
            ExportPanelViewModel = new ExportPanelViewModel(mainModel, LibgenObjectType.SCIMAG_ARTICLE, parentWindowContext);
            ExportPanelViewModel.ClosePanel += CloseExportPanel;
            OpenDetailsCommand = new Command(param => OpenDetails(param as SciMagArticle));
            SearchCommand = new Command(Search);
            ExportCommand = new Command(ShowExportPanel);
            ArticleDataGridEnterKeyCommand = new Command(ArticleDataGridEnterKeyPressed);
            Initialize();
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

        public ObservableCollection<SciMagArticle> Articles
        {
            get
            {
                return articles;
            }
            set
            {
                articles = value;
                NotifyPropertyChanged();
            }
        }

        public bool IsArticleGridVisible
        {
            get
            {
                return isArticleGridVisible;
            }
            set
            {
                isArticleGridVisible = value;
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

        public int JournalColumnWidth
        {
            get
            {
                return columnSettings.JournalColumnWidth;
            }
            set
            {
                columnSettings.JournalColumnWidth = value;
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

        public int DoiColumnWidth
        {
            get
            {
                return columnSettings.DoiColumnWidth;
            }
            set
            {
                columnSettings.DoiColumnWidth = value;
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

        public string ArticleCount
        {
            get
            {
                return articleCount;
            }
            set
            {
                articleCount = value;
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

        public SciMagArticle SelectedArticle { get; set; }

        public Command OpenDetailsCommand { get; }
        public Command SearchCommand { get; }
        public Command ExportCommand { get; }
        public Command ArticleDataGridEnterKeyCommand { get; }

        public event EventHandler<OpenSciMagDetailsEventArgs> OpenSciMagDetailsRequested;

        public override void ShowExportPanel()
        {
            if (IsArticleGridVisible)
            {
                IsArticleGridVisible = false;
                IsStatusBarVisible = false;
                IsExportPanelVisible = true;
                ExportPanelViewModel.ShowPanel(SearchQuery);
            }
        }

        private void Initialize()
        {
            isArticleGridVisible = true;
            isStatusBarVisible = true;
            isSearchProgressPanelVisible = false;
            isExportPanelVisible = false;
            UpdateArticleCount();
            Events.RaiseEvent(ViewModelEvent.RegisteredEventId.FOCUS_SEARCH_TEXT_BOX);
        }

        private void UpdateArticleCount()
        {
            ArticleCount = $"Найдено статей: {Articles.Count.ToFormattedString()}";
        }

        private void ArticleDataGridEnterKeyPressed()
        {
            OpenDetails(SelectedArticle);
        }

        private void OpenDetails(SciMagArticle article)
        {
            OpenSciMagDetailsRequested?.Invoke(this, new OpenSciMagDetailsEventArgs(article));
        }

        private async void Search()
        {
            if (!String.IsNullOrWhiteSpace(SearchQuery) && !IsSearchProgressPanelVisible && !IsExportPanelVisible)
            {
                Title = SearchQuery;
                IsArticleGridVisible = false;
                IsStatusBarVisible = false;
                IsSearchProgressPanelVisible = true;
                UpdateSearchProgressStatus(0);
                Progress<SearchProgress> searchProgressHandler = new Progress<SearchProgress>(HandleSearchProgress);
                CancellationToken cancellationToken = new CancellationToken();
                ObservableCollection<SciMagArticle> result = new ObservableCollection<SciMagArticle>();
                try
                {
                    result = await MainModel.SearchSciMagAsync(SearchQuery, searchProgressHandler, cancellationToken);
                }
                catch (Exception exception)
                {
                    ShowErrorWindow(exception, ParentWindowContext);
                }
                Articles = result;
                UpdateArticleCount();
                IsSearchProgressPanelVisible = false;
                IsArticleGridVisible = true;
                IsStatusBarVisible = true;
            }
        }

        private void HandleSearchProgress(SearchProgress searchProgress)
        {
            UpdateSearchProgressStatus(searchProgress.ItemsFound);
        }

        private void UpdateSearchProgressStatus(int articlesFound)
        {
            SearchProgressStatus = $"Найдено статей: {articlesFound.ToFormattedString()}";
        }

        private void CloseExportPanel(object sender, EventArgs e)
        {
            IsExportPanelVisible = false;
            IsArticleGridVisible = true;
            IsStatusBarVisible = true;
        }
    }
}
