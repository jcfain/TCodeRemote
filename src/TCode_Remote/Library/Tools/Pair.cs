﻿using System.Collections.Generic;
using System.ComponentModel;

namespace TCode_Remote.Library.Tools
{
    public class Pair<TKey, TValue> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected TKey _key;
        protected TValue _value;
        public Pair()
        {
        }

        public Pair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public Pair(KeyValuePair<TKey, TValue> kv)
        {
            Key = kv.Key;
            Value = kv.Value;
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public TKey Key
        {
            get { return _key; }
            set
            {
                if (
                    (_key == null && value != null)
                    || (_key != null && value == null)
                    || !_key.Equals(value))
                {
                    _key = value;
                    NotifyPropertyChanged("Key");
                }
            }
        }

        public TValue Value
        {
            get { return _value; }
            set
            {
                if (
                    (_value == null && value != null)
                    || (_value != null && value == null)
                    || (_value != null && !_value.Equals(value)))
                {
                    _value = value;
                    NotifyPropertyChanged("Value");
                }
            }
        }
    }
}
