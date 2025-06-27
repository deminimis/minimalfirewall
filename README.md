<h1>
  Minimal Firewall
  <a href="https://github.com/deminimis/minimalfirewall">
    <img src="https://github.com/deminimis/minimalfirewall/blob/main/assets/logo1.png" alt="Logo" width="100" height="100">
  </a>
</h1>

Minimal Firewall works in conjuncton with Windows Firewall to block all unknown connections by default, prompting you to create application based rules to mitigate malware, telemetry, and data leaks. Completely rid yourself of apps phoning home without your knowledge.



### 💾 Download the latest version [here](https://github.com/deminimis/minimalfirewall/releases)

## Core Features

* **Lockdown Mode:** The core of Minimal Firewall. Enable this mode to block all network connections without Windows Firewall rules by default. No program connects without your explicit say-so.

* **Real-Time Connection Alerts:** Get instant notifications when a blocked program tries to connect, allowing you to make a decision on the spot. You can choose whether to have annoying pop-ups or quiet, in-application notifications. 

* **Simple Rule Creation:** Allow or block any application permanently with a single click, or temporarily allow or block. 

* **Wildcard Rules:** Easily manage programs that update frequently (like web browsers) by creating rules that apply to any version within a specific folder.

* **UWP & Service Support:** Manage rules for modern Windows Store apps and background system services, not just traditional desktop programs.

* **Light & Dark Themes:** A clean user interface that's easy on the eyes, day or night.

* **100% Local and Private:** Minimal Firewall contains no telemetry, does not connect to the internet itself, and all your rules and logs are stored locally on your machine.

* **Portable:** Minimal Firewall can be copied as a portable app, and you can even delete all the rules created by it in the app.


## User Guide
The program is very intuitive, but for a concise user guide, see the [wiki](https://github.com/deminimis/minimalfirewall/wiki/Minimal-Firewall-User-Guide). 


## Why Use Minimal Firewall?

Minimal Firewall offers a more secure and integrated approach by interacting directly with the native Windows Firewall, eliminating the need for risky system modifications. Unlike Fort Firewall, which compromises system security by requiring core isolation to be disabled—exposing the kernel to potential exploits—Minimal Firewall preserves critical OS protections. Compared to Simplewall, which uses low-level WFP filters and introduces a larger attack surface with potential for conflicts or misconfigurations, Minimal Firewall leverages the stability, auditing, and trusted enforcement of the built-in firewall engine. This mitigates potential risks from WFP, such as potential exploits from malformed network packets or bugs in custom packet-parsing code. And unlike Tinywall, which fails to alert users of new or unauthorized outbound connections, Minimal Firewall provides real-time visibility and control, maintaining both usability and security without sacrificing system integrity.

|   |   |   |   |   |
|---|---|---|---|---|
|Feature|Minimal Firewall|TinyWall|SimpleWall|Fort Firewall|
|**Size**|~1MB|~2MB|~1MB|~6MB|
|**Portability**|✅|❌|✅|✅|
|**Core Isolation Revoked?**|No|No|No|Yes|
|**Connection Alerts**|✅|❌|✅|✅|
|**Open Source**|✅|✅|✅|✅|
|**Wildcards**|✅|❌|❌|✅|

## Screenshots

### You can choose to view pending connections silently inside the app or as an annoying popup.
![pending connection tab](https://github.com/deminimis/minimalfirewall/blob/main/assets/Screenshot%202025-06-24%20094754.png)

![popup](https://github.com/deminimis/minimalfirewall/blob/main/assets/Screenshot%202025-06-24%20095134.png) 

### Includes both light and dark themes

![light theme UWP](https://github.com/deminimis/minimalfirewall/blob/main/assets/Screenshot%202025-06-24%20095637.png)

![light theme services](https://github.com/deminimis/minimalfirewall/blob/main/assets/Screenshot%202025-06-24%20095730.png) 

### Easy or advanced rule creation

![Easy](https://github.com/deminimis/minimalfirewall/blob/main/assets/Screenshot%202025-06-24%20095751.png)

![Advanced](https://github.com/deminimis/minimalfirewall/blob/main/assets/Screenshot%202025-06-24%20095821.png)

## Security by Default

By leveraging the battle-tested Windows Defender Firewall, Minimal Firewall avoids reinventing critical security components, unlike WFP-based tools that inject code into the network stack or otherwise bypass the standard firewall arbitration logic and group policy enforcement, with potential brittle or buggy logic.

Using native Windows Firewall rules is more secure because they integrate directly with the Windows Filtering Platform at a privileged level, ensuring that traffic control policies are enforced consistently across the entire system—including during early boot stages and before third-party services initialize. These rules benefit from tight coupling with core Windows security subsystems (like IPsec, Group Policy, and service hardening), making them less susceptible to tampering or race conditions introduced by external tools. Additionally, Microsoft continuously hardens the native firewall with OS-level updates, offering a level of protection and stability that user-space or third-party implementations often lack.

* No Service Required: Creates persistent Windows Firewall rules, eliminating the need for a background service.
* No Network Activity: The application makes no network connections, ensuring no telemetry, update checks, or "phoning home."

### Secure Rule Creation
* Follows Microsoft's [best practices](https://support.microsoft.com/en-us/windows/risks-of-allowing-apps-through-windows-firewall-654559af-3f54-3dcf-349f-71ccd90bcc5c) for Windows Defender Firewall management, using application-based rules instead of risky port-based rules.
* Rules are:
  * Program-Specific: Tied to an executable path (e.g., C:\Path\To\program.exe) or a UWP app's Package Family Name (PFN).
  * Dynamic Ports: Allows apps to use necessary ports only while running, closing them when the app terminates.
  * Directional: Supports inbound or outbound control to enforce least privilege.
* Prevents malicious programs from impersonating allowed apps, enhancing security.
* Rules for Universal Windows Platform apps use secure Package Family Names (PFNs) to prevent bypass attempts.

## Technical Notes
Some system files in C:\Windows\System32 are owned by accounts like TrustedInstaller, limiting access even with admin privileges. 


## Architecture


Minimal Firewall is a WPF-based graphical user interface, written in C# against the .NET Framework 4.8, that serves as a robust management layer for the underlying Windows Filtering Platform (WFP). It does not use any custom kernel drivers, instead relying entirely on documented Windows APIs for maximum stability and security. Rule atomicity for multi-rule actions (e.g., "Allow All") is handled by deleting existing rules for a given application path before creating new ones, preventing rule conflicts.

The application's core efficacy is derived from its implementation of a default-deny outbound policy on all active network profiles (NET_FW_PROFILE_TYPE2_). This is set via the INetFwPolicy2 COM interface. Rather than employing complex packet-filtering drivers, the application leverages the native capabilities of WFP's auditing feature. Firewall rules are not stored in a proprietary format but are materialized directly as INetFwRule2 objects via the NetFwTypeLib COM Interop library. This ensures native integration, performance, and visibility within the standard "Windows Defender Firewall with Advanced Security" console. 



## Contributing

Contributions are welcome! Please submit issues or pull requests to the GitHub repository. If you have questions, feel free to submit an issue. 
Ensure code adheres to the project's security and simplicity principles.


## License

Minimal Firewall is licensed under the GNU Affero General Public License v3 (AGPL v3). For commercial or proprietary licensing, please contact me. 


![Virus Total](https://github.com/deminimis/minimalfirewall/blob/main/assets/Screenshot%202025-06-25%20093921.png)
