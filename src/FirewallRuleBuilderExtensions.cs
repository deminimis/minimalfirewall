// FirewallRuleBuilderExtensions.cs
using System;
using System.Linq;
using MinimalFirewall.TypedObjects;
using NetFwTypeLib;

namespace MinimalFirewall
{
    public static class FirewallRuleBuilderExtensions
    {
        public static INetFwRule2 WithName(this INetFwRule2 rule, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Rule name cannot be null or empty.", nameof(name));

            rule.Name = name;
            return rule;
        }

        public static INetFwRule2 WithDescription(this INetFwRule2 rule, string description)
        {
            rule.Description = description;
            return rule;
        }

        public static INetFwRule2 WithDirection(this INetFwRule2 rule, Directions direction)
        {
            rule.Direction = (NET_FW_RULE_DIRECTION_)direction;
            return rule;
        }

        public static INetFwRule2 WithAction(this INetFwRule2 rule, Actions action)
        {
            rule.Action = (NET_FW_ACTION_)action;
            return rule;
        }


        public static INetFwRule2 WithProtocol(this INetFwRule2 rule, int protocol)
        {
            rule.Protocol = protocol;
            return rule;
        }


        public static INetFwRule2 WithProtocol(this INetFwRule2 rule, NET_FW_IP_PROTOCOL_ protocol)
        {
            rule.Protocol = (int)protocol;
            return rule;
        }

        public static INetFwRule2 WithLocalPorts(this INetFwRule2 rule, string ports)
        {
            rule.LocalPorts = ports;
            return rule;
        }

        // single port using int
        public static INetFwRule2 WithLocalPort(this INetFwRule2 rule, int port)
        {
            rule.LocalPorts = port.ToString();
            return rule;
        }


        // multiple ports using array
        public static INetFwRule2 WithLocalPorts(this INetFwRule2 rule, params int[] ports)
        {
            if (ports == null || ports.Length == 0)
                throw new ArgumentException("Must provide at least one port.", nameof(ports));

            rule.LocalPorts = string.Join(",", ports);
            return rule;
        }

        public static INetFwRule2 WithRemotePorts(this INetFwRule2 rule, string ports)
        {
            rule.RemotePorts = ports;
            return rule;
        }

        // Set profiles (Domain, Private, Public).
        public static INetFwRule2 WithProfiles(this INetFwRule2 rule, NET_FW_PROFILE_TYPE2_ profiles)
        {
            rule.Profiles = (int)profiles;
            return rule;
        }

        public static INetFwRule2 ForApplication(this INetFwRule2 rule, string applicationPath)
        {
            rule.ApplicationName = applicationPath;
            return rule;
        }

        public static INetFwRule2 ForService(this INetFwRule2 rule, string serviceName)
        {
            rule.serviceName = serviceName;
            return rule;
        }

        public static INetFwRule2 WithGrouping(this INetFwRule2 rule, string group)
        {
            rule.Grouping = group;
            return rule;
        }

        public static INetFwRule2 IsEnabled(this INetFwRule2 rule, bool enabled = true)
        {
            rule.Enabled = enabled;
            return rule;
        }
    }
}