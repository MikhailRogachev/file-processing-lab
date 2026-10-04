Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("--------------------------------------------------------------------------------------------");
Console.WriteLine("Local AWS S3 Floci Bucket manager.");
Console.WriteLine("This is a simple command-line application to manage AWS S3 buckets using Floci.");
Console.WriteLine("Today is: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
Console.WriteLine("Welcome");
Console.WriteLine("--------------------------------------------------------------------------------------------");
Console.ForegroundColor = ConsoleColor.White;

var menuManager = new MenuManager();
var service = new AwsS3Service();
var sqsservice = new AwsSqsService();

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

        var buckets = await service.ListS3BucketsAsync(CancellationToken.None);

        if (buckets.Any())
        {
            foreach (var bucket in buckets)
            {
                Console.WriteLine("Name {0}, Region {1}, Arn {2}", bucket.BucketName, bucket.BucketRegion, bucket.BucketArn);
            }
        }
        else
        {
            Console.WriteLine("There is no any buckets found.");
        }

        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (key == "2")
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.WriteLine("Create new bucket");
        Console.Write("Enter Bucket name: ");
        Console.ForegroundColor = ConsoleColor.White;

        var bucketName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(bucketName))
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("The bucketname must have value");
            Console.ForegroundColor = ConsoleColor.White;
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Blue;

        try
        {
            var bucket = await service.CreateS3BucketAsync(bucketName, CancellationToken.None);

            Console.WriteLine("Name {0}, Region {1}, Arn {2}", bucket.BucketName, bucket.BucketRegion, bucket.BucketArn);

        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.White;
    }
    else if (key == "4")
    {
        try
        {
            var queues = await sqsservice.QueueListAsync(CancellationToken.None);

            if (!queues.Any())
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("--------------------------------------------------------------------------------------------");
                Console.Write("There is no any queue found.");
                Console.WriteLine("--------------------------------------------------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.White;
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.WriteLine("QUEUE LIST:");

            foreach (var queue in queues)
            {
                Console.WriteLine("Queue - {0}", queue);
            }

            Console.WriteLine("--------------------------------------------------------------------------------------------");
            Console.ForegroundColor = ConsoleColor.White;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(ex.Message);
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
    else if (key == "5")
    {
        var response = await service.UploadFileAsync("sample001.mp4", CancellationToken.None);

        if (!response)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("File is not uploaded");
            Console.ForegroundColor = ConsoleColor.White;
            continue;
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.WriteLine("The file uploaded successfully");
        Console.WriteLine("--------------------------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.White;
    }

}
