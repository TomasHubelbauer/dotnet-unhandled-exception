# .NET Core Unhandled Exception

In this repository I found out if unhandled exceptions are caught in .NET Core
and also if when using `async Task Main`, its unhandled exception will be caught.

1. `dotnet new console`
2. ``

Attaching the handler and throwing an exception later works:

```csharp
using System;

namespace dotnet_core_unhandled_exception
{
  class Program
  {
    static void Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled exception was caught!");
      };

      throw new Exception("Hello World!");
    }
  }
}
```

It works with `async Main`, too:

```csharp
using System;
using System.Threading.Tasks;

namespace dotnet_core_unhandled_exception
{
  class Program
  {
    static Task Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled exception was caught!");
      };

      throw new Exception("Hello World!");
    }
  }
}
```

It works even in a continuation:

```csharp
using System;
using System.Threading.Tasks;

namespace dotnet_core_unhandled_exception
{
  class Program
  {
    static async Task Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled exception was caught!");
      };

      await Task.Run(() => Console.WriteLine("Extra work..."));
      throw new Exception("Hello World!");
    }
  }
}
```

And it works when the exception bubbles from as awaited `Task`:

```csharp
using System;
using System.Threading.Tasks;

namespace dotnet_core_unhandled_exception
{
  class Program
  {
    static async Task Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled exception was caught!");
      };

      await Task.Run(() =>
      {
        throw new Exception("Hello World!");
      });

      Console.WriteLine("Fire and forget!");
    }
  }
}
```

It _doesn't_ work with fire and forget! By itself.

```csharp
using System;
using System.Threading.Tasks;

namespace dotnet_core_unhandled_exception
{
  class Program
  {
    static async Task Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled exception was caught!");
      };

      Task.Run(() =>
      {
        throw new Exception("Hello World!");
      });

      Console.WriteLine("Fire and forget!");
      Console.ReadLine(); // Do not terminate the main thread before the other has a change to fail
    }
  }
}
```

`Task.Factory.StartNew` obviously gives the same result.

Unfortunately I can't get `TaskScheduler.UnobservedTaskException` to work:

```csharp
using System;
using System.Threading.Tasks;

namespace dotnet_core_unhandled_exception
{
  class Program
  {
    static async Task Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled exception was caught!");
      };

      TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled task scheduler exception was caught!");
      };

      var _ = Task.Run(() =>
      {
        throw new Exception("Hello World!");
      });

      Console.WriteLine("Fire and forget!");
      Console.ReadLine(); // Hold the main thread until the task one has a change to fail
    }
  }
}
```

But when we await the task thread in any way, our normal handler catches it just fine:

```csharp
using System;
using System.Threading.Tasks;

namespace dotnet_core_unhandled_exception
{
  class Program
  {
    static async Task Main(string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled exception was caught!");
      };

      TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled task scheduler exception was caught!");
      };

      var _ = Task.Run(() =>
      {
        throw new Exception("Hello World!");
      });

      Console.WriteLine("Fire and forget!");
      _.Wait(); // Or `await` or whatever
    }
  }
}
```

Not sure why `TaskScheduler.UnobservedTaskException` doesn't work in my example.

## To-Do
