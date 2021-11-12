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

namespace MikyM.Common.Domain.Entities;

public abstract class Entity : Entity<long>
{
    protected Entity()
    {
    }

    protected Entity(long id)
        : base(id)
    {
    }
}

public abstract class Entity<TId>
{
    protected Entity()
    {
        CreatedAt ??= DateTime.UtcNow;
        UpdatedAt ??= CreatedAt;
    }

    protected Entity(TId id)
    {
        CreatedAt ??= DateTime.UtcNow;
        UpdatedAt ??= CreatedAt;
        Id = id;
    }

    public virtual TId Id { get; protected set; }
    public virtual DateTime? CreatedAt { get; set; }
    public virtual DateTime? UpdatedAt { get; set; }
    public virtual bool IsDisabled { get; set; }

    public override string ToString()
    {
        return Id.ToString();
    }

    public override bool Equals(object obj)
    {
        if (obj is not Entity<TId> other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetUnproxiedType(this) != GetUnproxiedType(other))
            return false;

        if (Id.Equals(default) || other.Id.Equals(default))
            return false;

        return Id.Equals(other.Id);
    }

    public static bool operator ==(Entity<TId> a, Entity<TId> b)
    {
        if (a  is null && b  is null)
            return true;

        if (a  is null || b  is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(Entity<TId> a, Entity<TId> b)
    {
        return !(a == b);
    }

    public override int GetHashCode()
    {
        return (GetUnproxiedType(this).ToString() + Id).GetHashCode();
    }

    internal static Type GetUnproxiedType(object obj)
    {
        const string EFCoreProxyPrefix = "Castle.Proxies.";
        const string NHibernateProxyPostfix = "Proxy";

        Type type = obj.GetType();
        string typeString = type.ToString();

        if (typeString.Contains(EFCoreProxyPrefix) || typeString.EndsWith(NHibernateProxyPostfix))
            return type.BaseType;

        return type;
    }
}