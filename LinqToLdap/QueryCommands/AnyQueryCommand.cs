﻿/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.DirectoryServices.Protocols;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;

namespace LinqToLdap.QueryCommands
{   
    internal class AnyQueryCommand : QueryCommand
    {
        public AnyQueryCommand(IQueryCommandOptions options, IObjectMapping mapping)
            : base(options, mapping, false)
        {
        }

        public override object Execute(DirectoryConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            if (Options.YieldNoResults) return false;

            SetDistinguishedName(namingContext);
            SearchRequest.Scope = scope;
            if (GetControl<PageResultRequestControl>(SearchRequest.Controls) != null)
            {
                throw new InvalidOperationException("Only one page request control can be sent to the server.");
            }
            if (pagingEnabled && !Options.WithoutPaging)
            {
                SearchRequest.Controls.Add(new PageResultRequestControl(1));
            }
            
            SearchRequest.TypesOnly = true;
            SearchRequest.Attributes.Add("cn");

            if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());
            var response = connection.SendRequest(SearchRequest) as SearchResponse;

            response.AssertSuccess();

            return response.Entries.Count > 0;
        }
    }
}
