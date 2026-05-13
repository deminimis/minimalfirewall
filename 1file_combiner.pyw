import tkinter as tk
from tkinter import filedialog, ttk, messagebox
import os

PROJECT_SOURCE_PATH = r"C:\Users\anon\PROGRAMMING\C#\SimpleFirewall\VS Minimal Firewall\MinimalFirewall-NET8\MinimalFirewall-Git\src"
OUTPUT_FILENAME = "1allfiles.txt"

PRESETS = {
    "1. Tab: Dashboard (Listener & Popups)": [
        "DashboardControl.cs", "DashboardControl.Designer.cs",
        "NotifierForm.cs", "NotifierForm.Designer.cs",
        "FirewallEventListenerService.cs",
        "MainViewModel.cs",
        "FirewallActionsService.cs",
        "BackgroundFirewallTaskService.cs",
        "DataModels.cs",
        "TypedObjects.cs",
        "MFWConstants.cs"
    ],
    "2. Tab: Rules (Management & Wizards)": [
        "RulesControl.cs", "RulesControl.Designer.cs",
        "CreateAdvancedRuleForm.cs", "CreateAdvancedRuleForm.Designer.cs",
        "CreateProgramRuleForm.cs", "CreateProgramRuleForm.Designer.cs",
        "RuleWizardForm.cs", "RuleWizardForm.Designer.cs",
        "FirewallRuleService.cs",
        "FirewallActionsService.cs",
        "FirewallDataService.cs",
        "DataModels.cs",
        "TypedObjects.cs",
        "MFWConstants.cs"
    ],
    "3. Tab: Wildcards (Dynamic Logic)": [
        "WildcardRulesControl.cs", "WildcardRulesControl.Designer.cs",
        "WildcardCreatorForm.cs", "WildcardCreatorForm.Designer.cs",
        "WildCardRuleService.cs",
        "FirewallActionsService.cs",
        "PathResolver.cs",
        "DataModels.cs",
        "MFWConstants.cs"
    ],
    "4. Tab: Audit (Sentry & Changes)": [
        "AuditControl.cs", "AuditControl.Designer.cs",
        "FirewallSentryService.cs",
        "ForeignRuleTracker.cs",
        "FirewallSnapshotService.cs",
        "MainViewModel.cs",
        "DataModels.cs",
        "TypedObjects.cs"
    ],
    "5. Tab: Live Connections (Traffic)": [
        "LiveConnectionsControl.cs", "LiveConnectionsControl.Designer.cs",
        "FirewallTraffic.cs",
        "TrafficMonitorViewModel.cs",
        "IconService.cs",
        "FirewallActionsService.cs",
        "DataModels.cs",
        "TypedObjects.cs"
    ],
    "6. Tab: Groups": [
        "GroupsControl.cs", "GroupsControl.Designer.cs",
        "FirewallGroups.cs",
        "FirewallRuleService.cs",
        "DataModels.cs"
    ],
    "7. Tab: Settings & App Infrastructure": [
        "SettingsControl.cs", "SettingsControl.Designer.cs",
        "AppSettings.cs",
        "Appdata.cs",
        "StartupService.cs",
        "UserActivityLogger.cs",
        "PublisherWhitelistService.cs",
        "UtilityServices.cs",
        "Program.cs",
        "MainForm.cs"
    ],
    "8. UI Framework & Custom Controls": [
        "DarkModeCS.cs",
        "AdaptiveUI.cs",
        "FlatComboBox.cs",
        "FlatProgressBar.cs",
        "FlatTabControl.cs",
        "SortableBindingList.cs",
        "StatusForm.cs",
        "StatusForm.Designer.cs",
        "Program.cs",
        "MainForm.Designer.cs"
    ],
    "9. FULL BACKEND (No UI)": [
        "FirewallRuleService.cs",
        "FirewallDataService.cs",
        "FirewallActionsService.cs",
        "BackgroundFirewallTaskService.cs",
        "FirewallSentryService.cs",
        "FirewallEventListenerService.cs",
        "WildCardRuleService.cs",
        "SystemDiscoveryService.cs",
        "DataModels.cs",
        "TypedObjects.cs"
    ]
}

class FileCombinerApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Minimal Firewall - Context Combiner")
        self.files = []

        self.root.columnconfigure(0, weight=1)
        self.root.rowconfigure(3, weight=1)

        self.main_frame = ttk.Frame(self.root, padding="10")
        self.main_frame.grid(row=0, column=0, sticky=(tk.W, tk.E, tk.N, tk.S))
        self.main_frame.columnconfigure(0, weight=1)
        
        title_label = ttk.Label(self.main_frame, text="Firewall Code Context Loader", font=("Segoe UI", 14, "bold"))
        title_label.grid(row=0, column=0, pady=(0, 10), sticky=tk.W)

        preset_frame = ttk.LabelFrame(self.main_frame, text="Select AI Context", padding="10")
        preset_frame.grid(row=1, column=0, sticky=(tk.W, tk.E), pady=(0, 10))
        preset_frame.columnconfigure(1, weight=1)

        ttk.Label(preset_frame, text="I need help with:").grid(row=0, column=0, padx=(0, 10))
        
        self.preset_combo = ttk.Combobox(preset_frame, values=list(PRESETS.keys()), state="readonly", width=40)
        self.preset_combo.grid(row=0, column=1, sticky=(tk.W, tk.E))
        self.preset_combo.current(0)

        load_btn = ttk.Button(preset_frame, text="Load Context Files", command=self.load_preset)
        load_btn.grid(row=0, column=2, padx=(10, 0))

        button_frame = ttk.Frame(self.main_frame)
        button_frame.grid(row=2, column=0, sticky=(tk.W, tk.E), pady=(0, 5))
        
        self.browse_files_button = ttk.Button(button_frame, text="Add Manual Files", command=self.browse_files)
        self.browse_files_button.pack(side=tk.LEFT, padx=(0, 5))

        self.skeleton_button = ttk.Button(button_frame, text="Add Code Skeleton", command=self.generate_and_add_skeleton)
        self.skeleton_button.pack(side=tk.LEFT, padx=5)

        self.remove_button = ttk.Button(button_frame, text="Remove Selected", command=self.remove_selected)
        self.remove_button.pack(side=tk.LEFT, padx=5)

        self.clear_button = ttk.Button(button_frame, text="Clear List", command=self.clear_list)
        self.clear_button.pack(side=tk.RIGHT, padx=5)

        self.tree = ttk.Treeview(self.root, columns=("File", "Path"), show="headings")
        self.tree.heading("File", text="File Name")
        self.tree.heading("Path", text="Full Path")
        self.tree.column("File", width=250)
        self.tree.column("Path", width=500)
        self.tree.grid(row=3, column=0, padx=10, sticky=(tk.W, tk.E, tk.N, tk.S))

        scrollbar = ttk.Scrollbar(self.root, orient=tk.VERTICAL, command=self.tree.yview)
        self.tree.configure(yscroll=scrollbar.set)
        scrollbar.grid(row=3, column=1, sticky=(tk.N, tk.S))

        self.combine_button = ttk.Button(self.root, text=f"Generate {OUTPUT_FILENAME}", command=self.combine_files)
        self.combine_button.grid(row=4, column=0, columnspan=2, pady=10, padx=10, sticky=(tk.W, tk.E))

        self.status_var = tk.StringVar()
        self.status_var.set(f"Project Source: {PROJECT_SOURCE_PATH}")
        status_bar = ttk.Label(self.root, textvariable=self.status_var, relief=tk.SUNKEN, anchor=tk.W, font=("Segoe UI", 8))
        status_bar.grid(row=5, column=0, columnspan=2, sticky=(tk.W, tk.E))

    def load_preset(self):
        selection = self.preset_combo.get()
        if not selection:
            return
        
        files_to_load = PRESETS.get(selection, [])
        loaded_count = 0
        
        for filename in files_to_load:
            full_path = os.path.join(PROJECT_SOURCE_PATH, filename)
            
            if os.path.exists(full_path):
                if full_path not in self.files:
                    self.files.append(full_path)
                    self.tree.insert("", tk.END, values=(filename, full_path))
                    loaded_count += 1
            else:
                found = False
                for root, dirs, files in os.walk(PROJECT_SOURCE_PATH):
                    if filename in files:
                        full_path = os.path.join(root, filename)
                        if full_path not in self.files:
                            self.files.append(full_path)
                            self.tree.insert("", tk.END, values=(filename, full_path))
                            loaded_count += 1
                            found = True
                        break
                if not found:
                    print(f"Warning: Could not find {filename}")

        self.status_var.set(f"Added {loaded_count} new files. Total: {len(self.files)}")

    def browse_files(self):
        file_paths = filedialog.askopenfilenames(initialdir=PROJECT_SOURCE_PATH)
        excluded_extensions = ('.resx', '.csproj.user', '.png', '.tmp')

        for file_path in file_paths:
            if file_path.lower().endswith(excluded_extensions):
                continue
            if file_path not in self.files:
                self.files.append(file_path)
                file_name = os.path.basename(file_path)
                self.tree.insert("", tk.END, values=(file_name, file_path))

    def remove_selected(self):
        selected_items = self.tree.selection()
        if not selected_items:
            return
            
        for item in selected_items:
            values = self.tree.item(item, 'values')
            file_path = values[1]
            if file_path in self.files:
                self.files.remove(file_path)
            self.tree.delete(item)
        self.status_var.set(f"Removed {len(selected_items)} files.")

    def clear_list(self):
        for item in self.tree.get_children():
            self.tree.delete(item)
        self.files.clear()
        self.status_var.set("List cleared.")

    def combine_files(self):
        if not self.files:
            messagebox.showwarning("No Files", "Please load a context preset or add files first.")
            return

        script_dir = os.path.dirname(os.path.abspath(__file__))
        output_file = os.path.join(script_dir, OUTPUT_FILENAME)
        skeleton_file = os.path.join(script_dir, "project_code_skeleton.txt")

        # Auto-regenerate skeleton if it is in the list
        if skeleton_file in self.files:
            self.build_skeleton(skeleton_file)
     
        try:
            with open(output_file, 'w', encoding='utf-8', errors='ignore') as outfile:
                outfile.write("CONTEXT SUMMARY:\n")
                outfile.write(f"Total Files: {len(self.files)}\n")
                outfile.write("-" * 30 + "\n")

                for f in self.files:
                    outfile.write(f"- {os.path.basename(f)}\n")
                outfile.write("=" * 50 + "\n\n")

                for file_path in self.files:
                    full_path = os.path.abspath(file_path).replace(os.sep, '/')
                    outfile.write(f"" + "-" * 50 + "\n")
                    outfile.write(f"// Full Path: {full_path}\n")
                    outfile.write("-" * 80 + "\n")

                    file_content = None
                    encodings_to_try = ['utf-8-sig', 'utf-8', 'cp1252', 'latin-1']
                    
                    for encoding in encodings_to_try:
                        try:
                            with open(file_path, 'r', encoding=encoding) as infile:
                                file_content = infile.read()
                            break
                        except (UnicodeDecodeError, TypeError):
                            continue
                    
                    if file_content is not None:
                        outfile.write(file_content)
                    else:
                        outfile.write(f"// Note: Could not decode file.\n")
                    outfile.write("\n\n")
            
            # The app stays open and just updates the status bar
            self.status_var.set(f"SUCCESS: {OUTPUT_FILENAME} updated.")
            
        except Exception as e:
            messagebox.showerror("Error", f"Failed to combine files: {str(e)}")

    def generate_and_add_skeleton(self):
        script_dir = os.path.dirname(os.path.abspath(__file__))
        skeleton_file = os.path.join(script_dir, "project_code_skeleton.txt")
        
        # Run the skeleton extraction
        self.build_skeleton(skeleton_file)
        
        # Add to list
        if os.path.exists(skeleton_file):
            if skeleton_file not in self.files:
                self.files.append(skeleton_file)
                self.tree.insert("", tk.END, values=("project_code_skeleton.txt", skeleton_file))
            self.status_var.set("Added Code Skeleton to list.")

    def build_skeleton(self, output_path):
        exclude_dirs = {'.git', 'node_modules', 'dist', 'build', '__pycache__', 'public', '.supabase', 'bin', 'obj'}
        allowed_exts = {'.go', '.ts', '.tsx', '.sql', '.cs'}
        
        with open(output_path, 'w', encoding='utf-8') as out:
            out.write("=== ARCHITECTURE SKELETON ===\n")
            out.write("This file contains all file paths and their structural definitions.\n\n")
            
            for root_dir, dirs, files in os.walk(PROJECT_SOURCE_PATH):
                dirs[:] = [d for d in dirs if d not in exclude_dirs]
                
                for file in files:
                    ext = os.path.splitext(file)[1].lower()
                    if ext in allowed_exts:
                        filepath = os.path.join(root_dir, file)
                        rel_path = os.path.relpath(filepath, PROJECT_SOURCE_PATH).replace('\\', '/')
                        
                        sigs = self.extract_signatures(filepath, ext)
                        if sigs:
                            out.write(f"\n📂 {rel_path}\n")
                            for sig in sigs:
                                out.write(f"    - {sig}\n")

    def extract_signatures(self, filepath, ext):
        signatures = []
        try:
            with open(filepath, 'r', encoding='utf-8') as f:
                for line in f:
                    stripped = line.strip()
                    
                    # Go
                    if ext == '.go':
                        if stripped.startswith('func ') or stripped.startswith('type '):
                            signatures.append(stripped.rstrip('{').strip())
                    # TS/React
                    elif ext in ['.ts', '.tsx']:
                        if (stripped.startswith('export const ') or 
                            stripped.startswith('export function ') or 
                            stripped.startswith('export interface ') or 
                            stripped.startswith('export type ') or 
                            stripped.startswith('export default function') or
                            stripped.startswith('class ')):
                            signatures.append(stripped.rstrip('{').strip())
                    # SQL
                    elif ext == '.sql':
                        lower_strip = stripped.lower()
                        if lower_strip.startswith('create table') or lower_strip.startswith('create or replace function'):
                            signatures.append(stripped.rstrip('(').strip())
                    # C#
                    elif ext == '.cs':
                        if (stripped.startswith('public class ') or 
                            stripped.startswith('public interface ') or
                            (stripped.startswith('public ') and '(' in stripped and ')' in stripped and not stripped.endswith(';')) or
                            (stripped.startswith('private ') and '(' in stripped and ')' in stripped and not stripped.endswith(';'))):
                            signatures.append(stripped.split('{')[0].strip())
        except Exception:
            pass
        return signatures

if __name__ == "__main__":
    root = tk.Tk()
    root.geometry("800x600")
    app = FileCombinerApp(root)
    root.mainloop()