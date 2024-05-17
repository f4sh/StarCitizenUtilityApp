using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
namespace StarCitizenUtilityApp
{
    class Program
    {
        private static Dictionary<string, string> pyroSchedule = new Dictionary<string, string>
        {

            ["2023-10-31T19:00:00"] = "UTC",
            ["2023-11-01T13:00:00"] = "UTC",
            ["2023-11-03T06:00:00"] = "UTC",
            ["2023-11-04T13:00:00"] = "UTC",
            ["2023-11-06T07:00:00"] = "UTC",
            ["2023-11-06T20:00:00"] = "UTC",
            ["2023-11-09T07:00:00"] = "UTC",
            ["2023-11-09T20:00:00"] = "UTC",
            ["2023-11-10T14:00:00"] = "UTC",
        };
        private static Dictionary<string, string> buyBackTokenSchedule = new Dictionary<string, string>
        {
            ["2024-01-08T17:00:00"] = "UTC",
            ["2024-04-08T16:00:00"] = "UTC",
            ["2024-07-08T16:00:00"] = "UTC",
            ["2024-10-07T16:00:00"] = "UTC"
        };
        static async Task Main(string[] args)
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("Welcome to the Star Citizen Utility App.\n");
                Console.WriteLine("It is advisable to regularly clear your cache,\nespecially immediately following the release of a new game build or hotfix.\n");
                Console.WriteLine("1. Clear Star Citizen and Nvidia cache files.");
                Console.WriteLine("2. Clear Star Citizen and AMD cache files.");
                Console.WriteLine("3. Manage your screenshots.");
                Console.WriteLine("4. Create a backup or delete the 'USER' folder in the game build.");
                Console.WriteLine("5. Check a Star Citizen user profile data.");
                Console.WriteLine("6. View the 2024 Buy Back Token Schedule.");
                Console.WriteLine("7. Check Pyro Technical Preview Build server opening times. [Based on data from Nov 1st, 2023]");
                Console.WriteLine("8. Exit\n");
                Console.Write("Select an option: ");
                string? option = Console.ReadLine();
                switch (option)
                {
                    case "1":
                        DeleteNvidiaCacheFiles();
                        break;
                    case "2":
                        DeleteAmdCacheFiles();
                        break;
                    case "3":
                        DeleteScreenShots();
                        break;
                    case "4":
                        ZipUserFolderToDesktop();
                        break;
                    case "5":
                        await CheckStarCitizenProfile();
                        break;
                    case "6":
                        CheckBuyBackTokenSchedule();
                        break;
                    case "7":
                        CheckServerOpeningTimes();
                        break;
                    case "8":
                        running = false;
                        Console.Clear();
                        Console.WriteLine("           ________");
                        Console.WriteLine("          |        \\");
                        Console.WriteLine("  ______   \\$$$$$$$$");
                        Console.WriteLine(" /      \\     /  $$");
                        Console.WriteLine("|  $$$$$$\\   /  $$");
                        Console.WriteLine("| $$  | $$  /  $$");
                        Console.WriteLine("| $$__/ $$ /  $$");
                        Console.WriteLine(" \\$$    $$|  $$");
                        Console.WriteLine("  \\$$$$$$  \\$$");
                        Console.WriteLine();
                        Console.WriteLine("o7 See you in the 'verse!");
                        Console.WriteLine("App will close in 2 seconds...");
                        Thread.Sleep(2000);
                        break;
                    default:
                        Console.WriteLine("Invalid option, please try again.");
                        break;
                }
                Console.Clear();
            }
        }
        static void DeleteNvidiaCacheFiles()
        {
            Console.Clear();
            ClearErrorLog();
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Star Citizen"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA", "DXCache"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA", "GLCache"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NVIDIA", "OptixCache"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "D3DSCache"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cache"));
            Console.WriteLine("\nStar Citizen and Nvidia cache deletion process is complete.");
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
            Console.Clear();
        }
        static void DeleteAmdCacheFiles()
        {
            Console.Clear();
            ClearErrorLog();
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Star Citizen"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AMD", "DXCache"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AMD", "GLCache"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AMD", "VkCache"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "D3DSCache"));
            DeleteCacheFolder(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cache"));
            Console.WriteLine("\nStar Citizen and AMD cache deletion process is complete.");
            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
            Console.Clear();
        }
        static string? FindStarCitizenFolder()
        {
            List<string> likelyPaths = new List<string>();

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    likelyPaths.Add(drive.Name);
                }
            }

            likelyPaths.Add(@"C:\Program Files\");
            likelyPaths.Add(@"C:\Program Files (x86)\");
            likelyPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            likelyPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            likelyPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            foreach (string basePath in likelyPaths)
            {
                string searchPath = Path.Combine(basePath, "Roberts Space Industries", "StarCitizen");
                if (Directory.Exists(searchPath))
                {
                    return searchPath;
                }
            }
            return null;
        }
        static IEnumerable<string> GetGameBuilds(string starCitizenFolderPath)
        {
            var directories = Directory.GetDirectories(starCitizenFolderPath);
            foreach (var dir in directories)
            {
                yield return new DirectoryInfo(dir).Name;
            }
        }
        static void ZipUserFolderToDesktop()
        {
            Console.Clear();
            ClearErrorLog();
            string? starCitizenFolderPath = FindStarCitizenFolder();
            if (string.IsNullOrEmpty(starCitizenFolderPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Star Citizen folder not found.");
                Console.ResetColor();
                return;
            }
            string[] validChoices = GetGameBuilds(starCitizenFolderPath).ToArray();
            if (validChoices.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No game builds found.");
                Console.ResetColor();
                return;
            }
            Console.WriteLine("Do you want to backup or delete the USER folder?");
            Console.WriteLine("1: Backup\n2: Delete");
            string? actionChoice = Console.ReadLine();
            if (actionChoice == "1")
            {
                Console.WriteLine("\nSelect the game build you want to backup:");
                for (int i = 0; i < validChoices.Length; i++)
                {
                    Console.WriteLine($"{i + 1}: {validChoices[i]}");
                }
                string? userInput = Console.ReadLine();
                if (!int.TryParse(userInput, out int choice) || choice < 1 || choice > validChoices.Length)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid choice. Backup process aborted.");
                    Console.ResetColor();
                    return;
                }
                BackupUserFolder(starCitizenFolderPath, validChoices[choice - 1]);
            }
            else if (actionChoice == "2")
            {
                Console.WriteLine("Select the game build you want to delete the USER folder from:");
                Console.WriteLine("1: Specific game build\n2: All builds");
                string? deleteOption = Console.ReadLine();
                if (deleteOption == "1")
                {
                    Console.WriteLine("Select the specific game build:");
                    for (int i = 0; i < validChoices.Length; i++)
                    {
                        Console.WriteLine($"{i + 1}: {validChoices[i]}");
                    }
                    string? buildChoice = Console.ReadLine();
                    if (!int.TryParse(buildChoice, out int buildIndex) || buildIndex < 1 || buildIndex > validChoices.Length)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid choice. Deletion process aborted.");
                        Console.ResetColor();
                        return;
                    }
                    DeleteUserFolder(starCitizenFolderPath, validChoices[buildIndex - 1]);
                }
                else if (deleteOption == "2")
                {
                    foreach (string build in validChoices)
                    {
                        DeleteUserFolder(starCitizenFolderPath, build);
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid choice. Deletion process aborted.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid action choice. Operation aborted.");
                Console.ResetColor();
            }
            Console.ResetColor();
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
            Console.Clear();
        }
        static void BackupUserFolder(string starCitizenFolderPath, string buildChoice)
        {
            string userFolderPath = Path.Combine(starCitizenFolderPath, buildChoice, "USER");
            if (!Directory.Exists(userFolderPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Star Citizen USER folder for {buildChoice} not found.");
                Console.ResetColor();
                return;
            }
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string zipFilePath = Path.Combine(desktopPath, $"StarCitizen_USER_{buildChoice}_Backup.zip");
            try
            {
                if (File.Exists(zipFilePath))
                {
                    Console.WriteLine("\nA backup file already exists on the desktop. Overwriting...");
                    File.Delete(zipFilePath);
                }
                ZipFile.CreateFromDirectory(userFolderPath, zipFilePath);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nBackup created successfully at: {zipFilePath}");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nError during backup: {ex.Message}");
            }
            finally
            {
                Console.ResetColor();
            }
        }
        static void DeleteUserFolder(string baseFolderPath, string build)
        {
            string pathToDelete = Path.Combine(baseFolderPath, build, "USER");
            if (!Directory.Exists(pathToDelete))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"USER folder not found for {build} build.");
                Console.ResetColor();
                return;
            }
            try
            {
                Directory.Delete(pathToDelete, true);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Deleted USER folder for {build}.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to delete USER folder for {build}: {ex.Message}");
            }
            finally
            {
                Console.ResetColor();
            }
        }
        static void DeleteScreenShots()
        {
            Console.Clear();
            ClearErrorLog();
            string? baseFolder = FindStarCitizenFolder();
            if (baseFolder == null)
            {
                Console.WriteLine("Star Citizen folder not found.");
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
                Console.Clear();
                return;
            }
            string[] validChoices = GetGameBuilds(baseFolder).ToArray();
            if (validChoices.Length == 0)
            {
                Console.WriteLine("No game builds found for screenshots deletion.");
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
                Console.Clear();
                return;
            }
            Console.WriteLine("Select an option:");
            Console.WriteLine("1: Provide a list of all the currently available screenshots for all game builds.");
            Console.WriteLine("2: Delete screenshots from a specific game build");
            Console.WriteLine("3: Delete screenshots from all builds");
            string? userOption = Console.ReadLine();
            if (userOption == "2")
            {
                Console.WriteLine("Select the specific game build:");
                for (int i = 0; i < validChoices.Length; i++)
                {
                    Console.WriteLine($"{i + 1}: {validChoices[i]}");
                }
                string? buildChoice = Console.ReadLine();
                if (!int.TryParse(buildChoice, out int buildIndex) || buildIndex < 1 || buildIndex > validChoices.Length)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid choice. Deletion process aborted.");
                    Console.ResetColor();
                    return;
                }
                DeleteScreenShotsFromBuild(baseFolder, validChoices[buildIndex - 1]);
            }
            else if (userOption == "3")
            {
                Console.WriteLine("Are you sure you want to delete Star Citizen screenshots for all game builds? (yes/no)");
                string? userInput = Console.ReadLine();
                if (userInput != null && userInput.Trim().ToLower() == "yes")
                {
                    foreach (var build in validChoices)
                    {
                        DeleteScreenShotsFromBuild(baseFolder, build);
                    }
                    Console.WriteLine("\nStar Citizen screenshots deletion process is complete.");
                }
                else
                {
                    Console.WriteLine("\nDeletion process canceled.");
                }
            }
            else if (userOption == "1")
            {
                ListAllScreenShots(baseFolder, validChoices);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid choice. Process aborted.");
                Console.ResetColor();
            }
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
            Console.Clear();
        }
        static void ListAllScreenShots(string baseFolderPath, string[] builds)
        {
            Console.WriteLine("Listing screenshots for all game builds:");
            foreach (var build in builds)
            {
                ListScreenShotsFromBuild(baseFolderPath, build);
            }
            Console.WriteLine("\nScreenshots listing process is complete.");
        }
        static void ListScreenShotsFromBuild(string baseFolderPath, string build)
        {
            string screenShotsPath = Path.Combine(baseFolderPath, build, "ScreenShots");
            string[] screenshotFiles = Directory.Exists(screenShotsPath) ? Directory.GetFiles(screenShotsPath) : new string[0];
            int screenshotCount = screenshotFiles.Length;
            if (screenshotCount > 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Screenshots for {build} build [{screenshotCount}]:");
                foreach (var screenshot in screenshotFiles)
                {
                    Console.WriteLine(Path.GetFileName(screenshot));
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No screenshots found for {build} build.");
            }
            Console.ResetColor();
        }
        static void DeleteScreenShotsFromBuild(string baseFolderPath, string build)
        {
            string screenShotsPath = Path.Combine(baseFolderPath, build, "ScreenShots");
            if (Directory.Exists(screenShotsPath) && Directory.EnumerateFiles(screenShotsPath).Any())
            {
                DeleteCacheFolder(screenShotsPath);
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No screenshots found for {build} build.");
            }
            Console.ResetColor();
        }
        static void DeleteCacheFolder(string path)
        {
            string folderName = Path.GetFileName(path);
            string? parentPath = Path.GetDirectoryName(path);
            string parentFolderName = parentPath != null ? Path.GetFileName(parentPath) : string.Empty;
            string combinedFolderName = !string.IsNullOrEmpty(parentFolderName) ? $"{parentFolderName} {folderName}" : folderName;
            bool isFolderExistent = Directory.Exists(path);
            if (isFolderExistent)
            {
                DirectoryInfo di = new DirectoryInfo(path);
                foreach (FileInfo file in di.GetFiles("*", SearchOption.AllDirectories))
                {
                    try
                    {
                        file.Delete();
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{file.Name} deleted.");
                        Console.ResetColor();
                    }
                    catch (IOException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Cannot delete (in use): {file.Name}");
                        Console.ResetColor();
                        File.AppendAllText("error_log.txt", $"{DateTime.Now}: Cannot delete (in use): {file.FullName}\n");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failed to delete: {file.Name} - Error: {ex.Message}");
                        Console.ResetColor();
                        File.AppendAllText("error_log.txt", $"{DateTime.Now}: Failed to delete: {file.FullName} - Error: {ex.Message}\n");
                    }
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{dir.Name} deleted.");
                        Console.ResetColor();
                    }
                    catch (IOException)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Cannot delete (in use): {dir.Name}");
                        Console.ResetColor();
                        File.AppendAllText("error_log.txt", $"{DateTime.Now}: Cannot delete (in use): {dir.FullName}\n");
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failed to delete: {dir.Name} - Error: {ex.Message}");
                        Console.ResetColor();
                        File.AppendAllText("error_log.txt", $"{DateTime.Now}: Failed to delete: {dir.FullName} - Error: {ex.Message}\n");
                    }
                }

                if (Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length > 0 || Directory.GetDirectories(path).Length > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{combinedFolderName} partially cleared. Some items could not be deleted (see error log for details).");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{combinedFolderName} completely cleared.");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{combinedFolderName} does not exist or has already been deleted.");
                Console.ResetColor();
            }
        }
        static void CheckBuyBackTokenSchedule()
        {
            Console.Clear();
            TimeZoneInfo localZone = TimeZoneInfo.Local;
            Console.WriteLine($"Your Local Timezone: {localZone.StandardName}");
            DateTime? nextEventTime = null;
            TimeSpan? timeLeft = null;
            bool isEventToday = false;
            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);
            foreach (var entry in buyBackTokenSchedule)
            {
                DateTime utcTime = DateTime.ParseExact(entry.Key, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                DateTime localEventTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);

                if (localNow.Date == localEventTime.Date)
                {
                    nextEventTime = localEventTime;
                    timeLeft = nextEventTime - localNow;
                    isEventToday = true;
                    break;
                }
                else if (localNow < localEventTime)
                {
                    nextEventTime = localEventTime;
                    timeLeft = nextEventTime - localNow;
                    break;
                }
            }
            if (isEventToday && timeLeft.HasValue)
            {
                Console.Write("The next Buy Back Token event is ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("TODAY!");
                Console.ResetColor();
                Console.WriteLine();
                Console.Write("Time left until the event starts: ");
                Console.ForegroundColor = ConsoleColor.Green;
                TimeSpan timeLeftValue = timeLeft.GetValueOrDefault();
                Console.WriteLine($"{timeLeftValue.Hours} hours, {timeLeftValue.Minutes} minutes, and {timeLeftValue.Seconds} seconds");
                Console.ResetColor();
            }
            else if (nextEventTime.HasValue)
            {
                Console.Write("The next Buy Back Token distribution is scheduled on ");
                Console.ForegroundColor = ConsoleColor.Green;
                DateTime nextEventTimeValue = nextEventTime.GetValueOrDefault();
                Console.WriteLine(nextEventTimeValue.ToString("f", CultureInfo.CreateSpecificCulture("en-US")));
                Console.ResetColor();
                Console.Write("Time remaining until the event: ");
                Console.ForegroundColor = ConsoleColor.Green;
                TimeSpan timeLeftValue = timeLeft.GetValueOrDefault();
                Console.WriteLine($"{timeLeftValue.Days} days, {timeLeftValue.Hours} hours, {timeLeftValue.Minutes} minutes, and {timeLeftValue.Seconds} seconds");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("No upcoming Buy Back Token distribution is scheduled or the schedule is out of date.");
            }

            AskForTimeZoneCheck(ScheduleType.BuyBackToken);
        }
        static void CheckServerOpeningTimes()
        {
            Console.Clear();

            TimeZoneInfo localZone = TimeZoneInfo.Local;
            Console.WriteLine($"Your Local Timezone: {localZone.StandardName}");

            DateTime? nextOpeningTime = null;
            TimeSpan? timeLeft = null;
            bool isServerOpen = false;
            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local);
            foreach (var entry in pyroSchedule)
            {
                DateTime utcTime = DateTime.ParseExact(entry.Key, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                DateTime localStartTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);
                DateTime localEndTime = localStartTime.AddHours(8);
                if (localNow >= localStartTime && localNow <= localEndTime)
                {
                    nextOpeningTime = localEndTime;
                    timeLeft = nextOpeningTime - localNow;
                    isServerOpen = true;
                    break;
                }
                else if (localNow < localStartTime)
                {
                    nextOpeningTime = localStartTime;
                    timeLeft = nextOpeningTime - localNow;
                    break;
                }
            }
            if (isServerOpen && timeLeft.HasValue)
            {
                Console.Write("The Pyro Technical Preview Build server is ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("OPEN now!");
                Console.ResetColor();
                Console.WriteLine();
                Console.Write("Test time remaining: ");
                Console.ForegroundColor = ConsoleColor.Green;
                TimeSpan timeLeftValue = timeLeft.GetValueOrDefault();
                Console.WriteLine($"{timeLeftValue.Hours} hours, {timeLeftValue.Minutes} minutes, and {timeLeftValue.Seconds} seconds");
                Console.ResetColor();
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
                Console.Clear();
            }
            else if (nextOpeningTime.HasValue)
            {
                Console.Write("The next Pyro Technical Preview Build server is scheduled to open on ");
                Console.ForegroundColor = ConsoleColor.Green;
                DateTime nextOpeningTimeValue = nextOpeningTime.GetValueOrDefault();
                Console.WriteLine(nextOpeningTimeValue.ToString("f", CultureInfo.CreateSpecificCulture("en-US")));
                Console.ResetColor();
                Console.Write("Time remaining until opening: ");
                Console.ForegroundColor = ConsoleColor.Green;
                TimeSpan timeLeftValue = timeLeft.GetValueOrDefault();
                Console.WriteLine($"{timeLeftValue.Days} days, {timeLeftValue.Hours} hours, {timeLeftValue.Minutes} minutes, and {timeLeftValue.Seconds} seconds");
                Console.ResetColor();

                Console.Write("Would you like to check a different time zone? (yes/no): ");
                string response = Console.ReadLine()?.ToLower() ?? string.Empty;
                if (response == "yes")
                {
                    ListTimeZoneOptions(ScheduleType.Pyro);
                }
                else
                {
                    Console.WriteLine("Press any key to return to the main menu...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
            else
            {
                Console.WriteLine("No upcoming server openings are scheduled or the schedule is out of date.");
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
                Console.Clear();
            }
        }
        enum ScheduleType
        {
            Pyro,
            BuyBackToken
        }
        static void AskForTimeZoneCheck(ScheduleType scheduleType)
        {
            Console.Write($"Would you like to check the {(scheduleType == ScheduleType.Pyro ? "Pyro server" : "Buy Back Token distribution")} times in a different time zone? (yes/no): ");
            string response = Console.ReadLine()?.ToLower() ?? string.Empty;
            if (response == "yes")
            {
                ListTimeZoneOptions(scheduleType);
            }
            else
            {
                Console.WriteLine("Press any key to return to the main menu...");
                Console.ReadKey();
                Console.Clear();
            }
        }
        static void ListTimeZoneOptions(ScheduleType scheduleType)
        {
            ReadOnlyCollection<TimeZoneInfo> timeZones = TimeZoneInfo.GetSystemTimeZones();
            for (int i = 0; i < timeZones.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {timeZones[i].DisplayName}");
            }
            Console.WriteLine("Enter the number of the time zone you want to check:");
            if (int.TryParse(Console.ReadLine(), out int timeZoneSelection) && timeZoneSelection >= 1 && timeZoneSelection <= timeZones.Count)
            {
                DisplayTimesForSelectedTimeZone(timeZones[timeZoneSelection - 1], scheduleType);
            }
            else
            {
                Console.WriteLine("Invalid selection. Please enter a number from the list.");
            }
        }
        static void DisplayTimesForSelectedTimeZone(TimeZoneInfo selectedTimeZone, ScheduleType scheduleType)
        {
            Console.Clear();
            Dictionary<string, string> currentSchedule = scheduleType == ScheduleType.Pyro ? pyroSchedule : buyBackTokenSchedule;

            DateTime? nextEventTime = null;
            TimeSpan? timeLeft = null;

            DateTime nowInSelectedTimeZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, selectedTimeZone);

            foreach (var entry in currentSchedule)
            {
                DateTime utcTime = DateTime.ParseExact(entry.Key, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                DateTime selectedZoneTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, selectedTimeZone);

                if (nowInSelectedTimeZone < selectedZoneTime)
                {
                    nextEventTime = selectedZoneTime;
                    timeLeft = nextEventTime.Value - nowInSelectedTimeZone;
                    break;
                }
            }

            Console.WriteLine($"Chosen Timezone: {selectedTimeZone.DisplayName}");
            if (nextEventTime.HasValue && timeLeft.HasValue)
            {
                Console.Write($"The next {(scheduleType == ScheduleType.Pyro ? "Pyro Technical Preview Build server" : "Buy Back Token distribution")} is scheduled to open on ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{nextEventTime.Value.ToString("f", CultureInfo.CreateSpecificCulture("en-US"))} ({selectedTimeZone.StandardName})");
                Console.ResetColor();
                Console.Write($"Time remaining until {(scheduleType == ScheduleType.Pyro ? "opening" : "tokens distribution")}: ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{timeLeft.Value.Days} days, {timeLeft.Value.Hours} hours, {timeLeft.Value.Minutes} minutes, and {timeLeft.Value.Seconds} seconds");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"No upcoming {(scheduleType == ScheduleType.Pyro ? "server openings" : "Buy Back Token distribution")} is scheduled or the schedule is out of date.");
            }
            Console.WriteLine("Press any key to return to the main menu...");
            Console.ReadKey();
            Console.Clear();
        }
        static void ClearErrorLog()
        {
            string errorLogPath = "error_log.txt";
            if (File.Exists(errorLogPath))
            {
                try
                {
                    File.Delete(errorLogPath);
                    Console.WriteLine("Previous error logs cleared.");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to clear error logs. Error: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }
        static async Task CheckStarCitizenProfile()
        {
            Console.Write("Enter Star Citizen Username: ");
            string username = Console.ReadLine() ?? string.Empty;

            if (!Regex.IsMatch(username, "^[a-zA-Z0-9-_]+$"))
            {
                Console.WriteLine("Invalid username format. Please enter a username containing only letters, numbers, dashes, and underscores.");
                Console.ReadKey();
                Console.Clear();
                return;
            }

            string url = $"https://robertsspaceindustries.com/citizens/{username}";
            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var profileNode = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='profile-content overview-content clearfix']");

            if (profileNode != null)
            {
                var citizenRecordNode = profileNode.SelectSingleNode(".//p[@class='entry citizen-record']/strong[@class='value']");
                var citizenRecord = citizenRecordNode?.InnerText.Trim();
                var nicknameNode = profileNode.SelectSingleNode(".//div[@class='info']/p[@class='entry']/strong[@class='value']");
                var nickname = nicknameNode?.InnerText.Trim();
                var handleNameNode = profileNode.SelectSingleNode(".//div[@class='profile left-col']//p[@class='entry'][span[@class='label']='Handle name']/strong[@class='value']");
                var handleName = handleNameNode?.InnerText.Trim();
                var badgeNode = profileNode.SelectSingleNode(".//div[@class='profile left-col']//p[@class='entry'][span[@class='icon']]/span[@class='value']");
                var badge = badgeNode?.InnerText.Trim();
                var enlistedDateNode = profileNode.SelectSingleNode(".//div[@class='left-col']//p[@class='entry'][span[@class='label']='Enlisted']/strong[@class='value']");
                var enlistedDate = enlistedDateNode?.InnerText.Trim();
                var locationNode = profileNode.SelectSingleNode(".//div[@class='left-col']//p[@class='entry'][span[@class='label']='Location']/strong[@class='value']");
                string location = locationNode != null ? Regex.Replace(locationNode.InnerText, @"\s+", " ").Trim() : "";
                var fluencyNode = profileNode.SelectSingleNode(".//div[@class='left-col']//p[@class='entry'][span[@class='label']='Fluency']/strong[@class='value']");
                var fluency = fluencyNode?.InnerText.Trim();
                var bioNode = profileNode.SelectSingleNode(".//div[@class='right-col']/div[@class='inner']/div[@class='entry bio']/div[@class='value']");
                var bio = bioNode?.InnerText.Trim();
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"UEE Citizen Record: {citizenRecord}");
                Console.ResetColor();
                Console.WriteLine($"Nickname: {nickname}");
                Console.WriteLine($"Handle Name: {handleName}");
                Console.WriteLine($"Badge: {badge}");
                Console.WriteLine($"Enlisted: {enlistedDate}");
                Console.WriteLine($"Location: {location}");
                Console.WriteLine($"Language Fluency: {fluency}");
                Console.WriteLine($"\nBio: {bio}");
                await ScrapeOrganizationPage(username);
            }
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Profile not found or unable to scrape the data.");
                Console.ResetColor();
            }

            Console.WriteLine("\nPress any key to return to the main menu...");
            Console.ReadKey();
            Console.Clear();
        }
        static async Task ScrapeOrganizationPage(string username)
        {
            string orgUrl = $"https://robertsspaceindustries.com/citizens/{username}/organizations";
            var httpClient = new HttpClient();
            var orgHtml = await httpClient.GetStringAsync(orgUrl);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(orgHtml);
            var orgNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'box-content org main visibility-V')]");
            if (orgNode != null)
            {
                var orgName = orgNode.SelectSingleNode(".//div[@class='info']/p[@class='entry']/a")?.InnerText.Trim();
                var orgSID = orgNode.SelectSingleNode(".//div[@class='info']/p[@class='entry'][span[@class='label']='Spectrum Identification (SID)']/strong")?.InnerText.Trim();
                var orgRank = orgNode.SelectSingleNode(".//div[@class='info']/p[@class='entry'][span[@class='label']='Organization rank']/strong")?.InnerText.Trim();
                Console.WriteLine($"\nMain Organization Name: {orgName}");
                Console.WriteLine($"Organization Spectrum Identification (SID): {orgSID}");
                Console.WriteLine($"Organization rank: {orgRank}");
                var rightColNode = orgNode.SelectSingleNode(".//div[@class='right-col']");
                if (rightColNode != null)
                {
                    var archetype = rightColNode.SelectSingleNode(".//p[@class='entry'][span[@class='label']='Archetype']/strong[@class='value']")?.InnerText.Trim();
                    var primaryLanguage = rightColNode.SelectSingleNode(".//p[@class='entry'][span[@class='label']='Prim. Language']/strong[@class='value']")?.InnerText.Trim();
                    var primaryActivity = rightColNode.SelectSingleNode(".//p[@class='entry'][span[@class='label']='Prim. Activity']/strong[@class='value']")?.InnerText.Trim();
                    var recruiting = rightColNode.SelectSingleNode(".//p[@class='entry'][span[@class='label']='Recruiting']/strong[@class='value']")?.InnerText.Trim();
                    var secondaryActivity = rightColNode.SelectSingleNode(".//p[@class='entry'][span[@class='label']='Sec. Activity']/strong[@class='value']")?.InnerText.Trim();
                    var rolePlay = rightColNode.SelectSingleNode(".//p[@class='entry'][span[@class='label']='Role Play']/strong[@class='value']")?.InnerText.Trim();
                    var commitment = rightColNode.SelectSingleNode(".//p[@class='entry'][span[@class='label']='Commitment']/strong[@class='value']")?.InnerText.Trim();
                    var exclusive = rightColNode.SelectSingleNode(".//p[@class='entry'][span[@class='label']='Exclusive']/strong[@class='value']")?.InnerText.Trim();
                    Console.WriteLine($"\nArchetype: {archetype}");
                    Console.WriteLine($"Primary Language: {primaryLanguage}");
                    Console.WriteLine($"Primary Activity: {primaryActivity}");
                    Console.WriteLine($"Recruiting: {recruiting}");
                    Console.WriteLine($"Secondary Activity: {secondaryActivity}");
                    Console.WriteLine($"Role Play: {rolePlay}");
                    Console.WriteLine($"Commitment: {commitment}");
                    Console.WriteLine($"Exclusive: {exclusive}");
                }
            }
            else
            {
                Console.WriteLine("\nMain Organization details not available or the organization is RESTRICTED.");
            }
        }

    }
}