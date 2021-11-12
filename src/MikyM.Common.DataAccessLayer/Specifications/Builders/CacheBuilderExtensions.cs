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

using EFCoreSecondLevelCacheInterceptor;

namespace MikyM.Common.DataAccessLayer.Specifications.Builders;

public static class CacheBuilderExtensions
{
    public static ICacheSpecificationBuilder<TEntity> WithExpirationMode<TEntity>(this ICacheSpecificationBuilder<TEntity> builder, CacheExpirationMode mode) where TEntity : class
    {
        builder.Specification.CacheExpirationMode = mode;

        return builder;
    }

    public static ICacheSpecificationBuilder<TEntity> WithExpirationTimeout<TEntity>(this ICacheSpecificationBuilder<TEntity> builder, TimeSpan timeout) where TEntity : class
    {
        builder.Specification.CacheTimeout = timeout;

        return builder;
    }
}