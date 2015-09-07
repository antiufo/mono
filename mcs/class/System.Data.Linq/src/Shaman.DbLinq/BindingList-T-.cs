#if CORECLR
using DbLinq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;

namespace System.ComponentModel
{
	/// <summary>Provides a generic collection that supports data binding.</summary>
	/// <typeparam name="T">The type of elements in the list.</typeparam>
	[Serializable]
	public class BindingList<T> : Collection<T>, IBindingList, IList, ICollection, IEnumerable, ICancelAddNew, IRaiseItemChangedEvents
	{
		private int addNewPos = -1;
		private bool raiseListChangedEvents = true;
		private bool raiseItemChangedEvents;
		[NonSerialized]
		private PropertyDescriptorCollection itemTypeProperties;
		[NonSerialized]
		private PropertyChangedEventHandler propertyChangedEventHandler;
		[NonSerialized]
		private AddingNewEventHandler onAddingNew;
		[NonSerialized]
		private ListChangedEventHandler onListChanged;
		[NonSerialized]
		private int lastChangeIndex = -1;
		private bool allowNew = true;
		private bool allowEdit = true;
		private bool allowRemove = true;
		private bool userSetAllowNew;
		/// <summary>Occurs before an item is added to the list.</summary>
		public event AddingNewEventHandler AddingNew
		{
			add
			{
				bool arg_23_0 = this.AllowNew;
				this.onAddingNew = (AddingNewEventHandler)Delegate.Combine(this.onAddingNew, value);
				if (arg_23_0 != this.AllowNew)
				{
					this.FireListChanged(ListChangedType.Reset, -1);
				}
			}
			remove
			{
				bool arg_23_0 = this.AllowNew;
				this.onAddingNew = (AddingNewEventHandler)Delegate.Remove(this.onAddingNew, value);
				if (arg_23_0 != this.AllowNew)
				{
					this.FireListChanged(ListChangedType.Reset, -1);
				}
			}
		}
		/// <summary>Occurs when the list or an item in the list changes.</summary>
		public event ListChangedEventHandler ListChanged
		{
			add
			{
				this.onListChanged = (ListChangedEventHandler)Delegate.Combine(this.onListChanged, value);
			}
			remove
			{
				this.onListChanged = (ListChangedEventHandler)Delegate.Remove(this.onListChanged, value);
			}
		}
		private bool ItemTypeHasDefaultConstructor
		{
			get
			{
				Type typeFromHandle = typeof(T);
				return typeFromHandle.GetTypeInfo().IsPrimitive || typeFromHandle.GetConstructor(EmptyArray<Type>.Instance) != null;
			}
		}
		/// <summary>Gets or sets a value indicating whether adding or removing items within the list raises <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> events.</summary>
		/// <returns>true if adding or removing items raises <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> events; otherwise, false. The default is true.</returns>
		public bool RaiseListChangedEvents
		{
			get
			{
				return this.raiseListChangedEvents;
			}
			set
			{
				if (this.raiseListChangedEvents != value)
				{
					this.raiseListChangedEvents = value;
				}
			}
		}
		private bool AddingNewHandled
		{
			get
			{
				return this.onAddingNew != null && this.onAddingNew.GetInvocationList().Length > 0;
			}
		}
		/// <summary>Gets or sets a value indicating whether you can add items to the list using the <see cref="M:System.ComponentModel.BindingList`1.AddNew" /> method.</summary>
		/// <returns>true if you can add items to the list with the <see cref="M:System.ComponentModel.BindingList`1.AddNew" /> method; otherwise, false. The default depends on the underlying type contained in the list.</returns>
		public bool AllowNew
		{
			get
			{
				if (this.userSetAllowNew || this.allowNew)
				{
					return this.allowNew;
				}
				return this.AddingNewHandled;
			}
			set
			{
				bool arg_15_0 = this.AllowNew;
				this.userSetAllowNew = true;
				this.allowNew = value;
				if (arg_15_0 != value)
				{
					this.FireListChanged(ListChangedType.Reset, -1);
				}
			}
		}
		/// <summary>Gets a value indicating whether new items can be added to the list using the <see cref="M:System.ComponentModel.BindingList`1.AddNew" /> method.</summary>
		/// <returns>true if you can add items to the list with the <see cref="M:System.ComponentModel.BindingList`1.AddNew" /> method; otherwise, false. The default depends on the underlying type contained in the list.</returns>
		bool IBindingList.AllowNew
		{
			get
			{
				return this.AllowNew;
			}
		}
		/// <summary>Gets or sets a value indicating whether items in the list can be edited.</summary>
		/// <returns>true if list items can be edited; otherwise, false. The default is true.</returns>
		public bool AllowEdit
		{
			get
			{
				return this.allowEdit;
			}
			set
			{
				if (this.allowEdit != value)
				{
					this.allowEdit = value;
					this.FireListChanged(ListChangedType.Reset, -1);
				}
			}
		}
		/// <summary>Gets a value indicating whether items in the list can be edited.</summary>
		/// <returns>true if list items can be edited; otherwise, false. The default is true.</returns>
		bool IBindingList.AllowEdit
		{
			get
			{
				return this.AllowEdit;
			}
		}
		/// <summary>Gets or sets a value indicating whether you can remove items from the collection. </summary>
		/// <returns>true if you can remove items from the list with the <see cref="M:System.ComponentModel.BindingList`1.RemoveItem(System.Int32)" /> method otherwise, false. The default is true.</returns>
		public bool AllowRemove
		{
			get
			{
				return this.allowRemove;
			}
			set
			{
				if (this.allowRemove != value)
				{
					this.allowRemove = value;
					this.FireListChanged(ListChangedType.Reset, -1);
				}
			}
		}
		/// <summary>Gets a value indicating whether items can be removed from the list.</summary>
		/// <returns>true if you can remove items from the list with the <see cref="M:System.ComponentModel.BindingList`1.RemoveItem(System.Int32)" /> method; otherwise, false. The default is true.</returns>
		bool IBindingList.AllowRemove
		{
			get
			{
				return this.AllowRemove;
			}
		}
		/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SupportsChangeNotification" />.</summary>
		/// <returns>true if a <see cref="E:System.ComponentModel.IBindingList.ListChanged" /> event is raised when the list changes or when an item changes; otherwise, false.</returns>
		bool IBindingList.SupportsChangeNotification
		{
			get
			{
				return this.SupportsChangeNotificationCore;
			}
		}
		/// <summary>Gets a value indicating whether <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> events are enabled.</summary>
		/// <returns>true if <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> events are supported; otherwise, false. The default is true.</returns>
		protected virtual bool SupportsChangeNotificationCore
		{
			get
			{
				return true;
			}
		}
		/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SupportsSearching" />.</summary>
		/// <returns>true if the list supports searching using the <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)" /> method; otherwise, false.</returns>
		bool IBindingList.SupportsSearching
		{
			get
			{
				return this.SupportsSearchingCore;
			}
		}
		/// <summary>Gets a value indicating whether the list supports searching.</summary>
		/// <returns>true if the list supports searching; otherwise, false. The default is false.</returns>
		protected virtual bool SupportsSearchingCore
		{
			get
			{
				return false;
			}
		}
		/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SupportsSorting" />.</summary>
		/// <returns>true if the list supports sorting; otherwise, false.</returns>
		bool IBindingList.SupportsSorting
		{
			get
			{
				return this.SupportsSortingCore;
			}
		}
		/// <summary>Gets a value indicating whether the list supports sorting.</summary>
		/// <returns>true if the list supports sorting; otherwise, false. The default is false.</returns>
		protected virtual bool SupportsSortingCore
		{
			get
			{
				return false;
			}
		}
		/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.IsSorted" />.</summary>
		/// <returns>true if <see cref="M:System.ComponentModel.IBindingListView.ApplySort(System.ComponentModel.ListSortDescriptionCollection)" /> has been called and <see cref="M:System.ComponentModel.IBindingList.RemoveSort" /> has not been called; otherwise, false.</returns>
		bool IBindingList.IsSorted
		{
			get
			{
				return this.IsSortedCore;
			}
		}
		/// <summary>Gets a value indicating whether the list is sorted. </summary>
		/// <returns>true if the list is sorted; otherwise, false. The default is false.</returns>
		protected virtual bool IsSortedCore
		{
			get
			{
				return false;
			}
		}
		/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SortProperty" />.</summary>
		/// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor" /> that is being used for sorting.</returns>
		PropertyDescriptor IBindingList.SortProperty
		{
			get
			{
				return this.SortPropertyCore;
			}
		}
		/// <summary>Gets the property descriptor that is used for sorting the list if sorting is implemented in a derived class; otherwise, returns null. </summary>
		/// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor" /> used for sorting the list.</returns>
		protected virtual PropertyDescriptor SortPropertyCore
		{
			get
			{
				return null;
			}
		}
		/// <summary>For a description of this member, see <see cref="P:System.ComponentModel.IBindingList.SortDirection" />.</summary>
		/// <returns>One of the <see cref="T:System.ComponentModel.ListSortDirection" /> values.</returns>
		ListSortDirection IBindingList.SortDirection
		{
			get
			{
				return this.SortDirectionCore;
			}
		}
		/// <summary>Gets the direction the list is sorted.</summary>
		/// <returns>One of the <see cref="T:System.ComponentModel.ListSortDirection" /> values. The default is <see cref="F:System.ComponentModel.ListSortDirection.Ascending" />. </returns>
		protected virtual ListSortDirection SortDirectionCore
		{
			get
			{
				return ListSortDirection.Ascending;
			}
		}
		/// <summary>Gets a value indicating whether item property value changes raise <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> events of type <see cref="F:System.ComponentModel.ListChangedType.ItemChanged" />. This member cannot be overridden in a derived class.</summary>
		/// <returns>true if the list type implements <see cref="T:System.ComponentModel.INotifyPropertyChanged" />, otherwise, false. The default is false.</returns>
		bool IRaiseItemChangedEvents.RaisesItemChangedEvents
		{
			get
			{
				return this.raiseItemChangedEvents;
			}
		}
		/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.BindingList`1" /> class using default values.</summary>
		public BindingList()
		{
			this.Initialize();
		}
		/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.BindingList`1" /> class with the specified list.</summary>
		/// <param name="list">An <see cref="T:System.Collections.Generic.IList`1" /> of items to be contained in the <see cref="T:System.ComponentModel.BindingList`1" />.</param>
		public BindingList(IList<T> list) : base(list)
		{
			this.Initialize();
		}
		private void Initialize()
		{
			this.allowNew = this.ItemTypeHasDefaultConstructor;
			if (typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T)))
			{
				this.raiseItemChangedEvents = true;
				foreach (T current in base.Items)
				{
					this.HookPropertyChanged(current);
				}
			}
		}
		/// <summary>Raises the <see cref="E:System.ComponentModel.BindingList`1.AddingNew" /> event.</summary>
		/// <param name="e">An <see cref="T:System.ComponentModel.AddingNewEventArgs" /> that contains the event data. </param>
		protected virtual void OnAddingNew(AddingNewEventArgs e)
		{
			if (this.onAddingNew != null)
			{
				this.onAddingNew(this, e);
			}
		}
		private object FireAddingNew()
		{
			AddingNewEventArgs addingNewEventArgs = new AddingNewEventArgs(null);
			this.OnAddingNew(addingNewEventArgs);
			return addingNewEventArgs.NewObject;
		}
		/// <summary>Raises the <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> event.</summary>
		/// <param name="e">A <see cref="T:System.ComponentModel.ListChangedEventArgs" /> that contains the event data. </param>
		protected virtual void OnListChanged(ListChangedEventArgs e)
		{
			if (this.onListChanged != null)
			{
				this.onListChanged(this, e);
			}
		}
		/// <summary>Raises a <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> event of type <see cref="F:System.ComponentModel.ListChangedType.Reset" />.</summary>
		public void ResetBindings()
		{
			this.FireListChanged(ListChangedType.Reset, -1);
		}
		/// <summary>Raises a <see cref="E:System.ComponentModel.BindingList`1.ListChanged" /> event of type <see cref="F:System.ComponentModel.ListChangedType.ItemChanged" /> for the item at the specified position.</summary>
		/// <param name="position">A zero-based index of the item to be reset.</param>
		public void ResetItem(int position)
		{
			this.FireListChanged(ListChangedType.ItemChanged, position);
		}
		private void FireListChanged(ListChangedType type, int index)
		{
			if (this.raiseListChangedEvents)
			{
				this.OnListChanged(new ListChangedEventArgs(type, index));
			}
		}
		/// <summary>Removes all elements from the collection.</summary>
		protected override void ClearItems()
		{
			this.EndNew(this.addNewPos);
			if (this.raiseItemChangedEvents)
			{
				foreach (T current in base.Items)
				{
					this.UnhookPropertyChanged(current);
				}
			}
			base.ClearItems();
			this.FireListChanged(ListChangedType.Reset, -1);
		}
		/// <summary>Inserts the specified item in the list at the specified index.</summary>
		/// <param name="index">The zero-based index where the item is to be inserted.</param>
		/// <param name="item">The item to insert in the list.</param>
		protected override void InsertItem(int index, T item)
		{
			this.EndNew(this.addNewPos);
			base.InsertItem(index, item);
			if (this.raiseItemChangedEvents)
			{
				this.HookPropertyChanged(item);
			}
			this.FireListChanged(ListChangedType.ItemAdded, index);
		}
		/// <summary>Removes the item at the specified index.</summary>
		/// <param name="index">The zero-based index of the item to remove. </param>
		/// <exception cref="T:System.NotSupportedException">You are removing a newly added item and <see cref="P:System.ComponentModel.IBindingList.AllowRemove" /> is set to false. </exception>
		protected override void RemoveItem(int index)
		{
			if (!this.allowRemove && (this.addNewPos < 0 || this.addNewPos != index))
			{
				throw new NotSupportedException();
			}
			this.EndNew(this.addNewPos);
			if (this.raiseItemChangedEvents)
			{
				this.UnhookPropertyChanged(base[index]);
			}
			base.RemoveItem(index);
			this.FireListChanged(ListChangedType.ItemDeleted, index);
		}
		/// <summary>Replaces the item at the specified index with the specified item.</summary>
		/// <param name="index">The zero-based index of the item to replace.</param>
		/// <param name="item">The new value for the item at the specified index. The value can be null for reference types.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		///   <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count" />.</exception>
		protected override void SetItem(int index, T item)
		{
			if (this.raiseItemChangedEvents)
			{
				this.UnhookPropertyChanged(base[index]);
			}
			base.SetItem(index, item);
			if (this.raiseItemChangedEvents)
			{
				this.HookPropertyChanged(item);
			}
			this.FireListChanged(ListChangedType.ItemChanged, index);
		}
		/// <summary>Discards a pending new item.</summary>
		/// <param name="itemIndex">The index of the of the new item to be added </param>
		public virtual void CancelNew(int itemIndex)
		{
			if (this.addNewPos >= 0 && this.addNewPos == itemIndex)
			{
				this.RemoveItem(this.addNewPos);
				this.addNewPos = -1;
			}
		}
		/// <summary>Commits a pending new item to the collection.</summary>
		/// <param name="itemIndex">The index of the new item to be added.</param>
		public virtual void EndNew(int itemIndex)
		{
			if (this.addNewPos >= 0 && this.addNewPos == itemIndex)
			{
				this.addNewPos = -1;
			}
		}
		/// <summary>Adds a new item to the collection.</summary>
		/// <returns>The item added to the list.</returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Forms.BindingSource.AllowNew" /> property is set to false. -or-A public default constructor could not be found for the current item type.</exception>
		public T AddNew()
		{
			return (T)((object)((IBindingList)this).AddNew());
		}
		/// <summary>Adds a new item to the list. For more information, see <see cref="M:System.ComponentModel.IBindingList.AddNew" />.</summary>
		/// <returns>The item added to the list.</returns>
		/// <exception cref="T:System.NotSupportedException">This method is not supported. </exception>
		object IBindingList.AddNew()
		{
			object obj = this.AddNewCore();
			this.addNewPos = ((obj != null) ? base.IndexOf((T)((object)obj)) : -1);
			return obj;
		}
		/// <summary>Adds a new item to the end of the collection.</summary>
		/// <returns>The item that was added to the collection.</returns>
		/// <exception cref="T:System.InvalidCastException">The new item is not the same type as the objects contained in the <see cref="T:System.ComponentModel.BindingList`1" />.</exception>
		protected virtual object AddNewCore()
		{
			object obj = this.FireAddingNew();
			if (obj == null)
			{
                obj = Activator.CreateInstance(typeof(T));
				//obj = SecurityUtils.SecureCreateInstance(typeof(T));
			}
			base.Add((T)((object)obj));
			return obj;
		}
		/// <summary>Sorts the list based on a <see cref="T:System.ComponentModel.PropertyDescriptor" /> and a <see cref="T:System.ComponentModel.ListSortDirection" />. For a complete description of this member, see <see cref="M:System.ComponentModel.IBindingList.ApplySort(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)" />. </summary>
		/// <param name="prop">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to sort by.</param>
		/// <param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection" /> values.</param>
		void IBindingList.ApplySort(PropertyDescriptor prop, ListSortDirection direction)
		{
			this.ApplySortCore(prop, direction);
		}
		/// <summary>Sorts the items if overridden in a derived class; otherwise, throws a <see cref="T:System.NotSupportedException" />.</summary>
		/// <param name="prop">A <see cref="T:System.ComponentModel.PropertyDescriptor" /> that specifies the property to sort on.</param>
		/// <param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection" />  values.</param>
		/// <exception cref="T:System.NotSupportedException">Method is not overridden in a derived class. </exception>
		protected virtual void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
		{
			throw new NotSupportedException();
		}
		/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.RemoveSort" /></summary>
		void IBindingList.RemoveSort()
		{
			this.RemoveSortCore();
		}
		/// <summary>Removes any sort applied with <see cref="M:System.ComponentModel.BindingList`1.ApplySortCore(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)" /> if sorting is implemented in a derived class; otherwise, raises <see cref="T:System.NotSupportedException" />.</summary>
		/// <exception cref="T:System.NotSupportedException">Method is not overridden in a derived class. </exception>
		protected virtual void RemoveSortCore()
		{
			throw new NotSupportedException();
		}
		/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.Find(System.ComponentModel.PropertyDescriptor,System.Object)" />.</summary>
		/// <returns>The index of the row that has the given <see cref="T:System.ComponentModel.PropertyDescriptor" /> .</returns>
		/// <param name="prop">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to search on.</param>
		/// <param name="key">The value of the <paramref name="property" /> parameter to search for.</param>
		int IBindingList.Find(PropertyDescriptor prop, object key)
		{
			return this.FindCore(prop, key);
		}
		/// <summary>Searches for the index of the item that has the specified property descriptor with the specified value, if searching is implemented in a derived class; otherwise, a <see cref="T:System.NotSupportedException" />.</summary>
		/// <returns>The zero-based index of the item that matches the property descriptor and contains the specified value.</returns>
		/// <param name="prop">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to search for.</param>
		/// <param name="key">The value of <paramref name="property" /> to match.</param>
		/// <exception cref="T:System.NotSupportedException">
		///   <see cref="M:System.ComponentModel.BindingList`1.FindCore(System.ComponentModel.PropertyDescriptor,System.Object)" /> is not overridden in a derived class.</exception>
		protected virtual int FindCore(PropertyDescriptor prop, object key)
		{
			throw new NotSupportedException();
		}
		/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.AddIndex(System.ComponentModel.PropertyDescriptor)" />.</summary>
		/// <param name="prop">The <see cref="T:System.ComponentModel.PropertyDescriptor" /> to add as a search criteria. </param>
		void IBindingList.AddIndex(PropertyDescriptor prop)
		{
		}
		/// <summary>For a description of this member, see <see cref="M:System.ComponentModel.IBindingList.RemoveIndex(System.ComponentModel.PropertyDescriptor)" />.</summary>
		/// <param name="prop">A <see cref="T:System.ComponentModel.PropertyDescriptor" /> to remove from the indexes used for searching.</param>
		void IBindingList.RemoveIndex(PropertyDescriptor prop)
		{
		}
		private void HookPropertyChanged(T item)
		{
			INotifyPropertyChanged notifyPropertyChanged = item as INotifyPropertyChanged;
			if (notifyPropertyChanged != null)
			{
				if (this.propertyChangedEventHandler == null)
				{
					this.propertyChangedEventHandler = new PropertyChangedEventHandler(this.Child_PropertyChanged);
				}
				notifyPropertyChanged.PropertyChanged += this.propertyChangedEventHandler;
			}
		}
		private void UnhookPropertyChanged(T item)
		{
			INotifyPropertyChanged notifyPropertyChanged = item as INotifyPropertyChanged;
			if (notifyPropertyChanged != null && this.propertyChangedEventHandler != null)
			{
				notifyPropertyChanged.PropertyChanged -= this.propertyChangedEventHandler;
			}
		}
		private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
#if false
            if (this.RaiseListChangedEvents)
			{
				if (sender == null || e == null || string.IsNullOrEmpty(e.PropertyName))
				{
					this.ResetBindings();
					return;
				}
				T t;
				try
				{
					t = (T)((object)sender);
				}
				catch (InvalidCastException)
				{
					this.ResetBindings();
					return;
				}
				int num = this.lastChangeIndex;
				if (num >= 0 && num < base.Count)
				{
					T t2 = base[num];
					if (t2.Equals(t))
					{
						goto IL_7B;
					}
				}
				num = base.IndexOf(t);
				this.lastChangeIndex = num;
				IL_7B:
				if (num == -1)
				{
					this.UnhookPropertyChanged(t);
					this.ResetBindings();
					return;
				}
				if (this.itemTypeProperties == null)
				{
                    itemTypeProperties = TypeDescriptor.GetProperties(typeof(T));
				}
				PropertyDescriptor propDesc = this.itemTypeProperties.Find(e.PropertyName, true);
				ListChangedEventArgs e2 = new ListChangedEventArgs(ListChangedType.ItemChanged, num, propDesc);
				this.OnListChanged(e2);
			}
#endif
		}
	}
}
#endif