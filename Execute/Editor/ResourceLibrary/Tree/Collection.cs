﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Specialized;

namespace ResourceLibrary
{
	public class ObservableCollectionAdv<T> : ObservableCollection<T>
	{
        public ObservableCollectionAdv()
            : base()
        {

        }
        public ObservableCollectionAdv(List<T> list)
            : base(list)
        {

        }
        public ObservableCollectionAdv(IEnumerable<T> collection)
            : base(collection)
        {

        }

        public void RemoveRange(int index, int count)
		{
			this.CheckReentrancy();
			var items = this.Items as List<T>;
			items.RemoveRange(index, count);
			OnReset();
		}

		public void InsertRange(int index, IEnumerable<T> collection)
		{
			this.CheckReentrancy();
			var items = this.Items as List<T>;
			items.InsertRange(index, collection);
			OnReset();
		}

		private void OnReset()
		{
			OnPropertyChanged("Count");
			OnPropertyChanged("Item[]");
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Reset));
		}

		private void OnPropertyChanged(string propertyName)
		{
			OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}
	}
}
