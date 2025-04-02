﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Si.EntityFramework.Extension.Exceptions;
using System.Data.Common;
using System.Text;

namespace Si.EntityFramework.Extension.Routing.Interceptor
{
    public class ReadForceInterceptor<TContext> : DbCommandInterceptor where TContext : DbContext
    {
        public ReadForceInterceptor() : base()
        {
        }
        public override InterceptionResult<DbCommand> CommandCreating(
        CommandCorrelatedEventData eventData,
        InterceptionResult<DbCommand> result)
        {
            if (eventData.Context is DbContext ctx)
            {
                var initialCommand = result.Result;
                if (initialCommand != null && !IsReadCommand(initialCommand))
                {
                    throw new ReadForceException("ReadForceInterceptor: Write operation detected");
                }
            }
            return base.CommandCreating(eventData, result);
        }
        private static bool IsReadCommand(DbCommand command)
        {
            var firstToken = GetFirstCommandToken(command.CommandText);
            return string.Equals(firstToken, "SELECT", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetFirstCommandToken(string sql)
        {
            using var reader = new StringReader(sql);
            var sb = new StringBuilder();
            while (true)
            {
                int c = reader.Read();
                if (c == -1) break;

                if (char.IsLetter((char)c))
                {
                    sb.Append((char)c);
                }
                else if (sb.Length > 0)
                {
                    break;
                }
            }
            return sb.ToString().ToUpperInvariant();
        }
    }
}
