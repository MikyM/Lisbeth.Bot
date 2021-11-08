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

using System;
using Hangfire.States;

namespace Lisbeth.Bot.Application.Helpers
{
    public sealed class ScheduledEnqueuedState : ScheduledState
    {
        public ScheduledEnqueuedState(TimeSpan enqueueIn) : this(DateTime.UtcNow.Add(enqueueIn))
        {
        }

        public ScheduledEnqueuedState(DateTime enqueuedAt, string? queue = null) : base(enqueuedAt.ToUniversalTime())
        {
            Queue = queue?.Trim();
        }

        public string? Queue { get; }
    }
}