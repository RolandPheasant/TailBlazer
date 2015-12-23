using System;
using System.Windows.Input;
using DynamicData.Binding;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Recent;
using TailBlazer.Infrastucture;

namespace TailBlazer.Views
{
    public class RecentFileProxy: AbstractNotifyPropertyChanged, IEquatable<RecentFileProxy>
    {
        private readonly RecentFile _recentFile;
        private string _label;

        public string Name => _recentFile.Name;

        public string OpenToolTip => $"Open {_recentFile.Name}";
        public string RemoveToolTip => $"Remove {_recentFile.Name}";
        public DateTime Timestamp => _recentFile.Timestamp;

        public ICommand OpenCommand { get; }
        public ICommand RemoveCommand { get; }

        public RecentFileProxy(RecentFile recentFile, 
            Action<RecentFile> openAction,
            Action<RecentFile> removeAction)
        {
            _recentFile = recentFile;

            OpenCommand = new Command(() => openAction(recentFile));
            RemoveCommand = new Command(() => removeAction(recentFile));
        }

        public string Label
        {
            get { return _label; }
            set  { SetAndRaise(ref _label, value); }
        }

        #region Equality

        public bool Equals(RecentFileProxy other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_recentFile, other._recentFile);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RecentFileProxy) obj);
        }

        public override int GetHashCode()
        {
            return (_recentFile != null ? _recentFile.GetHashCode() : 0);
        }

        public static bool operator ==(RecentFileProxy left, RecentFileProxy right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RecentFileProxy left, RecentFileProxy right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}