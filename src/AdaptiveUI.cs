// File: AdaptiveUI.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MinimalFirewall
{

    public struct ProtocolTypes : IEquatable<ProtocolTypes>
    {
        public static ProtocolTypes Any { get; } = new ProtocolTypes("Any", 256, false, false);
        public static ProtocolTypes TCP { get; } = new ProtocolTypes("TCP", 6, true, false);
        public static ProtocolTypes UDP { get; } = new ProtocolTypes("UDP", 17, true, false);
        public static ProtocolTypes ICMPv4 { get; } = new ProtocolTypes("ICMPv4", 1, false, true);
        public static ProtocolTypes ICMPv6 { get; } = new ProtocolTypes("ICMPv6", 58, false, true);
        public static ProtocolTypes IGMP { get; } = new ProtocolTypes("IGMP", 2, false, false);

        public string Name { get; }
        public short Value { get; }
        public bool SupportsPorts { get; }
        public bool SupportsIcmp { get; }

        private ProtocolTypes(string name, short value, bool supportsPorts, bool supportsIcmp)
        {
            Name = name;
            Value = value;
            SupportsPorts = supportsPorts;
            SupportsIcmp = supportsIcmp;
        }

        public override string ToString() => Name;
        public bool Equals(ProtocolTypes other) => this.Value == other.Value;
        public override bool Equals(object? obj) => obj is ProtocolTypes other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
    }


    public class FirewallRuleViewModel : INotifyPropertyChanged
    {
        private ProtocolTypes _selectedProtocol;
        public ProtocolTypes SelectedProtocol
        {
            get => _selectedProtocol;
            set
            {
                if (_selectedProtocol.Equals(value)) return;
                _selectedProtocol = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPortSectionVisible));
                OnPropertyChanged(nameof(IsIcmpSectionVisible));
            }
        }

        public bool IsPortSectionVisible => SelectedProtocol.SupportsPorts;
        public bool IsIcmpSectionVisible => SelectedProtocol.SupportsIcmp;

        public FirewallRuleViewModel()
        {
            SelectedProtocol = ProtocolTypes.Any;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}