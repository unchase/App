﻿namespace XamarinEvolve.Clients.UI
{
    using DotNetRu.DataStore.Audit.DataObjects;

    using Xamarin.Forms;

    using XamarinEvolve.Clients.Portable;

    public partial class SpeakersPage
    {
        public override AppPage PageType => AppPage.Speakers;

        SpeakersViewModel speakersViewModel;

        SpeakersViewModel ViewModel => this.speakersViewModel ?? (this.speakersViewModel = this.BindingContext as SpeakersViewModel);

        public SpeakersPage()
        {
            this.InitializeComponent();
            this.BindingContext = new SpeakersViewModel(this.Navigation);

            if (Device.RuntimePlatform == Device.Android)
            {
                this.ListViewSpeakers.Effects.Add(Effect.Resolve("Xpirit.ListViewSelectionOnTopEffect"));
            }

            this.ListViewSpeakers.ItemSelected += async (sender, e) =>
                {
                    if (!(this.ListViewSpeakers.SelectedItem is Speaker speaker))
                    {
                        return;
                    }

                    ContentPage destination;

                    if (Device.RuntimePlatform == Device.UWP)
                    {
                        destination = new SpeakerDetailsPageUWP(speaker);
                    }
                    else
                    {
                        destination = new SpeakerDetailsPage(speaker);
                    }

                    await NavigationService.PushAsync(this.Navigation, destination);
                    this.ListViewSpeakers.SelectedItem = null;
                };
        }

        void ListViewTapped(object sender, ItemTappedEventArgs e)
        {
            if (!(sender is ListView list))
            {
                return;
            }

            list.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            this.ListViewSpeakers.ItemTapped += this.ListViewTapped;
            if (this.ViewModel.Speakers?.Count == 0)
            {
                this.ViewModel.LoadSpeakersCommand.Execute(true);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            this.ListViewSpeakers.ItemTapped -= this.ListViewTapped;
        }
    }
}