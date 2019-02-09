namespace DotNetRu.DataStore.Audit.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using DotNetRu.DataStore.Audit.Services;
    using DotNetRu.Utils.Helpers;
    using Xamarin.Forms;

    public class TalkModel : IIdentified
    {
        private const string Delimiter = "|";

        private readonly IList<string> seeAlsoTalksIds;        

        private string haystack;

        public TalkModel(IList<string> seeAlsoTalksIds)
        {
            this.seeAlsoTalksIds = seeAlsoTalksIds == null ? new List<string>() : new List<string>(seeAlsoTalksIds);

            this.Speakers = new List<SpeakerModel>();
            this.Categories = new List<Category>();
        }

        public string Id { get; set; }

        public string Title { get; set; }

        public string ShortTitle { get; set; }

        public string Abstract { get; set; }

        public IEnumerable<SpeakerModel> Speakers { get; set; }

        public IEnumerable<TalkModel> SeeAlsoTalks => CachedModelsProvider<TalkModel>.Get(seeAlsoTalksIds);        

        [Obsolete("Room and other location&time info should move to session")]
        public Room Room { get; set; }

        public ICollection<Category> Categories { get; set; }

        [Obsolete("Room and other location&time info should move to session")]
        public DateTime? StartTime => DateTime.Now; // this.Sessions.First().StartTime.DateTime;

        [Obsolete("Room and other location&time info should move to session")]
        public DateTime? EndTime => DateTime.Now; //this.Sessions.First().EndTime.DateTime;

        public string PresentationUrl { get; set; }

        public string VideoUrl { get; set; }

        public string CodeUrl { get; set; }

        public string Haystack
        {
            get
            {
                if (this.haystack != null)
                {
                    return this.haystack;
                }

                var builder = new StringBuilder();
                builder.Append(Delimiter);
                builder.Append(this.Title);
                builder.Append(Delimiter);
                if (this.Categories != null)
                {
                    foreach (var c in this.Categories)
                    {
                        builder.Append($"{c.Name}{Delimiter}{c.ShortName}{Delimiter}");
                    }
                }

                if (this.Speakers != null)
                {
                    foreach (var p in this.Speakers)
                    {
                        builder.Append(
                            $"{p.FirstName} {p.LastName}{Delimiter}{p.FirstName}{Delimiter}{p.LastName}{Delimiter}");
                    }
                }

                this.haystack = builder.ToString();
                return this.haystack;
            }
        }

        public byte[] SpeakerAvatar => this.Speakers.Count() > 1
                                                ? ResourceHelper.ExtractResource(
                                                    "DotNetRu.DataStore.Audit.Storage.SeveralSpeakers.png")
                                                : this.Speakers.Single().AvatarSmall;

        public string SpeakerNames => string.Join(",", this.Speakers.Select(x => x.FullName));

        public ImageSource CommunityLogo => ImageSource.FromResource(
           "DotNetRu.DataStore.Audit.Images.logos." + this.Sessions.First().Meetup.CommunityID + ".png");

        public IEnumerable<SessionModel> Sessions { get; set; }
    }
}
