﻿using System;

namespace Desire_And_Doom
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var game = new DesireAndDoom())
                game.Run();
        }
    }
}