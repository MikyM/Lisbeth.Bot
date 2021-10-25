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


using Hangfire.Client;
using Hangfire.States;

namespace Lisbeth.Bot.Application.Helpers
{
    public sealed class QueueFilter : IClientFilter, IElectStateFilter
    {
        public const string QueueParameterName = "Queue";

        public void OnCreating(CreatingContext filterContext)
        {
            // not needed
        }

        public void OnCreated(CreatedContext filterContext)
        {
            string queue = null;

            switch (filterContext.InitialState)
            {
                case EnqueuedState enqueuedState:
                    queue = enqueuedState.Queue;
                    break;
                case ScheduledEnqueuedState scheduledEnqueuedState:
                    queue = scheduledEnqueuedState.Queue;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(queue)) filterContext.SetJobParameter(QueueParameterName, queue);
        }

        public void OnStateElection(ElectStateContext context)
        {
            if (context.CandidateState.Name != EnqueuedState.StateName) return;

            string queue = context.GetJobParameter<string>(QueueParameterName.Trim());

            if (string.IsNullOrWhiteSpace(queue)) queue = EnqueuedState.DefaultQueue;

            context.CandidateState = new EnqueuedState();
        }
    }
}
