using System.Text;

ShowGreeting();

var fileCollection = new List<FileItem>();
var menuManager = new MenuManager();
var target = ConnectionTarget.LOCAL;
var isRunning = true;

while (isRunning)
{
    foreach (var item in menuManager.MainMenu)
    {
        Console.WriteLine("{0}. {1}", item.Key, item.MenuDescription);
    }

    var key = GetKeyValue("Select the item from the menu");
    if (!ValidateInput(key)) { continue; }
    if (IsQuit(key!)) { break; }

    if (key == "1")
    {
        await GetBucketsCollectionAsync(CancellationToken.None);
    }
    else if (key == "2")
    {
        await GetFilesCollectionAsync(CancellationToken.None);
    }
    else if (key == "3")
    {
        await UploadFileAsync(CancellationToken.None);
    }
    else if (key == "4")
    {
        await ValidateFileAsync(CancellationToken.None);
    }
}

#region private methods and functions

async Task GetBucketsCollectionAsync(CancellationToken cancellationToken)
{
    try
    {
        var service = new S3BucketCollection(target);
        var bld = new StringBuilder();

        var response = await service.RunRequestAsync(new S3RequestContext(), cancellationToken);

        if (!response.IsSuccess)
        {
            ShowErrorMessage(response.Message);
            return;
        }

        bld.AppendLine("Bucket List:");

        foreach (var bucket in response.Context!)
        {
            bld.AppendLine(string.Format("Name {0}, Region {1}, Arn {2}", bucket.BucketName, bucket.BucketRegion, bucket.BucketArn));
        }

        ShowSucceessMessage(bld.ToString(), ConsoleColor.Green);
    }
    catch (Exception ex)
    {
        var bld = new StringBuilder();
        bld.AppendLine($"The Error occured during GetBacketsCollectionAsync function");
        bld.AppendLine($"Message: {ex.Message}, Error Type: {ex.GetType()}");

        ShowErrorMessage(bld.ToString());
    }
}

async Task GetFilesCollectionAsync(CancellationToken cancellationToken)
{
    try
    {
        var service = new S3ObjectCollection(target);
        var bld = new StringBuilder();

        fileCollection.Clear();

        var response = await service.RunRequestAsync(new S3RequestContext
        {
            BucketName = service.Bucket,
            Path = service.FolderName
        }, cancellationToken);

        if (!response.IsSuccess)
        {
            ShowErrorMessage(response.Message);
            return;
        }

        fileCollection = response.Context!.ToList();

        if (fileCollection.Any())
        {
            bld.AppendLine("Files Collection:");

            foreach (var file in fileCollection)
            {
                bld.AppendLine(string.Format("|{0,-3}|    {1}", file.Index, file.FileName));
            }
        }

        ShowSucceessMessage(bld.ToString());
    }
    catch (Exception ex)
    {
        var bld = new StringBuilder();
        bld.AppendLine($"The Error occured during GetFilesCollectionAsync function");
        bld.AppendLine($"Message: {ex.Message}, Error Type: {ex.GetType()}");

        ShowErrorMessage(bld.ToString());
    }
}

async Task UploadFileAsync(CancellationToken cancelToken)
{
    if (target == ConnectionTarget.REMOTE)
    {
        ShowErrorMessage("The file upload is available for the LOCAL mode only.");
        return;
    }

    try
    {

        var menuExist = menuManager.GetTestDataFilesMenu();

        if (!menuExist)
        {
            ShowErrorMessage(menuManager.FileMenu);
            return;
        }

        ShowSucceessMessage(menuManager.FileMenu, ConsoleColor.DarkBlue);

        var fn = GetKeyValue("Please select number of the file to upload");
        if (!ValidateInput(fn, false)) { return; }
        if (IsQuit(fn!)) { return; }

        if (!int.TryParse(fn, out int index))
        {
            ShowErrorMessage("Invalid file number selected. Numbers allowed only.");
            return;
        }

        var fileitem = menuManager.FileMenuItems.FirstOrDefault(p => p.Index == index);
        if (fileitem == null)
        {
            ShowErrorMessage(string.Format("The file with number {0} is not found.", index));
            return;
        }

        // add file identifier
        var fi = GetKeyValue("Entry File Identifier (leave empty if it's not applicable):");

        // start process
        var service = new S3FileUpload(target);
        var response = await service.RunRequestAsync(new S3RequestContext
        {
            BucketName = service.Bucket,
            FileName = fileitem.FileName,
            Path = fileitem.Path,
            PackageIdentifier = !string.IsNullOrWhiteSpace(fi) ? fi : string.Empty
        }, cancelToken);

        if (response.IsSuccess)
        {
            var bld = new StringBuilder();
            bld.AppendLine(response.Message);
            bld.AppendLine(response.Context!.ToString());

            ShowSucceessMessage(bld.ToString(), ConsoleColor.DarkGreen);
        }
        else
        {
            ShowErrorMessage(response.Message);
        }
    }
    catch (Exception ex)
    {
        var bld = new StringBuilder();
        bld.AppendLine($"The Error occured during UploadFileAsync function");
        bld.AppendLine($"Message: {ex.Message}, Error Type: {ex.GetType()}");

        ShowErrorMessage(bld.ToString());
    }
}

