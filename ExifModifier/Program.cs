using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ExifModifier
{
    class Program
    {
        enum PropertyItemId
        {
            ExifImageDateTime = 0x0132, //The date and time of image creation. In Exif standard, it is the date and time the file was changed.
            ExifImageDateTimeOriginal = 0x9003, //The date and time when the original image data was generated. For a digital still camera the date and time the picture was taken are recorded.
            ExifPhotoDateTimeDigitized = 0x9004, //The date and time when the image was stored as digital data.
            //ExifImageTimeZoneOffset = 0x882a, //This optional tag encodes the time zone of the camera clock (relative to Greenwich Mean Time) used to create the DataTimeOriginal tag-value when the picture was taken. It may also contain the time zone offset of the clock used to create the DateTime tag-value when the image was modified.
            ExifPhotoOffsetTime = 0x9010, //Time difference from Universal Time Coordinated including daylight saving time of DateTime tag.
            ExifPhotoOffsetTimeOriginal = 0x9011, //Time difference from Universal Time Coordinated including daylight saving time of DateTimeOriginal tag.
            ExifPhotoOffsetTimeDigitized = 0x9012, //Time difference from Universal Time Coordinated including daylight saving time of DateTimeDigitized tag.
        }

        static void Main(string[] args)
        {
            //Explore current and child directories
            string currentDirectory = Directory.GetCurrentDirectory();

            //currentDirectory = Path.Combine(new string[] { currentDirectory, "Img" });

            IEnumerable<string> files = Directory.EnumerateFiles(currentDirectory, "*.jp*g", SearchOption.AllDirectories);

            if (files.Count() == 0) return;

            foreach (var file in files)
            {
                Image currentImage = Image.FromFile(file);

                //Update LastWriteTime
                if (currentImage.PropertyItems.Any(p => p.Id == (int)PropertyItemId.ExifImageDateTimeOriginal))
                {
                    PropertyItem exifImageDateTimeOriginal = currentImage.GetPropertyItem((int)PropertyItemId.ExifImageDateTimeOriginal);

                    currentImage.Dispose();

                    string exifImageDateTimeOriginalStr = Encoding.UTF8.GetString(exifImageDateTimeOriginal.Value, 0, exifImageDateTimeOriginal.Len - 1);

                    DateTime.TryParseExact(exifImageDateTimeOriginalStr, "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime exifImageDateTimeOriginalParsed);
                    Console.WriteLine(exifImageDateTimeOriginalParsed);

                    if(File.GetLastWriteTime(file) != exifImageDateTimeOriginalParsed)
                    {
                        File.SetLastWriteTime(file, exifImageDateTimeOriginalParsed);
                        //File.SetLastWriteTimeUtc(file, exifImageDateTimeOriginalParsed.ToUniversalTime());
                    }
                }
                //Update Exif dates
                else
                {
                    Console.WriteLine("null");

                    var propertyItem = (PropertyItem)FormatterServices.GetUninitializedObject(typeof(PropertyItem));

                    DateTime lastWriteTime = File.GetLastWriteTime(file);

                    propertyItem.Type = 2;
                    propertyItem.Value = Encoding.UTF8.GetBytes(lastWriteTime.ToString("yyyy:MM:dd HH:mm:ss") + '\0'); //'\0' es un byte nulo
                    propertyItem.Len = propertyItem.Value.Length;

                    foreach (int i in new int[]{ 
                                        (int)PropertyItemId.ExifImageDateTime, 
                                        (int)PropertyItemId.ExifImageDateTimeOriginal, 
                                        (int)PropertyItemId.ExifPhotoDateTimeDigitized})
                    {
                        propertyItem.Id = i;
                        currentImage.SetPropertyItem(propertyItem);
                    }

                    // save to a memorystream
                    MemoryStream memoryStream = new MemoryStream();
                    currentImage.Save(memoryStream, currentImage.RawFormat);

                    // dispose old image
                    currentImage.Dispose();

                    // save new image to same filename
                    Image newImage = Image.FromStream(memoryStream);
                    newImage.Save(file);
                    
                    File.SetLastWriteTime(file, lastWriteTime);
                }
            }

            Console.WriteLine($"Press any key to close.");
            Console.ReadKey();
        }
    }
}
