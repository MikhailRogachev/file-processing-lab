using aws_file_validation.Services;

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("--------------------------------------------------------------------------------------------");
Console.WriteLine("File Validator prototype");
Console.WriteLine("This is a simple command-line application to validate files inside AWS s3 backet");
Console.WriteLine("Today is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
Console.WriteLine("Welcome");
Console.WriteLine("--------------------------------------------------------------------------------------------");
Console.ForegroundColor = ConsoleColor.White;

var fileCollection = new List<FileItem>();
var menuManager = new MenuManager();
var service = new S3BacketService();
var isRunning = true;

while (isRunning)
{
    foreach (var item in menuManager.MainMenu)
    {
        Console.WriteLine("{0}. {1}", item.Key, item.MenuDescription);
    }

    var key = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(key) || !menuManager.IsKeyValid(key.ToUpper()))
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("Invalid key {0}. Please try again", key);
        Console.ForegroundColor = ConsoleColor.White;
        continue;
    }

    if (!string.IsNullOrWhiteSpace(key) && key.ToUpper() == Settings.QuitKey)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.WriteLine("Quit the application");
        Console.WriteLine("Today is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Console.WriteLine("See you soon");
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.White;
        break;
    }

    if (key == "1")
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
    else if (key == "3")
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("Enter file number: ");
        var value = Console.ReadLine();
        Console.ForegroundColor = ConsoleColor.White;


        if (!int.TryParse(value, out int index))
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.WriteLine("Invalid index is entered - {0}", value);
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            continue;
        }

        if (!fileCollection.Any())
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.WriteLine("Get File Collection (1) first");
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            continue;
        }

        var filename = fileCollection.Where(p => p.index == index).Select(p => p.filename).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(filename))
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.WriteLine("Filename is not exist for the key {0}", index);
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;

            continue;
        }

        var detectedmime = await service.GetFileHeaderAsync(filename, CancellationToken.None);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.WriteLine(detectedmime);
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.White;
    }
}
