// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Lisbeth.Bot.Application.Hangfire
{
    public class PreserveOriginalQueueAttribute : JobFilterAttribute, IApplyStateFilter
    {
        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            if (context.NewState is not EnqueuedState enqueuedState) return;

            var originalQueue =
                SerializationHelper.Deserialize<string>(
                    context.Connection.GetJobParameter(context.BackgroundJob.Id, "OriginalQueue"));

            if (originalQueue is not null)
                enqueuedState.Queue = originalQueue;
            else
                context.Connection.SetJobParameter(context.BackgroundJob.Id, "OriginalQueue",
                    SerializationHelper.Serialize(enqueuedState.Queue));
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            // not needed
        }
    }
}