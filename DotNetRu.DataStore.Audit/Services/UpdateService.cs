namespace DotNetRu.DataStore.Audit.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using AutoMapper;
    using DotNetRu.Clients.UI;
    using DotNetRu.DataStore.Audit.RealmModels;
    using DotNetRu.Utils;
    using DotNetRu.Utils.Interfaces;
    using Flurl;
    using Flurl.Http;
    using PushNotifications;
    using RealmGenerator.Entities;    
    using Realms;
    using Xamarin.Forms;

    public static class UpdateService
    {
        public static async Task<UpdateResults> UpdateAudit()
        {
            var result = UpdateResults.None;
            try
            {
                var logger = DependencyService.Get<ILogger>();

                logger.Track("AuditUpdate. Started updating audit");

                string currentCommitSha;
                using (var auditRealm = Realm.GetInstance("Audit.realm"))
                {
                    var auditVersion = auditRealm.All<AuditVersion>().Single();
                    currentCommitSha = auditVersion.CommitHash;
                }

                var config = AppConfig.GetConfig();

                var updateContent = await config.UpdateFunctionURL
                    .SetQueryParam("fromCommitSha", currentCommitSha)
                    .GetJsonAsync<UpdateContent>();

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                using (var auditRealm = Realm.GetInstance("Audit.realm"))
                {
                    using (var trans = auditRealm.BeginWrite())
                    {
                        var mapper = GetAutoMapper(auditRealm);

                        UpdateModels(mapper, auditRealm, updateContent.Speakers);
                        UpdateModels(mapper, auditRealm, updateContent.Friends);
                        UpdateModels(mapper, auditRealm, updateContent.Venues);
                        UpdateModels(mapper, auditRealm, updateContent.Talks);
                        UpdateModels(mapper, auditRealm, updateContent.Meetups);

                        var auditVersion = auditRealm.All<AuditVersion>().Single();

                        auditVersion.CommitHash = updateContent.LatestVersion;
                        auditRealm.Add(auditVersion, update: true);

                        trans.Commit();
                    }
                }

                stopwatch.Stop();

                logger.Track("AuditUpdate. Finished updating audit");

                if (updateContent.Speakers.Length > 0 ||
                    updateContent.Photos.Length > 0)
                {
                    result = result | UpdateResults.Speakers;
                }
                if (updateContent.Friends.Length > 0 ||
                    updateContent.Speakers.Length > 0 ||
                    updateContent.Venues.Length > 0 ||
                    updateContent.Talks.Length > 0 ||
                    updateContent.Meetups.Length > 0)
                {
                    result = result | UpdateResults.Meetups;
                }
                if (updateContent.Friends.Length > 0)
                {
                    result = result | UpdateResults.Friends;
                }
            }
            catch (Exception e)
            {
                DotNetRuLogger.Report(e);
            }

            return result;
        }

        private static void UpdateModels<T>(IMapper mapper, Realm auditRealm, IEnumerable<T> entities)
        {            
            foreach (var entity in entities)
            {
                var realmType = mapper.ConfigurationProvider.GetAllTypeMaps().Single(x => x.SourceType == typeof(T))
                    .DestinationType;

                var realmObject = mapper.Map(entity, typeof(T), realmType);
                
                auditRealm.Add(realmObject as RealmObject, update: true);                
            }
        }

        private static IMapper GetAutoMapper(Realm auditRealm)
        {
            var config = new MapperConfiguration(
                cfg =>
                    {
                        cfg.CreateMap<VenueEntity, Venue>();
                        cfg.CreateMap<FriendEntity, Friend>();
                        cfg.CreateMap<CommunityEntity, Community>();
                        cfg.CreateMap<TalkEntity, Talk>().AfterMap(
                            (src, dest) =>
                                {
                                    foreach (string speakerId in src.SpeakerIds)
                                    {
                                        var speaker = auditRealm.Find<Speaker>(speakerId);

                                        dest.Speakers.Add(speaker);
                                    }

                                    if (src.SeeAlsoTalkIds != null)
                                    {                                        
                                        foreach (string talkId in src.SeeAlsoTalkIds)
                                        {
                                            dest.SeeAlsoTalksIds.Add(talkId);
                                        }
                                    }
                                });
                        cfg.CreateMap<SessionEntity, Session>().AfterMap(
                            (src, dest) =>
                                {
                                    dest.Talk = auditRealm.Find<Talk>(src.TalkId);
                                });
                        cfg.CreateMap<MeetupEntity, Meetup>()
                            .ForMember(
                                dest => dest.Sessions,
                                o => o.MapFrom(src => src.Sessions))
                            .AfterMap(
                                (src, dest) =>
                                    {
                                        if (src.FriendIds != null)
                                        {
                                            foreach (string friendId in src.FriendIds)
                                            {
                                                var friend = auditRealm.Find<Friend>(friendId);
                                                dest.Friends.Add(friend);
                                            }
                                        }

                                        dest.Venue = auditRealm.Find<Venue>(src.VenueId);
                                    });
                    });

            return config.CreateMapper();            
        }
    }

    [Flags]
    public enum UpdateResults
    {
        None = 0,
        Speakers = 1,
        Meetups = 2,
        Friends = 4
    }
}
