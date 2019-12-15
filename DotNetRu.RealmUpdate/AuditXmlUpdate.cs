using System.Collections.Generic;
using System.Linq;
using DotNetRu.Models.XML;
using MoreLinq;

namespace DotNetRu.RealmUpdate
{
    public class AuditXmlUpdate
    {
        public IEnumerable<CommunityEntity> Communities { get; set; }

        public IEnumerable<FriendEntity> Friends { get; set; }

        public IEnumerable<MeetupEntity> Meetups { get; set; }

        public IEnumerable<SpeakerEntity> Speakers { get; set; }

        public IEnumerable<TalkEntity> Talks { get; set; }

        public IEnumerable<VenueEntity> Venues { get; set; }

        public AuditXmlUpdate Merge(AuditXmlUpdate other)
        {
            return new AuditXmlUpdate
            {
                Communities = this.Communities.Concat(other.Communities).DistinctBy(x => x.Name),
                Friends = this.Friends.Concat(other.Friends).DistinctBy(x => x.Id),
                Meetups = this.Meetups.Concat(other.Meetups).DistinctBy(x => x.Id),
                Speakers = this.Speakers.Concat(other.Speakers).DistinctBy(x => x.Id),
                Talks = this.Talks.Concat(other.Talks).DistinctBy(x => x.Id),
                Venues = this.Venues.Concat(other.Venues).DistinctBy(x => x.Id)
            };
        }
    }
}
