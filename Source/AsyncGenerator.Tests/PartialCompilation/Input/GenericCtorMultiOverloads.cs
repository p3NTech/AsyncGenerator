﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsyncGenerator.TestCases;

namespace AsyncGenerator.Tests.PartialCompilation.Input
{
	public class FakeClass<T>
	{
		private readonly Func<IList<T>> _getList;
		private readonly Func<Task<IList<T>>> _getListAsync;

		private FakeClass(Func<IList<T>> getList, Func<Task<IList<T>>> getListAsync)
		{
			_getList = getList;
			_getListAsync = getListAsync;
		}
	}

	/// <summary>
	/// Github Issue 40
	/// </summary>
	public class GenericCtorMultiOverloads
	{
		#if TEST

		public static Ctor Create()
		{
			return new FakeClass<T>(List<T>, ListAsync<T>);
		}

		#endif

		private IList<T> List<T>()
		{
			var list = new List<T>();
			List(list);
			return list;
		}

		private void List<T>(IList<T> results)
		{
			SimpleFile.Read();
		}

	}
}
