﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace HardenWindowsSecurity;

public partial class GUIMain
{

	// Partial class definition for handling navigation and view models
	public partial class NavigationVM : ViewModelBase
	{

		// Method to handle the Logs view, including loading
		private void BitLockerView(object obj)
		{
			// Check if the view is already cached
			if (_viewCache.TryGetValue("BitLockerView", out var cachedView))
			{
				CurrentView = cachedView;
				return;
			}

			if (GlobalVars.path is null)
			{
				throw new InvalidOperationException("GlobalVars.path cannot be null.");
			}

			// if Admin privileges are not available, return and do not proceed any further
			// Will prevent the page from being loaded since the CurrentView won't be set/changed
			if (!UserPrivCheck.IsAdmin())
			{
				Logger.LogMessage("BitLocker page can only be used when running the Harden Windows Security Application with Administrator privileges", LogTypeIntel.ErrorInteractionRequired);
				return;
			}

			// Construct the file path for the Logs view XAML
			string xamlPath = Path.Combine(GlobalVars.path, "Resources", "XAML", "BitLocker.xaml");

			// Read the XAML content from the file
			string xamlContent = File.ReadAllText(xamlPath);

			// Parse the XAML content to create a UserControl
			GUIBitLocker.View = (UserControl)XamlReader.Parse(xamlContent);

			// Find the Parent Grid
			GUIBitLocker.ParentGrid = (Grid)GUIBitLocker.View.FindName("ParentGrid");

			GUIBitLocker.TabControl = GUIBitLocker.ParentGrid.FindName("TabControl") as TabControl ?? throw new InvalidOperationException("TabControl could not be found");

			if (GUIBitLocker.TabControl.FindName("OSDriveGrid") is not Grid OSDriveGrid ||
				GUIBitLocker.TabControl.FindName("NonOSDrivesGrid") is not Grid NonOSDrivesGrid ||
				GUIBitLocker.TabControl.FindName("RemovableDrivesGrid") is not Grid RemovableDrivesGrid ||
				GUIBitLocker.TabControl.FindName("BackupGrid") is not Grid BackupGrid)
			{
				throw new InvalidOperationException("BitLocker view grids could not be found");
			}


			#region OS Drives

			GUIBitLocker.TextBlockStartupKeySelection = OSDriveGrid.FindName("TextBlockStartupKeySelection") as TextBlock;
			GUIBitLocker.BitLockerSecurityLevelComboBox = OSDriveGrid.FindName("BitLockerSecurityLevelComboBox") as ComboBox ?? throw new InvalidOperationException("BitLockerSecurityLevelComboBox could not be found");
			GUIBitLocker.PIN1 = OSDriveGrid.FindName("PIN1") as PasswordBox ?? throw new InvalidOperationException("PIN1 password box could not be found");
			GUIBitLocker.PIN2 = OSDriveGrid.FindName("PIN2") as PasswordBox ?? throw new InvalidOperationException("PIN2 password box could not be found");
			GUIBitLocker.RefreshRemovableDrivesInOSDriveSection = OSDriveGrid.FindName("RefreshRemovableDrivesInOSDriveSection") as Button ?? throw new InvalidOperationException("RefreshRemovableDrivesInOSDriveSection button could not be found");
			Image? RefreshButtonIcon1 = OSDriveGrid.FindName("RefreshButtonIcon1") as Image ?? throw new InvalidOperationException("RefreshButtonIcon1 could not be found");
			GUIBitLocker.RemovableDrivesComboBox = OSDriveGrid.FindName("RemovableDrivesComboBox") as ComboBox ?? throw new InvalidOperationException("RemovableDrivesComboBox could not be found");
			Button OSDriveEncryptButton = OSDriveGrid.FindName("OSDriveEncryptButton") as Button ?? throw new InvalidOperationException("OSDriveEncryptButton could not be found.");

			// Event handler for when the refresh button is pressed
			GUIBitLocker.RefreshRemovableDrivesInOSDriveSection.Click += async (sender, e) =>
			{
				await Task.Run(() =>
				{

					// Get the Removable drives list
					List<BitLocker.BitLockerVolume>? UndeterminedRemovableDrivesList = BitLocker.GetAllEncryptedVolumeInfo(false, true);
					// Only get the writable removable drives
					GUIBitLocker.RemovableDrivesList = VolumeWritabilityCheck.GetWritableVolumes(UndeterminedRemovableDrivesList);

					// Update the ComboBox with the removable drives using Application's Dispatcher
					app.Dispatcher.Invoke(() =>
					{
						GUIBitLocker.RemovableDrivesComboBox.ItemsSource = GUIBitLocker.RemovableDrivesList?.Select(D => D.MountPoint);
					});
				});
			};

			// Updates UI elements in the OS Drive section for Enhanced Level
			static void UpdateEnhancedLevelElements()
			{
				// Using the Application dispatcher
				app.Dispatcher.Invoke(() =>
				{
					// Retrieve the ComboBoxItem
					var selectedItem = GUIBitLocker.BitLockerSecurityLevelComboBox!.SelectedItem;
					if (selectedItem is ComboBoxItem comboBoxItem)
					{
						// Get the actual string content from ComboBoxItem
						string selectedString = comboBoxItem.Content!.ToString()!;

						// Make sure the Startup key related elements are only enabled for the Enhanced security level
						if (string.Equals(selectedString, "Normal", StringComparison.OrdinalIgnoreCase))
						{
							GUIBitLocker.RefreshRemovableDrivesInOSDriveSection!.IsEnabled = false;
							GUIBitLocker.RemovableDrivesComboBox!.IsEnabled = false;
							GUIBitLocker.TextBlockStartupKeySelection!.Opacity = 0.3;
							GUIBitLocker.RefreshRemovableDrivesInOSDriveSection.Opacity = 0.4;
							GUIBitLocker.RemovableDrivesComboBox.Opacity = 0.4;
						}
						else
						{
							GUIBitLocker.RefreshRemovableDrivesInOSDriveSection!.IsEnabled = true;
							GUIBitLocker.RemovableDrivesComboBox!.IsEnabled = true;
							GUIBitLocker.TextBlockStartupKeySelection!.Opacity = 1;
							GUIBitLocker.RefreshRemovableDrivesInOSDriveSection.Opacity = 1;
							GUIBitLocker.RemovableDrivesComboBox.Opacity = 1;
						}
					}
				});
			}

			// Run this once during GUI load to enable/disable the elements properly
			UpdateEnhancedLevelElements();

			// Event handler for security level selection
			GUIBitLocker.BitLockerSecurityLevelComboBox.SelectionChanged += (sender, e) =>
			{
				UpdateEnhancedLevelElements();
			};


			#endregion


			#region Non-OS Drives
			GUIBitLocker.RefreshNonOSDrives = NonOSDrivesGrid.FindName("RefreshNonOSDrives") as Button ?? throw new InvalidOperationException("RefreshNonOSDrives button could not be found");
			Image? RefreshButtonIcon2 = NonOSDrivesGrid.FindName("RefreshButtonIcon2") as Image ?? throw new InvalidOperationException("RefreshButtonIcon2 could not be found");
			GUIBitLocker.NonOSDrivesComboBox = NonOSDrivesGrid.FindName("NonOSDrivesComboBox") as ComboBox ?? throw new InvalidOperationException("NonOSDrivesComboBox button could not be found");
			Button NonOSDriveEncryptButton = NonOSDrivesGrid.FindName("NonOSDriveEncryptButton") as Button ?? throw new InvalidOperationException("NonOSDriveEncryptButton button could not be found");

			// Event handler for when the refresh button is pressed
			GUIBitLocker.RefreshNonOSDrives.Click += async (sender, e) =>
			{
				await Task.Run(() =>
				{
					// Get the Non-OS drives list
					GUIBitLocker.NonOSDrivesList = BitLocker.GetAllEncryptedVolumeInfo(true, false);

					// Update the ComboBox with the Non-OS drives using Application's Dispatcher
					app.Dispatcher.Invoke(() =>
					{
						GUIBitLocker.NonOSDrivesComboBox.ItemsSource = GUIBitLocker.NonOSDrivesList.Select(D => $"{D.MountPoint}");
					});
				});
			};

			#endregion



			#region Removable Drives
			GUIBitLocker.RefreshRemovableDrivesForRemovableDrivesSection = RemovableDrivesGrid.FindName("RefreshRemovableDrivesForRemovableDrivesSection") as Button ?? throw new InvalidOperationException("RefreshRemovableDrivesForRemovableDrivesSection button could not be found");
			Image? RefreshButtonIcon3 = RemovableDrivesGrid.FindName("RefreshButtonIcon3") as Image ?? throw new InvalidOperationException("RefreshButtonIcon3 could not be found");
			GUIBitLocker.RemovableDrivesInRemovableDrivesGridComboBox = RemovableDrivesGrid.FindName("RemovableDrivesInRemovableDrivesGridComboBox") as ComboBox ?? throw new InvalidOperationException("RemovableDrivesInRemovableDrivesGridComboBox button could not be found");
			GUIBitLocker.Password1 = RemovableDrivesGrid.FindName("Password1") as PasswordBox ?? throw new InvalidOperationException("Password1 password box could not be found");
			GUIBitLocker.Password2 = RemovableDrivesGrid.FindName("Password2") as PasswordBox ?? throw new InvalidOperationException("Password2 password box could not be found");
			Button RemovableDriveEncryptButton = NonOSDrivesGrid.FindName("RemovableDriveEncryptButton") as Button ?? throw new InvalidOperationException("RemovableDriveEncryptButton button could not be found");


			// Event handler for when the refresh button is pressed
			GUIBitLocker.RefreshRemovableDrivesForRemovableDrivesSection.Click += async (sender, e) =>
			{
				await Task.Run(() =>
				{
					// Get the Removable drives list
					List<BitLocker.BitLockerVolume>? UndeterminedRemovableDrivesList = BitLocker.GetAllEncryptedVolumeInfo(false, true);
					// Only get the writable removable drives
					GUIBitLocker.RemovableDrivesList = VolumeWritabilityCheck.GetWritableVolumes(UndeterminedRemovableDrivesList);

					// Update the ComboBox with the Removable drives using Application's Dispatcher
					app.Dispatcher.Invoke(() =>
					{
						GUIBitLocker.RemovableDrivesInRemovableDrivesGridComboBox.ItemsSource = GUIBitLocker.RemovableDrivesList?.Select(D => D.MountPoint);
					});
				});
			};

			#endregion


			#region Backup

			GUIBitLocker.RecoveryKeysDataGrid = BackupGrid.FindName("RecoveryKeysDataGrid") as DataGrid ?? throw new InvalidOperationException("RecoveryKeysDataGrid could not be found");
			GUIBitLocker.BackupButton = BackupGrid.FindName("BackupButton") as Button ?? throw new InvalidOperationException("BackupButton could not be found");
			GUIBitLocker.RefreshButtonForBackup = BackupGrid.FindName("RefreshButtonForBackup") as Button ?? throw new InvalidOperationException("RefreshButtonForBackup could not be found");
			Image? RefreshButtonForBackupIcon = BackupGrid.FindName("RefreshButtonForBackupIcon") as Image ?? throw new InvalidOperationException("RefreshButtonForBackupIcon could not be found");
			Image? BackupButtonIcon = BackupGrid.FindName("BackupButtonIcon") as Image ?? throw new InvalidOperationException("BackupButtonIcon could not be found");

			// Add image to the BackupButtonIcon
			BitmapImage BackupButtonIconBitmapImage = new();
			BackupButtonIconBitmapImage.BeginInit();
			BackupButtonIconBitmapImage.UriSource = new Uri(Path.Combine(GlobalVars.path!, "Resources", "Media", "ExportIconBlack.png"));
			BackupButtonIconBitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Load the image data into memory
			BackupButtonIconBitmapImage.EndInit();
			BackupButtonIcon.Source = BackupButtonIconBitmapImage;

			#region Make the scrolling using mouse wheel or trackpad work on DataGrid
			// The DataGrid needs to hand over the scrolling event to the main ScrollViewer element

			// Find the ScrollViewer element
			ScrollViewer MainScrollViewer = GUIBitLocker.ParentGrid.FindName("MainScrollViewer") as ScrollViewer ?? throw new InvalidOperationException("No scrollbar founds");

			// Handle the PreviewMouseWheel event of the DataGrid by handing it off to the main ScrollViewer
			GUIBitLocker.RecoveryKeysDataGrid.PreviewMouseWheel += (object sender, MouseWheelEventArgs e) =>
			{
				if (!e.Handled)
				{
					e.Handled = true;

					MouseWheelEventArgs eventArg = new(e.MouseDevice, e.Timestamp, e.Delta)
					{
						RoutedEvent = UIElement.MouseWheelEvent,
						Source = sender
					};

					MainScrollViewer.RaiseEvent(eventArg);
				}
			};

			#endregion


			// Event handler to refresh the recovery key info in the DataGrid
			GUIBitLocker.RefreshButtonForBackup.Click += async (sender, e) =>
			{
				// Perform the main tasks on another thread to avoid freezing the GUI
				await Task.Run(() =>
				{
					GUIBitLocker.CreateBitLockerVolumeViewModel(false);
				});
			};

			// Event handler to export and backup the recovery keys to a file
			GUIBitLocker.BackupButton.Click += async (sender, e) =>
			{
				// Perform the main tasks on another thread to avoid freezing the GUI
				await Task.Run(() =>
				{
					GUIBitLocker.CreateBitLockerVolumeViewModel(true);
				});
			};


			#endregion

			// Add the same Refresh image to multiple sources
			BitmapImage RefreshButtonIcon1BitmapImage = new();
			RefreshButtonIcon1BitmapImage.BeginInit();
			RefreshButtonIcon1BitmapImage.UriSource = new Uri(Path.Combine(GlobalVars.path!, "Resources", "Media", "RefreshButtonIcon.png"));
			RefreshButtonIcon1BitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Load the image data into memory
			RefreshButtonIcon1BitmapImage.EndInit();
			RefreshButtonIcon1.Source = RefreshButtonIcon1BitmapImage;
			RefreshButtonIcon2.Source = RefreshButtonIcon1BitmapImage;
			RefreshButtonIcon3.Source = RefreshButtonIcon1BitmapImage;
			RefreshButtonForBackupIcon.Source = RefreshButtonIcon1BitmapImage;

			// Register the buttons and TabControl that will be enabled/disabled based on current activity
			ActivityTracker.RegisterUIElement(OSDriveEncryptButton);
			ActivityTracker.RegisterUIElement(NonOSDriveEncryptButton);
			ActivityTracker.RegisterUIElement(RemovableDriveEncryptButton);
			ActivityTracker.RegisterUIElement(GUIBitLocker.TabControl);



			// To ensure BitLocker settings have been applied prior to using BitLocker encryption
			static void applyGroupPolicies()
			{

				if (!BitLocker.PoliciesApplied)
				{

					// if LGPO doesn't already exist in the working directory, then download it
					if (!Path.Exists(GlobalVars.LGPOExe))
					{
						Logger.LogMessage("LGPO.exe doesn't exist, downloading it.", LogTypeIntel.Information);
						AsyncDownloader.PrepDownloadedFiles(GlobalVars.LGPOExe, null, null, true);
					}
					else
					{
						Logger.LogMessage("LGPO.exe already exists, skipping downloading it.", LogTypeIntel.Information);
					}

					// Apply the BitLocker group policies
					BitLockerSettings.Invoke();

					// Refresh the group policies to apply the changes instantly
					ProcessStarter.RunCommand("GPUpdate.exe", "/force");

					// Set the flag to true so this section won't happen again
					BitLocker.PoliciesApplied = true;
				}
				else
				{
					Logger.LogMessage("BitLocker group policies already applied.", LogTypeIntel.Information);
				}

			}


			// OS Drive tab's Encrypt button event handler
			OSDriveEncryptButton.Click += async (sender, e) =>
			{

				// Only continue if there is no activity other places
				if (ActivityTracker.IsActive)
				{
					return;
				}

				try
				{

					// mark as activity started
					ActivityTracker.IsActive = true;

					// Reset this flag to false indicating no errors Occurred so far
					BitLocker.HasErrorsOccurred = false;

					string? SecurityLevel = null;
					// OS Drive - PINs
					string? PIN1 = null;
					string? PIN2 = null;
					// OS Drive - Removable Drive ComboBox
					string? RemovableDriveLetter = null;

					// Using the Application dispatcher to query UI elements' values only
					app.Dispatcher.Invoke(() =>
					{

						// Retrieve the ComboBoxItem of the Security Level in OS Drive tab
						// Because we are using the index to access it
						var selectedItem = GUIBitLocker.BitLockerSecurityLevelComboBox!.SelectedItem;
						if (selectedItem is ComboBoxItem comboBoxItem)
						{
							// Get the actual string content from ComboBoxItem
							SecurityLevel = comboBoxItem.Content?.ToString()!;
						}

						// Get the PIN values as plain texts since CIM needs them that way
						PIN1 = GUIBitLocker.PIN1.Password;
						PIN2 = GUIBitLocker.PIN2.Password;

						// Retrieve the ComboBoxItem of the Removable drive in the OS Drive tab
						RemovableDriveLetter = GUIBitLocker.RemovableDrivesComboBox!.SelectedItem?.ToString();
					});


					await Task.Run(() =>
					{

						applyGroupPolicies();

						Logger.LogMessage($"Executing BitLocker Ops for the OS Drive with {SecurityLevel} security level.", LogTypeIntel.Information);

						if (string.IsNullOrWhiteSpace(PIN1) || string.IsNullOrWhiteSpace(PIN2))
						{
							Logger.LogMessage("Both PIN boxes must be entered.", LogTypeIntel.ErrorInteractionRequired);
							return;
						}

						// Make sure the PINs match
						if (!string.Equals(PIN1, PIN2, StringComparison.OrdinalIgnoreCase))
						{
							Logger.LogMessage("PINs don't match.", LogTypeIntel.ErrorInteractionRequired);
							return;
						}
						{
							Logger.LogMessage($"PINs matched.", LogTypeIntel.Information);
						}

						// Get the system directory path
						string systemDirectory = Environment.SystemDirectory;

						// Extract the drive letter
						string systemDrive = Path.GetPathRoot(systemDirectory) ?? throw new InvalidOperationException("System/OS drive letter could not be found");

						string TrimmedSystemDrive = systemDrive.TrimEnd('\\');

						// Determine the security level of the OS encryption
						if (string.Equals(SecurityLevel, "Normal", StringComparison.OrdinalIgnoreCase))
						{
							BitLocker.Enable(TrimmedSystemDrive, BitLocker.OSEncryptionType.Normal, PIN1, null, true);
						}
						else
						{
							if (string.IsNullOrWhiteSpace(RemovableDriveLetter))
							{
								Logger.LogMessage("No Removable Drive selected for the Enhanced security level.", LogTypeIntel.ErrorInteractionRequired);
								return;
							}

							BitLocker.Enable(TrimmedSystemDrive, BitLocker.OSEncryptionType.Enhanced, PIN1, RemovableDriveLetter, true);
						}


						if (!BitLocker.HasErrorsOccurred)
						{
							// Display notification at the end if no errors occurred
							ToastNotification.Show(ToastNotification.Type.EndOfBitLocker, null, null, null, "Operation System Drive");
						}

					}); // End of Async Thread

				}
				finally
				{
					// mark as activity completed
					ActivityTracker.IsActive = false;
				}
			};


			// Non-OS Drive tab's Encrypt button event handler
			NonOSDriveEncryptButton.Click += async (sender, e) =>
			{

				// Only continue if there is no activity other places
				if (ActivityTracker.IsActive)
				{
					return;
				}

				try
				{
					// mark as activity started
					ActivityTracker.IsActive = true;

					// Reset this flag to false indicating no errors Occurred so far
					BitLocker.HasErrorsOccurred = false;

					// Drives ComboBox
					string? NonOSDrivesLetter = null;

					// Using the Application dispatcher to query UI elements' values only
					app.Dispatcher.Invoke(() =>
					{
						// Retrieve the ComboBoxItem in the Non-OS Drives tab
						NonOSDrivesLetter = GUIBitLocker.NonOSDrivesComboBox!.SelectedItem?.ToString();

					});

					await Task.Run(() =>
					{

						applyGroupPolicies();

						if (NonOSDrivesLetter is null)
						{
							Logger.LogMessage("No Non-OS Drive selected", LogTypeIntel.ErrorInteractionRequired);
							return;
						}

						Logger.LogMessage($"Executing BitLocker Ops for the Non-OS Drives on drive {NonOSDrivesLetter} .", LogTypeIntel.Information);

						BitLocker.Enable(NonOSDrivesLetter, true);


						if (!BitLocker.HasErrorsOccurred)
						{
							// Display notification at the end if no errors occurred
							ToastNotification.Show(ToastNotification.Type.EndOfBitLocker, null, null, null, "Non-OS Drive");
						}

						return;

					}); // End of Async Thread

				}

				finally
				{
					// mark as activity completed
					ActivityTracker.IsActive = false;
				}
			};



			// Removable Drive tab's Encrypt button event handler
			RemovableDriveEncryptButton.Click += async (sender, e) =>
			{

				// Only continue if there is no activity other places
				if (ActivityTracker.IsActive)
				{
					return;
				}

				try
				{

					// mark as activity started
					ActivityTracker.IsActive = true;

					// Reset this flag to false indicating no errors Occurred so far
					BitLocker.HasErrorsOccurred = false;

					// Removable Drive ComboBox
					string? RemovableDrivesTabDriveSelection = null;
					// Passwords
					string? Password1 = null;
					string? Password2 = null;

					// Using the Application dispatcher to query UI elements' values only
					app.Dispatcher.Invoke(() =>
					{
						// Retrieve the ComboBoxItem in the Removable Drives tab
						RemovableDrivesTabDriveSelection = GUIBitLocker.RemovableDrivesInRemovableDrivesGridComboBox!.SelectedItem?.ToString();

						// Get the Password values as plain texts since CIM needs them way that
						Password1 = GUIBitLocker.Password1.Password;
						Password2 = GUIBitLocker.Password2.Password;
					});

					await Task.Run(() =>
					{
						applyGroupPolicies();

						Logger.LogMessage($"Executing BitLocker Ops for the Removable Drives on drive {RemovableDrivesTabDriveSelection} .", LogTypeIntel.Information);

						if (string.IsNullOrWhiteSpace(Password1) || string.IsNullOrWhiteSpace(Password2))
						{
							Logger.LogMessage("Both Password boxes must be entered.", LogTypeIntel.ErrorInteractionRequired);
							return;
						}

						// Make sure the Passwords match
						if (!string.Equals(Password1, Password2, StringComparison.OrdinalIgnoreCase))
						{
							Logger.LogMessage("Passwords don't match.", LogTypeIntel.ErrorInteractionRequired);
							return;
						}
						{
							Logger.LogMessage($"Passwords matched.", LogTypeIntel.Information);
						}

						if (RemovableDrivesTabDriveSelection is null)
						{
							Logger.LogMessage("No Removable Drive selected", LogTypeIntel.ErrorInteractionRequired);
							return;
						}

						BitLocker.Enable(RemovableDrivesTabDriveSelection, Password1, true);


						if (!BitLocker.HasErrorsOccurred)
						{
							// Display notification at the end if no errors occurred
							ToastNotification.Show(ToastNotification.Type.EndOfBitLocker, null, null, null, "Removable Drive");
						}

					}); // End of Async Thread

				}

				finally
				{
					// mark as activity completed
					ActivityTracker.IsActive = false;
				}
			};


			// Cache the view before setting it as the CurrentView
			_viewCache["BitLockerView"] = GUIBitLocker.View;

			// Set the CurrentView to the Protect view
			CurrentView = GUIBitLocker.View;
		}
	}
}
