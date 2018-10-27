﻿namespace XamarinEvolve.DataStore.Mock.Stores
{
    using System.Threading.Tasks;

    using XamarinEvolve.DataObjects;
    using XamarinEvolve.DataStore.Mock.Abstractions;

    public class FeedbackStore : BaseStore<Feedback>, IFeedbackStore
    {
        public Task<bool> LeftFeedback(TalkModel talkModel)
        {
            return Task.FromResult(Settings.LeftFeedback(talkModel.Id));
        }

        public async Task DropFeedback()
        {
            await Settings.ClearFeedback();
        }

        public override Task<bool> InsertAsync(Feedback item)
        {
            Settings.LeaveFeedback(item.SessionId, true);
            return Task.FromResult(true);
        }

        public override Task<bool> RemoveAsync(Feedback item)
        {
            Settings.LeaveFeedback(item.SessionId, false);
            return Task.FromResult(true);
        }
    }
}

