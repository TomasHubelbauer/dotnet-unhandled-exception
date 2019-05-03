using System;
using System.Threading.Tasks;

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

      TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs e) =>
      {
        Console.WriteLine("Unhandled task scheduler exception was caught!");
      };

      var _ = Task.Run(() =>
      {
        throw new Exception("Hello World!");
      });

      Console.WriteLine("Fire and forget!");
      Console.ReadLine();
    }
  }
}
