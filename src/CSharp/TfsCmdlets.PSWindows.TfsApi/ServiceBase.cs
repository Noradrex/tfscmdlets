﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsCmdlets.Core.Adapters;
using TfsCmdlets.PSWindows.TfsApi.Adapters;

namespace TfsCmdlets.PSWindows.TfsApi
{
    public abstract class ServiceBase<T, TAdapter> 
        where T: class
        where TAdapter : IAdapter
    {
        protected T GetItem(object item, params object[] scopeObjects)
        {
            var items = GetItems(item, scopeObjects).ToList();

            if (items.Count == 0)
                throw new Exception($"Invalid {ItemName} name '{item}'");

            if (items.Count == 1)
                return items[0];

            var names = string.Join(", ", items.Select(ItemDescriptor).ToArray());
            throw new Exception($"Ambiguous name '{item}' matches {items.Count} {ItemName}s: {names}. Please choose a more specific value and try again");
        }

        protected IEnumerable<T> GetItems(object item, params object[] scopeObjects)
        {
            ScopeObjects so = new ScopeObjects(scopeObjects);

            while (true)
            {
                switch (item)
                {
                    case PSObject pso:
                    {
                        item = pso.BaseObject;
                        continue;
                    }
                    case TAdapter a:
                    {
                        item = a.Instance;
                        continue;
                    }
                    case T t:
                    {
                        yield return t;
                        break;
                    }
                    case string s when PatternSearch:
                    {
                        foreach (var t in GetAllItems(item, so).Where(o => ItemDescriptor(o).IsLike(s)))
                        {
                            yield return t;
                        }
                        break;
                    }
                    case object o:
                    {
                        foreach (var t in GetAllItems(item, so).Where<T>(CustomSearch))
                        {
                            yield return t;
                        }
                        break;
                    }
                    default:
                    {
                        throw new ArgumentException($"Invalid {ItemName}");
                    }
                }
                break;
            }
        }

        protected abstract string ItemName { get; }

        protected abstract Func<T, string> ItemDescriptor { get; }

        protected abstract IEnumerable<T> GetAllItems(object item, ScopeObjects so);

        protected virtual bool PatternSearch => true;

        protected virtual Func<object,bool> CustomSearch => (_ => true);
    }

    public class ScopeObjects
    {
        public ScopeObjects(IReadOnlyList<object> scopeObjects)
        {
            var i = scopeObjects.Count;

            if (--i >= 0) Credential = scopeObjects[i];
            if (--i >= 0) Server = scopeObjects[i];
            if (--i >= 0) Collection = scopeObjects[i];
            if (--i >= 0) Project = scopeObjects[i];
            if (--i >= 0) Team = scopeObjects[i];

            All = scopeObjects;
        }

        public object Team { get; private set; }
        public object Project { get; private set; }
        public object Collection { get; private set; }
        public object Server { get; private set; }
        public object Credential { get; private set; }
        public IReadOnlyList<object> All { get; private set; }
    }
}