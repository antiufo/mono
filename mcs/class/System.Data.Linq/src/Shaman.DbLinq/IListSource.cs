#if CORECLR
using System;
using System.Collections;
namespace System.ComponentModel
{
	/// <summary>Provides functionality to an object to return a list that can be bound to a data source.</summary>
	public interface IListSource
	{
		/// <summary>Gets a value indicating whether the collection is a collection of <see cref="T:System.Collections.IList" /> objects.</summary>
		/// <returns>true if the collection is a collection of <see cref="T:System.Collections.IList" /> objects; otherwise, false.</returns>
		bool ContainsListCollection
		{
			get;
		}
		/// <summary>Returns an <see cref="T:System.Collections.IList" /> that can be bound to a data source from an object that does not implement an <see cref="T:System.Collections.IList" /> itself.</summary>
		/// <returns>An <see cref="T:System.Collections.IList" /> that can be bound to a data source from the object.</returns>
		IList GetList();
	}
}
#endif