async Task ValidateFileAsync(CancellationToken cancellationToken)
{
    try
    {
        var bld = new StringBuilder();

        if (!fileCollection.Any())
        {
            bld.AppendLine("Please run menu 'Get the list of files in the bucket.'");
            bld.AppendLine("to get files collection from the Bucket");

            ShowSucceessMessage(bld.ToString(), ConsoleColor.DarkRed);

            return;
        }

        bld.AppendLine("Files Collection to select file:");

        foreach (var file in fileCollection)
        {
            bld.AppendLine(string.Format("|{0,-3}|    {1}", file.Index, file.FileName));
        }

        Console.WriteLine(bld.ToString());
        var fn = GetKeyValue("Please select number of the file to validate");
        if (!ValidateInput(fn, false)) { return; }
        if (IsQuit(fn!)) { return; }

        if (!int.TryParse(fn, out int index))
        {
            ShowErrorMessage("Invalid file number selected. Numbers allowed only.");
            return;
        }

        var fileitem = fileCollection.FirstOrDefault(p => p.Index == index);
        if (fileitem == null)
        {
            ShowErrorMessage(string.Format("The file with number {0} is not found.", index));
            return;
        }

        var service = new S3FileValidator();
        var response = await service.ValidateFileAsync(fileitem.FileName, target, cancellationToken);

        if (!service.IsValid)
        {
            ShowErrorMessage(response);
            return;
        }

        ShowSucceessMessage(response, ConsoleColor.DarkGreen);

    }
    catch (Exception ex)
    {
        var bld = new StringBuilder();
        bld.AppendLine($"The Error occured during ValidateFileAsync function");
        bld.AppendLine($"Message: {ex.Message}, Error Type: {ex.GetType()}");

        ShowErrorMessage(bld.ToString());
    }
}

string? GetKeyValue(string description)
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write("{0}: ", description);
    var value = Console.ReadLine();
    Console.ForegroundColor = ConsoleColor.White;

    return value;
}

void ShowSucceessMessage(string message, ConsoleColor color = ConsoleColor.DarkYellow)
{
    Console.ForegroundColor = color;
    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.WriteLine(message);
    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.ForegroundColor = ConsoleColor.White;
}

void ShowErrorMessage(string message)
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
        ShowErrorMessage($"The Key seleted is empty. Please try again");
        Console.ForegroundColor = ConsoleColor.White;
        return false;
    }

    if (ismenu && !menuManager.IsKeyValid(key.ToUpper()))
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        ShowErrorMessage($"Invalid key {key}. Please try again");
        Console.ForegroundColor = ConsoleColor.White;
        return false;
    }

    return true;
}

bool IsQuit(string key)
{
    if (key.ToUpper() == KeySettings.QuitKey)
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
    Console.WriteLine("Local AWS S3 Floci Bucket manager.");
    Console.WriteLine("This is a simple command-line application to manage AWS S3 buckets using Floci.");
    Console.WriteLine("Today is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    Console.WriteLine("Welcome");
    Console.WriteLine("--------------------------------------------------------------------------------------------");
    Console.ForegroundColor = ConsoleColor.White;
}

#endregion
