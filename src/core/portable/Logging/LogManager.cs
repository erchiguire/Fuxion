﻿using Fuxion.Factories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
namespace Fuxion.Logging
{
	public class LogManager
	{
        internal static ILogFactory factory;
        public static ILog Create<T>()
		{
			return Create(typeof(T));
		}
		public static ILog Create(Type declaringType)
		{
			if (declaringType == null) throw new ArgumentNullException("The argument cannot be null", "declaringType");
            if (factory == null)
            {
                try {
                    factory = Factory.Get<ILogFactory>();
                    factory.Initialize();
                }
                catch { }
            }
            return factory == null
                ? new WrapLog(declaringType)
                : factory.Create(declaringType);
		}
	}
}
