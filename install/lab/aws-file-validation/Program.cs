using aws_file_validation.Services;

var fileCollection = new List<FileItem>();
var menuManager = new MenuManager();
var service = new S3BacketService();
var isRunning = true;

ShowGreeting();

while (isRunning)
{
    foreach (var item in menuManager.MainMenu)
    {
        Console.WriteLine("{0}. {1}", item.Key, item.MenuDescription);
    }

    var key = GetKeyValue("Your choice");

    if (!ValidateInput(key)) { continue; }
    if (IsQuit(key!)) { break; }

    if (key == "1")
    {
        await GetFilesCollectionAsync();
    }
    else if (key == "2")
    {
        FileExtensionDetected();
    }
    else if (key == "3")
    {
        var value = GetKeyValue("Enter file's number");
        if (!int.TryParse(value, out int index))
        {
            DisplayError($"Invalid index is entered - {value}");
            continue;
        }

        if (!fileCollection.Any())
        {
            DisplayError("Get File Collection first. Run option 1.");
            continue;
        }

        var filename = fileCollection.Where(p => p.index == index).Select(p => p.filename).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(filename))
        {
            DisplayError($"Filename is not exist for the key {index}");
            continue;
        }

        var detectedmime = await service.GetFileHeaderAsync(filename, CancellationToken.None);
        DisplaySuccess($"The MIME found: {detectedmime}");
    }
    else if (key == "4")
    {
        await GetFileMetadataAsync();
    }
}

#region functionality implementation

async Task GetFilesCollectionAsync()
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("--------------------------------------------------------------------------------------------");

    fileCollection.Clear();

    fileCollection = await service.GetFilesCollectionAsync(CancellationToken.None);

    if (fileCollection.Any())
    {
        foreach (var file in fileCollection)
        {
            Console.WriteLine("|{0,-3}|    {1}", file.index, file.filename);
        }
    }
    else
    {
        Console.WriteLine("There is no any files found.");
    }

    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.ForegroundColor = ConsoleColor.White;
}

string? GetKeyValue(string description)
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write("{0}: ", description);
    var value = Console.ReadLine();
    Console.ForegroundColor = ConsoleColor.White;

    return value;
}

void FileExtensionDetected()
{
    var value = GetKeyValue("Enter file's number");

    if (!ValidateInput(value, false))
    {
        return;
    }

    if (!GetSelectedFileIndex(value!, out int index))
    {
        return;
    }

    var file = fileCollection.First(p => p.index == index);
    var extension = Path.GetExtension(file.filename);

    DisplaySuccess($"The file extension found: {extension}");
}

bool GetSelectedFileIndex(string fileIndex, out int index)
{
    index = -1;

    if (!int.TryParse(fileIndex, out int id))
    {
        DisplayError($"Invalid index is entered - {fileIndex}");
        return false;
    }

    if (!fileCollection.Any())
    {
        DisplayError("Get File Collection first. Run option 1.");
        return false;
    }

    if (!fileCollection.Any(p => p.index == id))
    {
        DisplayError($"There is no any file with index {id} in the filecollection.");
        return false;
    }

    index = id;
    return true;
}

async Task GetFileMetadataAsync()
{
    var value = GetKeyValue("Enter file's number to retrieve metadata");

    if (!ValidateInput(value, false))
    {
        return;
    }

    if (!GetSelectedFileIndex(value!, out int index))
    {
        return;
    }

    var file = fileCollection.First(p => p.index == index);
    var response = await service.GetMetadataAsync(file.filename, CancellationToken.None);

    DisplaySuccess(response);
}

#endregion

#region Display messages and reposts

void DisplaySuccess(string message)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.WriteLine(message);
    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.ForegroundColor = ConsoleColor.White;
}

void DisplayError(string message)
{
    Console.ForegroundColor = ConsoleColor.DarkRed;
    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.WriteLine(message);
    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.ForegroundColor = ConsoleColor.White;
}

bool ValidateInput(string? key, bool ismenu = true)
{
    if (string.IsNullOrWhiteSpace(key))
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        DisplayError($"The Key seleted is empty. Please try again");
        Console.ForegroundColor = ConsoleColor.White;
        return false;
    }

    if (ismenu && !menuManager.IsKeyValid(key.ToUpper()))
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        DisplayError($"Invalid key {key}. Please try again");
        Console.ForegroundColor = ConsoleColor.White;
        return false;
    }

    return true;
}

bool IsQuit(string key)
{
    if (key.ToUpper() == Settings.QuitKey)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.WriteLine("Quit the application");
        Console.WriteLine("Today is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Console.WriteLine("See you soon");
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.White;
        return true;
    }

    return false;
}

void ShowGreeting()
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.WriteLine("File Validator prototype");
    Console.WriteLine("This is a simple command-line application to validate files inside AWS s3 backet");
    Console.WriteLine("Today is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    Console.WriteLine("Welcome");
    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.ForegroundColor = ConsoleColor.White;
}

#endregion
