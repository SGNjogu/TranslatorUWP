using Microsoft.Data.Sqlite;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using SQLiteCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SQLiteConnection = Microsoft.Data.Sqlite.SqliteConnection;

namespace SpeechlyTouch.Helpers
{
    public class DataExportHelper
    {
        private static IWorkbook workbook { get; set; }
        public static async Task GetExcelFile()
        {
            try
            {
                PrepareFile();

                // Save the file
                var saveFileDialog = new FileSavePicker();
                saveFileDialog.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                saveFileDialog.FileTypeChoices.Add("Plain Text", new List<string>() { ".xlsx" });
                saveFileDialog.SuggestedFileName = "TalaSessionsExportFile";

                StorageFile file = await saveFileDialog.PickSaveFileAsync();
                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);

                    // write to file
                    using (var item = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        workbook.Write(item.AsStream());
                    }

                    // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw ex;
            }
        }
        private static void PrepareFile()
        {
            try
            {
                // Create a .xlsx workbook
                workbook = new XSSFWorkbook();

                // Create some cell styles
                IFont font = workbook.CreateFont();
                font.IsBold = true;
                font.FontHeightInPoints = 11;
                ICellStyle boldStyle = workbook.CreateCellStyle();
                boldStyle.SetFont(font);

                // Create a worksheet
                ISheet sheet = workbook.CreateSheet(string.Format("Translations {0:dd-MM-yyyy}", DateTime.UtcNow));

                // Create header row and columns
                IRow row = sheet.CreateRow(0);
                ICell cell = row.CreateCell(0);
                cell.SetCellValue("Session Name");
                cell = row.CreateCell(1);
                cell.SetCellValue("Session Number");
                cell = row.CreateCell(2);
                cell.SetCellValue("Session Date");
                cell = row.CreateCell(3);
                cell.SetCellValue("Session Id");
                cell = row.CreateCell(4);
                cell.SetCellValue("Session Start Time");
                cell = row.CreateCell(5);
                cell.SetCellValue("Session Duration (mm:ss)");
                cell = row.CreateCell(6);
                cell.SetCellValue("Original Language");
                cell = row.CreateCell(7);
                cell.SetCellValue("Translated Language");
                cell = row.CreateCell(8);
                cell.SetCellValue("User");
                cell = row.CreateCell(9);
                cell.SetCellValue("Original Text");
                cell = row.CreateCell(10);
                cell.SetCellValue("Translated Text");
                cell = row.CreateCell(11);
                cell.SetCellValue("Transcription Timestamp");
                cell = row.CreateCell(12);
                cell.SetCellValue("Text Sentiment");

                // Set all cells in the header row to bold
                foreach (var cellItem in row.Cells) cellItem.CellStyle = boldStyle;

                var dbPath = Constants.ResolvedDatabasePath;
                //Read data from the db and add it to the excel file

                string cs = $@"Data Source={dbPath};Mode=ReadWrite;";

                Console.WriteLine($"Database Path: {cs}");

                string data = String.Empty;

                using (SQLiteConnection con = new SQLiteConnection(cs))
                {
                    con.Open();

                    string stm = "SELECT Session.SessionName, Session.SessionNumber, Session.RecordDate, Session.ID, Session.RawStartTime, Session.Duration, Session.SourceLanguage, Session.TargeLanguage, Transcription.ChatUser, Transcription.OriginalText, Transcription.TranslatedText, Transcription.Timestamp, Transcription.Sentiment " +
                            "FROM Transcription INNER JOIN Session ON Session.ID = Transcription.SessionId";

                    using (SQLiteCommand cmd = new SQLiteCommand(stm, con))
                    {
                        using (SqliteDataReader rdr = cmd.ExecuteReader())
                        {
                            int rowNumber = 1;
                            while (rdr.Read()) // Reading Rows
                            {
                                row = sheet.CreateRow(rowNumber++);
                                for (int i = 0; i <= 11; i++)
                                {
                                    cell = row.CreateCell(i);
                                    cell.SetCellValue(rdr.GetValue(i).ToString());
                                    if (!string.IsNullOrEmpty(rdr.GetValue(i).ToString()))
                                    {
                                        Console.WriteLine(rdr.GetValue(i).ToString());
                                    }
                                    else
                                    {
                                        Console.WriteLine("VALUE IS NULL");
                                    }
                                }
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CREATION_ERROR:{ex.Message}");
                throw ex;
            }
        }

        public static async Task<StorageFile> ShareFile()
        {
            try
            {
                PrepareFile();

                StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
                var files = await storageFolder.GetFilesAsync();

                if (files.Count != 0)
                {
                    foreach (var item in files)
                    {
                        await item.DeleteAsync();
                    }
                }

                StorageFile file = await storageFolder.CreateFileAsync("TalaSessionsExportFile.xlsx", CreationCollisionOption.GenerateUniqueName);

                if (file != null)
                {
                    // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync.
                    CachedFileManager.DeferUpdates(file);

                    // write to file
                    using (var item = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        workbook.Write(item.AsStream());
                    }

                    // Let Windows know that we're finished changing the file so the other app can update the remote version of the file.
                    // Completing updates may require Windows to ask for user input.
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    return file;
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
