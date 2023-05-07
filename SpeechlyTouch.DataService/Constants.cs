﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace SpeechlyTouch.DataService
{
    public static class Constants
    {
        public const SQLite.SQLiteOpenFlags Flags =
           // open the database in read/write mode
           SQLite.SQLiteOpenFlags.ReadWrite |
           // create the database if it doesn't exist
           SQLite.SQLiteOpenFlags.Create |
           // enable multi-threaded database access
           SQLite.SQLiteOpenFlags.SharedCache;

    }
}
