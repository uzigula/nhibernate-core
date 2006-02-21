#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using NHibernate.Collection;
using NHibernate.Collection.Generic;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.Util;

namespace NHibernate.Type
{
	/// <summary>
	/// An <see cref="IType"/> that maps an <see cref="IList&lt;T&gt;"/> collection
	/// to the database using list semantics.
	/// </summary>
	public class GenericListType : ListType
	{
		System.Type constructedType = null;
		System.Type wrappedType = null;
		ConstructorInfo nonwrappedConstructor = null;
		ConstructorInfo wrappedConstructor = null;

		/// <summary>
		/// Initializes a new instance of a <see cref="GenericListType"/> class for
		/// a specific role.
		/// </summary>
		/// <param name="role">The role the persistent collection is in.</param>
		public GenericListType(string role, string propertyRef, System.Type elementClass)
			: base( role, propertyRef )
		{
			System.Type definition = typeof(Collection.Generic.PersistentGenericList<>);
			System.Type[] typeArgs = new System.Type[] { elementClass };

			constructedType = definition.MakeGenericType(typeArgs);
			wrappedType = typeof(System.Collections.Generic.IList<>).MakeGenericType(typeArgs);

			nonwrappedConstructor = constructedType.GetConstructor(ReflectHelper.AnyVisibilityInstance, null, new System.Type[] { typeof(ISessionImplementor) }, null);
			wrappedConstructor = constructedType.GetConstructor(ReflectHelper.AnyVisibilityInstance, null, new System.Type[] { typeof(ISessionImplementor), wrappedType }, null);
		}

		/// <summary>
		/// Instantiates a new <see cref="IPersistentCollection"/> for the list.
		/// </summary>
		/// <param name="session">The current <see cref="ISessionImplementor"/> for the list.</param>
		/// <param name="persister"></param>
		/// <returns></returns>
		public override IPersistentCollection Instantiate(ISessionImplementor session, ICollectionPersister persister)
		{
			return nonwrappedConstructor.Invoke(new object[] { session }) as IPersistentCollection;
		}

		/// <summary></summary>
		public override System.Type ReturnedClass
		{
			get { return wrappedType; }
		}

		/// <summary>
		/// Wraps an <see cref="IList&lt;T&gt;"/> in a <see cref="PersistentGenericList&lt;T&gt;"/>.
		/// </summary>
		/// <param name="session">The <see cref="ISessionImplementor"/> for the collection to be a part of.</param>
		/// <param name="collection">The unwrapped <see cref="IList&lt;T&gt;"/>.</param>
		/// <returns>
		/// An <see cref="PersistentGenericList&lt;T&gt;"/> that wraps the non NHibernate <see cref="IList&lt;T&gt;"/>.
		/// </returns>
		public override IPersistentCollection Wrap(ISessionImplementor session, object collection)
		{
			return wrappedConstructor.Invoke(new object[] { session, collection }) as IPersistentCollection;
		}

		//TODO: Add() & Clear() methods - need to see if these should be refactored back into
		// their own version of Copy or a DoCopy.  The Copy() method used to be spread out amongst
		// the various collections, but since they all had common code Add() and Clear() were made
		// virtual since that was where most of the logic was.  A different/better way might be to
		// have a Copy on the base collection that handles the standard checks and then a DoCopy
		// that performs the actual copy.
	}
}
#endif