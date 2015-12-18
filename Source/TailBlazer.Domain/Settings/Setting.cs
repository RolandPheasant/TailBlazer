using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.Settings
{

    public class Setting<T> : IEquatable<Setting<T>>, IDisposable, ISetting<T>
    {
        private readonly ILogger _logger;
        private readonly ISettingsStore _settingsStore;
        private readonly IConverter<T> _converter;
        private readonly string _key;
        private string _rawValue;

        private readonly ISubject<T> _changed = new ReplaySubject<T>(1);
        private T _value;

        public IObservable<T> Value => _changed.AsObservable();

        public Setting(ILogger logger, ISettingsStore settingsStore, IConverter<T> converter, string key)
        {
            _logger = logger;
            _settingsStore = settingsStore;
            _converter = converter;
            _key = key;
            
            try
            {
                //make this awaitable
                var state = _settingsStore.Load(_key);
                _rawValue = state.Value;
                _value = converter.Convert(state);
                _changed.OnNext(_value);

            }
            catch (Exception ex)
            {
                _value = converter.GetDefaultValue();
                _rawValue= converter.Convert(_value).Value;
                _logger.Error(ex, "Problem reading {0}", _key);
            }
    
        }
        
        public void Write(T value)
        {
            var converted = _converter.Convert(value);

            if (_rawValue != converted.Value)
            {
                _rawValue = converted.Value;
                _value = value;

                try
                {
                    //make this awaitable
                    _settingsStore.Save(_key, converted);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex,"Problem writing {0}", value);
                }
                _changed.OnNext(value);
            }
        }

        #region Equality

        public bool Equals(Setting<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_key, other._key);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Setting<T>)obj);
        }

        public override int GetHashCode()
        {
            return _key?.GetHashCode() ?? 0;
        }
        

        public static bool operator ==(Setting<T> left, Setting<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Setting<T> left, Setting<T> right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return _key;
        }

        public void Dispose()
        {
            _changed.OnCompleted();
        }
    }
}