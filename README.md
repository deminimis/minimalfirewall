<h1>
  Minimal Firewall
  <a href="https://github.com/deminimis/minimalfirewall">
    <img src="https://github.com/deminimis/minimalfirewall/blob/main/assets/logo1.png" alt="Logo" width="100" height="100">
  </a>
</h1>


Minimal Firewall enhances the built-in Windows Firewall to block all unknown network connections by default, giving you complete control. It prompts you for action when an application tries to connect, allowing you to mitigate malware, stop unwanted telemetry, and prevent data leaks. With Minimal Firewall, no application "phones home" without your explicit permission.

### 💾 Download the latest version [here](https://github.com/deminimis/minimalfirewall/releases)

## Core Features

- **Lockdown Mode:** The heart of Minimal Firewall. When enabled, it configures the Windows Firewall to block all outbound connections that don't have an explicit "Allow" rule. No program connects without your say-so.
    
- **Real-Time Connection Alerts:** Get instant notifications when a blocked program attempts network access. Choose between interactive pop-ups for immediate action or silent, in-app alerts on the dashboard to review later.
    
- **Simple & Advanced Rule Creation:**
    
    - **Program Rules:** Allow or block applications with a single click.
        
    - **Advanced Rules:** Create detailed rules based on protocol (TCP/UDP/ICMP), local/remote ports, IP addresses, services, and network profiles (Domain, Private, Public).
        
- **Firewall Auditing:** The Audit tab shows you a log of firewall rules that were created, modified, or deleted by other applications, giving you visibility into background changes.
    
- **Live Traffic Monitoring:** The "Live Connections" tab displays all active TCP connections on your system in real-time, showing which process is connected to which remote address.
    
- **Wildcard Rules:** Easily manage applications that update frequently (like web browsers) by creating rules that apply to any executable within a specific folder.
    
- **UWP & Service Support:** Manage rules for modern Windows Store (UWP) apps and background system services, not just traditional desktop programs.
    
- **Light & Dark Themes:** A clean, modern user interface that's easy on the eyes, day or night.
    
- **100% Local and Private:** Minimal Firewall contains no telemetry, does not connect to the internet, and stores all rules and logs locally on your machine.
    
- **Portable:** Minimal Firewall is a single executable that requires no installation. All rules are native to Windows Firewall, so no custom drivers or services are left behind.
    

## User Guide

The program is designed to be intuitive. For a concise user guide, see the [wiki](https://github.com/deminimis/minimalfirewall/wiki/Minimal-Firewall-User-Guide).

## Why Use Minimal Firewall?

Minimal Firewall offers a secure and integrated approach by managing the native Windows Firewall, eliminating the need for custom drivers or risky system modifications.

|Feature|Minimal Firewall|TinyWall|SimpleWall|Fort Firewall|
|---|---|---|---|---|
|**Size**|~2MB|~2MB|~1MB|~6MB|
|**Portability**|✅|❌|✅|✅|
|**Requires Core Isolation Off?**|No|No|No|Yes|
|**Connection Alerts**|✅|❌|✅|✅|
|**Advanced Rule Editor**|✅|❌|✅|✅|
|**Firewall Change Auditing**|✅|❌|❌|❌|
|**Wildcards**|✅|❌|❌|✅|
|**Open Source**|✅|✅|✅|✅|
|**Avoids low-level filters**||✅|✅|❌|❌|



## Screenshots

#### The Dashboard shows blocked connections for you to manage.

#### Or, choose to get interactive pop-up notifications.

#### Includes both light and dark themes.

#### Create simple program rules or powerful, detailed advanced rules.

## FAQ

1. **Do I need to keep the app running?**
    
    - No. Once you enable Lockdown Mode, the policy is set in the Windows Firewall itself. You can close Minimal Firewall completely, and the block-by-default rule will remain active. You only need to run the app when you want to authorize a new program or change a rule.
        

## Security by Default

By leveraging the battle-tested Windows Defender Firewall, Minimal Firewall avoids reinventing the wheel. It uses documented Microsoft APIs to ensure stability and security.

- **No Service Required:** Creates persistent Windows Firewall rules, eliminating the need for its own background service.
    
- **No Network Activity:** The application itself makes no network connections. No telemetry, no update checks, no "phoning home."
    
- **Auditing:** Allows you to see if other applications silently add or change rules in the Windows Firewall.
    

### Secure Rule Creation

- Follows Microsoft's [best practices](https://support.microsoft.com/en-us/windows/risks-of-allowing-apps-through-windows-firewall-654559af-3f54-3dcf-349f-71ccd90bcc5c) for firewall management by favoring application-based rules over risky port-based rules.
    
- Rules are program-specific, tied to an executable's path or a UWP app's Package Family Name, preventing malicious programs from impersonating an allowed app on the same port.
    

## Technical Architecture

Minimal Firewall is a **Windows Forms** application written in **C#** on the **.NET 8** platform. It serves as a user-friendly management layer for the native **Windows Firewall with Advanced Security**.

- **Core Interaction:** It uses the `NetFwTypeLib` COM Interop library to interact with the `INetFwPolicy2` interface, which is the standard API for managing Windows Firewall rules and policies.
    
- **Connection Alerting:** It listens for Event ID `5157` ("The Windows Filtering Platform has blocked a connection") in the Windows Security event log. This is a native, efficient way to detect blocked connection attempts without a custom driver.
    
- **Auditing:** It uses a `ManagementEventWatcher` (WMI) to monitor for real-time changes to the `MSFT_NetFirewallRule` class, allowing it to detect when other processes modify the firewall ruleset.
    
- **Live Traffic:** The live connection monitor uses the `GetExtendedTcpTable` function from `iphlpapi.dll` to retrieve a list of active TCP connections and their associated Process IDs.
    
- **No Drivers:** It does not use any custom kernel drivers, relying entirely on documented Windows APIs for maximum stability and security.

## Special Thanks
For dark theme, Minimal Firewall uses a modified version of [Dark-Mode-Forms](https://github.com/BlueMystical/Dark-Mode-Forms). 
    

## Contributing

Contributions are welcome! Please submit issues or pull requests to the GitHub repository. If you have questions, feel free to open an issue.

## License

Minimal Firewall is licensed under the GNU Affero General Public License v3 (AGPL v3). For commercial or proprietary licensing, please contact me.
