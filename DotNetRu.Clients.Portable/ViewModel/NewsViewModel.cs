﻿namespace XamarinEvolve.Clients.Portable.ViewModel
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using DotNetRu.DataStore.Audit.Models;

    using FormsToolkit;

    using MvvmHelpers;

    using Xamarin.Forms;

    using XamarinEvolve.Utils;
    using XamarinEvolve.Utils.Helpers;

    /// <inheritdoc />
    /// <summary>
    /// The feed view model.
    /// </summary>
    public class NewsViewModel : ViewModelBase
    {
        public ObservableRangeCollection<Tweet> Tweets { get; } = new ObservableRangeCollection<Tweet>();

        public ObservableRangeCollection<TalkModel> Sessions { get; } = new ObservableRangeCollection<TalkModel>();

        public DateTime NextForceRefresh { get; set; }

        public NewsViewModel()
        {
            this.Title = "News";
            this.NextForceRefresh = DateTime.UtcNow.AddMinutes(45);

            MessagingService.Current.Subscribe(
                "conferencefeedback_finished",
                (m) => { Device.BeginInvokeOnMainThread(this.EvaluateVisualState); });
        }

        private ICommand refreshCommand;

        public ICommand RefreshCommand => this.refreshCommand
                                          ?? (this.refreshCommand = new Command(
                                                  async () => await this.ExecuteRefreshCommandAsync()));

        async Task ExecuteRefreshCommandAsync()
        {
            try
            {
                this.NextForceRefresh = DateTime.UtcNow.AddMinutes(45);
                this.IsBusy = true;
                var tasks = new[]
                                {
                                    this.ExecuteLoadNotificationsCommandAsync(), this.ExecuteLoadSocialCommandAsync(),
                                    this.ExecuteLoadSessionsCommandAsync()
                                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                ex.Data["method"] = "ExecuteRefreshCommandAsync";
                this.Logger.Report(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        Notification notification;

        public Notification Notification
        {
            get => this.notification;
            set => this.SetProperty(ref this.notification, value);
        }

        bool loadingNotifications;

        public bool LoadingNotifications
        {
            get => this.loadingNotifications;
            set => this.SetProperty(ref this.loadingNotifications, value);
        }

        ICommand loadNotificationsCommand;

        public ICommand LoadNotificationsCommand => this.loadNotificationsCommand
                                                    ?? (this.loadNotificationsCommand = new Command(
                                                            async () =>
                                                                await this.ExecuteLoadNotificationsCommandAsync()));

        async Task ExecuteLoadNotificationsCommandAsync()
        {
            if (this.LoadingNotifications) return;
            this.LoadingNotifications = true;
            try
            {
                this.Notification = await this.StoreManager.NotificationStore.GetLatestNotification();
            }
            catch (Exception ex)
            {
                ex.Data["method"] = "ExecuteLoadNotificationsCommandAsync";
                this.Logger.Report(ex);
            }
            finally
            {
                this.LoadingNotifications = false;
            }
        }

        bool loadingSessions;

        public bool LoadingSessions
        {
            get => this.loadingSessions;
            set => this.SetProperty(ref this.loadingSessions, value);
        }

        ICommand loadSessionsCommand;

        public ICommand LoadSessionsCommand => this.loadSessionsCommand
                                               ?? (this.loadSessionsCommand = new Command(
                                                       async () => await this.ExecuteLoadSessionsCommandAsync()));

        private async Task ExecuteLoadSessionsCommandAsync()
        {
            if (this.LoadingSessions) return;

            this.LoadingSessions = true;

            try
            {
                this.NoSessions = false;
                this.Sessions.Clear();
                this.OnPropertyChanged("Sessions");

                //var sessions = await this.StoreManager.SessionStore.GetNextSessions(2);
                //if (sessions != null) this.Sessions.AddRange(sessions);

                this.NoSessions = this.Sessions.Count == 0;
            }
            catch (Exception ex)
            {
                ex.Data["method"] = "ExecuteLoadSessionsCommandAsync";
                this.Logger.Report(ex);
                this.NoSessions = true;
            }
            finally
            {
                this.LoadingSessions = false;
            }
        }

        public void EvaluateVisualState()
        {
            this.OnPropertyChanged(nameof(this.ShowBuyTicketButton));
            this.OnPropertyChanged(nameof(this.ShowConferenceFeedbackButton));
        }

        bool noSessions;

        public bool NoSessions
        {
            get => this.noSessions;
            set => this.SetProperty(ref this.noSessions, value);
        }

        TalkModel selectedTalkModel;

        public TalkModel SelectedTalkModel
        {
            get => this.selectedTalkModel;
            set
            {
                this.selectedTalkModel = value;
                this.OnPropertyChanged();
                if (this.selectedTalkModel == null) return;

                MessagingService.Current.SendMessage(MessageKeys.NavigateToSession, this.selectedTalkModel);

                this.SelectedTalkModel = null;
            }
        }

        bool loadingSocial;

        public bool LoadingSocial
        {
            get => this.loadingSocial;
            set => this.SetProperty(ref this.loadingSocial, value);
        }

        public bool ShowBuyTicketButton =>
            FeatureFlags.ShowBuyTicketButton && EventInfo.StartOfConference.AddDays(-1) >= DateTime.Now;

        public bool ShowConferenceFeedbackButton => FeatureFlags.ShowConferenceFeedbackButton;

        public string SocialHeader => "Social";

        ICommand buyTicketNowCommand;

        public ICommand BuyTicketNowCommand => this.buyTicketNowCommand
                                               ?? (this.buyTicketNowCommand =
                                                       new Command(this.ExecuteBuyTicketNowCommand));

        void ExecuteBuyTicketNowCommand()
        {
            this.LaunchBrowserCommand.Execute(EventInfo.TicketUrl);
        }

        ICommand showConferenceFeedbackCommand;

        public ICommand ShowConferenceFeedbackCommand => this.showConferenceFeedbackCommand
                                                         ?? (this.showConferenceFeedbackCommand = new Command(
                                                                 this.ExecuteShowConferenceFeedbackCommand));

        void ExecuteShowConferenceFeedbackCommand()
        {
            MessagingService.Current.SendMessage(MessageKeys.NavigateToConferenceFeedback);
        }

        ICommand loadSocialCommand;

        public ICommand LoadSocialCommand => this.loadSocialCommand
                                             ?? (this.loadSocialCommand = new Command(
                                                     async () => await this.ExecuteLoadSocialCommandAsync()));

        async Task ExecuteLoadSocialCommandAsync()
        {
            if (this.LoadingSocial) return;

            this.LoadingSocial = true;
            try
            {
                this.SocialError = false;
                this.Tweets.Clear();
                this.Tweets.ReplaceRange(await TweetHelper.Get());
            }
            catch (Exception ex)
            {
                this.SocialError = true;
                ex.Data["method"] = "ExecuteLoadSocialCommandAsync";
                this.Logger.Report(ex);
            }
            finally
            {
                this.LoadingSocial = false;
            }
        }

        bool socialError;

        public bool SocialError
        {
            get => this.socialError;
            set => this.SetProperty(ref this.socialError, value);
        }

        Tweet selectedTweet;

        public Tweet SelectedTweet
        {
            get => this.selectedTweet;
            set
            {
                this.selectedTweet = value;
                this.OnPropertyChanged();
                if (this.selectedTweet == null) return;

                this.LaunchBrowserCommand.Execute(this.selectedTweet.Url);

                this.SelectedTweet = null;
            }
        }
    }
}
