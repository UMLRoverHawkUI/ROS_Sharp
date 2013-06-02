using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace _Gamepad64bitCompat
{
    class Program
    {
        static void Main(string[] args)
        {
            new ThreadStaticAttribute(CSNamedPipeClient.SystemIONamedPipeClient.Run(".", @"\\\pipe\GamePadHawtness"));
            Thread.Sleep(10);
            CSNamedPipeClient.SystemIONamedPipeClient.Kill();
        }
    }
}
